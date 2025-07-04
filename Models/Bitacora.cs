﻿namespace ProyectoParqueoFinal.Models
{
    public class Bitacora
    {
        public int IdBitacora { get; set; }
        public string TipoIngreso { get; set; }
        public DateTime FechaHora { get; set; }

        public int? VehiculosIdVehiculo { get; set; }
        public string NumeroPlaca { get; set; }
        public Vehiculo Vehiculo { get; set; }

        public int ParqueoIdParqueo { get; set; }
        public string? TipoVehiculo { get; set; }
        public bool UsaEspacio7600 { get; set; }
        public Parqueo Parqueo { get; set; }
    }
}
