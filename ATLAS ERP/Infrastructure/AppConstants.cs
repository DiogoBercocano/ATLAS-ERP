namespace ATLAS_ERP.Infrastructure
{
    public static class Roles
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string Admin      = "Admin";
        public const string Gerente    = "Gerente";
        public const string Funcionario = "Funcionario";
    }

    public static class EmpresaStatus
    {
        public const string Ativa    = "Ativa";
        public const string Pendente = "Pendente";
    }

    public static class VendaStatus
    {
        public const string Aberta    = "Aberta";
        public const string Concluida = "Concluida";
        public const string Cancelada = "Cancelada";
    }

    public static class SessionKeys
    {
        public const string UsuarioLogado = "UsuarioLogado";
        public const string UsuarioId     = "UsuarioId";
        public const string Role          = "Role";
        public const string CargoId       = "CargoId";
        public const string CargoNome     = "CargoNome";
        public const string EmpresaId          = "EmpresaId";
        public const string MustChangePassword  = "MustChangePassword";
    }
}
