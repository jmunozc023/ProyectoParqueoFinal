using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ProyectoParqueoFinal.Data;
using ProyectoParqueoFinal.Models;
using ProyectoParqueoFinal.ViewModels;

namespace ProyectoParqueoFinal.Controllers
{
    public class AdministrarVehiculosController : Controller
    {
        private readonly AppDBContext _appDBcontext;

        public AdministrarVehiculosController(AppDBContext appDBcontext)
        {
            _appDBcontext = appDBcontext;
        }
        //Controlador Get para la vista de administración de vehiculos
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public IActionResult AdministrarVehiculos()
        {
            var vehiculos = _appDBcontext.Vehiculos.Include(v => v.Usuario).ToList(); //Obtiene los datos de los vehiculos
            ViewData["Vehiculos"] = vehiculos; 

            var usuarios = _appDBcontext.Usuarios.ToList(); //Obtiene los datos de los usuarios
            ViewData["Usuarios"] = usuarios;
            return View();
        }
        //Controlador Post para la vista de administración de vehiculos
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public async Task<IActionResult> AdministrarVehiculos(AdministrarVehiculosVM modelo1) //Crea un nuevo vehiculo
        {
            bool placaExists = await _appDBcontext.Vehiculos.AnyAsync(v => v.NumeroPlaca == modelo1.NumeroPlaca); // Verifica si la placa ya existe
            if (placaExists) //Validacion de la placa si ya existe
            {
                ViewData["Mensaje"] = "La placa ingresada ya está registrada en el sistema.";
                return View();
            }

            int vehiculosCount = await _appDBcontext.Vehiculos.CountAsync(v => v.UsuariosIdUsuario == modelo1.UsuariosIdUsuario); // Verifica cuántos vehículos tiene el usuario
            if (vehiculosCount >= 2) //Validacion de la cantidad de vehiculos que tiene el usuario
            {
                ViewData["Mensaje"] = "El usuario ya tiene el máximo permitido de 2 vehículos registrados.";
                return View();
            }

            Vehiculo vehiculo = new Vehiculo //Crea un nuevo vehiculo
            {
                Marca = modelo1.Marca,
                Modelo = modelo1.Modelo,
                Color = modelo1.Color,
                NumeroPlaca = modelo1.NumeroPlaca,
                TipoVehiculo = modelo1.TipoVehiculo,
                UsaEspacio7600 = modelo1.UsaEspacio7600,
                UsuariosIdUsuario = modelo1.UsuariosIdUsuario
            };

            if (string.IsNullOrEmpty(modelo1.Marca)) //Validacion de la marca
            {
                ViewData["Mensaje"] = "El campo Marca es obligatorio.";
                return View(modelo1);
            }
            if (string.IsNullOrEmpty(modelo1.Modelo)) //Validacion del modelo
            {
                ViewData["Mensaje"] = "El campo Modelo es obligatorio.";
                return View(modelo1);
            }
            if (string.IsNullOrEmpty(modelo1.Color)) //Validacion del color
            {
                ViewData["Mensaje"] = "El campo Color es obligatorio.";
                return View(modelo1);
            }
            if (string.IsNullOrEmpty(modelo1.NumeroPlaca)) //Validacion de la placa
            {
                ViewData["Mensaje"] = "El campo Numero de placa es obligatorio.";
                return View(modelo1);
            }
            if (string.IsNullOrEmpty(modelo1.TipoVehiculo)) //Validacion del tipo de vehiculo
            {
                ViewData["Mensaje"] = "El campo Tipo de vehiculo es obligatorio.";
                return View(modelo1);
            }

            using (var transaction = await _appDBcontext.Database.BeginTransactionAsync()) //Inicia una transacción
            {
                try
                {
                    _appDBcontext.Vehiculos.Add(vehiculo); //Agrega el vehiculo a la base de datos
                    await _appDBcontext.SaveChangesAsync(); //Guarda los cambios en la base de datos
                    await transaction.CommitAsync();

                    ViewData["Mensaje"] = "Vehiculo registrado exitosamente";
                    return RedirectToAction("AdministrarVehiculos"); //Redirige a la vista de administración de vehiculos
                }
                catch
                {
                    await transaction.RollbackAsync(); //Revierte la transacción en caso de error
                    ViewData["Mensaje"] = "Error al guardar el vehiculo.";
                    return View();
                }
            }
        }
        //Controlador Post para eliminacion de vehiculos
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public async Task<IActionResult> EliminarVehiculo(int idVehiculo)
        {
            var vehiculo = await _appDBcontext.Vehiculos.FindAsync(idVehiculo); //Busca el vehiculo por su ID
            if (vehiculo == null)
            {
                return NotFound();
            }
            _appDBcontext.Vehiculos.Remove(vehiculo); //Elimina el vehiculo
            await _appDBcontext.SaveChangesAsync(); //Guarda los cambios en la base de datos
            ViewData["Mensaje"] = "Vehiculo eliminado exitosamente";
            return RedirectToAction("AdministrarVehiculos"); //Redirige a la vista de administración de vehiculos
        }

    }
}
