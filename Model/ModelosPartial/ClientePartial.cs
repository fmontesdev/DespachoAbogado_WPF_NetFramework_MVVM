namespace Model
{
    /// <summary>
    /// Clase parcial de Cliente para propiedades calculadas de presentación.
    /// Las clases parciales en C# deben estar en el mismo namespace que la clase principal,
    /// independientemente de la ubicación física del archivo.
    /// </summary>
    public partial class Cliente
    {
        /// <summary>
        /// Obtiene el nombre completo del cliente (nombre y apellidos)
        /// </summary>
        public string NombreCompleto => $"{Nombre} {Apellidos}";

        /// <summary>
        /// Obtiene el nombre completo del cliente con DNI para mostrar en ComboBox
        /// </summary>
        public string NombreCompletoConDni => $"{Nombre} {Apellidos} - {Dni}";
    }
}
