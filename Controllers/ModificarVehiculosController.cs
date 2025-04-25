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
        //Controlador Get para la vista de modificacion de vehiculos
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> ModificarVehiculos(int IdVehiculo)
        {
            var vehiculo = await _appDBcontext.Vehiculos.Where(v => v.IdVehiculo == IdVehiculo).FirstOrDefaultAsync(); //Busca el vehiculo por su ID
            if (vehiculo == null)
            {
                return NotFound();
            }
            var modelo1 = new ModificarVehiculosVM //Crea un nuevo modelo de vehiculo
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
            return View("ModificarVehiculos", modelo1); //Devuelve la vista de modificacion de vehiculos
        }
        //Controlador Post para la vista de modificacion de vehiculos
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public async Task<IActionResult> ModificarVehiculos(ModificarVehiculosVM modelo1)
        {
            var vehiculo = await _appDBcontext.Vehiculos.Where(v => v.NumeroPlaca == modelo1.NumeroPlaca).FirstOrDefaultAsync(); //Busca el vehiculo por su placa
            if (vehiculo == null) return NotFound(); //Si no se encuentra el vehiculo, devuelve un error 404
            vehiculo.Modelo = modelo1.Modelo; //Actualiza el modelo del vehiculo
            vehiculo.Color = modelo1.Color; //Actualiza el color del vehiculo
            vehiculo.NumeroPlaca = modelo1.NumeroPlaca; //Actualiza el numero de placa del vehiculo
            vehiculo.TipoVehiculo = modelo1.TipoVehiculo;   //Actualiza el tipo de vehiculo
            vehiculo.UsaEspacio7600 = modelo1.UsaEspacio7600; //Actualiza el estado de "UsaEspacio7600"
            _appDBcontext.Update(vehiculo); //Actualiza el vehiculo en la base de datos
            await _appDBcontext.SaveChangesAsync(); //Guarda los cambios en la base de datos
            ViewData["Mensaje"] = "Vehiculo modificado exitosamente";
            return RedirectToAction("AdministrarVehiculos", "AdministrarVehiculos"); //Redirige a la vista de administración de vehiculos
        }
    }
}
