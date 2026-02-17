using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Model;
using Model.Configuracion;
using Model.Repositorios;

namespace ViewModel.Services
{
    /// <summary>
    /// Servicio que encapsula la lógica de negocio para la gestión de citas.
    /// Actúa como intermediario entre el ViewModel y el Repositorio
    /// </summary>
    public class CitaService
    {
        private readonly CitaRepositorio _repo;

        /// <summary>
        /// Constructor que inicializa el repositorio de citas
        /// </summary>
        public CitaService()
        {
            _repo = new CitaRepositorio();
        }

        /// <summary>
        /// Obtiene todos los horarios del despacho
        /// </summary>
        /// <returns>Lista de horarios disponibles</returns>
        public List<string> ObtenerTodosLosHorarios()
        {
            return new List<string>(HorariosDespacho.Horarios);
        }

        /// <summary>
        /// Obtiene los horarios disponibles para una fecha específica
        /// Filtra los horarios ya ocupados por citas programadas y los que ya han pasado si la fecha es hoy
        /// </summary>
        /// <param name="fecha">Fecha de la cita</param>
        /// <returns>Lista de horarios disponibles</returns>
        public async Task<List<string>> ObtenerHorariosDisponiblesAsync(DateTime fecha)
        {
            // Obtener todas las citas PROGRAMADAS en esa fecha (ignorar canceladas o realizadas)
            var todasLasCitas = await _repo.SeleccionarAsync();
            var citasEnFecha = todasLasCitas
                .Where(c => c.Fecha.Date == fecha.Date && c.Estado?.ToLower() == "programada")
                .Select(c => c.Horario)
                .ToList();

            // Filtrar horarios ya ocupados
            var horariosDisponibles = HorariosDespacho.Horarios
                .Where(h => !citasEnFecha.Contains(h))
                .ToList();

            // Si la fecha es hoy, filtrar horarios cuya hora de inicio sea posterior a la hora actual
            if (fecha.Date == DateTime.Now.Date)
            {
                TimeSpan horaActual = DateTime.Now.TimeOfDay;
                horariosDisponibles = horariosDisponibles
                    .Where(h =>
                    {
                        // Obtener la hora de inicio del horario (ej: "09:00-10:00" → "09:00")
                        string horaInicio = h.Split('-')[0];
                        TimeSpan horaInicioHorario = TimeSpan.Parse(horaInicio);
                        // Solo mostrar si la hora de inicio del horario es posterior a la hora actual
                        return horaInicioHorario > horaActual;
                    })
                    .ToList();
            }

            return horariosDisponibles;
        }

        /// <summary>
        /// Obtiene todas las citas de la base de datos
        /// </summary>
        /// <returns>Lista de todas las citas registradas</returns>
        public async Task<List<Cita>> ObtenerCitasAsync()
        {
            return await _repo.SeleccionarAsync();
        }

        /// <summary>
        /// Crea una nueva cita en la base de datos después de validar las reglas de negocio
        /// </summary>
        /// <param name="cita">Cita a crear</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        /// <exception cref="ArgumentNullException">Si la cita es nula</exception>
        /// <exception cref="ArgumentException">Si el horario no está disponible</exception>
        public async Task CrearCitaAsync(Cita cita)
        {
            if (cita == null)
                throw new ArgumentNullException(nameof(cita), "La cita no puede ser nula");

            // Validar que el horario esté disponible
            var horariosDisponibles = await ObtenerHorariosDisponiblesAsync(cita.Fecha);
            if (!horariosDisponibles.Contains(cita.Horario))
            {
                throw new ArgumentException("El horario seleccionado no está disponible para esta fecha");
            }

            await _repo.CrearAsync(cita);
        }

        /// <summary>
        /// Actualiza los datos de una cita existente después de validar las reglas de negocio
        /// </summary>
        /// <param name="cita">Cita con los datos actualizados</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        /// <exception cref="ArgumentNullException">Si la cita es nula</exception>
        public async Task ActualizarCitaAsync(Cita cita)
        {
            if (cita == null)
                throw new ArgumentNullException(nameof(cita), "La cita no puede ser nula");

            await _repo.GuardarAsync();
        }

        /// <summary>
        /// Elimina una cita de la base de datos
        /// </summary>
        /// <param name="cita">Cita a eliminar</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        /// <exception cref="ArgumentNullException">Si la cita es nula</exception>
        /// <exception cref="InvalidOperationException">Si la cita tiene estado realizada</exception>
        public async Task EliminarCitaAsync(Cita cita)
        {
            if (cita == null)
                throw new ArgumentNullException(nameof(cita), "La cita no puede ser nula");

            // Validar que la cita no tenga estado realizada
            if (cita.Estado?.ToLower() == "realizada")
            {
                throw new InvalidOperationException("No se puede eliminar una cita ya realizada");
            }

            await _repo.EliminarAsync(cita);
        }
    }
}

