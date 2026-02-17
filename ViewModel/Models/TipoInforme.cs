namespace ViewModel.Models
{
    /// <summary>
    /// Enumeración que define los tipos de informes disponibles en el sistema
    /// </summary>
    public enum TipoInforme
    {
        /// <summary>
        /// Informe de agenda de citas por día
        /// </summary>
        AgendaCitas,

        /// <summary>
        /// Informe de actuaciones pendientes
        /// </summary>
        ActuacionesPendientes,

        /// <summary>
        /// Informe de expedientes agrupados por estado
        /// </summary>
        ExpedientesPorEstado
    }
}
