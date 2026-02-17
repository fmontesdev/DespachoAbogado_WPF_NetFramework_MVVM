using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model;
using Model.Repositorios;

namespace ViewModel.Services
{
    /// <summary>
    /// Servicio que encapsula la lógica de negocio para la gestión de actuaciones.
    /// Actúa como intermediario entre el ViewModel y el Repositorio
    /// </summary>
    public class ActuacionService
    {
        private readonly ActuacionRepositorio _repo;

        /// <summary>
        /// Constructor que inicializa el repositorio de actuaciones
        /// </summary>
        public ActuacionService()
        {
            _repo = new ActuacionRepositorio();
        }

        /// <summary>
        /// Obtiene todas las actuaciones de la base de datos
        /// </summary>
        /// <returns>Lista de todas las actuaciones registradas</returns>
        public async Task<List<Actuacion>> ObtenerActuacionesAsync()
        {
            return await _repo.SeleccionarAsync();
        }

        /// <summary>
        /// Crea una nueva actuación en la base de datos después de validar las reglas de negocio
        /// </summary>
        /// <param name="actuacion">Actuación a crear</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        /// <exception cref="ArgumentNullException">Si la actuación es nula</exception>
        public async Task CrearActuacionAsync(Actuacion actuacion)
        {
            if (actuacion == null)
                throw new ArgumentNullException(nameof(actuacion), "La actuación no puede ser nula");

            // Establecer fecha y hora actual automáticamente
            actuacion.FechaHora = DateTime.Now;

            // Establecer estado pendiente por defecto
            actuacion.Estado = "pendiente";

            await _repo.CrearAsync(actuacion);
        }

        /// <summary>
        /// Actualiza los datos de una actuación existente después de validar las reglas de negocio
        /// </summary>
        /// <param name="actuacion">Actuación con los datos actualizados</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        /// <exception cref="ArgumentNullException">Si la actuación es nula</exception>
        /// <exception cref="InvalidOperationException">Si el expediente está archivado o cerrado</exception>
        public async Task ActualizarActuacionAsync(Actuacion actuacion)
        {
            if (actuacion == null)
                throw new ArgumentNullException(nameof(actuacion), "La actuación no puede ser nula");

            // Validar que el expediente no esté archivado o cerrado
            string estadoExpediente = actuacion.Expediente?.Estado?.ToLower();
            if (estadoExpediente == "archivado" || estadoExpediente == "cerrado")
            {
                throw new InvalidOperationException("No se puede actualizar una actuación de un expediente archivado o cerrado");
            }

            await _repo.GuardarAsync();
        }

        /// <summary>
        /// Elimina una actuación de la base de datos
        /// </summary>
        /// <param name="actuacion">Actuación a eliminar</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        /// <exception cref="ArgumentNullException">Si la actuación es nula</exception>
        /// <exception cref="InvalidOperationException">Si el expediente está archivado o cerrado</exception>
        public async Task EliminarActuacionAsync(Actuacion actuacion)
        {
            if (actuacion == null)
                throw new ArgumentNullException(nameof(actuacion), "La actuación no puede ser nula");

            // Validar que el expediente no esté archivado o cerrado
            string estadoExpediente = actuacion.Expediente?.Estado?.ToLower();
            if (estadoExpediente == "archivado" || estadoExpediente == "cerrado")
            {
                throw new InvalidOperationException("No se puede eliminar una actuación de un expediente archivado o cerrado");
            }

            await _repo.EliminarAsync(actuacion);
        }
    }
}
