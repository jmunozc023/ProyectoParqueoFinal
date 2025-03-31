using System.ComponentModel.DataAnnotations;
namespace ProyectoParqueoFinal.ViewModels
{
    public class CambioPasswordVM
    {
        public string CorreoElectronico { get; set; }
        [Required(ErrorMessage = "La contraseña actual es requerida.")]
        [DataType(DataType.Password)]
        public string PasswordActual { get; set; }
        [Required(ErrorMessage = "La nueva contraseña es requerida.")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,100}$", ErrorMessage = "La contraseña debe tener al menos una letra mayúscula, una letra minúscula, un número y un carácter especial.")]
        public string NuevoPassword { get; set; }
        [Required(ErrorMessage = "La confirmación de la nueva contraseña es requerida.")]
        [DataType(DataType.Password)]
        [Compare("NuevoPassword", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmacionNuevoPassword { get; set; }

    }
}
