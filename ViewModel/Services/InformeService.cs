using System;
using System.Threading.Tasks;
using Model.DataSets;
using Model.Repositorios;

namespace ViewModel.Services
{
    /// <summary>
    /// Servicio para la generación de informes con Crystal Reports.
    /// Coordina la obtención de datos desde los repositorios y su transformación a DataSets
    /// </summary>
    public class InformeService
    {
        private readonly CitaRepositorio _citaRepo;
        private readonly ClienteRepositorio _clienteRepo;
        private readonly ExpedienteRepositorio _expedienteRepo;
        private readonly ActuacionRepositorio _actuacionRepo;

        /// <summary>
        /// Constructor que inicializa los repositorios necesarios
        /// </summary>
        public InformeService()
        {
            _citaRepo = new CitaRepositorio();
            _clienteRepo = new ClienteRepositorio();
            _expedienteRepo = new ExpedienteRepositorio();
            _actuacionRepo = new ActuacionRepositorio();
        }

        /// <summary>
        /// Genera el DataSet para el informe de Agenda de Citas por día
        /// </summary>
        /// <param name="fecha">Fecha para filtrar las citas</param>
        /// <returns>DataSet tipado con las citas del día especificado</returns>
        public async Task<dsAgendaCitas> GenerarDataSetAgendaCitasAsync(DateTime fecha)
        {
            return await _citaRepo.ObtenerDataSetAgendaCitasAsync(fecha);
        }

        /// <summary>
        /// Genera el DataSet para el informe de Expedientes por Estado
        /// </summary>
        /// <returns>DataSet tipado con todos los expedientes agrupados por estado</returns>
        public async Task<dsExpedientesPorEstado> GenerarDataSetExpedientesPorEstadoAsync()
        {
            return await _expedienteRepo.ObtenerDataSetExpedientesPorEstadoAsync();
        }

        /// <summary>
        /// Genera el DataSet para el informe de Actuaciones Pendientes
        /// </summary>
        /// <returns>DataSet tipado con las actuaciones pendientes de expedientes activos</returns>
        public async Task<dsActuacionesPendientes> GenerarDataSetActuacionesPendientesAsync()
        {
            return await _actuacionRepo.ObtenerDataSetActuacionesPendientesAsync();
        }
    }
}
