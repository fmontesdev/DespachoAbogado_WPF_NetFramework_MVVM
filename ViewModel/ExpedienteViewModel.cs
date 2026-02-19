using Model;
using Model.Mappers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ViewModel.Command;
using ViewModel.Services;
using ViewModel.Validadores;

namespace ViewModel
{
    /// <summary>
    /// ViewModel para la vista principal de gestión de expedientes (ExpedientesView).
    /// Maneja la lista de expedientes, selección y operaciones CRUD
    /// </summary>
    public class ExpedienteViewModel : INotifyPropertyChanged
    {
        private readonly ExpedienteService _expedienteService;
        private readonly ClienteService _clienteService;
        private Expediente _expedienteSeleccionado;
        private string _nombreCompletoCliente;
        private string _titulo;
        private string _descripcion;
        private string _jurisdiccion;
        private string _organo;
        private DateTime? _cierre;
        private string _estado;
        private string _errorMessage;
        private string _buscar;
        private List<Expediente> _todosLosExpedientes;
        private bool _inicializado = false;
        private string _filtroClientePendiente = null;

        /// <summary>
        /// Colección observable de expedientes para el DataGrid.
        /// Se actualiza automáticamente en la interfaz de usuario
        /// </summary>
        public ObservableCollection<Expediente> Expedientes { get; set; }

        /// <summary>
        /// Lista de clientes disponibles para el ComboBox
        /// </summary>
        public ObservableCollection<Cliente> Clientes { get; set; }

        /// <summary>
        /// Cliente seleccionado (para crear nuevo expediente)
        /// </summary>
        public Cliente ClienteSeleccionado { get; set; }

        /// <summary>
        /// Expediente seleccionado en el DataGrid.
        /// Al seleccionar un expediente, se cargan sus datos en los campos
        /// </summary>
        public Expediente ExpedienteSeleccionado
        {
            get => _expedienteSeleccionado;
            set
            {
                _expedienteSeleccionado = value;
                OnPropertyChanged(nameof(ExpedienteSeleccionado));

                // Cargar datos en los campos cuando se selecciona un expediente
                if (_expedienteSeleccionado != null)
                {
                    NombreCompletoCliente = _expedienteSeleccionado.Cliente.NombreCompleto;
                    Titulo = _expedienteSeleccionado.Titulo;
                    Descripcion = _expedienteSeleccionado.Descripcion;
                    Jurisdiccion = _expedienteSeleccionado.Jurisdiccion;
                    Organo = _expedienteSeleccionado.Organo;
                    Cierre = _expedienteSeleccionado.Cierre;
                    Estado = _expedienteSeleccionado.Estado;
                    ClienteSeleccionado = _expedienteSeleccionado.Cliente;
                }
                else
                {
                    LimpiarFormulario();
                }
            }
        }

        /// <summary>
        /// Nombre completo del cliente del expediente seleccionado
        /// </summary>
        public string NombreCompletoCliente
        {
            get => _nombreCompletoCliente;
            set
            {
                _nombreCompletoCliente = value;
                OnPropertyChanged(nameof(NombreCompletoCliente));
            }
        }

        /// <summary>
        /// Título del expediente enlazado al TextBox de edición
        /// </summary>
        public string Titulo
        {
            get => _titulo;
            set
            {
                _titulo = value;
                OnPropertyChanged(nameof(Titulo));
            }
        }

        /// <summary>
        /// Descripción del expediente enlazado al TextBox de edición
        /// </summary>
        public string Descripcion
        {
            get => _descripcion;
            set
            {
                _descripcion = value;
                OnPropertyChanged(nameof(Descripcion));
            }
        }

        /// <summary>
        /// Jurisdicción del expediente enlazado al ComboBox de edición
        /// </summary>
        public string Jurisdiccion
        {
            get => _jurisdiccion;
            set
            {
                _jurisdiccion = value;
                OnPropertyChanged(nameof(Jurisdiccion));
            }
        }

        /// <summary>
        /// Órgano judicial del expediente enlazado al TextBox de edición
        /// </summary>
        public string Organo
        {
            get => _organo;
            set
            {
                _organo = value;
                OnPropertyChanged(nameof(Organo));
            }
        }

        /// <summary>
        /// Fecha de cierre del expediente enlazado al DatePicker de edición
        /// </summary>
        public DateTime? Cierre
        {
            get => _cierre;
            set
            {
                _cierre = value;
                OnPropertyChanged(nameof(Cierre));
            }
        }

        /// <summary>
        /// Estado del expediente enlazado al ComboBox de edición
        /// </summary>
        public string Estado
        {
            get => _estado;
            set
            {
                _estado = value;
                OnPropertyChanged(nameof(Estado));
            }
        }

