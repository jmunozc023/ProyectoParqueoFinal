namespace ProyectoParqueoFinal.ViewModels
{
    public class SeguridadVM
    {
        public int IdBitacora { get; set; }
        public string TipoIngreso { get; set; }
        public DateTime FechaHora { get; set; }
        public string NumeroPlaca { get; set; }
        public bool UsaEspacio7600 { get; set; }
        public int? VehiculosIdVehiculo { get; set; }
        public int ParqueoIdParqueo { get; set; }
    }
}
