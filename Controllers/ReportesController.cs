using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ProyectoParqueoFinal.Data;
using ProyectoParqueoFinal.Models;
using ProyectoParqueoFinal.ViewModels;
using System.Security.Claims;

namespace ProyectoParqueoFinal.Controllers
{
    public class ReportesController : Controller
    {
        private readonly AppDBContext _appDBcontext;
        public ReportesController(AppDBContext appDBcontext)
        {
            _appDBcontext = appDBcontext;
        }
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> ReportesAdministrador(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var parqueos = await _appDBcontext.Parqueos.ToListAsync();
            var vehiculos = await _appDBcontext.Vehiculos.ToListAsync();

            // Si no hay fechas seleccionadas, usar un rango predeterminado
            if (!fechaInicio.HasValue) fechaInicio = DateTime.Now.AddDays(-30);
            if (!fechaFin.HasValue) fechaFin = DateTime.Now;

            // Obtener todas las bitácoras sin filtrar por fecha
            var bitacoras = await _appDBcontext.Bitacoras
                .Include(b => b.Vehiculo)
                .ToListAsync();

            // Aplicar el filtro de fechas solo a los intentos fallidos
            var intentosFallidos = bitacoras
                .Where(b => b.TipoIngreso == "Intento fallido" && b.FechaHora >= fechaInicio && b.FechaHora <= fechaFin)
                .ToList();

            Dictionary<int, dynamic> ocupacionParqueos = new Dictionary<int, dynamic>();

            foreach (var parqueo in parqueos)
            {
                var ingresosAutomoviles = bitacoras.Count(b => b.ParqueoIdParqueo == parqueo.IdParqueo && b.TipoIngreso == "Entrada" && b.Vehiculo != null && b.Vehiculo.TipoVehiculo == "Automovil" && b.Vehiculo.UsaEspacio7600 != true);
                var salidasAutomoviles = bitacoras.Count(b => b.ParqueoIdParqueo == parqueo.IdParqueo && b.TipoIngreso == "Salida" && b.Vehiculo != null && b.Vehiculo.TipoVehiculo == "Automovil" && b.Vehiculo.UsaEspacio7600 != true);

                var espaciosDisponiblesAutos = parqueo.CapacidadAutomoviles - ingresosAutomoviles + salidasAutomoviles;

                var ingresosMotocicletas = bitacoras.Count(b => b.ParqueoIdParqueo == parqueo.IdParqueo && b.TipoIngreso == "Entrada" && b.Vehiculo != null && b.Vehiculo.TipoVehiculo == "Motocicleta" && b.Vehiculo.UsaEspacio7600 != true);
                var salidasMotocicletas = bitacoras.Count(b => b.ParqueoIdParqueo == parqueo.IdParqueo && b.TipoIngreso == "Salida" && b.Vehiculo != null && b.Vehiculo.TipoVehiculo == "Motocicleta" && b.Vehiculo.UsaEspacio7600 != true);

                var espaciosDisponiblesMotos = parqueo.CapacidadMotocicletas - ingresosMotocicletas + salidasMotocicletas;

                var ingresosLey7600 = bitacoras.Count(b => b.ParqueoIdParqueo == parqueo.IdParqueo && b.TipoIngreso == "Entrada" && b.Vehiculo != null && b.Vehiculo.UsaEspacio7600 == true);
                var salidasLey7600 = bitacoras.Count(b => b.ParqueoIdParqueo == parqueo.IdParqueo && b.TipoIngreso == "Salida" && b.Vehiculo != null && b.Vehiculo.UsaEspacio7600 == true);

                var espaciosDisponibles7600 = parqueo.CapacidadLey7600 - ingresosLey7600 + salidasLey7600;

                ocupacionParqueos[parqueo.IdParqueo] = new
                {
                    NombreParqueo = parqueo.NombreParqueo,
                    EspaciosDisponiblesAutos = espaciosDisponiblesAutos,
                    EspaciosDisponiblesMotos = espaciosDisponiblesMotos,
                    EspaciosDisponibles7600 = espaciosDisponibles7600
                };
            }


            ViewData["OcupacionParqueos"] = ocupacionParqueos;
            ViewData["Vehiculos"] = vehiculos;
            ViewData["Bitacoras"] = bitacoras;
            ViewData["IntentosFallidos"] = intentosFallidos;
            ViewData["FechaInicio"] = fechaInicio.Value.ToString("yyyy-MM-dd");
            ViewData["FechaFin"] = fechaFin.Value.ToString("yyyy-MM-dd");

            return View();
        }

