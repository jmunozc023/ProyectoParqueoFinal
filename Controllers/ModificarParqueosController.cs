using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoParqueoFinal.Data;
using ProyectoParqueoFinal.ViewModels;

namespace ProyectoParqueoFinal.Controllers
{
    public class ModificarParqueosController : Controller
    {
        private readonly AppDBContext _appDBcontext;
        public ModificarParqueosController(AppDBContext appDBcontext)
        {
            _appDBcontext = appDBcontext;
        }

        //Controlador Get para la vista de modificacion de parqueos
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> ModificarParqueos(int IdParqueo)
        {
            var parqueo = await _appDBcontext.Parqueos.Where(p => p.IdParqueo == IdParqueo).FirstOrDefaultAsync(); //Busca el parqueo por su ID
            if (parqueo == null)
            {
                return NotFound();
            }
            var modelo = new ModificarParqueosVM //Crea un nuevo modelo de parqueo
            {
                NombreParqueo = parqueo.NombreParqueo,
                Ubicacion = parqueo.Ubicacion,
                CapacidadAutomoviles = parqueo.CapacidadAutomoviles,
                CapacidadMotocicletas = parqueo.CapacidadMotocicletas,
                CapacidadLey7600 = parqueo.CapacidadLey7600
            };
            return View("ModificarParqueos", modelo); //Devuelve la vista de modificacion de parqueos
        }

        //Controlador Post para la vista de modificacion de parqueos
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public async Task<IActionResult> ModificarParqueos(ModificarParqueosVM modelo)
        {
            var parqueo = await _appDBcontext.Parqueos.Where(p => p.NombreParqueo == modelo.NombreParqueo).FirstOrDefaultAsync(); //Busca el parqueo por su nombre
            if (parqueo == null) return NotFound(); //Si no se encuentra el parqueo, devuelve un error 404
            parqueo.NombreParqueo = modelo.NombreParqueo; //Actualiza el nombre del parqueo
            parqueo.Ubicacion = modelo.Ubicacion; //Actualiza la ubicacion del parqueo
            parqueo.CapacidadAutomoviles = modelo.CapacidadAutomoviles; //Actualiza la capacidad de automoviles del parqueo
            parqueo.CapacidadMotocicletas = modelo.CapacidadMotocicletas;   //Actualiza la capacidad de motocicletas del parqueo
            parqueo.CapacidadLey7600 = modelo.CapacidadLey7600; //Actualiza la capacidad de ley 7600 del parqueo
            _appDBcontext.Update(parqueo);  //Actualiza el parqueo en la base de datos
            await _appDBcontext.SaveChangesAsync(); //Guarda los cambios en la base de datos
            ViewData["Mensaje"] = "Parqueo modificado exitosamente";
            return RedirectToAction("AdministracionParqueo", "AdministracionParqueo"); //Redirige a la vista de administración de parqueos
        }
    }
}
