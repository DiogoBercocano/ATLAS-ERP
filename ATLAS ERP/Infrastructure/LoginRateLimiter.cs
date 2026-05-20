using System;
using System.Collections.Concurrent;
using System.Threading;

namespace ATLAS_ERP.Infrastructure
{
    /// <summary>
    /// Rate limiter in-memory para o endpoint de login. Aplica sliding window duplo:
    /// uma janela por IP (proteção contra varredura horizontal de credenciais) e outra
    /// por email (proteção contra brute force vertical em um único usuário).
    ///
    /// Estado mantido por AppDomain (mesmo padrão de PermissaoCache). Em web farm, cada
    /// instância terá contadores próprios — aceitável porque o atacante teria que
    /// distribuir as tentativas, o que reduz o trafego efetivo por nó.
    ///
    /// Sem dependência externa. Thread-safe via ConcurrentDictionary + lock por bucket.
    /// </summary>
    public static class LoginRateLimiter
    {
        // Limites por IP — atinge brute force horizontal e ataques distribuídos por proxy único.
        private const int IP_MAX_TENTATIVAS = 10;
        private static readonly TimeSpan IP_JANELA  = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan IP_LOCKOUT = TimeSpan.FromMinutes(15);

        // Limites por email — protege a conta mesmo que o atacante rotacione IPs (credential stuffing).
        private const int EMAIL_MAX_TENTATIVAS = 5;
        private static readonly TimeSpan EMAIL_JANELA  = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan EMAIL_LOCKOUT = TimeSpan.FromMinutes(30);

        // Cleanup oportunístico: roda no máximo uma vez por intervalo, no fluxo normal de requests.
        private static readonly TimeSpan CLEANUP_INTERVAL = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan ENTRY_TTL        = TimeSpan.FromHours(1);

        private static long _lastCleanupTicks = DateTime.UtcNow.Ticks;

        private static readonly ConcurrentDictionary<string, Bucket> _buckets =
            new ConcurrentDictionary<string, Bucket>(StringComparer.OrdinalIgnoreCase);

        private sealed class Bucket
        {
            public DateTime PrimeiraFalhaUtc;
            public DateTime UltimaFalhaUtc;
            public int Tentativas;
            public DateTime? LockedUntilUtc;
        }

        public sealed class Resultado
        {
            public bool Bloqueado;
            public TimeSpan TempoRestante;
            /// <summary>"ip" ou "email" — usado apenas em logs, não exposto ao cliente.</summary>
            public string Motivo;
            public int TentativasRestantes;
        }

        /// <summary>
        /// Verifica se a combinação (IP, email) já está bloqueada por lockout ativo.
        /// Não incrementa contadores. Use antes da validação de credencial.
        /// </summary>
        public static Resultado VerificarBloqueio(string ip, string email)
        {
            MaybeCleanup();
            var agora = DateTime.UtcNow;

            var resIp = ChecarLockout(KeyIp(ip), agora);
            if (resIp != null) { resIp.Motivo = "ip"; return resIp; }

            var resEmail = ChecarLockout(KeyEmail(email), agora);
            if (resEmail != null) { resEmail.Motivo = "email"; return resEmail; }

            return new Resultado { Bloqueado = false };
        }

        /// <summary>
        /// Registra uma tentativa de login falha. Retorna Bloqueado=true quando o threshold
        /// é atingido nesta chamada (cliente deve ser informado do lockout).
        /// </summary>
        public static Resultado RegistrarFalha(string ip, string email)
        {
            var agora = DateTime.UtcNow;
            var lockIp    = AcumularFalha(KeyIp(ip),    agora, IP_MAX_TENTATIVAS,    IP_JANELA,    IP_LOCKOUT);
            var lockEmail = AcumularFalha(KeyEmail(email), agora, EMAIL_MAX_TENTATIVAS, EMAIL_JANELA, EMAIL_LOCKOUT);

            if (lockIp    != null) { lockIp.Motivo    = "ip";    return lockIp; }
            if (lockEmail != null) { lockEmail.Motivo = "email"; return lockEmail; }
            return new Resultado { Bloqueado = false };
        }