        [Authorize(Roles = "Seguridad")]
        [HttpGet]
        public async Task<IActionResult> ReportesSeguridad()
        {
            var nombreParqueo = User.Claims.Where(c => c.Type == ClaimTypes.StreetAddress).Select(c => c.Value).SingleOrDefault();
            var parqueoAsignado = await _appDBcontext.Parqueos.FirstOrDefaultAsync(p => p.NombreParqueo == nombreParqueo);

            if (parqueoAsignado == null)
            {
                TempData["Error"] = "No se encontró el parqueo asignado.";
                return View();
            }

            var bitacoras = await _appDBcontext.Bitacoras.Include(b => b.Vehiculo).ToListAsync();

            var ocupacionActual = new Dictionary<string, object>
            {
                { "NombreParqueo", parqueoAsignado.NombreParqueo },
                { "EspaciosDisponiblesAutos", parqueoAsignado.CapacidadAutomoviles - bitacoras.Count(b => b.ParqueoIdParqueo == parqueoAsignado.IdParqueo && b.TipoIngreso == "Entrada" && b.Vehiculo?.TipoVehiculo == "Automovil" && !b.Vehiculo.UsaEspacio7600) + 
                bitacoras.Count(b => b.ParqueoIdParqueo == parqueoAsignado.IdParqueo && b.TipoIngreso == "Salida" && b.Vehiculo != null && b.Vehiculo.TipoVehiculo == "Automovil" && b.Vehiculo.UsaEspacio7600 != true)},
                { "EspaciosDisponiblesMotos", parqueoAsignado.CapacidadMotocicletas - bitacoras.Count(b => b.ParqueoIdParqueo == parqueoAsignado.IdParqueo && b.TipoIngreso == "Entrada" && b.Vehiculo?.TipoVehiculo == "Motocicleta" && !b.Vehiculo.UsaEspacio7600) +
                bitacoras.Count(b => b.ParqueoIdParqueo == parqueoAsignado.IdParqueo && b.TipoIngreso == "Salida" && b.Vehiculo != null && b.Vehiculo.TipoVehiculo == "Motocicleta" && b.Vehiculo.UsaEspacio7600 != true)},
                { "EspaciosDisponibles7600", parqueoAsignado.CapacidadLey7600 - bitacoras.Count(b => b.ParqueoIdParqueo == parqueoAsignado.IdParqueo && b.TipoIngreso == "Entrada" && b.Vehiculo?.UsaEspacio7600 == true) +
                bitacoras.Count(b => b.ParqueoIdParqueo == parqueoAsignado.IdParqueo && b.TipoIngreso == "Salida" && b.Vehiculo != null && b.Vehiculo.UsaEspacio7600 == true)}
            };

            var otrosParqueos = await _appDBcontext.Parqueos.Where(p => p.IdParqueo != parqueoAsignado.IdParqueo).ToListAsync();
            var ocupacionOtrosParqueos = otrosParqueos.Select(parqueo => new Dictionary<string, object>
            {
                { "NombreParqueo", parqueo.NombreParqueo },
                { "EspaciosDisponiblesAutos", parqueo.CapacidadAutomoviles - bitacoras.Count(b => b.ParqueoIdParqueo == parqueo.IdParqueo && b.TipoIngreso == "Entrada" && b.Vehiculo?.TipoVehiculo == "Automovil" && !b.Vehiculo.UsaEspacio7600) +
                bitacoras.Count(b => b.ParqueoIdParqueo == parqueo.IdParqueo && b.TipoIngreso == "Salida" && b.Vehiculo != null && b.Vehiculo.TipoVehiculo == "Automovil" && b.Vehiculo.UsaEspacio7600 != true)},
                { "EspaciosDisponiblesMotos", parqueo.CapacidadMotocicletas - bitacoras.Count(b => b.ParqueoIdParqueo == parqueo.IdParqueo && b.TipoIngreso == "Entrada" && b.Vehiculo?.TipoVehiculo == "Motocicleta" && !b.Vehiculo.UsaEspacio7600) +
                bitacoras.Count(b => b.ParqueoIdParqueo == parqueo.IdParqueo && b.TipoIngreso == "Salida" && b.Vehiculo != null && b.Vehiculo.TipoVehiculo == "Motocicleta" && b.Vehiculo.UsaEspacio7600 != true)},
                { "EspaciosDisponibles7600", parqueo.CapacidadLey7600 - bitacoras.Count(b => b.ParqueoIdParqueo == parqueo.IdParqueo && b.TipoIngreso == "Entrada" && b.Vehiculo?.UsaEspacio7600 == true) +
                bitacoras.Count(b => b.ParqueoIdParqueo == parqueo.IdParqueo && b.TipoIngreso == "Salida" && b.Vehiculo != null && b.Vehiculo.UsaEspacio7600 == true)}
            }).ToList();

            ViewData["OcupacionActual"] = ocupacionActual;
            ViewData["OcupacionOtrosParqueos"] = ocupacionOtrosParqueos;

            return View();
        }

