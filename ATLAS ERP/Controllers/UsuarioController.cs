using System.Linq;
using System.Web.Mvc;
using ATLAS_ERP.Data;
using ATLAS_ERP.Models;
using ATLAS_ERP.Filters;

namespace ATLAS_ERP.Controllers
{
    [RoleFilter("Admin")]
    public class UsuarioController : Controller
    {
        private readonly AtlasContext db = new AtlasContext();

        private int EmpresaId => (int)Session["EmpresaId"];

        public ActionResult Index()
        {
            var usuarios = db.Usuarios
                             .Where(u => u.EmpresaId == EmpresaId)
                             .ToList();
            return View(usuarios);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Usuario user)
        {
            if (ModelState.IsValid)
            {
                user.Ativo = true;
                user.EmpresaId = EmpresaId;
                db.Usuarios.Add(user);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(user);
        }

        // GET — redireciona caso alguém acesse /Usuario/Edit direto
        public ActionResult Edit()
        {
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Edit(int UsuarioId, string Name, string Email, string Role, string Ativo)
        {
            var u = db.Usuarios
                      .FirstOrDefault(x => x.UsuarioId == UsuarioId && x.EmpresaId == EmpresaId);
            if (u != null)
            {
                u.Name = Name;
                u.Email = Email;
                u.Role = Role;
                u.Ativo = Ativo == "true";
                db.Entry(u).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int usuarioId)
        {
            var u = db.Usuarios
                      .FirstOrDefault(x => x.UsuarioId == usuarioId && x.EmpresaId == EmpresaId);
            if (u != null)
            {
                db.Usuarios.Remove(u);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}