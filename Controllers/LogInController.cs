using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoParqueoFinal.Data;
using ProyectoParqueoFinal.Models;
using ProyectoParqueoFinal.ViewModels;
using System.Security.Claims;

namespace ProyectoParqueoFinal.Controllers
{
    public class LogInController : Controller
    {
        private readonly AppDBContext _appDBcontext; // Asigna el database context a una variable privada
        public LogInController(AppDBContext appDBContext) // Constructor que recibe el database context
        {
            _appDBcontext = appDBContext;
        }
        [HttpGet]
        public IActionResult LogIn() // Método para mostrar la vista de inicio de sesión
        {
            if (User.Identity!.IsAuthenticated) return RedirectToAction("Index", "Home"); // Si el usuario ya está autenticado, redirige a la página principal
            return View(); // Devuelve la vista
        }

        [HttpPost] // Método para autenticar un usuario
        public async Task<IActionResult> LogIn(LogInVM modelo)
        {
            if (!ModelState.IsValid) // Verifica si el modelo es válido
            {
                ViewData["Mensaje"] = "Datos inválidos.";
                return View();
            }

            Usuario? usuario_encontrado = await _appDBcontext.Usuarios// Busca el usuario en la base de datos
                .FirstOrDefaultAsync(u => u.CorreoElectronico == modelo.CorreoElectronico);
            TempData["UserEmail"] = usuario_encontrado.CorreoElectronico; // Guarda el correo del usuario en la cookie
            TempData["UserRole"] = usuario_encontrado.Rol; // Guarda el rol del usuario en la cookie

            // Verificación de existencia y contraseña
            if (usuario_encontrado == null || usuario_encontrado.Password != EncryptPassword(modelo.Password))
            {
                ViewData["Mensaje"] = "Usuario o contraseña incorrectos.";
                return View();
            }

            // Validación de cambio de contraseña pendiente
            if (usuario_encontrado.RequiereCambioPassword)
            {
                ViewData["Mensaje"] = "Debe cambiar su contraseña antes de continuar.";
                return RedirectToAction("CambiarPassword", "CambioPassword", new { correo = usuario_encontrado.CorreoElectronico });
            }

            // Creación de Claims para autenticación
            List<Claim> claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, usuario_encontrado.CorreoElectronico),
            new Claim(ClaimTypes.Role, usuario_encontrado.Rol)
        };
            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            AuthenticationProperties authProperties = new AuthenticationProperties { AllowRefresh = true };

            if (usuario_encontrado.Rol == "Seguridad") // Si el usuario es de rol de seguridad, redirige a la selección de parqueo
            {
                ViewData["Mensaje"] = "Debe seleccionar el parqueo al que está asignado.";
                return RedirectToAction("SeleccionarParqueo", "SeleccionParqueo");
            }

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties); // Autentica al usuario
            ViewData["Mensaje"] = "Inicio de sesión exitoso.";
            return RedirectToAction("Index", "Home"); // Redirige a la página principal
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
