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

        //Controlador Get para la vista de modificacion de usuarios
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult>ModificarUsuarios(string correo)
        {
            var usuario = await _appDBcontext.Usuarios.Where(u => u.CorreoElectronico == correo).FirstOrDefaultAsync(); // Busca el usuario en la base de datos
            if (usuario == null)
            {
                return NotFound();
            }
            var modelo = new ModificarUsuariosVM // Crea un nuevo modelo de usuario
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
            return View("ModificarUsuarios", modelo); // Devuelve la vista de modificacion de usuarios

        }
        //Controlador Post para la vista de modificacion de usuarios
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public async Task<IActionResult> ModificarUsuarios(AdministrarUsuariosVM modelo)
        {

                var usuario = await _appDBcontext.Usuarios.Where(u => u.CorreoElectronico == modelo.CorreoElectronico).FirstOrDefaultAsync(); // Busca el usuario en la base de datos
            if (usuario == null) return NotFound(); // Si no se encuentra el usuario, devuelve un error 404
            usuario.Nombre = modelo.Nombre; // Actualiza el nombre del usuario
            usuario.Apellido = modelo.Apellido; // Actualiza el apellido del usuario
            usuario.CorreoElectronico = modelo.CorreoElectronico; // Actualiza el correo electronico del usuario
            usuario.FechaNacimiento = modelo.FechaNacimiento; // Actualiza la fecha de nacimiento del usuario
            usuario.Cedula = modelo.Cedula; // Actualiza la cedula del usuario
            usuario.NumeroCarne = modelo.NumeroCarne; // Actualiza el numero de carne del usuario
            usuario.Password = modelo.Password; // Actualiza la contraseña del usuario
            usuario.Rol = modelo.Rol; // Actualiza el rol del usuario
            usuario.RequiereCambioPassword = modelo.RequiereCambioPassword; // Actualiza el estado de "RequiereCambioContrasena"
            _appDBcontext.Update(usuario); // Actualiza el usuario en la base de datos
            await _appDBcontext.SaveChangesAsync(); // Guarda los cambios en la base de datos
            ViewData["Mensaje"] = "Usuario modificado exitosamente";
            return RedirectToAction("AdministrarUsuarios", "AdministrarUsuarios"); // Redirige a la vista de administración de usuarios
        }
    }
}
