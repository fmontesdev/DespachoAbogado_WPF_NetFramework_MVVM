using Model.Mappers;

namespace Model
{
    /// <summary>
    /// Clase parcial de Expediente para propiedades calculadas de presentación.
    /// Convierte enums de BD a texto formateado para mostrar en la UI sin modificar la estructura de datos original.
    /// Las clases parciales en C# deben estar en el mismo namespace que la clase principal,
    /// independientemente de la ubicación física del archivo.
    /// </summary>
    public partial class Expediente
    {
        /// <summary>
        /// Obtiene la jurisdicción formateada para mostrar en la UI
        /// </summary>
        public string JurisdiccionFormateada => JurisdiccionMapper.DeBDaUI(Jurisdiccion);

        /// <summary>
        /// Obtiene el estado formateado para mostrar en la UI
        /// </summary>
        public string EstadoFormateado => EstadoExpedienteMapper.DeBDaUI(Estado);

        /// <summary>
        /// Obtiene el código del expediente con nombre completo y DNI del cliente para mostrar en ComboBox
        /// Formato: "E26001 - Juan García López - 12345678A"
        /// </summary>
        public string CodigoConClienteYDni => Cliente != null 
            ? $"{Codigo} - {Cliente.NombreCompleto} - {Cliente.Dni}" 
            : Codigo ?? string.Empty;
    }
}

