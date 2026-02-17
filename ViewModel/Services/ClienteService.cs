using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Model;
using Model.Repositorios;
using ViewModel.Validadores;

namespace ViewModel.Services
{
    /// <summary>
    /// Servicio que encapsula la lógica de negocio para la gestión de clientes.
    /// Actúa como intermediario entre el ViewModel y el Repositorio
    /// </summary>
    public class ClienteService
    {
        private readonly ClienteRepositorio _repo;

        /// <summary>
        /// Constructor que inicializa el repositorio de clientes
        /// </summary>
        public ClienteService()
        {
            _repo = new ClienteRepositorio();
        }

        /// <summary>
        /// Obtiene todos los clientes de la base de datos
        /// </summary>
        /// <returns>Lista de todos los clientes registrados</returns>
        public async Task<List<Cliente>> ObtenerClientesAsync()
        {
            return await _repo.SeleccionarAsync();
        }

        /// <summary>
        /// Crea un nuevo cliente en la base de datos después de validar las reglas de negocio
        /// </summary>
        /// <param name="cliente">Cliente a crear</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        /// <exception cref="ArgumentNullException">Si el cliente es nulo</exception>
        /// <exception cref="ArgumentException">Si los datos del cliente no son válidos o si el DNI o Email ya existen</exception>
        public async Task CrearClienteAsync(Cliente cliente)
        {
            if (cliente == null)
                throw new ArgumentNullException(nameof(cliente), "El cliente no puede ser nulo");

            // Validar unicidad de DNI y Email
            await ValidarDniUnicoAsync(cliente.Dni, null);
            await ValidarEmailUnicoAsync(cliente.Email, null);

            await _repo.CrearAsync(cliente);
        }

        /// <summary>
        /// Actualiza los datos de un cliente existente después de validar las reglas de negocio
        /// </summary>
        /// <param name="cliente">Cliente con los datos actualizados</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        /// <exception cref="ArgumentNullException">Si el cliente es nulo</exception>
        /// <exception cref="ArgumentException">Si los datos del cliente no son válidos o si el DNI o Email ya existen en otro cliente</exception>
        public async Task ActualizarClienteAsync(Cliente cliente)
        {
            if (cliente == null)
                throw new ArgumentNullException(nameof(cliente), "El cliente no puede ser nulo");

            // Validar unicidad de DNI y Email (excluyendo el cliente actual)
            await ValidarDniUnicoAsync(cliente.Dni, cliente.IdCliente);
            await ValidarEmailUnicoAsync(cliente.Email, cliente.IdCliente);

            await _repo.GuardarAsync();
        }

        /// <summary>
        /// Elimina un cliente de la base de datos si no tiene expedientes o citas asociadas
        /// </summary>
        /// <param name="cliente">Cliente a eliminar</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        /// <exception cref="ArgumentNullException">Si el cliente es nulo</exception>
        /// <exception cref="InvalidOperationException">Si el cliente tiene expedientes o citas asociadas</exception>
        public async Task EliminarClienteAsync(Cliente cliente)
        {
            if (cliente == null)
                throw new ArgumentNullException(nameof(cliente), "El cliente no puede ser nulo");

            // Verificar si tiene expedientes
            if (cliente.Expediente != null && cliente.Expediente.Any())
            {
                throw new InvalidOperationException(
                    "No se puede eliminar un cliente con expedientes asociados. " +
                    "Primero elimine los expedientes.");
            }

            // Verificar si tiene citas
            if (cliente.Cita != null && cliente.Cita.Any())
            {
                throw new InvalidOperationException(
                    "No se puede eliminar un cliente con citas asociadas. " +
                    "Primero elimine las citas.");
            }

            await _repo.EliminarAsync(cliente);
        }

        /// <summary>
        /// Valida que el DNI sea único en la base de datos
        /// </summary>
        /// <param name="dni">DNI a validar</param>
        /// <param name="idClienteActual">ID del cliente actual (null si es nuevo cliente)</param>
        /// <exception cref="ArgumentException">Si el DNI ya existe en otro cliente</exception>
        private async Task ValidarDniUnicoAsync(string dni, int? idClienteActual)
        {
            if (!string.IsNullOrWhiteSpace(dni))
            {
                var clientes = await _repo.SeleccionarAsync();
                var dniExiste = clientes.Any(c =>
                    c.Dni != null &&
                    c.Dni.Trim().Equals(dni.Trim(), StringComparison.OrdinalIgnoreCase) &&
                    c.IdCliente != idClienteActual);

                if (dniExiste)
                {
                    throw new ArgumentException($"El DNI '{dni}' ya está registrado en otro cliente");
                }
            }
        }

        /// <summary>
        /// Valida que el Email sea único en la base de datos
        /// </summary>
        /// <param name="email">Email a validar</param>
        /// <param name="idClienteActual">ID del cliente actual (null si es nuevo cliente)</param>
        /// <exception cref="ArgumentException">Si el Email ya existe en otro cliente</exception>
        private async Task ValidarEmailUnicoAsync(string email, int? idClienteActual)
        {
            if (!string.IsNullOrWhiteSpace(email))
            {
                var clientes = await _repo.SeleccionarAsync();
                var emailExiste = clientes.Any(c =>
                    c.Email != null &&
                    c.Email.Trim().Equals(email.Trim(), StringComparison.OrdinalIgnoreCase) &&
                    c.IdCliente != idClienteActual);

                if (emailExiste)
                {
                    throw new ArgumentException($"El Email '{email}' ya está registrado en otro cliente");
                }
            }
        }
    }
}
