using Model.DataSets;
using Model.Mappers;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Repositorios
{
    /// <summary>
    /// Repositorio para gestionar las operaciones de acceso a datos de expedientes
    /// </summary>
    public class ExpedienteRepositorio
    {
        /// <summary>
        /// Instancia del contexto de la base de datos
        /// </summary>
        private readonly DespachoAbogadoEntities _context = new DespachoAbogadoEntities();

        /// <summary>
        /// Obtiene todos los expedientes de la base de datos incluyendo sus relaciones
        /// </summary>
        /// <returns>Lista de todos los expedientes con sus relaciones asociadas</returns>
        public async Task<List<Expediente>> SeleccionarAsync()
        {
            return await _context.Expediente.Include("Cliente").Include("Actuacion").Include("Cita").ToListAsync();
        }

        /// <summary>
        /// Crea un nuevo expediente en la base de datos
        /// </summary>
        /// <param name="entity">Expediente a crear</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        public async Task CrearAsync(Expediente entity)
        {
            _context.Expediente.Add(entity);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Guarda los cambios realizados en un expediente
        /// </summary>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        public async Task GuardarAsync()
        {
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Elimina un expediente de la base de datos
        /// </summary>
        /// <param name="entity">Expediente a eliminar</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        public async Task EliminarAsync(Expediente entity)
        {
            _context.Expediente.Remove(entity);
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

        /// <summary>
        /// Genera el DataSet para el informe de Expedientes por Estado
        /// </summary>
        /// <returns>DataSet tipado con todos los expedientes agrupados por estado</returns>
        public async Task<dsExpedientesPorEstado> ObtenerDataSetExpedientesPorEstadoAsync()
        {
            // Crear una instancia del DataSet tipado
            var dataSet = new dsExpedientesPorEstado();

            // Obtener todos los expedientes incluyendo relaciones
            var expedientes = await _context.Expediente
                .Include("Cliente")
                .Include("Actuacion")
                .OrderBy(e => e.Estado)
                .ThenBy(e => e.Apertura)
                .ToListAsync();

            // Mapear entidades al DataSet
            foreach (var expediente in expedientes)
            {
                dataSet.Expedientes.AddExpedientesRow(
                    expediente.Codigo,
                    expediente.Cliente.NombreCompleto,
                    expediente.Titulo,
                    expediente.Apertura.ToString("dd/MM/yyyy"),
                    expediente.Cierre.HasValue ? expediente.Cierre.Value.ToString("dd/MM/yyyy") : "Sin cierre",
                    EstadoExpedienteMapper.DeBDaUI(expediente.Estado),
                    expediente.Actuacion.Count
                );
            }

            return dataSet;
        }
    }
}
