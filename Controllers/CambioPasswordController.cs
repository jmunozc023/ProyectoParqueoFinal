using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoParqueoFinal.Data;
using ProyectoParqueoFinal.Models;
using ProyectoParqueoFinal.ViewModels;

namespace ProyectoParqueoFinal.Controllers
{

    public class CambioPasswordController : Controller
    {
        private readonly AppDBContext _appDBcontext; // Asigna el database context a una variable privada
        public CambioPasswordController(AppDBContext appDBContext) // Constructor que recibe el database context
        {
            _appDBcontext = appDBContext;
        }

        // Método para mostrar la vista de cambio de contraseña
        [HttpGet]
        public IActionResult CambiarPassword(string correo) // Método para mostrar la vista de cambio de contraseña
        {

            if (string.IsNullOrEmpty(correo))
            {
                return NotFound(); // Si no hay correo, devuelve 404
            }
            var usuario = _appDBcontext.Usuarios.FirstOrDefault(u => u.CorreoElectronico == correo); // Busca el usuario en la base de datos

            var model = new CambioPasswordVM
            {
                CorreoElectronico = correo // Add the email to the model
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CambiarPassword(CambioPasswordVM modelo) // Método para cambiar la contraseña
        {
            if (!ModelState.IsValid) // Verifica si el modelo es válido
            {
                ViewData["Mensaje"] = "Datos inválidos.";
                return View(modelo);
            }

            if (modelo.NuevoPassword != modelo.ConfirmacionNuevoPassword) // Verifica que las contraseñas coincidan
            {
                ViewData["Mensaje"] = "Las contraseñas no coinciden.";
                return View(modelo);
            }

            var correo = modelo.CorreoElectronico; // Obtiene el correo del modelo
            if (correo == null)
            {
                return NotFound();
            }

            Usuario? usuario = await _appDBcontext.Usuarios // Busca el usuario en la base de datos
                .FirstOrDefaultAsync(u => u.CorreoElectronico == correo);

            if (usuario == null) // Verifica si el usuario existe
            {
                return NotFound();
            }

            // Verifica que la contraseña actual coincida
            if (usuario.Password != EncryptPassword(modelo.PasswordActual)) // Verifica que la contraseña actual coincida
            {
                ViewData["Mensaje"] = "La contraseña actual es incorrecta.";
                return View(modelo);
            }

            if(modelo.PasswordActual == modelo.NuevoPassword) // Verifica que la nueva contraseña no sea igual a la actual
            {
                ViewData["Mensaje"] = "La nueva contraseña no puede ser igual a la actual.";
                return View(modelo);
            }

            // Cambia la contraseña y actualiza el estado de "RequiereCambioContrasena"
            usuario.Password = modelo.NuevoPassword;
            usuario.RequiereCambioPassword = false;
            _appDBcontext.Usuarios.Update(usuario);
            await _appDBcontext.SaveChangesAsync();
            ViewData["Mensaje"] = "Contraseña cambiada exitosamente.";

            return RedirectToAction("LogIn", "LogIn"); // Redirige a la vista de inicio de sesión
        }
        private string EncryptPassword(string password) // Método para encriptar la contraseña
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create()) // Crea un objeto SHA256
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password)); // Convierte la contraseña a bytes y la encripta
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower(); // Devuelve la contraseña encriptada
            }
        }
    }
}
