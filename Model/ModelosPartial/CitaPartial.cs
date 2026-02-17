using Model.Mappers;

namespace Model
{
    /// <summary>
    /// Clase parcial de Cita para propiedades calculadas de presentación.
    /// Convierte enums de BD a texto formateado para mostrar en la UI sin modificar la estructura de datos original.
    /// Las clases parciales en C# deben estar en el mismo namespace que la clase principal,
    /// independientemente de la ubicación física del archivo.
    /// </summary>
    public partial class Cita
    {
        /// <summary>
        /// Obtiene la modalidad de la cita formateada para mostrar en la UI
        /// </summary>
        public string ModalidadFormateada => ModalidadCitaMapper.DeBDaUI(Modalidad);

        /// <summary>
        /// Obtiene el estado de la cita formateado para mostrar en la UI
        /// </summary>
        public string EstadoFormateado => EstadoCitaMapper.DeBDaUI(Estado);
    }
}
