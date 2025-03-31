namespace ProyectoParqueoFinal.ViewModels
{
    public class AdministrarUsuariosVM
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string CorreoElectronico { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string Cedula { get; set; }
        public string NumeroCarne { get; set; }
        public string Password { get; set; }
        public string Rol { get; set; }
        public bool RequiereCambioPassword { get; set; }

    }
}
