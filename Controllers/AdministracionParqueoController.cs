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
        //Controlador Get para la vista de administración de parqueos
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public IActionResult AdministracionParqueo() //Obtiene los datos de los parqueos
        {
            var parqueos = _appDBcontext.Parqueos.ToList();
            ViewData["Parqueos"] = parqueos;
            return View();
        }

        //Controlador Post para la vista de administración de parqueos
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public async Task<IActionResult> AdministracionParqueo(Parqueo parqueo)
        {
            bool parqueoExists = await _appDBcontext.Parqueos.AnyAsync(p => p.NombreParqueo == parqueo.NombreParqueo); // Verifica si el parqueo ya existe
            if (parqueoExists) //Validacion del parqueo si ya existe
            {
                ViewData["Mensaje"] = "El parqueos ingresado ya está registrado en el sistema.";
                return View();
            }
            Parqueo parqueo1 = new Parqueo //Crea un nuevo parqueo
            {
                NombreParqueo = parqueo.NombreParqueo,
                Ubicacion = parqueo.Ubicacion,
                CapacidadAutomoviles = parqueo.CapacidadAutomoviles,
                CapacidadMotocicletas = parqueo.CapacidadMotocicletas,
                CapacidadLey7600 = parqueo.CapacidadLey7600
            };
            _appDBcontext.Add(parqueo1); //Agrega el parqueo a la base de datos
            await _appDBcontext.SaveChangesAsync();
            ViewData["Mensaje"] = "Parqueo registrado exitosamente";
            return RedirectToAction("AdministracionParqueo", "AdministracionParqueo"); //Redirige a la vista de administración de parqueos
        }
        //Controlador Pos para eliminacion de parqueos
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public async Task<IActionResult> EliminarParqueos(int IdParqueo) //Elimina un parqueo
        {
            var parqueo = await _appDBcontext.Parqueos.FindAsync(IdParqueo); //Busca el parqueo por su ID
            if (parqueo == null)
            {
                return NotFound();
            }
            using (var transaction = await _appDBcontext.Database.BeginTransactionAsync()) //Inicia una transacción
            {
                try
                {
                    _appDBcontext.Parqueos.Remove(parqueo); //Elimina el parqueo
                    await _appDBcontext.SaveChangesAsync(); //Guarda los cambios en la base de datos
                    await transaction.CommitAsync();
                    ViewData["Mensaje"] = "Parqueo eliminado exitosamente";
                    return RedirectToAction("AdministracionParqueo"); //Redirige a la vista de administración de parqueos
                }
                catch
                {
                    await transaction.RollbackAsync(); //Revierte la transacción en caso de error
                    ViewData["Mensaje"] = "Error al eliminar el parqueo.";
                    return View();
                }
            }
        }


    }
}
