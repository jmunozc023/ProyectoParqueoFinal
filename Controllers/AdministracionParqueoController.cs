using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoParqueoFinal.Data;
using ProyectoParqueoFinal.Models;

namespace ProyectoParqueoFinal.Controllers
{
    public class AdministracionParqueoController : Controller
    {
        private readonly AppDBContext _appDBcontext;
        public AdministracionParqueoController(AppDBContext appDBcontext)
        {
            _appDBcontext = appDBcontext;
        }
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public IActionResult AdministracionParqueo()
        {
            var parqueos = _appDBcontext.Parqueos.ToList();
            ViewData["Parqueos"] = parqueos;
            return View();
        }
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public async Task<IActionResult> AdministracionParqueo(Parqueo parqueo)
        {
            bool parqueoExists = await _appDBcontext.Parqueos.AnyAsync(p => p.NombreParqueo == parqueo.NombreParqueo);
            if (parqueoExists)
            {
                ViewData["Mensaje"] = "El parqueos ingresado ya está registrado en el sistema.";
                return View();
            }
            Parqueo parqueo1 = new Parqueo
            {
                NombreParqueo = parqueo.NombreParqueo,
                Ubicacion = parqueo.Ubicacion,
                CapacidadAutomoviles = parqueo.CapacidadAutomoviles,
                CapacidadMotocicletas = parqueo.CapacidadMotocicletas,
                CapacidadLey7600 = parqueo.CapacidadLey7600
            };
            _appDBcontext.Add(parqueo1);
            await _appDBcontext.SaveChangesAsync();
            ViewData["Mensaje"] = "Parqueo registrado exitosamente";
            return RedirectToAction("AdministracionParqueo", "AdministracionParqueo");
        }
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public async Task<IActionResult> EliminarParqueos(int IdParqueo)
        {
            var parqueo = await _appDBcontext.Parqueos.FindAsync(IdParqueo);
            if (parqueo == null)
            {
                return NotFound();
            }
            using (var transaction = await _appDBcontext.Database.BeginTransactionAsync())
            {
                try
                {
                    _appDBcontext.Parqueos.Remove(parqueo);
                    await _appDBcontext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    ViewData["Mensaje"] = "Parqueo eliminado exitosamente";
                    return RedirectToAction("AdministracionParqueo");
                }
                catch
                {
                    await transaction.RollbackAsync();
                    ViewData["Mensaje"] = "Error al eliminar el parqueo.";
                    return View();
                }
            }
        }


    }
}