        /// <summary>
        /// Criterio de búsqueda global para filtrar expedientes
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
        /// Total de expedientes registrados en la colección
        /// </summary>
        public int TotalExpedientes => Expedientes?.Count ?? 0;

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
        /// Diccionario de jurisdicciones disponibles para el ComboBox. Convierte enums de BD a texto amigable para la UI
        /// </summary>
        public Dictionary<string, string> JurisdiccionesDisponibles { get; }

        /// <summary>
        /// Diccionario de estados disponibles para el ComboBox. Convierte enums de BD a texto amigable para la UI
        /// </summary>
        public Dictionary<string, string> EstadosDisponibles { get; }

        /// <summary>
        /// Comando para crear un nuevo expediente
        /// </summary>
        public ICommand NuevoCommand { get; }

        /// <summary>
        /// Comando para editar el expediente seleccionado
        /// </summary>
        public ICommand EditarCommand { get; }

        /// <summary>
        /// Comando para eliminar el expediente seleccionado
        /// </summary>
        public ICommand EliminarCommand { get; }

        /// <summary>
        /// Comando para limpiar los filtros de búsqueda
        /// </summary>
        public ICommand LimpiarBusquedaCommand { get; }

        /// <summary>
        /// Comando para ver las actuaciones del expediente seleccionado
        /// </summary>
        public ICommand VerActuacionesCommand { get; }

        /// <summary>
        /// Constructor que inicializa los servicios, comandos y carga los datos iniciales
        /// </summary>
        public ExpedienteViewModel()
        {
            _expedienteService = new ExpedienteService();
            _clienteService = new ClienteService();
            Expedientes = new ObservableCollection<Expediente>();
            Clientes = new ObservableCollection<Cliente>();
            _todosLosExpedientes = new List<Expediente>();

            // Inicializar diccionarios para ComboBox
            JurisdiccionesDisponibles = JurisdiccionMapper.ObtenerTodas();
            EstadosDisponibles = EstadoExpedienteMapper.ObtenerTodos();

            // Inicializar Commands
            NuevoCommand = new RelayCommand(NuevoExpediente);
            EditarCommand = new RelayCommand(EditarExpediente);
            EliminarCommand = new RelayCommand(EliminarExpediente);
            LimpiarBusquedaCommand = new RelayCommand(LimpiarBusqueda);
            VerActuacionesCommand = new RelayCommand(VerActuacionesExpediente);

            // Cargar expedientes y clientes al inicializar
            InicializarAsync();
        }

        /// <summary>
        /// Inicializa la carga de datos de forma asíncrona y selecciona el primer expediente
        /// </summary>
        public async void InicializarAsync()
        {
            await CargarExpedientesAsync();

            // Cargar clientes solo la primera vez (para evitar recargas innecesarias)
            if (!_inicializado)
            {
                await CargarClientesAsync();
                _inicializado = true;
            }

            // Si hay un filtro pendiente, aplicarlo ahora
            if (!string.IsNullOrEmpty(_filtroClientePendiente))
            {
                Buscar = _filtroClientePendiente;
                _filtroClientePendiente = null;
            }
            else
            {
                SeleccionarPrimero();
            }
        }

        /// <summary>
        /// Carga la lista de expedientes desde la base de datos y actualiza la colección observable
        /// </summary>
        private async Task CargarExpedientesAsync()
        {
            try
            {
                ErrorMessage = string.Empty;

                var expedientes = await _expedienteService.ObtenerExpedientesAsync();
                _todosLosExpedientes = expedientes;

                Expedientes.Clear();
                foreach (var expediente in expedientes)
                {
                    Expedientes.Add(expediente);
                }

                OnPropertyChanged(nameof(TotalExpedientes));
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al cargar expedientes: {ex.Message}";
            }
        }

