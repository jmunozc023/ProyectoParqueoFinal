using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using ProyectoParqueoFinal.Data;
using System.Security.Claims;

namespace ProyectoParqueoFinal.Controllers
{
    public class SeleccionParqueoController : Controller // Inherit from Controller to access User and RedirectToAction
    {
        private readonly AppDBContext _appDBcontext;

        public SeleccionParqueoController(AppDBContext appDBcontext)
        {
            _appDBcontext = appDBcontext;
        }

        [HttpGet]
        public IActionResult SeleccionarParqueo() // Método para mostrar la vista de selección de parqueo
        {
            if (User.Identity!.IsAuthenticated) return RedirectToAction("Index", "Home"); // Si el usuario ya está autenticado, redirige a la página principal

            var parqueos = _appDBcontext.Parqueos.ToList(); // Obtiene la lista de parqueos
            ViewData["Parqueos"] = parqueos; // Asigna la lista de parqueos a la vista
            ViewData["UserEmail"] = TempData["UserEmail"]; // Obtiene el correo del usuario
            ViewData["UserRole"] = TempData["UserRole"]; // Obtiene el rol del usuario

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SeleccionarParqueo(string NombreParqueo, string UserEmail, string UserRole) // Método para asignar un parqueo al usuario con rol de seguridad
        {
            if (NombreParqueo == null) return NotFound(); // Si no hay parqueo, devuelve 404

            if (UserEmail == null || UserRole == null) return Unauthorized(); // Si no hay correo o rol, devuelve 401

            // Add the assigned parqueo to the user's claims
            List<Claim> claims = new List<Claim> // Crea una lista de claims con el correo, rol y nombre del parqueo
                    {
                        new Claim(ClaimTypes.Name, UserEmail),
                        new Claim(ClaimTypes.Role, UserRole),
                        new Claim(ClaimTypes.StreetAddress, NombreParqueo)
                    };
            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme); // Crea un objeto ClaimsIdentity
            AuthenticationProperties authProperties = new AuthenticationProperties // Crea un objeto AuthenticationProperties
            {
                AllowRefresh = true,
            };
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties); // Autentica al usuario
            ViewData["Mensaje"] = "Inicio de sesión exitoso.";
            return RedirectToAction("Index", "Home");// Redirige a la página principal
        }
    }
}