        /// <summary>
        /// Limpa contadores do IP e do email após login bem-sucedido. Evita que um usuário
        /// legítimo seja bloqueado por falhas anteriores logo após acertar a senha.
        /// </summary>
        public static void RegistrarSucesso(string ip, string email)
        {
            Bucket _;
            _buckets.TryRemove(KeyIp(ip), out _);
            _buckets.TryRemove(KeyEmail(email), out _);
        }

        private static Resultado ChecarLockout(string key, DateTime agora)
        {
            Bucket b;
            if (!_buckets.TryGetValue(key, out b)) return null;

            lock (b)
            {
                if (b.LockedUntilUtc.HasValue && b.LockedUntilUtc.Value > agora)
                {
                    return new Resultado
                    {
                        Bloqueado = true,
                        TempoRestante = b.LockedUntilUtc.Value - agora
                    };
                }
            }
            return null;
        }

        private static Resultado AcumularFalha(string key, DateTime agora, int max, TimeSpan janela, TimeSpan lockout)
        {
            var b = _buckets.GetOrAdd(key, _ => new Bucket { PrimeiraFalhaUtc = agora });

            lock (b)
            {
                if (b.LockedUntilUtc.HasValue && b.LockedUntilUtc.Value > agora)
                {
                    return new Resultado
                    {
                        Bloqueado = true,
                        TempoRestante = b.LockedUntilUtc.Value - agora
                    };
                }

                // Lockout expirou — reinicia janela.
                if (b.LockedUntilUtc.HasValue && b.LockedUntilUtc.Value <= agora)
                {
                    b.LockedUntilUtc = null;
                    b.Tentativas = 0;
                    b.PrimeiraFalhaUtc = agora;
                }

                // Janela rolou — reinicia contador.
                if (agora - b.PrimeiraFalhaUtc > janela)
                {
                    b.PrimeiraFalhaUtc = agora;
                    b.Tentativas = 0;
                }

                b.Tentativas++;
                b.UltimaFalhaUtc = agora;

                if (b.Tentativas >= max)
                {
                    b.LockedUntilUtc = agora.Add(lockout);
                    return new Resultado
                    {
                        Bloqueado = true,
                        TempoRestante = lockout
                    };
                }
            }
            return null;
        }

        /// <summary>
        /// Cleanup oportunístico de buckets expirados — não roda em thread dedicada.
        /// Apenas o request que ganhar o CompareExchange faz o trabalho; demais saem cedo.
        /// </summary>
        private static void MaybeCleanup()
        {
            var nowTicks = DateTime.UtcNow.Ticks;
            var prev = Interlocked.Read(ref _lastCleanupTicks);
            if (nowTicks - prev < CLEANUP_INTERVAL.Ticks) return;
            if (Interlocked.CompareExchange(ref _lastCleanupTicks, nowTicks, prev) != prev) return;

            var agora = DateTime.UtcNow;
            var cutoff = agora.Subtract(ENTRY_TTL);

            foreach (var kv in _buckets)
            {
                var b = kv.Value;
                bool remover;
                lock (b)
                {
                    remover = (!b.LockedUntilUtc.HasValue || b.LockedUntilUtc.Value < agora)
                              && b.UltimaFalhaUtc < cutoff;
                }
                if (remover)
                {
                    Bucket _;
                    _buckets.TryRemove(kv.Key, out _);
                }
            }
        }

        private static string KeyIp(string ip)
            => "ip::" + (string.IsNullOrEmpty(ip) ? "unknown" : ip);

        private static string KeyEmail(string email)
            => "email::" + (string.IsNullOrEmpty(email) ? "anon" : email.Trim().ToLowerInvariant());
    }
}
