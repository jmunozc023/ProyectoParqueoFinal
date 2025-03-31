using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoParqueoFinal.Data;
using ProyectoParqueoFinal.ViewModels;

namespace ProyectoParqueoFinal.Controllers
{
    public class ModificarVehiculosController : Controller
    {
        private readonly AppDBContext _appDBcontext;
        public ModificarVehiculosController(AppDBContext appDBcontext)
        {
            _appDBcontext = appDBcontext;
        }
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> ModificarVehiculos(int IdVehiculo)
        {
            var vehiculo = await _appDBcontext.Vehiculos.Where(v => v.IdVehiculo == IdVehiculo).FirstOrDefaultAsync();
            if (vehiculo == null)
            {
                return NotFound();
            }
            var modelo1 = new ModificarVehiculosVM
            {
                IdVehiculo = vehiculo.IdVehiculo,
                Marca = vehiculo.Marca,
                Modelo = vehiculo.Modelo,
                Color = vehiculo.Color,
                NumeroPlaca = vehiculo.NumeroPlaca,
                TipoVehiculo = vehiculo.TipoVehiculo,
                UsaEspacio7600 = vehiculo.UsaEspacio7600,
                UsuariosIdUsuario = vehiculo.UsuariosIdUsuario            
            };
            return View("ModificarVehiculos", modelo1);
        }
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public async Task<IActionResult> ModificarVehiculos(ModificarVehiculosVM modelo1)
        {
            var vehiculo = await _appDBcontext.Vehiculos.Where(v => v.NumeroPlaca == modelo1.NumeroPlaca).FirstOrDefaultAsync();
            if (vehiculo == null) return NotFound();
            vehiculo.Marca = modelo1.Marca;
            vehiculo.Modelo = modelo1.Modelo;
            vehiculo.Color = modelo1.Color;
            vehiculo.NumeroPlaca = modelo1.NumeroPlaca;
            vehiculo.TipoVehiculo = modelo1.TipoVehiculo;
            vehiculo.UsaEspacio7600 = modelo1.UsaEspacio7600;
            _appDBcontext.Update(vehiculo);
            await _appDBcontext.SaveChangesAsync();
            ViewData["Mensaje"] = "Vehiculo modificado exitosamente";
            return RedirectToAction("AdministrarVehiculos", "AdministrarVehiculos");
        }
    }
}
