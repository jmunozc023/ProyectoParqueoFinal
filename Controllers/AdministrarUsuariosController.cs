using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoParqueoFinal.Data;
using ProyectoParqueoFinal.Models;
using ProyectoParqueoFinal.ViewModels;


namespace ProyectoParqueoFinal.Controllers
{

    public class AdministrarUsuariosController : Controller
    {
        private readonly AppDBContext _appDBcontext;

        public AdministrarUsuariosController(AppDBContext appDBcontext)
        {
            _appDBcontext = appDBcontext;
        }
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public IActionResult AdministrarUsuarios()
        {
            var usuarios = _appDBcontext.Usuarios.ToList();
            ViewData["Usuarios"] = usuarios;
            return View();
        }
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public async Task<IActionResult> AdministrarUsuarios(AdministrarUsuariosVM modelo)
        {
            bool emailExists = await _appDBcontext.Usuarios.AnyAsync(u => u.CorreoElectronico == modelo.CorreoElectronico);
            if (emailExists)
            {
                ViewData["Mensaje"] = "El correo ingresado ya está registrado en el sistema.";
                return View();
            }
            bool cedulaExists = await _appDBcontext.Usuarios.AnyAsync(u => u.Cedula == modelo.Cedula);
            if (cedulaExists)
            {
                ViewData["Mensaje"] = "La cedula ingresada ya está registrada en el sistema. ";
                return View();
            }
            bool carneExists = await _appDBcontext.Usuarios.AnyAsync(u => u.NumeroCarne == modelo.NumeroCarne);
            if (carneExists)
            {
                ViewData["Mensaje"] = "El número de Carnet ingresado ya esta registrado en el sistema. ";
            }
            Usuario usuario = new Usuario
            {
                Nombre = modelo.Nombre,
                Apellido = modelo.Apellido,
                CorreoElectronico = modelo.CorreoElectronico,
                FechaNacimiento = modelo.FechaNacimiento,
                Cedula = modelo.Cedula,
                NumeroCarne = modelo.NumeroCarne,
                Password = modelo.Password,
                Rol = modelo.Rol,
                RequiereCambioPassword = true

            };
            using (var transaction = await _appDBcontext.Database.BeginTransactionAsync())
            {
                try
                {
                    _appDBcontext.Usuarios.Add(usuario);
                    await _appDBcontext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    ViewData["Mensaje"] = "Usuario registrado exitosamente";
                    return RedirectToAction("AdministrarUsuarios");
                }
                catch
                {
                    await transaction.RollbackAsync();
                    ViewData["Mensaje"] = "Error al guardar el usuario.";
                    return View();
                }
            }
            

        }
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public async Task<IActionResult> EliminarUsuario(int IdUsuario)
        {
            var usuario = await _appDBcontext.Usuarios.FindAsync(IdUsuario);
            if (usuario == null)
            {
                return NotFound();
            }
            using (var transaction = await _appDBcontext.Database.BeginTransactionAsync())
            {
                try
                {
                    _appDBcontext.Usuarios.Remove(usuario);
                    await _appDBcontext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    ViewData["Mensaje"] = "Usuario eliminado exitosamente";
                    return RedirectToAction("AdministrarUsuarios");
                }
                catch
                {
                    await transaction.RollbackAsync();
                    ViewData["Mensaje"] = "Error al eliminar el usuario.";
                    return View();
                }
            }
        }
    }
}
