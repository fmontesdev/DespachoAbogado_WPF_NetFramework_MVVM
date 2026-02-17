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
    /// Repositorio para gestionar las operaciones de acceso a datos de actuaciones
    /// </summary>
    public class ActuacionRepositorio
    {
        /// <summary>
        /// Instancia del contexto de la base de datos
        /// </summary>
        private readonly DespachoAbogadoEntities _context = new DespachoAbogadoEntities();

        /// <summary>
        /// Obtiene todas las actuaciones de la base de datos incluyendo su expediente
        /// </summary>
        /// <returns>Lista de todas las actuaciones con su expediente asociado</returns>
        public async Task<List<Actuacion>> SeleccionarAsync()
        {
            return await _context.Actuacion.Include("Expediente").ToListAsync();
        }

        /// <summary>
        /// Crea una nueva actuación en la base de datos
        /// </summary>
        /// <param name="entity">Actuación a crear</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        public async Task CrearAsync(Actuacion entity)
        {
            _context.Actuacion.Add(entity);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Guarda los cambios realizados en una actuación
        /// </summary>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        public async Task GuardarAsync()
        {
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Elimina una actuación de la base de datos
        /// </summary>
        /// <param name="entity">Actuación a eliminar</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        public async Task EliminarAsync(Actuacion entity)
        {
            _context.Actuacion.Remove(entity);
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
        /// Genera el DataSet para el informe de Actuaciones Pendientes
        /// </summary>
        /// <returns>DataSet tipado con las actuaciones pendientes de expedientes activos</returns>
        public async Task<dsActuacionesPendientes> ObtenerDataSetActuacionesPendientesAsync()
        {
            // Crear una instancia del DataSet tipado
            var dataSet = new dsActuacionesPendientes();

            // Obtener actuaciones pendientes de expedientes no archivados ni cerrados
            var actuaciones = await _context.Actuacion
                .Include("Expediente")
                .Include("Expediente.Cliente")
                .Where(a => a.Estado == "pendiente" && 
                           a.Expediente.Estado != "archivado" && 
                           a.Expediente.Estado != "cerrado")
                .OrderBy(a => a.FechaHora)
                .ToListAsync();

            // Mapear entidades al DataSet
            foreach (var actuacion in actuaciones)
            {
                dataSet.Actuaciones.AddActuacionesRow(
                    actuacion.FechaHora,
                    actuacion.Expediente.Codigo,
                    actuacion.Expediente.Cliente.NombreCompleto,
                    TipoActuacionMapper.DeBDaUI(actuacion.Tipo),
                    string.IsNullOrEmpty(actuacion.Descripcion) ? "Sin descripción" : actuacion.Descripcion
                );
            }

            return dataSet;
        }
    }
}
