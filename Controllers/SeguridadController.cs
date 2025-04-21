using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoParqueoFinal.Data;
using ProyectoParqueoFinal.Models;
using ProyectoParqueoFinal.ViewModels;
using System.Security.Claims;

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
            var bitacoras = await _appDBcontext.Bitacoras
                  .Include(b => b.Vehiculo)
                  .Where(b => b.ParqueoIdParqueo == parqueo.IdParqueo && b.Vehiculo != null)
                  .ToListAsync();            
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
                var intentosFallidosPrevios = await _appDBcontext.Bitacoras.CountAsync(b => b.VehiculosIdVehiculo == null && b.ParqueoIdParqueo == parqueo.IdParqueo && b.NumeroPlaca == modelo.NumeroPlaca && b.TipoIngreso == "Intento fallido");
                if (intentosFallidosPrevios > 0)
                {
                    await RegistrarBitacora("Intento fallido", modelo.NumeroPlaca, parqueo.IdParqueo, null);
                    TempData["Semaforo"] = "red";
                    TempData["Error"] = "El vehiculo no esta registrado y no puede ingresar nuevamente.";
                    return RedirectToAction("GestionParqueo");
                }
                else
                {
                    var ultimoRegistro = await _appDBcontext.Bitacoras
                    .Where(b => b.NumeroPlaca == modelo.NumeroPlaca && b.ParqueoIdParqueo == parqueo.IdParqueo)
                    .OrderByDescending(b => b.FechaHora)
                    .FirstOrDefaultAsync();

                    if (ultimoRegistro != null && ultimoRegistro.TipoIngreso == "Entrada" && accion == "Entrada")
                    {
                        TempData["Error"] = "El vehículo ya ingresó y aún no ha registrado una salida.";
                        return RedirectToAction("GestionParqueo");
                    }
                    await RegistrarBitacora("Intento fallido", modelo.NumeroPlaca, parqueo.IdParqueo, null);
                    TempData["Semaforo"] = "red";
                    TempData["Mensaje"] = "Ingreso permitido sin registro del vehiculo. Por favor, registre el vehiculo para proximos ingresos.";
                    return RedirectToAction("GestionParqueo");
                }
            }
            if (accion == "Entrada")
            {
                var espaciosDisponibles = CalcularEspaciosDisponibles(vehiculo.TipoVehiculo, parqueo, vehiculo.UsaEspacio7600);
                if (espaciosDisponibles == 0)
                {
                    await RegistrarBitacora("Intento fallido", modelo.NumeroPlaca, parqueo.IdParqueo, vehiculo?.IdVehiculo);
                    TempData["Semaforo"] = "red";
                    TempData["Error"] = "No hay espacios disponibles para el tipo de vehiculo.";
                    return RedirectToAction("GestionParqueo");
                }
                var ultimoRegistro = await _appDBcontext.Bitacoras
                .Where(b => b.NumeroPlaca == modelo.NumeroPlaca && b.ParqueoIdParqueo == parqueo.IdParqueo)
                .OrderByDescending(b => b.FechaHora)
                .FirstOrDefaultAsync();

                if (ultimoRegistro != null && ultimoRegistro.TipoIngreso == "Entrada" && accion == "Entrada")
                {
                    TempData["Error"] = "El vehículo ya ingresó y aún no ha registrado una salida.";
                    return RedirectToAction("GestionParqueo");
                }
                await RegistrarBitacora("Entrada", modelo.NumeroPlaca, parqueo.IdParqueo, vehiculo.IdVehiculo);
                TempData["Semaforo"] = "green";
                TempData["Mensaje"] = "Ingreso registrado correctamente.";
            }
            else if (accion == "Salida")
            {
                if (vehiculo == null)
                {
                    var bitacoraSinRegistro = await _appDBcontext.Bitacoras
                        .FirstOrDefaultAsync(b => b.NumeroPlaca == modelo.NumeroPlaca && b.VehiculosIdVehiculo == null && (b.TipoIngreso == "Entrada" || b.TipoIngreso == "Intento fallido") && b.ParqueoIdParqueo == parqueo.IdParqueo);

                    if (bitacoraSinRegistro == null)
                    {
                        TempData["Error"] = "El vehículo no está registrado y no tiene un ingreso previo permitido.";
                        return RedirectToAction("GestionParqueo");
                    }

                    await RegistrarBitacora("Salida", modelo.NumeroPlaca, parqueo.IdParqueo, null);
                    TempData["Mensaje"] = "Salida registrada correctamente para un vehículo sin registro previo.";
                }
                else
                {
                    await RegistrarBitacora("Salida", modelo.NumeroPlaca, parqueo.IdParqueo, vehiculo?.IdVehiculo);
                    TempData["Mensaje"] = "Salida registrada correctamente.";
                }
            }
            return RedirectToAction("GestionParqueo");
        }
        private int CalcularEspaciosDisponibles(string tipoVehiculo, Parqueo parqueo, bool usaEspacio7600)
        {
            var bitacoras = _appDBcontext.Bitacoras.Include(b => b.Vehiculo).Where(b => b.ParqueoIdParqueo == parqueo.IdParqueo).ToList();

            int ingresos = bitacoras.Count(b => b.TipoIngreso == "Entrada" && b.Vehiculo != null &&
                                                b.Vehiculo.TipoVehiculo == tipoVehiculo &&
                                                b.Vehiculo.UsaEspacio7600 == usaEspacio7600);

            int salidas = bitacoras.Count(b => b.TipoIngreso == "Salida" && b.Vehiculo != null &&
                                               b.Vehiculo.TipoVehiculo == tipoVehiculo &&
                                               b.Vehiculo.UsaEspacio7600 == usaEspacio7600);

            if (tipoVehiculo == "Automovil" && !usaEspacio7600)
                return parqueo.CapacidadAutomoviles - ingresos + salidas;
            else if (tipoVehiculo == "Motocicleta" && !usaEspacio7600)
                return parqueo.CapacidadMotocicletas - ingresos + salidas;
            else if (usaEspacio7600)  // Filtrar SOLO los vehículos que usan espacio 7600
                return parqueo.CapacidadLey7600 - bitacoras.Count(b => b.TipoIngreso == "Entrada" &&
                                                                       b.Vehiculo != null &&
                                                                       b.Vehiculo.UsaEspacio7600) +
                                                 bitacoras.Count(b => b.TipoIngreso == "Salida" &&
                                                                      b.Vehiculo != null &&
                                                                      b.Vehiculo.UsaEspacio7600);

            return 0;
        }

        private async Task RegistrarBitacora(string tipoIngreso, string numeroPlaca, int idParqueo, int? idVehiculo)
        {
            try
            {
                var vehiculo = idVehiculo.HasValue ? await _appDBcontext.Vehiculos.FindAsync(idVehiculo) : null;
                var usaEspacio7600 = vehiculo?.UsaEspacio7600 ?? false; 
                var bitacora = new Bitacora
                {
                    TipoIngreso = tipoIngreso,
                    FechaHora = DateTime.Now,
                    VehiculosIdVehiculo = idVehiculo,
                    NumeroPlaca = numeroPlaca,
                    ParqueoIdParqueo = idParqueo,
                    TipoVehiculo = vehiculo?.TipoVehiculo,
                    UsaEspacio7600 = usaEspacio7600 
                };

                _appDBcontext.Bitacoras.Add(bitacora);
                await _appDBcontext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                TempData["Error"] = "An error occurred while saving the changes. Please try again.";
                throw;
            }
        }

    }
}
