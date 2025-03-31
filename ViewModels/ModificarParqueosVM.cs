namespace ProyectoParqueoFinal.ViewModels
{
    public class ModificarParqueosVM
    {
        public int IdParqueo { get; set; }
        public string NombreParqueo { get; set; }
        public string Ubicacion { get; set; }
        public int CapacidadAutomoviles { get; set; }
        public int CapacidadMotocicletas { get; set; }
        public int CapacidadLey7600 { get; set; }
    }
}
