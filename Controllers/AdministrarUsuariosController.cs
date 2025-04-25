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

        //Controlador Get para la vista de administración de usuarios
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public IActionResult AdministrarUsuarios()
        {
            var usuarios = _appDBcontext.Usuarios.ToList(); //Obtiene los datos de los usuarios
            ViewData["Usuarios"] = usuarios;
            return View();
        }

        //Controlador Post para la vista de administración de usuarios
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public async Task<IActionResult> AdministrarUsuarios(AdministrarUsuariosVM modelo) //Crea un nuevo usuario
        {
            bool emailExists = await _appDBcontext.Usuarios.AnyAsync(u => u.CorreoElectronico == modelo.CorreoElectronico); // Verifica si el correo ya existe
            if (emailExists) //Validacion del correo si ya existe
            {
                ViewData["Mensaje"] = "El correo ingresado ya está registrado en el sistema.";
                return View();
            }
            bool cedulaExists = await _appDBcontext.Usuarios.AnyAsync(u => u.Cedula == modelo.Cedula); // Verifica si la cedula ya existe
            if (cedulaExists) //Validacion de la cedula si ya existe
            {
                ViewData["Mensaje"] = "La cedula ingresada ya está registrada en el sistema. ";
                return View();
            }
            bool carneExists = await _appDBcontext.Usuarios.AnyAsync(u => u.NumeroCarne == modelo.NumeroCarne); // Verifica si el carnet ya existe
            if (carneExists) //Validacion del carnet si ya existe
            {
                ViewData["Mensaje"] = "El número de Carnet ingresado ya esta registrado en el sistema. ";
            }
            Usuario usuario = new Usuario //Crea un nuevo usuario
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
            using (var transaction = await _appDBcontext.Database.BeginTransactionAsync()) //Inicia una transacción
            {
                try
                {
                    _appDBcontext.Usuarios.Add(usuario); //Agrega el usuario a la base de datos
                    await _appDBcontext.SaveChangesAsync(); //Guarda los cambios en la base de datos
                    await transaction.CommitAsync();

                    ViewData["Mensaje"] = "Usuario registrado exitosamente";
                    return RedirectToAction("AdministrarUsuarios"); //Redirige a la vista de administración de usuarios
                }
                catch
                {
                    await transaction.RollbackAsync(); //Revierte la transacción en caso de error
                    ViewData["Mensaje"] = "Error al guardar el usuario.";
                    return View();
                }
            }
            

        }

        //Controlador Post para la eliminacion de usuarios
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public async Task<IActionResult> EliminarUsuario(int IdUsuario)
        {
            var usuario = await _appDBcontext.Usuarios.FindAsync(IdUsuario); //Busca el usuario por su ID
            if (usuario == null)
            {
                return NotFound();
            }
            using (var transaction = await _appDBcontext.Database.BeginTransactionAsync()) //Inicia una transacción
            {
                try
                {
                    _appDBcontext.Usuarios.Remove(usuario); //Elimina el usuario
                    await _appDBcontext.SaveChangesAsync(); //  Guarda los cambios en la base de datos
                    await transaction.CommitAsync();
                    ViewData["Mensaje"] = "Usuario eliminado exitosamente";
                    return RedirectToAction("AdministrarUsuarios"); //Redirige a la vista de administración de usuarios
                }
                catch
                {
                    await transaction.RollbackAsync(); //Revierte la transacción en caso de error
                    ViewData["Mensaje"] = "Error al eliminar el usuario.";
                    return View();
                }
            }
        }
    }
}
