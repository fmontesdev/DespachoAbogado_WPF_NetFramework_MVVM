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
    /// Servicio que encapsula la lógica de negocio para la gestión de expedientes.
    /// Actúa como intermediario entre el ViewModel y el Repositorio
    /// </summary>
    public class ExpedienteService
    {
        private readonly ExpedienteRepositorio _repo;

        /// <summary>
        /// Constructor que inicializa el repositorio de expedientes
        /// </summary>
        public ExpedienteService()
        {
            _repo = new ExpedienteRepositorio();
        }

        /// <summary>
        /// Obtiene todos los expedientes de la base de datos
        /// </summary>
        /// <returns>Lista de todos los expedientes registrados</returns>
        public async Task<List<Expediente>> ObtenerExpedientesAsync()
        {
            return await _repo.SeleccionarAsync();
        }

        /// <summary>
        /// Crea un nuevo expediente en la base de datos después de validar las reglas de negocio
        /// </summary>
        /// <param name="expediente">Expediente a crear</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        /// <exception cref="ArgumentNullException">Si el expediente es nulo</exception>
        /// <exception cref="ArgumentException">Si los datos del expediente no son válidos o si el código ya existe</exception>
        public async Task CrearExpedienteAsync(Expediente expediente)
        {
            if (expediente == null)
                throw new ArgumentNullException(nameof(expediente), "El expediente no puede ser nulo");

            // Generar código automáticamente
            expediente.Codigo = await GenerarCodigoExpedienteAsync();

            // Establecer fecha de apertura automáticamente
            expediente.Apertura = DateTime.Now;

            // Validar unicidad del código (aunque lo generamos automáticamente, es una validación adicional)
            await ValidarCodigoUnicoAsync(expediente.Codigo, null);

            await _repo.CrearAsync(expediente);
        }

        /// <summary>
        /// Actualiza los datos de un expediente existente después de validar las reglas de negocio
        /// </summary>
        /// <param name="expediente">Expediente con los datos actualizados</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        /// <exception cref="ArgumentNullException">Si el expediente es nulo</exception>
        /// <exception cref="ArgumentException">Si los datos del expediente no son válidos</exception>
        public async Task ActualizarExpedienteAsync(Expediente expediente)
        {
            if (expediente == null)
                throw new ArgumentNullException(nameof(expediente), "El expediente no puede ser nulo");

            await _repo.GuardarAsync();
        }

        /// <summary>
        /// Elimina un expediente de la base de datos si no tiene citas, actuaciones o documentos asociados
        /// </summary>
        /// <param name="expediente">Expediente a eliminar</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        /// <exception cref="ArgumentNullException">Si el expediente es nulo</exception>
        /// <exception cref="InvalidOperationException">Si el expediente tiene citas, actuaciones o documentos asociados</exception>
        public async Task EliminarExpedienteAsync(Expediente expediente)
        {
            if (expediente == null)
                throw new ArgumentNullException(nameof(expediente), "El expediente no puede ser nulo");

            // Verificar si tiene actuaciones asociadas
            if (expediente.Actuacion != null && expediente.Actuacion.Any())
            {
                int cantidadActuaciones = expediente.Actuacion.Count;
                string mensaje = cantidadActuaciones == 1
                    ? "No se puede eliminar el expediente porque tiene 1 actuación asociada."
                    : $"No se puede eliminar el expediente porque tiene {cantidadActuaciones} actuaciones asociadas.";
                
                throw new InvalidOperationException(mensaje);
            }

            // Verificar si tiene citas asociadas
            if (expediente.Cita != null && expediente.Cita.Any())
            {
                int cantidadCitas = expediente.Cita.Count;
                string mensaje = cantidadCitas == 1
                    ? "No se puede eliminar el expediente porque tiene 1 cita asociada."
                    : $"No se puede eliminar el expediente porque tiene {cantidadCitas} citas asociadas.";

                throw new InvalidOperationException(mensaje);
            }

            await _repo.EliminarAsync(expediente);
        }

        /// <summary>
        /// Genera un código único para el expediente siguiendo el formato E + año(2 dígitos) + número secuencial (4 dígitos)
        /// Ejemplo: E260001 para el primer expediente de 2026
        /// </summary>
        /// <returns>Código de expediente generado</returns>
        private async Task<string> GenerarCodigoExpedienteAsync()
        {
            var expedientes = await _repo.SeleccionarAsync();
            var añoActual = DateTime.Now.Year;
            var añoCorto = añoActual.ToString().Substring(2); // Obtiene los últimos 2 dígitos del año

            // Buscar expedientes del año actual
            var expedientesDelAño = expedientes
                .Where(e => e.Codigo != null && e.Codigo.StartsWith($"E{añoCorto}"))
                .ToList();

            int nuevoNumero;

            if (expedientesDelAño.Any())
            {
                // Obtener el número más alto del año actual y sumar 1
                nuevoNumero = expedientesDelAño
                    .Select(e => int.TryParse(e.Codigo.Substring(3), out int num) ? num : 0)
                    .Max() + 1;
            }
            else
            {
                // Si no hay expedientes del año actual, comenzar desde 0001
                nuevoNumero = 1;
            }

            // Formato: E + año(2 dígitos) + número(4 dígitos con ceros a la izquierda)
            return $"E{añoCorto}{nuevoNumero:D4}";
        }

        /// <summary>
        /// Valida que el código sea único en la base de datos
        /// </summary>
        /// <param name="codigo">Código a validar</param>
        /// <param name="idExpedienteActual">ID del expediente actual (null si es nuevo expediente)</param>
        /// <exception cref="ArgumentException">Si el código ya existe en otro expediente</exception>
        private async Task ValidarCodigoUnicoAsync(string codigo, int? idExpedienteActual)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                return;

            var expedientes = await _repo.SeleccionarAsync();
            var codigoExiste = expedientes.Any(e =>
                e.Codigo != null &&
                e.Codigo.Trim().Equals(codigo.Trim()) &&
                e.IdExpediente != idExpedienteActual);

            if (codigoExiste)
            {
                throw new ArgumentException($"El código '{codigo}' ya está registrado en otro expediente");
            }
        }
    }
}