        /// <summary>
        /// Carga la lista de clientes desde la base de datos
        /// </summary>
        private async Task CargarClientesAsync()
        {
            try
            {
                var clientes = await _clienteService.ObtenerClientesAsync();

                Clientes.Clear();
                foreach (var cliente in clientes)
                {
                    Clientes.Add(cliente);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al cargar clientes: {ex.Message}";
            }
        }

        /// <summary>
        /// Selecciona el primer expediente de la lista
        /// </summary>
        private void SeleccionarPrimero()
        {
            if (Expedientes.Count > 0)
            {
                ExpedienteSeleccionado = Expedientes[0];
            }
            else
            {
                ExpedienteSeleccionado = null;
            }
        }

        /// <summary>
        /// Mantiene la selección del expediente en el índice especificado.
        /// Si el índice es inválido, selecciona el primero
        /// </summary>
        private void MantieneSeleccion(int indice)
        {
            if (Expedientes.Count == 0)
            {
                ExpedienteSeleccionado = null;
            }
            else if (indice >= 0 && indice < Expedientes.Count)
            {
                ExpedienteSeleccionado = Expedientes[indice];
            }
            else
            {
                SeleccionarPrimero();
            }
        }

        /// <summary>
        /// Selecciona el expediente anterior al índice especificado al eliminar
        /// </summary>
        private void SeleccionaAnterior(int indiceEliminado)
        {
            if (Expedientes.Count == 0)
            {
                ExpedienteSeleccionado = null;
            }
            else if (indiceEliminado == 0)
            {
                ExpedienteSeleccionado = Expedientes[0];
            }
            else
            {
                int nuevoIndice = indiceEliminado - 1;
                if (nuevoIndice >= Expedientes.Count)
                {
                    nuevoIndice = Expedientes.Count - 1;
                }
                ExpedienteSeleccionado = Expedientes[nuevoIndice];
            }
        }

        /// <summary>
        /// Abre la ventana modal para crear un nuevo expediente
        /// </summary>
        private void NuevoExpediente()
        {
            VentanaNuevoExpediente?.Invoke();
        }

        /// <summary>
        /// Edita el expediente seleccionado con los datos de los campos,
        /// validando los datos y guardando los cambios en la base de datos
        /// </summary>
        private async void EditarExpediente()
        {
            if (ExpedienteSeleccionado == null)
            {
                ErrorMessage = "No hay ningún expediente seleccionado para editar";
            }
            else if (ValidarFormulario())
            {
                try
                {
                    ErrorMessage = string.Empty;

                    int indiceExpedienteActual = Expedientes.IndexOf(ExpedienteSeleccionado);

                    // Actualizar propiedades del expediente seleccionado
                    ExpedienteSeleccionado.Titulo = Titulo.Trim();
                    ExpedienteSeleccionado.Descripcion = string.IsNullOrWhiteSpace(Descripcion) ? null : Descripcion.Trim();
                    ExpedienteSeleccionado.Jurisdiccion = Jurisdiccion;
                    ExpedienteSeleccionado.Organo = string.IsNullOrWhiteSpace(Organo) ? null : Organo.Trim();
                    ExpedienteSeleccionado.Estado = Estado;

                    // Asignar fecha de cierre automáticamente según el estado seleccionado
                    if (Estado == "cerrado" && Cierre == null)
                    {
                        ExpedienteSeleccionado.Cierre = DateTime.Now;
                    }
                    else if (Estado != "cerrado")
                    {
                        ExpedienteSeleccionado.Cierre = null;
                    }

                    await _expedienteService.ActualizarExpedienteAsync(ExpedienteSeleccionado);

                    await CargarExpedientesAsync();
                    MantieneSeleccion(indiceExpedienteActual);
                }
                catch (ArgumentException ex)
                {
                    ErrorMessage = $"{ex.Message}";
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Error al editar expediente: {ex.Message}";
                }
            }
        }

        /// <summary>
        /// Solicita confirmación para eliminar el expediente seleccionado
        /// </summary>
        private void EliminarExpediente()
        {
            if (ExpedienteSeleccionado == null)
            {
                ErrorMessage = "No hay ningún expediente seleccionado para eliminar";

            }
            else
            {
                ErrorMessage = string.Empty;
                ConfirmarEliminar?.Invoke((ExpedienteSeleccionado.IdExpediente, ExpedienteSeleccionado.Codigo));
            }
        }

        /// <summary>
        /// Confirma y ejecuta la eliminación del expediente de la base de datos
        /// </summary>
        public async void ConfirmarEliminarExpediente(int idExpediente)
        {
            try
            {
                if (ExpedienteSeleccionado == null || ExpedienteSeleccionado.IdExpediente != idExpediente)
                {
                    ErrorMessage = "El expediente seleccionado no coincide con el expediente a eliminar";
                }
                else
                {
                    int indiceExpedienteEliminado = Expedientes.IndexOf(ExpedienteSeleccionado);

                    await _expedienteService.EliminarExpedienteAsync(ExpedienteSeleccionado);

                    await CargarExpedientesAsync();
                    SeleccionaAnterior(indiceExpedienteEliminado);
                }
            }
            catch (InvalidOperationException ex)
            {
                ErrorMessage = $"{ex.Message}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al eliminar expediente: {ex.Message}";
            }
        }

        /// <summary>
        /// Valida los datos del formulario de edición de expedientes
        /// </summary>
        private bool ValidarFormulario()
        {
            string mensajeError;

            bool esValido = ExpedienteValidador.ValidarExpediente(
                Titulo,
                Descripcion,
                Jurisdiccion,
                Organo,
                Cierre,
                Estado,
                out mensajeError
            );

            if (!esValido)
            {
                ErrorMessage = mensajeError;
            }

            return esValido;
        }

        /// <summary>
        /// Aplica el filtro de búsqueda global sobre la lista completa de expedientes
        /// </summary>
        private void AplicarFiltros()
        {
            if (_todosLosExpedientes != null)
            {
                var expedientesFiltrados = _todosLosExpedientes.AsEnumerable();

                if (!string.IsNullOrWhiteSpace(Buscar))
                {
                    string busqueda = Buscar.Trim().ToUpper();
                    expedientesFiltrados = expedientesFiltrados
                        .Where(e =>
                            (e.Codigo != null && e.Codigo.ToUpper().Contains(busqueda)) ||
                            (e.Cliente != null && e.Cliente.Nombre != null && e.Cliente.Nombre.ToUpper().Contains(busqueda)) ||
                            (e.Cliente != null && e.Cliente.Apellidos != null && e.Cliente.Apellidos.ToUpper().Contains(busqueda)) ||
                            (e.Cliente != null && e.Cliente.NombreCompleto != null && e.Cliente.NombreCompleto.ToUpper().Contains(busqueda)) ||
                            (e.Titulo != null && e.Titulo.ToUpper().Contains(busqueda)) ||
                            (e.Jurisdiccion != null && JurisdiccionMapper.DeBDaUI(e.Jurisdiccion).ToUpper().Contains(busqueda)) ||
                            (e.Estado != null && EstadoExpedienteMapper.DeBDaUI(e.Estado).ToUpper().Contains(busqueda))
                        )
                        .ToList();
                }

                Expedientes.Clear();
                foreach (var expediente in expedientesFiltrados)
                {
                    Expedientes.Add(expediente);
                }

                OnPropertyChanged(nameof(TotalExpedientes));
                SeleccionarPrimero();
            }
        }

        /// <summary>
        /// Limpia el filtro de búsqueda y muestra todos los expedientes
        /// </summary>
        private void LimpiarBusqueda()
        {
            Buscar = string.Empty;
        }

        /// <summary>
        /// Navega a la vista de Actuaciones con el expediente seleccionado como filtro
        /// </summary>
        private void VerActuacionesExpediente()
        {
            if (ExpedienteSeleccionado == null)
            {
                ErrorMessage = "No hay ningún expediente seleccionado";
            }
            else
            {
                ErrorMessage = string.Empty;
                IrAActuaciones?.Invoke(ExpedienteSeleccionado.Codigo);
            }
        }

        /// <summary>
        /// Aplica un filtro para mostrar solo los expedientes del cliente especificado
        /// </summary>
        /// <param name="nombreCompletoCliente">Nombre completo del cliente por el cual filtrar</param>
        public void FiltrarPorCliente(string nombreCompletoCliente)
        {
            if (!string.IsNullOrEmpty(nombreCompletoCliente))
            {
                if (_inicializado)
                {
                    // Ya está inicializado, aplicar filtro inmediatamente
                    Buscar = nombreCompletoCliente;
                }
                else
                {
                    // Todavía no está inicializado, guardar para aplicar después
                    _filtroClientePendiente = nombreCompletoCliente;
                }
            }
        }

        /// <summary>
        /// Limpia los campos del formulario de edición
        /// </summary>
        private void LimpiarFormulario()
        {
            Titulo = string.Empty;
            Descripcion = string.Empty;
            Jurisdiccion = null;
            Organo = string.Empty;
            Cierre = null;
            Estado = null;
            ClienteSeleccionado = null;
        }

        /// <summary>
        /// Acción para comunicar con la Vista y abrir la ventana de nuevo expediente
        /// </summary>
        public Action VentanaNuevoExpediente { get; set; }

        /// <summary>
        /// Acción para comunicar con la Vista y solicitar confirmación de eliminación
        /// </summary>
        public Action<(int IdExpediente, string Codigo)> ConfirmarEliminar { get; set; }

        /// <summary>
        /// Acción para navegar a la vista de Actuaciones con el expediente seleccionado
        /// </summary>
        public Action<string> IrAActuaciones { get; set; }

        /// <summary>
        /// Evento que notifica cambios en las propiedades para actualizar la interfaz
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Método auxiliar para invocar el evento PropertyChanged
        /// </summary>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
