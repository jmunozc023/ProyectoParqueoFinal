namespace ProyectoParqueoFinal.Models
{
    public class Vehiculo
    {
        public int IdVehiculo { get; set; }
        public string Marca { get; set; }
        public string Modelo { get; set; }
        public string Color { get; set; }
        public string NumeroPlaca { get; set; }
        public string TipoVehiculo { get; set; }
        public bool UsaEspacio7600 { get; set; }

        public int UsuariosIdUsuario { get; set; }
        public Usuario Usuario { get; set; }

        public ICollection<Bitacora> Bitacoras { get; set; }
    }
}