        // GET: Ahora se quiere crear un reporte para los usuarios y los administrativos en el cual se muestre el historial de uso de los parqueos filtrados por mes
        [Authorize(Roles = "Usuario, Administrador")]
        [HttpGet]
        public async Task<IActionResult> ReporteHistorialUso(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var vehiculos = await _appDBcontext.Vehiculos.ToListAsync();
            var bitacoras = await _appDBcontext.Bitacoras.Include(b => b.Vehiculo).ToListAsync();
            // Si no hay fechas seleccionadas, usar un rango predeterminado
            if (!fechaInicio.HasValue) fechaInicio = DateTime.Now.AddDays(-30);
            if (!fechaFin.HasValue) fechaFin = DateTime.Now;
            var historialUso = bitacoras
                .Where(b => b.FechaHora >= fechaInicio && b.FechaHora <= fechaFin)
                .ToList();
            ViewData["HistorialUso"] = historialUso;
            ViewData["Vehiculos"] = vehiculos;
            ViewData["FechaInicio"] = fechaInicio.Value.ToString("yyyy-MM-dd");
            ViewData["FechaFin"] = fechaFin.Value.ToString("yyyy-MM-dd");
            return View();
        }

        [Authorize(Roles = "Estudiante,Administrativo")]
        [HttpGet]
        public async Task<IActionResult> ReportesUsuario(int? mes)
        {
            var correoUsuario = User.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();
            var usuario = await _appDBcontext.Usuarios.Include(u => u.Vehiculos).FirstOrDefaultAsync(u => u.CorreoElectronico == correoUsuario);
            var IdUsuario = usuario.IdUsuario;

            if (usuario == null)
            {
                TempData["Error"] = "No se encontró el usuario.";
                return View();
            }

            var bitacorasQuery = _appDBcontext.Bitacoras.Include(b => b.Vehiculo).Where(b => b.Vehiculo.UsuariosIdUsuario == usuario.IdUsuario);

            // Filtrar por mes si se especifica
            if (mes.HasValue)
            {
                bitacorasQuery = bitacorasQuery.Where(b => b.FechaHora.Month == mes);
            }

            var historial = await bitacorasQuery.OrderByDescending(b => b.FechaHora).ToListAsync();

            ViewData["Historial"] = historial;
            ViewData["MesSeleccionado"] = mes;

            return View();
        }



    }
}
