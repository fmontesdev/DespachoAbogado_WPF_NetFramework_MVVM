using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Repositorios
{
    /// <summary>
    /// Repositorio para gestionar las operaciones de acceso a datos de clientes
    /// </summary>
    public class ClienteRepositorio
    {
        /// <summary>
        /// Instancia del contexto de la base de datos
        /// </summary>
        private readonly DespachoAbogadoEntities _context = new DespachoAbogadoEntities();

        /// <summary>
        /// Obtiene todos los clientes de la base de datos incluyendo sus expedientes y citas
        /// </summary>
        /// <returns>Lista de todos los clientes con sus expedientes y citas asociados</returns>
        public async Task<List<Cliente>> SeleccionarAsync()
        {
            return await _context.Cliente.Include("Expediente").Include("Cita").ToListAsync();
        }

        /// <summary>
        /// Crea un nuevo cliente en la base de datos
        /// </summary>
        /// <param name="entity">Cliente a crear</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        public async Task CrearAsync(Cliente entity)
        {
            _context.Cliente.Add(entity);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Guarda los cambios realizados en un cliente
        /// </summary>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        public async Task GuardarAsync()
        {
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Elimina un cliente de la base de datos
        /// </summary>
        /// <param name="entity">Cliente a eliminar</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        public async Task EliminarAsync(Cliente entity)
        {
            _context.Cliente.Remove(entity);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Inicia una transacción en la base de datos
        /// </summary>
        /// <returns>Objeto que representa la transacción iniciada</returns>
        public DbContextTransaction IniciarTransaccion()
        {
            return _context.Database.BeginTransaction();
        }

        /// <summary>
        /// Obtiene el contexto actual de Entity Framework
        /// </summary>
        /// <returns>Contexto de Entity Framework para operaciones dentro de transacciones</returns>
        public DespachoAbogadoEntities ObtenerContexto()
        {
            return _context;
        }
    }
}
