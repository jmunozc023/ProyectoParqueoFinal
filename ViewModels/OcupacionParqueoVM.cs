namespace ProyectoParqueoFinal.ViewModels
{
    public class OcupacionParqueoVM
    {
        public int IdBitacora { get; set; }
        public string TipoIngreso { get; set; }
        public DateTime FechaHora { get; set; }
        public string NumeroPlaca { get; set; }
        public int? VehiculosIdVehiculo { get; set; }
        public int ParqueoIdParqueo { get; set; }
        public string TipoVehiculo { get; set; }
        public string NombreParqueo { get; set; }
        public int EspaciosDisponiblesAutos { get; set; }
        public int EspaciosDisponiblesMotos { get; set; }
        public int EspaciosDisponibles7600 { get; set; }

    }
}
