using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoParqueoFinal.Data;
using ProyectoParqueoFinal.ViewModels;

namespace ProyectoParqueoFinal.Controllers
{
    public class ModificarUsuariosController : Controller
    {
        private readonly AppDBContext _appDBcontext;

        public ModificarUsuariosController(AppDBContext appDBcontext)
        {
            _appDBcontext = appDBcontext;
        }

        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult>ModificarUsuarios(string correo)
        {
            var usuario = await _appDBcontext.Usuarios.Where(u => u.CorreoElectronico == correo).FirstOrDefaultAsync();
            if (usuario == null)
            {
                return NotFound();
            }
            var modelo = new ModificarUsuariosVM
            {
                IdUsuario = usuario.IdUsuario,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                CorreoElectronico = usuario.CorreoElectronico,
                FechaNacimiento = usuario.FechaNacimiento,
                Cedula = usuario.Cedula,
                NumeroCarne = usuario.NumeroCarne,
                Rol = usuario.Rol,
                RequiereCambioPassword = usuario.RequiereCambioPassword
            };
            return View("ModificarUsuarios", modelo);

        }
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public async Task<IActionResult> ModificarUsuarios(AdministrarUsuariosVM modelo)
        {

                var usuario = await _appDBcontext.Usuarios.Where(u => u.CorreoElectronico == modelo.CorreoElectronico).FirstOrDefaultAsync();
                if (usuario == null) return NotFound();
                usuario.Nombre = modelo.Nombre;
                usuario.Apellido = modelo.Apellido;
                usuario.CorreoElectronico = modelo.CorreoElectronico;
                usuario.FechaNacimiento = modelo.FechaNacimiento;
                usuario.Cedula = modelo.Cedula;
                usuario.NumeroCarne = modelo.NumeroCarne;
                usuario.Password = modelo.Password;
                usuario.Rol = modelo.Rol;
                usuario.RequiereCambioPassword = modelo.RequiereCambioPassword;
                _appDBcontext.Update(usuario);
                await _appDBcontext.SaveChangesAsync();
                ViewData["Mensaje"] = "Usuario modificado exitosamente";
            return RedirectToAction("AdministrarUsuarios", "AdministrarUsuarios");
        }
    }
}
