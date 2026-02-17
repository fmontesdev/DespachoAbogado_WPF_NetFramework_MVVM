namespace ViewModel.Models
{
    /// <summary>
    /// Representa la información de un informe disponible en el sistema
    /// </summary>
    public class Informe
    {
        /// <summary>
        /// Tipo de informe que identifica qué datos mostrar
        /// </summary>
        public TipoInforme Tipo { get; set; }

        /// <summary>
        /// Título del informe que se muestra en la interfaz
        /// </summary>
        public string Titulo { get; set; }

        /// <summary>
        /// Descripción detallada del informe
        /// </summary>
        public string Descripcion { get; set; }
    }
}
