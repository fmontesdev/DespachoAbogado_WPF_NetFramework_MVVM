using System.Collections.Generic;

namespace ViewModel.Models
{
    /// <summary>
    /// Clase estática que proporciona la lista de informes disponibles en el sistema
    /// </summary>
    public static class ListaInformes
    {
        /// <summary>
        /// Obtiene la lista de todos los informes disponibles con su configuración
        /// </summary>
        /// <returns>Lista de objetos Informe con los informes del sistema</returns>
        public static List<Informe> ObtenerInformesDisponibles()
        {
            return new List<Informe>
            {
                new Informe
                {
                    Tipo = TipoInforme.AgendaCitas,
                    Titulo = "📅 Agenda de Citas por Día",
                    Descripcion = "Visualiza todas las citas programadas para un día específico con detalles de cliente, expediente, modalidad y estado."
                },
                new Informe
                {
                    Tipo = TipoInforme.ActuacionesPendientes,
                    Titulo = "📋 Actuaciones Pendientes",
                    Descripcion = "Listado de actuaciones pendientes de expedientes activos con fecha/hora, expediente, tipo, descripción y agrupadas por cliente."
                },
                new Informe
                {
                    Tipo = TipoInforme.ExpedientesPorEstado,
                    Titulo = "📊 Expedientes por Estado",
                    Descripcion = "Estadísticas de expedientes agrupados por estado con cantidad total y porcentaje de cada categoría."
                }
            };
        }
    }
}
