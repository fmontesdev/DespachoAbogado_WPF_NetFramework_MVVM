using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Model;
using ViewModel.Command;
using ViewModel.Services;
using ViewModel.Validadores;

namespace ViewModel
{
    /// <summary>
    /// ViewModel para la vista principal de gestión de clientes (ClientesView).
    /// Maneja la lista de clientes, selección y operaciones CRUD
    /// </summary>
    public class ClienteViewModel : INotifyPropertyChanged
    {
        private readonly ClienteService _clienteService;
        private Cliente _clienteSeleccionado;
        private string _nombre;
        private string _apellidos;
        private string _dni;
        private string _telefono;
        private string _email;
        private string _poblacion;
        private string _direccion;
        private string _errorMessage;
        private string _buscar;
        private List<Cliente> _todosLosClientes;

        /// <summary>
        /// Colección observable de clientes para el DataGrid.
        /// Se actualiza automáticamente en la interfaz de usuario
        /// </summary>
        public ObservableCollection<Cliente> Clientes { get; set; }

        /// <summary>
        /// Cliente seleccionado en el DataGrid.
        /// Al seleccionar un cliente, se cargan sus datos en los TextBox
        /// </summary>
        public Cliente ClienteSeleccionado
        {
            get => _clienteSeleccionado;
            set
            {
                _clienteSeleccionado = value;
                OnPropertyChanged(nameof(ClienteSeleccionado));
                
                // Cargar datos en los TextBox cuando se selecciona un cliente
                if (_clienteSeleccionado != null)
                {
                    Nombre = _clienteSeleccionado.Nombre;
                    Apellidos = _clienteSeleccionado.Apellidos;
                    Dni = _clienteSeleccionado.Dni;
                    Telefono = _clienteSeleccionado.Telefono;
                    Email = _clienteSeleccionado.Email;
                    Poblacion = _clienteSeleccionado.Poblacion;
                    Direccion = _clienteSeleccionado.Direccion;
                }
                else
                {
                    LimpiarFormulario();
                }
            }
        }

        /// <summary>
        /// Nombre del cliente enlazado al TextBox de edición
        /// </summary>
        public string Nombre
        {
            get => _nombre;
            set
            {
                _nombre = value;
                OnPropertyChanged(nameof(Nombre));
            }
        }

        /// <summary>
        /// Apellidos del cliente enlazado al TextBox de edición
        /// </summary>
        public string Apellidos
        {
            get => _apellidos;
            set
            {
                _apellidos = value;
                OnPropertyChanged(nameof(Apellidos));
            }
        }

        /// <summary>
        /// DNI del cliente enlazado al TextBox de edición
        /// </summary>
        public string Dni
        {
            get => _dni;
            set
            {
                _dni = value;
                OnPropertyChanged(nameof(Dni));
            }
        }

        /// <summary>
        /// Teléfono del cliente enlazado al TextBox de edición
        /// </summary>
        public string Telefono
        {
            get => _telefono;
            set
            {
                _telefono = value;
                OnPropertyChanged(nameof(Telefono));
            }
        }

