using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoParqueoFinal.Data;
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

        //Controlador Get para la vista de reportes de administrador
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> ReportesAdministrador(DateTime? fechaInicio, DateTime? fechaFin) 
        {
            var parqueos = await _appDBcontext.Parqueos.ToListAsync(); // Obtiene todos los parqueos
            var vehiculos = await _appDBcontext.Vehiculos.ToListAsync(); // Obtiene todos los vehiculos

            // Validacion de fechas, si no se especifican, se asignan valores por defecto
            if (!fechaInicio.HasValue) fechaInicio = DateTime.Now.AddDays(-30); // 30 días atrás
            if (!fechaFin.HasValue) fechaFin = DateTime.Now; // Fecha actual

            // Obtener todas las bitácoras sin filtrar por fecha
            var bitacoras = await _appDBcontext.Bitacoras
                .Include(b => b.Vehiculo)
                .ToListAsync();

            // Aplicar el filtro de fechas solo a los intentos fallidos
            var intentosFallidos = bitacoras
                .Where(b => b.TipoIngreso == "Intento fallido" && b.FechaHora >= fechaInicio && b.FechaHora <= fechaFin)
                .ToList();

            Dictionary<int, dynamic> ocupacionParqueos = new Dictionary<int, dynamic>(); // Crear un diccionario para almacenar la ocupación de los parqueos

            foreach (var parqueo in parqueos) // Iterar sobre cada parqueo
            {
                var ingresosAutomoviles = bitacoras.Count(b => b.ParqueoIdParqueo == parqueo.IdParqueo && b.TipoIngreso == "Entrada" && b.Vehiculo != null && b.Vehiculo.TipoVehiculo == "Automovil" && b.Vehiculo.UsaEspacio7600 != true); //Se obtiene la cantidad de ingresos de automoviles
                var salidasAutomoviles = bitacoras.Count(b => b.ParqueoIdParqueo == parqueo.IdParqueo && b.TipoIngreso == "Salida" && b.Vehiculo != null && b.Vehiculo.TipoVehiculo == "Automovil" && b.Vehiculo.UsaEspacio7600 != true); //Se obtiene la cantidad de salidas de automoviles

                var espaciosDisponiblesAutos = parqueo.CapacidadAutomoviles - ingresosAutomoviles + salidasAutomoviles; // Se calcula la cantidad de espacios disponibles para automoviles

                var ingresosMotocicletas = bitacoras.Count(b => b.ParqueoIdParqueo == parqueo.IdParqueo && b.TipoIngreso == "Entrada" && b.Vehiculo != null && b.Vehiculo.TipoVehiculo == "Motocicleta" && b.Vehiculo.UsaEspacio7600 != true); //Se obtiene la cantidad de ingresos de motocicletas
                var salidasMotocicletas = bitacoras.Count(b => b.ParqueoIdParqueo == parqueo.IdParqueo && b.TipoIngreso == "Salida" && b.Vehiculo != null && b.Vehiculo.TipoVehiculo == "Motocicleta" && b.Vehiculo.UsaEspacio7600 != true); //Se obtiene la cantidad de salidas de motocicletas

                var espaciosDisponiblesMotos = parqueo.CapacidadMotocicletas - ingresosMotocicletas + salidasMotocicletas; // Se calcula la cantidad de espacios disponibles para motocicletas

                var ingresosLey7600 = bitacoras.Count(b => b.ParqueoIdParqueo == parqueo.IdParqueo && b.TipoIngreso == "Entrada" && b.Vehiculo != null && b.Vehiculo.UsaEspacio7600 == true); //Se obtiene la cantidad de ingresos de ley 7600
                var salidasLey7600 = bitacoras.Count(b => b.ParqueoIdParqueo == parqueo.IdParqueo && b.TipoIngreso == "Salida" && b.Vehiculo != null && b.Vehiculo.UsaEspacio7600 == true); //Se obtiene la cantidad de salidas de ley 7600

                var espaciosDisponibles7600 = parqueo.CapacidadLey7600 - ingresosLey7600 + salidasLey7600; // Se calcula la cantidad de espacios disponibles para ley 7600

                ocupacionParqueos[parqueo.IdParqueo] = new // Crear un nuevo objeto anónimo para almacenar la ocupación
                {
                    NombreParqueo = parqueo.NombreParqueo,
                    EspaciosDisponiblesAutos = espaciosDisponiblesAutos,
                    EspaciosDisponiblesMotos = espaciosDisponiblesMotos,
                    EspaciosDisponibles7600 = espaciosDisponibles7600
                };
            }


            ViewData["OcupacionParqueos"] = ocupacionParqueos; // Asignar el diccionario a ViewData
            ViewData["Vehiculos"] = vehiculos; // Asignar la lista de vehiculos a ViewData
            ViewData["Bitacoras"] = bitacoras; // Asignar la lista de bitacoras a ViewData
            ViewData["IntentosFallidos"] = intentosFallidos; // Asignar la lista de intentos fallidos a ViewData
            ViewData["FechaInicio"] = fechaInicio.Value.ToString("yyyy-MM-dd"); // Formato de fecha para la vista
            ViewData["FechaFin"] = fechaFin.Value.ToString("yyyy-MM-dd"); // Formato de fecha para la vista

            return View();
        }
        //Controlador Get para la vista de reportes de seguridad
        [Authorize(Roles = "Seguridad")]
        [HttpGet]
        public async Task<IActionResult> ReportesSeguridad()
        {
            var nombreParqueo = User.Claims.Where(c => c.Type == ClaimTypes.StreetAddress).Select(c => c.Value).SingleOrDefault(); // Obtiene el nombre del parqueo desde la cookie
            var parqueoAsignado = await _appDBcontext.Parqueos.FirstOrDefaultAsync(p => p.NombreParqueo == nombreParqueo); // Busca el parqueo por su nombre

            if (parqueoAsignado == null) // Si no se encuentra el parqueo, devuelve un error
            {
                TempData["Error"] = "No se encontró el parqueo asignado.";
                return View();
            }

            var bitacoras = await _appDBcontext.Bitacoras.Include(b => b.Vehiculo).ToListAsync(); // Obtiene todas las bitácoras

            var ocupacionActual = new Dictionary<string, object> // Crear un diccionario para almacenar la ocupación actual
            {
                { "NombreParqueo", parqueoAsignado.NombreParqueo },
                { "EspaciosDisponiblesAutos", parqueoAsignado.CapacidadAutomoviles - bitacoras.Count(b => b.ParqueoIdParqueo == parqueoAsignado.IdParqueo && b.TipoIngreso == "Entrada" && b.Vehiculo?.TipoVehiculo == "Automovil" && !b.Vehiculo.UsaEspacio7600) + 
                bitacoras.Count(b => b.ParqueoIdParqueo == parqueoAsignado.IdParqueo && b.TipoIngreso == "Salida" && b.Vehiculo != null && b.Vehiculo.TipoVehiculo == "Automovil" && b.Vehiculo.UsaEspacio7600 != true)},
                { "EspaciosDisponiblesMotos", parqueoAsignado.CapacidadMotocicletas - bitacoras.Count(b => b.ParqueoIdParqueo == parqueoAsignado.IdParqueo && b.TipoIngreso == "Entrada" && b.Vehiculo?.TipoVehiculo == "Motocicleta" && !b.Vehiculo.UsaEspacio7600) +
                bitacoras.Count(b => b.ParqueoIdParqueo == parqueoAsignado.IdParqueo && b.TipoIngreso == "Salida" && b.Vehiculo != null && b.Vehiculo.TipoVehiculo == "Motocicleta" && b.Vehiculo.UsaEspacio7600 != true)},
                { "EspaciosDisponibles7600", parqueoAsignado.CapacidadLey7600 - bitacoras.Count(b => b.ParqueoIdParqueo == parqueoAsignado.IdParqueo && b.TipoIngreso == "Entrada" && b.Vehiculo?.UsaEspacio7600 == true) +
                bitacoras.Count(b => b.ParqueoIdParqueo == parqueoAsignado.IdParqueo && b.TipoIngreso == "Salida" && b.Vehiculo != null && b.Vehiculo.UsaEspacio7600 == true)}
            };

            var otrosParqueos = await _appDBcontext.Parqueos.Where(p => p.IdParqueo != parqueoAsignado.IdParqueo).ToListAsync(); // Obtiene todos los parqueos excepto el asignado
            var ocupacionOtrosParqueos = otrosParqueos.Select(parqueo => new Dictionary<string, object> // Crear un nuevo diccionario para almacenar la ocupación de otros parqueos
            {
                { "NombreParqueo", parqueo.NombreParqueo },
                { "EspaciosDisponiblesAutos", parqueo.CapacidadAutomoviles - bitacoras.Count(b => b.ParqueoIdParqueo == parqueo.IdParqueo && b.TipoIngreso == "Entrada" && b.Vehiculo?.TipoVehiculo == "Automovil" && !b.Vehiculo.UsaEspacio7600) +
                bitacoras.Count(b => b.ParqueoIdParqueo == parqueo.IdParqueo && b.TipoIngreso == "Salida" && b.Vehiculo != null && b.Vehiculo.TipoVehiculo == "Automovil" && b.Vehiculo.UsaEspacio7600 != true)},
                { "EspaciosDisponiblesMotos", parqueo.CapacidadMotocicletas - bitacoras.Count(b => b.ParqueoIdParqueo == parqueo.IdParqueo && b.TipoIngreso == "Entrada" && b.Vehiculo?.TipoVehiculo == "Motocicleta" && !b.Vehiculo.UsaEspacio7600) +
                bitacoras.Count(b => b.ParqueoIdParqueo == parqueo.IdParqueo && b.TipoIngreso == "Salida" && b.Vehiculo != null && b.Vehiculo.TipoVehiculo == "Motocicleta" && b.Vehiculo.UsaEspacio7600 != true)},
                { "EspaciosDisponibles7600", parqueo.CapacidadLey7600 - bitacoras.Count(b => b.ParqueoIdParqueo == parqueo.IdParqueo && b.TipoIngreso == "Entrada" && b.Vehiculo?.UsaEspacio7600 == true) +
                bitacoras.Count(b => b.ParqueoIdParqueo == parqueo.IdParqueo && b.TipoIngreso == "Salida" && b.Vehiculo != null && b.Vehiculo.UsaEspacio7600 == true)}
            }).ToList();

            ViewData["OcupacionActual"] = ocupacionActual; // Asignar la ocupación actual a ViewData
            ViewData["OcupacionOtrosParqueos"] = ocupacionOtrosParqueos; // Asignar la ocupación de otros parqueos a ViewData

            return View();
        }
        //Controlador Get para la vista de reportes de Estudiante y Administrativo
        [Authorize(Roles = "Estudiante,Administrativo")]
        [HttpGet]
        public async Task<IActionResult> ReportesUsuario(int? mes) // Obtiene el historial de bitácoras del usuario
        {
            var correoUsuario = User.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault(); // Obtiene el correo del usuario autenticado
            var usuario = await _appDBcontext.Usuarios.Include(u => u.Vehiculos).FirstOrDefaultAsync(u => u.CorreoElectronico == correoUsuario); // Busca el usuario en la base de datos
            var IdUsuario = usuario.IdUsuario; // Obtiene el ID del usuario

            if (usuario == null) // Si no se encuentra el usuario, devuelve un error
            {
                TempData["Error"] = "No se encontró el usuario.";
                return View();
            }

            var bitacorasQuery = _appDBcontext.Bitacoras.Include(b => b.Vehiculo).Where(b => b.Vehiculo.UsuariosIdUsuario == usuario.IdUsuario); // Obtiene todas las bitácoras del usuario

            // Filtrar por mes si se especifica
            if (mes.HasValue)
            {
                bitacorasQuery = bitacorasQuery.Where(b => b.FechaHora.Month == mes);   // Filtra las bitácoras por el mes seleccionado
            }

            var historial = await bitacorasQuery.OrderByDescending(b => b.FechaHora).ToListAsync(); // Ordena las bitácoras por fecha de forma descendente

            ViewData["Historial"] = historial; // Asignar el historial a ViewData
            ViewData["MesSeleccionado"] = mes; // Asignar el mes seleccionado a ViewData

            return View();
        }



    }
}
