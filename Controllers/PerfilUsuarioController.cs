using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoParqueoFinal.Data;
using ProyectoParqueoFinal.ViewModels;
using System.Security.Claims;

namespace ProyectoParqueoFinal.Controllers
{
    public class PerfilUsuarioController : Controller
    {
        private readonly AppDBContext _appDBcontext; // Asigna el database context a una variable privada
        public PerfilUsuarioController(AppDBContext appDBcontext) // Constructor que recibe el database context
        {
            _appDBcontext = appDBcontext;
        }
        [Authorize]
        [HttpGet]
        public IActionResult PerfilUsuario() // Método para mostrar la vista de perfil de usuario
        {
            var usuarioEmail = User.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault(); // Obtiene el nombre del usuario autenticado
            // Obtiene el nombre del usuario desde la cookie
            if (string.IsNullOrEmpty(usuarioEmail))
            {
                return RedirectToAction("Index", "Home"); // Redirige si no hay cookie
            }

            // Busca todos los datos del usuario en la base de datos junto con sus vehículos
            var usuario = _appDBcontext.Usuarios
                .Include(u => u.Vehiculos) // Incluye los vehículos relacionados
                .FirstOrDefault(u => u.CorreoElectronico == usuarioEmail);

            if (usuario == null)
            {
                return RedirectToAction("Index", "Home"); // Redirige si no encuentra el usuario
            }
            var perfilUsuarioVM = new PerfilUsuarioVM
            {
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                CorreoElectronico = usuario.CorreoElectronico,
                FechaNacimiento = usuario.FechaNacimiento,
                Cedula = usuario.Cedula,
                NumeroCarne = usuario.NumeroCarne,
                Vehiculos = usuario.Vehiculos.ToList() // Pasar la lista de vehículos correctamente
            };

            return View(perfilUsuarioVM);

        }

    }
}