        /// <summary>
        /// Email del cliente enlazado al TextBox de edición
        /// </summary>
        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
            }
        }

        /// <summary>
        /// Población del cliente enlazado al TextBox de edición
        /// </summary>
        public string Poblacion
        {
            get => _poblacion;
            set
            {
                _poblacion = value;
                OnPropertyChanged(nameof(Poblacion));
            }
        }

        /// <summary>
        /// Dirección del cliente enlazado al TextBox de edición
        /// </summary>
        public string Direccion
        {
            get => _direccion;
            set
            {
                _direccion = value;
                OnPropertyChanged(nameof(Direccion));
            }
        }

        /// <summary>
        /// Criterio de búsqueda global para filtrar clientes por nombre, apellidos, DNI o población
        /// </summary>
        public string Buscar
        {
            get => _buscar;
            set
            {
                _buscar = value;
                OnPropertyChanged(nameof(Buscar));
                AplicarFiltros();
            }
        }

        /// <summary>
        /// Total de clientes registrados en la colección
        /// </summary>
        public int TotalClientes => Clientes?.Count ?? 0;

        /// <summary>
        /// Mensaje de error para mostrar al usuario
        /// </summary>
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
                OnPropertyChanged(nameof(HasError));
            }
        }

        /// <summary>
        /// Indica si hay un error activo para mostrar en la interfaz
        /// </summary>
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        /// <summary>
        /// Comando para crear un nuevo cliente (abre ventana modal)
        /// </summary>
        public ICommand NuevoCommand { get; }

        /// <summary>
        /// Comando para editar el cliente seleccionado
        /// </summary>
        public ICommand EditarCommand { get; }

        /// <summary>
        /// Comando para eliminar el cliente seleccionado
        /// </summary>
        public ICommand EliminarCommand { get; }

        /// <summary>
        /// Comando para limpiar los filtros de búsqueda
        /// </summary>
        public ICommand LimpiarBusquedaCommand { get; }

        /// <summary>
        /// Comando para ver los expedientes del cliente seleccionado
        /// </summary>
        public ICommand VerExpedientesCommand { get; }

        /// <summary>
        /// Constructor que inicializa el servicio, comandos y carga los datos iniciales
        /// </summary>
        public ClienteViewModel()
        {
            _clienteService = new ClienteService();
            Clientes = new ObservableCollection<Cliente>();
            _todosLosClientes = new List<Cliente>();

            // Inicializar Commands
            NuevoCommand = new RelayCommand(NuevoCliente);
            EditarCommand = new RelayCommand(EditarCliente);
            EliminarCommand = new RelayCommand(EliminarCliente);
            LimpiarBusquedaCommand = new RelayCommand(LimpiarBusqueda);
            VerExpedientesCommand = new RelayCommand(VerExpedientesCliente);

            // Cargar clientes al inicializar
            InicializarAsync();
        }

        /// <summary>
        /// Inicializa la carga de datos de forma asíncrona y selecciona el primer cliente
        /// </summary>
        public async void InicializarAsync()
        {
            await CargarClientesAsync();
            SeleccionarPrimero();
        }

        /// <summary>
        /// Carga la lista de clientes desde la base de datos y actualiza la colección observable
        /// </summary>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        private async Task CargarClientesAsync()
        {
            try
            {
                ErrorMessage = string.Empty;

                var clientes = await _clienteService.ObtenerClientesAsync();
                _todosLosClientes = clientes;

                Clientes.Clear();
                foreach (var cliente in clientes)
                {
                    Clientes.Add(cliente);
                }

                OnPropertyChanged(nameof(TotalClientes));
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al cargar clientes: {ex.Message}";
            }
        }

        /// <summary>
        /// Selecciona el primer cliente de la lista
        /// </summary>
        private void SeleccionarPrimero()
        {
            if (Clientes.Count > 0)
            {
                ClienteSeleccionado = Clientes[0];
            }
            else
            {
                ClienteSeleccionado = null;
            }
        }

        /// <summary>
        /// Mantiene la selección del cliente en el índice especificado.
        /// Si el índice es inválido, selecciona el primero
        /// </summary>
        /// <param name="indice">Índice del cliente a seleccionar</param>
        private void MantieneSeleccion(int indice)
        {
            if (Clientes.Count == 0)
            {
                ClienteSeleccionado = null;
            }
            else if (indice >= 0 && indice < Clientes.Count)
            {
                ClienteSeleccionado = Clientes[indice];
            }
            else
            {
                SeleccionarPrimero();
            }
        }

        /// <summary>
        /// Selecciona el cliente anterior al índice especificado al eliminar.
        /// Si se eliminó el primero, selecciona el nuevo primero.
        /// Si no quedan clientes, no selecciona ninguno
        /// </summary>
        /// <param name="indiceEliminado">Índice del cliente que fue eliminado</param>
        private void SeleccionaAnterior(int indiceEliminado)
        {
            if (Clientes.Count == 0)
            {
                ClienteSeleccionado = null;
            }
            else if (indiceEliminado == 0)
            {
                // Se eliminó el primero, seleccionar el nuevo primero
                ClienteSeleccionado = Clientes[0];
            }
            else
            {
                // Seleccionar el anterior, pero asegurarse de no exceder los límites
                int nuevoIndice = indiceEliminado - 1;
                if (nuevoIndice >= Clientes.Count)
                {
                    nuevoIndice = Clientes.Count - 1;
                }
                ClienteSeleccionado = Clientes[nuevoIndice];
            }
        }

        /// <summary>
        /// Abre la ventana modal para crear un nuevo cliente
        /// </summary>
        private void NuevoCliente()
        {
            VentanaNuevoCliente?.Invoke();
        }

        /// <summary>
        /// Edita el cliente seleccionado con los datos de los TextBox,
        /// validando los datos y guardando los cambios en la base de datos
        /// </summary>
        private async void EditarCliente()
        {
            // Verifica que haya una fila seleccionada
            if (ClienteSeleccionado == null)
            {                 
                ErrorMessage = "No hay ningún cliente seleccionado para editar";
            }
            else if (ValidarFormulario())
            {
                try
                {
                    ErrorMessage = string.Empty;

                    // Guardar el índice del cliente seleccionado para mantener la selección
                    int indiceClienteActual = Clientes.IndexOf(ClienteSeleccionado);

                    // Actualizar propiedades del cliente seleccionado
                    ClienteSeleccionado.Nombre = Nombre.Trim();
                    ClienteSeleccionado.Apellidos = Apellidos.Trim();
                    ClienteSeleccionado.Dni = Dni.Trim();
                    ClienteSeleccionado.Telefono = Telefono.Trim();
                    ClienteSeleccionado.Email = Email.Trim();
                    ClienteSeleccionado.Poblacion = Poblacion.Trim();
                    ClienteSeleccionado.Direccion = Direccion.Trim();

                    // Guardar en base de datos
                    await _clienteService.ActualizarClienteAsync(ClienteSeleccionado);
                    
                    // Recargar y mantener la selección del cliente editado
                    await CargarClientesAsync();
                    MantieneSeleccion(indiceClienteActual);
                }
                catch (ArgumentException ex)
                {
                    ErrorMessage = $"{ex.Message}";
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Error al editar cliente: {ex.Message}";
                }
            }
        }

        /// <summary>
        /// Solicita confirmación para eliminar el cliente seleccionado
        /// </summary>
        private void EliminarCliente()
        {
            if (ClienteSeleccionado == null)
            {
                ErrorMessage = "No hay ningún cliente seleccionado para eliminar";

            }
            else
            {
                ErrorMessage = string.Empty;
                ConfirmarEliminar?.Invoke((ClienteSeleccionado.IdCliente, ClienteSeleccionado.Nombre, ClienteSeleccionado.Apellidos));
            }
        }

        /// <summary>
        /// Confirma y ejecuta la eliminación del cliente de la base de datos
        /// </summary>
        /// <param name="idCliente">Identificador del cliente a eliminar</param>
        public async void ConfirmarEliminarCliente(int idCliente)
        {
            try
            {
                // Usar el objeto ClienteSeleccionado que ya tenemos disponible
                if (ClienteSeleccionado == null || ClienteSeleccionado.IdCliente != idCliente)
                {
                    ErrorMessage = "El cliente seleccionado no coincide con el cliente a eliminar";
                }
                else
                {
                    // Guardar el índice del cliente que vamos a eliminar
                    int indiceClienteEliminado = Clientes.IndexOf(ClienteSeleccionado);

                    await _clienteService.EliminarClienteAsync(ClienteSeleccionado);

                    // Recargar y seleccionar el cliente anterior
                    await CargarClientesAsync();
                    SeleccionaAnterior(indiceClienteEliminado);
                }
            }
            catch (InvalidOperationException ex)
            {
                // Error de negocio (ej: cliente tiene expedientes o citas)
                ErrorMessage = $"{ex.Message}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al eliminar cliente: {ex.Message}";
            }
        }

        /// <summary>
        /// Valida los datos del formulario de edición de clientes
        /// </summary>
        /// <returns>True si los datos son válidos, false en caso contrario</returns>
        private bool ValidarFormulario()
        {
            string mensajeError;
            
            bool esValido = ClienteValidador.ValidarCliente(
                Nombre,
                Apellidos,
                Dni,
                Telefono,
                Email,
                Poblacion,
                Direccion,
                out mensajeError
            );

            if (!esValido)
            {
                ErrorMessage = mensajeError;
            }

            return esValido;
        }

        /// <summary>
        /// Aplica el filtro de búsqueda global sobre la lista completa de clientes.
        /// Busca coincidencias en: Nombre, Apellidos, DNI y Población
        /// </summary>
        private void AplicarFiltros()
        {
            // Verifica que la lista completa no sea nula
            if (_todosLosClientes != null)
            {
                var clientesFiltrados = _todosLosClientes.AsEnumerable();

                // Filtrar por búsqueda global (nombre, apellidos, DNI o población)
                if (!string.IsNullOrWhiteSpace(Buscar))
                {
                    string busqueda = Buscar.Trim().ToUpper();
                    clientesFiltrados = clientesFiltrados
                        .Where(c =>
                            (c.Nombre != null && c.Nombre.ToUpper().Contains(busqueda)) ||
                            (c.Apellidos != null && c.Apellidos.ToUpper().Contains(busqueda)) ||
                            (c.Dni != null && c.Dni.ToUpper().Contains(busqueda)) ||
                            (c.Poblacion != null && c.Poblacion.ToUpper().Contains(busqueda))
                        )
                        .ToList();
                }

                // Actualizar colección
                Clientes.Clear();
                foreach (var cliente in clientesFiltrados)
                {
                    Clientes.Add(cliente);
                }

                OnPropertyChanged(nameof(TotalClientes));

                // Seleccionar el primer cliente de la lista filtrada
                SeleccionarPrimero();
            }
        }

        /// <summary>
        /// Limpia el filtro de búsqueda y muestra todos los clientes
        /// </summary>
        private void LimpiarBusqueda()
        {
            Buscar = string.Empty;
        }

        /// <summary>
        /// Navega a la vista de Expedientes con el cliente seleccionado como filtro
        /// </summary>
        private void VerExpedientesCliente()
        {
            if (ClienteSeleccionado == null)
            {
                ErrorMessage = "No hay ningún cliente seleccionado";
            }
            else
            {
                ErrorMessage = string.Empty;
                IrAExpedientes?.Invoke(ClienteSeleccionado.NombreCompleto);
            }
        }

        /// <summary>
        /// Limpia los campos del formulario de edición
        /// </summary>
        private void LimpiarFormulario()
        {
            Nombre = string.Empty;
            Apellidos = string.Empty;
            Dni = string.Empty;
            Telefono = string.Empty;
            Email = string.Empty;
            Poblacion = string.Empty;
            Direccion = string.Empty;
        }

        /// <summary>
        /// Acción para comunicar con la Vista y abrir la ventana de nuevo cliente
        /// </summary>
        public Action VentanaNuevoCliente { get; set; }
        
        /// <summary>
        /// Acción para comunicar con la Vista y solicitar confirmación de eliminación
        /// </summary>
        public Action<(int IdCliente, string Nombre, string Apellidos)> ConfirmarEliminar { get; set; }

        /// <summary>
        /// Acción para navegar a la vista de Expedientes con el cliente seleccionado
        /// </summary>
        public Action<string> IrAExpedientes { get; set; }

        /// <summary>
        /// Evento que notifica cambios en las propiedades para actualizar la interfaz
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        
        /// <summary>
        /// Método auxiliar para invocar el evento PropertyChanged
        /// </summary>
        /// <param name="propertyName">Nombre de la propiedad que cambió</param>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
