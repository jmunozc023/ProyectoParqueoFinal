using System.ComponentModel.DataAnnotations;

namespace ProyectoParqueoFinal.ViewModels
{
    public class ModificarVehiculosVM
    {
        public int IdVehiculo { get; set; }
        [Required(ErrorMessage = "El campo Marca es obligatorio.")]

        public string Marca { get; set; }
        [Required(ErrorMessage = "El campo Modelo es obligatorio.")]

        public string Modelo { get; set; }
        [Required(ErrorMessage = "El campo Color es obligatorio.")]

        public string Color { get; set; }
        [Required(ErrorMessage = "El campo Numero de placa es obligatorio.")]

        public string NumeroPlaca { get; set; }
        [Required(ErrorMessage = "El campo Tipo de vehiculo es obligatorio.")]

        public string TipoVehiculo { get; set; }

        public bool UsaEspacio7600 { get; set; }

        public int UsuariosIdUsuario { get; set; }
    }
}
