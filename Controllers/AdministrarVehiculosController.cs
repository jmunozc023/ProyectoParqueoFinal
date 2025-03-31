using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ProyectoParqueoFinal.Data;
using ProyectoParqueoFinal.Models;
using ProyectoParqueoFinal.ViewModels;

namespace ProyectoParqueoFinal.Controllers
{
    public class AdministrarVehiculosController : Controller
    {
        private readonly AppDBContext _appDBcontext;

        public AdministrarVehiculosController(AppDBContext appDBcontext)
        {
            _appDBcontext = appDBcontext;
        }

        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public IActionResult AdministrarVehiculos()
        {
            var vehiculos = _appDBcontext.Vehiculos.Include(v => v.Usuario).ToList();
            ViewData["Vehiculos"] = vehiculos;

            var usuarios = _appDBcontext.Usuarios.ToList();
            ViewData["Usuarios"] = usuarios;
            return View();
        }
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public async Task<IActionResult> AdministrarVehiculos(AdministrarVehiculosVM modelo1)
        {
            bool placaExists = await _appDBcontext.Vehiculos.AnyAsync(v => v.NumeroPlaca == modelo1.NumeroPlaca);
            if (placaExists)
            {
                ViewData["Mensaje"] = "La placa ingresada ya está registrada en el sistema.";
                return View();
            }

            int vehiculosCount = await _appDBcontext.Vehiculos.CountAsync(v => v.UsuariosIdUsuario == modelo1.UsuariosIdUsuario);
            if (vehiculosCount >= 2)
            {
                ViewData["Mensaje"] = "El usuario ya tiene el máximo permitido de 2 vehículos registrados.";
                return View();
            }

            Vehiculo vehiculo = new Vehiculo
            {
                Marca = modelo1.Marca,
                Modelo = modelo1.Modelo,
                Color = modelo1.Color,
                NumeroPlaca = modelo1.NumeroPlaca,
                TipoVehiculo = modelo1.TipoVehiculo,
                UsaEspacio7600 = modelo1.UsaEspacio7600,
                UsuariosIdUsuario = modelo1.UsuariosIdUsuario
            };

            if (string.IsNullOrEmpty(modelo1.Marca))
            {
                ViewData["Mensaje"] = "El campo Marca es obligatorio.";
                return View(modelo1);
            }
            if (string.IsNullOrEmpty(modelo1.Modelo))
            {
                ViewData["Mensaje"] = "El campo Modelo es obligatorio.";
                return View(modelo1);
            }
            if (string.IsNullOrEmpty(modelo1.Color))
            {
                ViewData["Mensaje"] = "El campo Color es obligatorio.";
                return View(modelo1);
            }
            if (string.IsNullOrEmpty(modelo1.NumeroPlaca))
            {
                ViewData["Mensaje"] = "El campo Numero de placa es obligatorio.";
                return View(modelo1);
            }
            if (string.IsNullOrEmpty(modelo1.TipoVehiculo))
            {
                ViewData["Mensaje"] = "El campo Tipo de vehiculo es obligatorio.";
                return View(modelo1);
            }

            using (var transaction = await _appDBcontext.Database.BeginTransactionAsync())
            {
                try
                {
                    _appDBcontext.Vehiculos.Add(vehiculo);
                    await _appDBcontext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    ViewData["Mensaje"] = "Vehiculo registrado exitosamente";
                    return RedirectToAction("AdministrarVehiculos");
                }
                catch
                {
                    await transaction.RollbackAsync();
                    ViewData["Mensaje"] = "Error al guardar el vehiculo.";
                    return View();
                }
            }
        }
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public async Task<IActionResult> EliminarVehiculo(int idVehiculo)
        {
            var vehiculo = await _appDBcontext.Vehiculos.FindAsync(idVehiculo);
            if (vehiculo == null)
            {
                return NotFound();
            }
            _appDBcontext.Vehiculos.Remove(vehiculo);
            await _appDBcontext.SaveChangesAsync();
            ViewData["Mensaje"] = "Vehiculo eliminado exitosamente";
            return RedirectToAction("AdministrarVehiculos");
        }

    }
}
