using Microsoft.AspNetCore.Mvc;
using ProyectoParqueoFinal.Data;
using ProyectoParqueoFinal.Models;
using Microsoft.EntityFrameworkCore;
using ProyectoParqueoFinal.ViewModels;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using AspNetCoreGeneratedDocument;
using Microsoft.AspNetCore.Authorization;

namespace ProyectoParqueoFinal.Controllers
{
    public class SeguridadController : Controller
    {
        private readonly AppDBContext _appDBcontext;
        public SeguridadController(AppDBContext appDBcontext)
        {
            _appDBcontext = appDBcontext;
        }

        [Authorize(Roles = "Seguridad")]
        [HttpGet]
        public async Task<IActionResult> GestionParqueo()
        { 
            var nombrePaqueo = User.Claims.Where(c => c.Type == ClaimTypes.StreetAddress).Select(c => c.Value).SingleOrDefault();
            var parqueo = await _appDBcontext.Parqueos.FirstOrDefaultAsync(p => p.NombreParqueo == nombrePaqueo);

            if (parqueo == null)
            {
                TempData["Error"] = "No se encontró el parqueo asignado.";
                return View();
            }
            var vehiculos = await _appDBcontext.Vehiculos.ToListAsync();
            var bitacoras = await _appDBcontext.Bitacoras.Include(b => b.Vehiculo).Where(b => b.ParqueoIdParqueo == parqueo.IdParqueo).ToListAsync();
            
            var ingresosAutomoviles = bitacoras.Count(b => b.TipoIngreso == "Entrada" && b.Vehiculo.TipoVehiculo == "Automovil" && b.Vehiculo.UsaEspacio7600 != true);
            var salidasAutomoviles = bitacoras.Count(b => b.TipoIngreso == "Salida" && b.Vehiculo.TipoVehiculo == "Automovil" && b.Vehiculo.UsaEspacio7600 != true);
            var espaciosDisponiblesAutos = parqueo.CapacidadAutomoviles - ingresosAutomoviles + salidasAutomoviles;

            var ingresosMotociclestas = bitacoras.Count(b => b.TipoIngreso == "Entrada" && b.Vehiculo.TipoVehiculo == "Motocicleta" && b.Vehiculo.UsaEspacio7600 != true);
            var salidasMotocicletas = bitacoras.Count(b => b.TipoIngreso == "Salida" && b.Vehiculo.TipoVehiculo == "Motocicleta" && b.Vehiculo.UsaEspacio7600 != true);
            var espaciosDisponiblesMotos = parqueo.CapacidadMotocicletas - ingresosMotociclestas + salidasMotocicletas;

            var ingresosLey7600 = bitacoras.Count(b => b.TipoIngreso == "Entrada" && b.Vehiculo.UsaEspacio7600 == true);
            var salidasLey7600 = bitacoras.Count(b => b.TipoIngreso == "Salida" && b.Vehiculo.UsaEspacio7600 == true);
            var espaciosDisponibles7600 = parqueo.CapacidadLey7600 - ingresosLey7600 + salidasLey7600;

            ViewData["EspaciosDisponiblesAutos"] = espaciosDisponiblesAutos;
            ViewData["EspaciosDisponiblesMotos"] = espaciosDisponiblesMotos;
            ViewData["EspaciosDisponibles7600"] = espaciosDisponibles7600;
            ViewData["Vehiculos"] = vehiculos;
            ViewData["Bitacoras"] = bitacoras;
            ViewData["Parqueo"] = parqueo;
            return View();

        }
        [Authorize(Roles = "Seguridad")]
        [HttpPost]
        public async Task<IActionResult> GestionParqueo(SeguridadVM modelo, string accion)
        {
            var nombrePaqueo = User.Claims.Where(c => c.Type == ClaimTypes.StreetAddress).Select(c => c.Value).SingleOrDefault();
            var parqueo = await _appDBcontext.Parqueos.FirstOrDefaultAsync(p => p.NombreParqueo == nombrePaqueo);
            if (parqueo == null)
            {
                TempData["Error"] = "No se encontró el parqueo asignado.";
                return View();
            }
            var idParqueo = parqueo.IdParqueo;
            idParqueo = modelo.ParqueoIdParqueo;
            var vehiculo = await _appDBcontext.Vehiculos.FirstOrDefaultAsync(v => v.NumeroPlaca == modelo.NumeroPlaca);
            if (vehiculo == null && accion == "Entrada")
            {
                var intentosFallidosPrevios = await _appDBcontext.Bitacoras.CountAsync(b => b.VehiculosIdVehiculo == 0 && b.ParqueoIdParqueo == parqueo.IdParqueo && b.TipoIngreso == "Intento fallido");
                if (intentosFallidosPrevios > 0)
                {
                    await RegistrarBitacora("Intento fallido", modelo.NumeroPlaca, parqueo.IdParqueo, 0);
                    TempData["Error"] = "El vehiculo no esta registrado y no puede ingresar nuevamente.";
                    return RedirectToAction("GestionParqueo");
                }
                else
                {
                    await RegistrarBitacora("Entrada", modelo.NumeroPlaca, parqueo.IdParqueo, 0);
                    TempData["Mensaje"] = "Ingreso permitido sin registro del vehiculo. Por favor, registre el vehiculo para proximos ingresos. ";
                    return RedirectToAction("GestionParqueo");
                }
            }
            if (accion == "Entrada")
            {
                var espaciosDisponibles = CalcularEspaciosDisponibles(vehiculo.TipoVehiculo, parqueo, vehiculo.UsaEspacio7600);
                if (espaciosDisponibles <= 0)
                {
                    await RegistrarBitacora("Intento fallido", modelo.NumeroPlaca, parqueo.IdParqueo, vehiculo?.IdVehiculo ?? 0);
                    TempData["Error"] = "No hay espacios disponibles para el tipo de vehiculo.";
                    return RedirectToAction("GestionParqueo");
                }
                await RegistrarBitacora("Entrada", modelo.NumeroPlaca, parqueo.IdParqueo, vehiculo.IdVehiculo);
                TempData["Mensaje"] = "Ingreso registrado correctamente.";
            }
            else if (accion == "Salida")
            {
                await RegistrarBitacora("Salida", modelo.NumeroPlaca, parqueo.IdParqueo, vehiculo.IdVehiculo);
                TempData["Mensaje"] = "Salida registrada correctamente.";
            }
            return RedirectToAction("GestionParqueo");
        }
        private int CalcularEspaciosDisponibles(string tipoVehiculo, Parqueo parqueo, bool usaEspacio7600)
        { 
            var bitacoras = _appDBcontext.Bitacoras.Include(b =>b.Vehiculo).Where(b => b.ParqueoIdParqueo == parqueo.IdParqueo).ToList();
            int ingresos = bitacoras.Count(b => b.TipoIngreso == "Entrada" && b.Vehiculo.TipoVehiculo == tipoVehiculo && b.Vehiculo.UsaEspacio7600 == usaEspacio7600);
            int salidas = bitacoras.Count(b => b.TipoIngreso == "Salida" && b.Vehiculo.TipoVehiculo == tipoVehiculo && b.Vehiculo.UsaEspacio7600 == usaEspacio7600);
            if (tipoVehiculo == "Automovil")
                return parqueo.CapacidadAutomoviles - ingresos + salidas;
            else if (tipoVehiculo == "Motocicleta")
                return parqueo.CapacidadMotocicletas - ingresos + salidas;
            else if (usaEspacio7600)
                return parqueo.CapacidadLey7600 - ingresos + salidas;
            return 0;
        }
        private async Task RegistrarBitacora(string tipoIngreso, string numeroPlaca, int idParqueo, int idVehiculo)
        {
            if (idVehiculo == 0)
            {
                idVehiculo = -1;
            }
            var bitacora = new Bitacora
            {
                TipoIngreso = tipoIngreso,
                FechaHora = DateTime.Now,
                VehiculosIdVehiculo = idVehiculo,
                ParqueoIdParqueo = idParqueo
            };
            _appDBcontext.Bitacoras.Add(bitacora);
            await _appDBcontext.SaveChangesAsync();
        }
        //    if (vehiculo == null)
        //    {
        //        TempData["Error"] = "No se encontró el vehículo.";
        //        return View();
        //    }
        //    var idVehiculo = vehiculo.IdVehiculo;
        //    idVehiculo = modelo.VehiculosIdVehiculo;
        //    var bitacora = new Bitacora
        //    {
        //        TipoIngreso = modelo.TipoIngreso,
        //        FechaHora = DateTime.Now,
        //        VehiculosIdVehiculo = modelo.VehiculosIdVehiculo,
        //        ParqueoIdParqueo = parqueo.IdParqueo
        //    };
        //    _appDBcontext.Bitacoras.Add(bitacora);
        //    await _appDBcontext.SaveChangesAsync();
        //    return RedirectToAction("GestionParqueo");
        //}

    }
}
