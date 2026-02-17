using Model.Mappers;

namespace Model
{
    /// <summary>
    /// Clase parcial de Actuacion para propiedades calculadas de presentación.
    /// Convierte enums de BD a texto formateado para mostrar en la UI sin modificar la estructura de datos original.
    /// Las clases parciales en C# deben estar en el mismo namespace que la clase principal,
    /// independientemente de la ubicación física del archivo.
    /// </summary>
    public partial class Actuacion
    {
        /// <summary>
        /// Obtiene el tipo de actuación formateado para mostrar en la UI
        /// </summary>
        public string TipoFormateado => TipoActuacionMapper.DeBDaUI(Tipo);

        /// <summary>
        /// Obtiene el estado formateado para mostrar en la UI
        /// </summary>
        public string EstadoFormateado => EstadoActuacionMapper.DeBDaUI(Estado);
    }
}

