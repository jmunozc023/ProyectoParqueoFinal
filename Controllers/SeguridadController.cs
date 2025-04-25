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
        //Controlador Get para la vista de gestion de parqueo
        [Authorize(Roles = "Seguridad")]
        [HttpGet]
        public async Task<IActionResult> GestionParqueo()
        { 
            var nombrePaqueo = User.Claims.Where(c => c.Type == ClaimTypes.StreetAddress).Select(c => c.Value).SingleOrDefault(); // Obtiene el nombre del parqueo desde la cookie
            var parqueo = await _appDBcontext.Parqueos.FirstOrDefaultAsync(p => p.NombreParqueo == nombrePaqueo); // Busca el parqueo por su nombre

            if (parqueo == null) // Verifica si el parqueo existe
            {
                TempData["Error"] = "No se encontró el parqueo asignado.";
                return View();
            }
            var vehiculos = await _appDBcontext.Vehiculos.ToListAsync(); // Obtiene todos los vehículos registrados
            var bitacoras = await _appDBcontext.Bitacoras // Obtiene todas las bitácoras de ingreso y salida
                  .Include(b => b.Vehiculo)
                  .Where(b => b.ParqueoIdParqueo == parqueo.IdParqueo && b.Vehiculo != null)
                  .ToListAsync();            
            var ingresosAutomoviles = bitacoras.Count(b => b.TipoIngreso == "Entrada" && b.Vehiculo.TipoVehiculo == "Automovil" && b.Vehiculo.UsaEspacio7600 != true); //Obtiene los ingresos de automoviles
            var salidasAutomoviles = bitacoras.Count(b => b.TipoIngreso == "Salida" && b.Vehiculo.TipoVehiculo == "Automovil" && b.Vehiculo.UsaEspacio7600 != true); //Obtiene las salidas de automoviles
            var espaciosDisponiblesAutos = parqueo.CapacidadAutomoviles - ingresosAutomoviles + salidasAutomoviles; //Calcula los espacios disponibles para automoviles

            var ingresosMotociclestas = bitacoras.Count(b => b.TipoIngreso == "Entrada" && b.Vehiculo.TipoVehiculo == "Motocicleta" && b.Vehiculo.UsaEspacio7600 != true); //Obtiene los ingresos de motocicletas
            var salidasMotocicletas = bitacoras.Count(b => b.TipoIngreso == "Salida" && b.Vehiculo.TipoVehiculo == "Motocicleta" && b.Vehiculo.UsaEspacio7600 != true); //Obtiene las salidas de motocicletas
            var espaciosDisponiblesMotos = parqueo.CapacidadMotocicletas - ingresosMotociclestas + salidasMotocicletas; //Calcula los espacios disponibles para motocicletas

            var ingresosLey7600 = bitacoras.Count(b => b.TipoIngreso == "Entrada" && b.Vehiculo.UsaEspacio7600 == true); //Obtiene los ingresos de ley 7600
            var salidasLey7600 = bitacoras.Count(b => b.TipoIngreso == "Salida" && b.Vehiculo.UsaEspacio7600 == true); //Obtiene las salidas de ley 7600
            var espaciosDisponibles7600 = parqueo.CapacidadLey7600 - ingresosLey7600 + salidasLey7600; //Calcula los espacios disponibles para ley 7600

            ViewData["EspaciosDisponiblesAutos"] = espaciosDisponiblesAutos; // Calcula los espacios disponibles para automoviles
            ViewData["EspaciosDisponiblesMotos"] = espaciosDisponiblesMotos; // Calcula los espacios disponibles para motocicletas
            ViewData["EspaciosDisponibles7600"] = espaciosDisponibles7600; // Calcula los espacios disponibles para ley 7600
            ViewData["Vehiculos"] = vehiculos; // Asigna la lista de vehículos a la vista
            ViewData["Bitacoras"] = bitacoras; // Asigna la lista de bitácoras a la vista
            ViewData["Parqueo"] = parqueo; // Asigna el parqueo a la vista
            return View();

        }
        //Controlador Post para la vista de gestion de parqueo
        [Authorize(Roles = "Seguridad")]
        [HttpPost]
        public async Task<IActionResult> GestionParqueo(SeguridadVM modelo, string accion)
        {
            var nombrePaqueo = User.Claims.Where(c => c.Type == ClaimTypes.StreetAddress).Select(c => c.Value).SingleOrDefault(); // Obtiene el nombre del parqueo desde la cookie
            var parqueo = await _appDBcontext.Parqueos.FirstOrDefaultAsync(p => p.NombreParqueo == nombrePaqueo); // Busca el parqueo por su nombre
            if (parqueo == null) // Verifica si el parqueo existe
            {
                TempData["Error"] = "No se encontró el parqueo asignado.";
                return View();
            }
            var idParqueo = parqueo.IdParqueo; // Asigna el ID del parqueo a la variable
            idParqueo = modelo.ParqueoIdParqueo; 
            var vehiculo = await _appDBcontext.Vehiculos.FirstOrDefaultAsync(v => v.NumeroPlaca == modelo.NumeroPlaca); // Busca el vehículo por su número de placa
            if (vehiculo == null && accion == "Entrada") // Verifica si el vehículo no está registrado y la acción es "Entrada"
            {
                var intentosFallidosPrevios = await _appDBcontext.Bitacoras.CountAsync(b => b.VehiculosIdVehiculo == null && b.ParqueoIdParqueo == parqueo.IdParqueo && b.NumeroPlaca == modelo.NumeroPlaca && b.TipoIngreso == "Intento fallido"); // Cuenta los intentos fallidos previos
                if (intentosFallidosPrevios > 0) // Si hay intentos fallidos previos verifica que no excedan el valor de 1 intento
                {
                    await RegistrarBitacora("Intento fallido", modelo.NumeroPlaca, parqueo.IdParqueo, null); // Registra el intento fallido
                    TempData["Semaforo"] = "red"; // Cambia el semáforo a rojo
                    TempData["Error"] = "El vehiculo no esta registrado y no puede ingresar nuevamente."; // Mensaje de error
                    return RedirectToAction("GestionParqueo"); // Redirige a la vista de gestión de parqueo
                }
                else
                {
                    var ultimoRegistro = await _appDBcontext.Bitacoras // Busca el último registro de bitácora
                    .Where(b => b.NumeroPlaca == modelo.NumeroPlaca && b.ParqueoIdParqueo == parqueo.IdParqueo)
                    .OrderByDescending(b => b.FechaHora)
                    .FirstOrDefaultAsync();

                    if (ultimoRegistro != null && ultimoRegistro.TipoIngreso == "Entrada" && accion == "Entrada") // Verifica si el ultimo registro es de entrada
                    {
                        TempData["Error"] = "El vehículo ya ingresó y aún no ha registrado una salida.";
                        return RedirectToAction("GestionParqueo");
                    }
                    await RegistrarBitacora("Intento fallido", modelo.NumeroPlaca, parqueo.IdParqueo, null); // Registra el intento fallido
                    TempData["Semaforo"] = "red"; // Cambia el semáforo a rojo
                    TempData["Mensaje"] = "Ingreso permitido sin registro del vehiculo. Por favor, registre el vehiculo para proximos ingresos.";
                    return RedirectToAction("GestionParqueo"); // Redirige a la vista de gestión de parqueo
                }
            }
            if (accion == "Entrada") // Verifica si la acción es "Entrada"
            {
                var espaciosDisponibles = CalcularEspaciosDisponibles(vehiculo.TipoVehiculo, parqueo, vehiculo.UsaEspacio7600); // Calcula los espacios disponibles
                if (espaciosDisponibles <= 0) // Verifica si no hay espacios disponibles 
                {
                    await RegistrarBitacora("Intento fallido", modelo.NumeroPlaca, parqueo.IdParqueo, vehiculo?.IdVehiculo); // Registra el intento fallido
                    TempData["Semaforo"] = "red"; // Cambia el semáforo a rojo
                    TempData["Error"] = "No hay espacios disponibles para el tipo de vehiculo."; // Mensaje de error
                    return RedirectToAction("GestionParqueo"); // Redirige a la vista de gestión de parqueo
                }
                var ultimoRegistro = await _appDBcontext.Bitacoras // Busca el último registro de bitácora
                .Where(b => b.NumeroPlaca == modelo.NumeroPlaca && b.ParqueoIdParqueo == parqueo.IdParqueo)
                .OrderByDescending(b => b.FechaHora)
                .FirstOrDefaultAsync();

                if (ultimoRegistro != null && ultimoRegistro.TipoIngreso == "Entrada" && accion == "Entrada") // Verifica si el ultimo registro es de entrada
                {
                    TempData["Error"] = "El vehículo ya ingresó y aún no ha registrado una salida.";
                    return RedirectToAction("GestionParqueo");
                }
                await RegistrarBitacora("Entrada", modelo.NumeroPlaca, parqueo.IdParqueo, vehiculo.IdVehiculo); // Registra la entrada
                TempData["Semaforo"] = "green"; // Cambia el semáforo a verde
                TempData["Mensaje"] = "Ingreso registrado correctamente.";
            }
            else if (accion == "Salida") // Verifica si la acción es "Salida"
            {
                if (vehiculo == null) // Verifica si el vehículo no está registrado
                {
                    var bitacoraSinRegistro = await _appDBcontext.Bitacoras // Busca la bitácora sin registro
                        .FirstOrDefaultAsync(b => b.NumeroPlaca == modelo.NumeroPlaca && b.VehiculosIdVehiculo == null && (b.TipoIngreso == "Entrada" || b.TipoIngreso == "Intento fallido") && b.ParqueoIdParqueo == parqueo.IdParqueo); 

                    if (bitacoraSinRegistro == null) // Verifica si no hay bitacora de entrada
                    {
                        TempData["Error"] = "El vehículo no está registrado y no tiene un ingreso previo permitido.";
                        return RedirectToAction("GestionParqueo"); // Redirige a la vista de gestión de parqueo
                    }

                    await RegistrarBitacora("Salida", modelo.NumeroPlaca, parqueo.IdParqueo, null); // Registra la salida
                    TempData["Mensaje"] = "Salida registrada correctamente para un vehículo sin registro previo.";
                }
                else
                {
                    await RegistrarBitacora("Salida", modelo.NumeroPlaca, parqueo.IdParqueo, vehiculo?.IdVehiculo); // Registra la salida
                    TempData["Mensaje"] = "Salida registrada correctamente.";
                }
            }
            return RedirectToAction("GestionParqueo"); // Redirige a la vista de gestión de parqueo
        }
        private int CalcularEspaciosDisponibles(string tipoVehiculo, Parqueo parqueo, bool usaEspacio7600) //Metodo auxiliar para calcular los espacios disponibles
        {
            var bitacoras = _appDBcontext.Bitacoras.Include(b => b.Vehiculo).Where(b => b.ParqueoIdParqueo == parqueo.IdParqueo).ToList(); // Obtiene todas las bitácoras del parqueo

            int ingresos = bitacoras.Count(b => b.TipoIngreso == "Entrada" && b.Vehiculo != null && // Obtiene los ingresos
                                                b.Vehiculo.TipoVehiculo == tipoVehiculo &&
                                                b.Vehiculo.UsaEspacio7600 == usaEspacio7600);

            int salidas = bitacoras.Count(b => b.TipoIngreso == "Salida" && b.Vehiculo != null && // Obtiene las salidas
                                               b.Vehiculo.TipoVehiculo == tipoVehiculo &&
                                               b.Vehiculo.UsaEspacio7600 == usaEspacio7600);

            if (tipoVehiculo == "Automovil" && !usaEspacio7600) //Filtro aplicado para identificar los vehiculos que estan registrador con uso de espacio 7600
                return parqueo.CapacidadAutomoviles - ingresos + salidas; //Retorna la capacidad de automoviles
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

        private async Task RegistrarBitacora(string tipoIngreso, string numeroPlaca, int idParqueo, int? idVehiculo) //Metodo auxiliar para registrar la bitacora
        {
            try
            {
                var vehiculo = idVehiculo.HasValue ? await _appDBcontext.Vehiculos.FindAsync(idVehiculo) : null; // Busca el vehiculo por su ID
                var usaEspacio7600 = vehiculo?.UsaEspacio7600 ?? false; // Verifica si el vehiculo usa espacio 7600
                var bitacora = new Bitacora // Crea una nueva bitácora
                {
                    TipoIngreso = tipoIngreso,
                    FechaHora = DateTime.Now,
                    VehiculosIdVehiculo = idVehiculo,
                    NumeroPlaca = numeroPlaca,
                    ParqueoIdParqueo = idParqueo,
                    TipoVehiculo = vehiculo?.TipoVehiculo,
                    UsaEspacio7600 = usaEspacio7600 
                };

                _appDBcontext.Bitacoras.Add(bitacora); // Agrega la bitácora a la base de datos
                await _appDBcontext.SaveChangesAsync(); // Guarda los cambios
            }
            catch (DbUpdateException ex)
            {
                TempData["Error"] = "An error occurred while saving the changes. Please try again.";
                throw;
            }
        }

    }
}
