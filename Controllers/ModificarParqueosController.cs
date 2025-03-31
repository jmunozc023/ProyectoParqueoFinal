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
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> ModificarParqueos(int IdParqueo)
        {
            var parqueo = await _appDBcontext.Parqueos.Where(p => p.IdParqueo == IdParqueo).FirstOrDefaultAsync();
            if (parqueo == null)
            {
                return NotFound();
            }
            var modelo = new ModificarParqueosVM
            {
                NombreParqueo = parqueo.NombreParqueo,
                Ubicacion = parqueo.Ubicacion,
                CapacidadAutomoviles = parqueo.CapacidadAutomoviles,
                CapacidadMotocicletas = parqueo.CapacidadMotocicletas,
                CapacidadLey7600 = parqueo.CapacidadLey7600
            };
            return View("ModificarParqueos", modelo);
        }
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public async Task<IActionResult> ModificarParqueos(ModificarParqueosVM modelo)
        {
            var parqueo = await _appDBcontext.Parqueos.Where(p => p.NombreParqueo == modelo.NombreParqueo).FirstOrDefaultAsync();
            if (parqueo == null) return NotFound();
            parqueo.NombreParqueo = modelo.NombreParqueo;
            parqueo.Ubicacion = modelo.Ubicacion;
            parqueo.CapacidadAutomoviles = modelo.CapacidadAutomoviles;
            parqueo.CapacidadMotocicletas = modelo.CapacidadMotocicletas;
            parqueo.CapacidadLey7600 = modelo.CapacidadLey7600;
            _appDBcontext.Update(parqueo);
            await _appDBcontext.SaveChangesAsync();
            ViewData["Mensaje"] = "Parqueo modificado exitosamente";
            return RedirectToAction("AdministracionParqueo", "AdministracionParqueo");
        }
    }
}
