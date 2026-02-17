using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Model;
using Model.Configuracion;
using Model.Mappers;
using ViewModel.Command;
using ViewModel.Services;
using ViewModel.Validadores;

namespace ViewModel
{
    /// <summary>
    /// ViewModel para la ventana de creación de nueva cita
    /// </summary>
    public class NuevaCitaViewModel : INotifyPropertyChanged
    {
        private readonly CitaService _citaService;
        private readonly ClienteService _clienteService;
        private readonly ExpedienteService _expedienteService;
        private Cliente _clienteSeleccionado;
        private Expediente _expedienteSeleccionado;
        private DateTime? _fecha;
        private string _horario;
        private string _modalidad;
        private string _motivo;
        private string _errorMessage;
        private string _buscarCliente;
        private string _buscarExpediente;

        /// <summary>
        /// Lista de clientes disponibles para el ComboBox
        /// </summary>
        public ObservableCollection<Cliente> Clientes { get; set; }

        /// <summary>
        /// Lista completa de clientes (para filtrado)
        /// </summary>
        private ObservableCollection<Cliente> _todosLosClientes;

        /// <summary>
        /// Lista de expedientes disponibles para el ComboBox (filtrada por cliente)
        /// </summary>
        public ObservableCollection<Expediente> Expedientes { get; set; }

        /// <summary>
        /// Lista completa de expedientes (para filtrado)
        /// </summary>
        private ObservableCollection<Expediente> _todosLosExpedientes;

        /// <summary>
        /// Lista de horarios disponibles para el ComboBox
        /// </summary>
        public ObservableCollection<string> HorariosDisponibles { get; set; }

        /// <summary>
        /// Cliente seleccionado
        /// </summary>
        public Cliente ClienteSeleccionado
        {
            get => _clienteSeleccionado;
            set
            {
                if (_clienteSeleccionado != value)
                {
                    _clienteSeleccionado = value;
                    OnPropertyChanged(nameof(ClienteSeleccionado));

                    // Cuando se selecciona un cliente, filtrar los expedientes de ese cliente
                    if (_clienteSeleccionado != null)
                    {
                        FiltrarExpedientesPorCliente();
                        ActualizarHorariosDisponibles();
                    }
                }
            }
        }

        /// <summary>
        /// Expediente seleccionado (opcional)
        /// </summary>
        public Expediente ExpedienteSeleccionado
        {
            get => _expedienteSeleccionado;
            set
            {
                if (_expedienteSeleccionado != value)
                {
                    _expedienteSeleccionado = value;
                    OnPropertyChanged(nameof(ExpedienteSeleccionado));
                }
            }
        }

        /// <summary>
        /// Indica si no hay expedientes para el cliente seleccionado
        /// </summary>
        public bool NoHayExpedientes => Expedientes.Count == 0 && ClienteSeleccionado != null;

        /// <summary>
        /// Indica si el ComboBox de expedientes está habilitado
        /// Solo si hay cliente seleccionado Y tiene expedientes
        /// </summary>
        public bool ExpedientesHabilitado => ClienteSeleccionado != null && !NoHayExpedientes;
        
        /// <summary>
        /// Mensaje placeholder para el ComboBox de expedientes
        /// </summary>
        public string PlaceholderExpediente
        {
            get
            {
                if (ClienteSeleccionado == null || NoHayExpedientes)
                {
                    return "Sin expedientes";
                }
                    
                return "Expediente (opcional)";
            }
        }

        /// <summary>
        /// Texto de búsqueda del cliente en el ComboBox
        /// </summary>
        public string BuscarCliente
        {
            get => _buscarCliente;
            set
            {
                _buscarCliente = value;
                OnPropertyChanged(nameof(BuscarCliente));
                FiltrarClientes();
            }
        }

        /// <summary>
        /// Texto de búsqueda del expediente en el ComboBox
        /// </summary>
        public string BuscarExpediente
        {
            get => _buscarExpediente;
            set
            {
                _buscarExpediente = value;
                OnPropertyChanged(nameof(BuscarExpediente));
                FiltrarExpedientes();
            }
        }

        /// <summary>
        /// Fecha de la cita (por defecto: hoy)
        /// </summary>
        public DateTime? Fecha
        {
            get => _fecha;
            set
            {
                _fecha = value;
                OnPropertyChanged(nameof(Fecha));
                ActualizarHorariosDisponibles();
            }
        }

        /// <summary>
        /// Horario seleccionado
        /// </summary>
        public string Horario
        {
            get => _horario;
            set
            {
                _horario = value;
                OnPropertyChanged(nameof(Horario));
            }
        }

        /// <summary>
        /// Modalidad de la cita
        /// </summary>
        public string Modalidad
        {
            get => _modalidad;
            set
            {
                _modalidad = value;
                OnPropertyChanged(nameof(Modalidad));
            }
        }

        /// <summary>
        /// Motivo de la cita
        /// </summary>
        public string Motivo
        {
            get => _motivo;
            set
            {
                _motivo = value;
                OnPropertyChanged(nameof(Motivo));
            }
        }

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
        /// Indica si hay un error activo
        /// </summary>
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        /// <summary>
        /// Diccionario de modalidades disponibles
        /// </summary>
        public System.Collections.Generic.Dictionary<string, string> ModalidadesDisponibles { get; }

        /// <summary>
        /// Comando para crear la cita
        /// </summary>
        public ICommand CrearCommand { get; }

        /// <summary>
        /// Comando para cancelar
        /// </summary>
        public ICommand CancelarCommand { get; }

        /// <summary>
        /// Acción para cerrar la ventana con éxito (DialogResult = true)
        /// </summary>
        public Action CerrarVentanaExito { get; set; }

        /// <summary>
        /// Acción para cerrar la ventana cancelando (DialogResult = false)
        /// </summary>
        public Action CerrarVentanaCancelar { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public NuevaCitaViewModel()
        {
            _citaService = new CitaService();
            _clienteService = new ClienteService();
            _expedienteService = new ExpedienteService();
            Clientes = new ObservableCollection<Cliente>();
            _todosLosClientes = new ObservableCollection<Cliente>();
            Expedientes = new ObservableCollection<Expediente>();
            _todosLosExpedientes = new ObservableCollection<Expediente>();
            HorariosDisponibles = new ObservableCollection<string>();

            ModalidadesDisponibles = ModalidadCitaMapper.ObtenerTodas();

            // Seleccionar la primera modalidad por defecto (presencial)
            if (ModalidadesDisponibles.Count > 0)
            {
                Modalidad = ModalidadesDisponibles.First().Key;
            }

            // Establecer la fecha por defecto como hoy
            Fecha = DateTime.Now;

            CrearCommand = new RelayCommand(CrearCita);
            CancelarCommand = new RelayCommand(Cancelar);

            CargarClientesAsync();
            CargarExpedientesAsync();
        }

        /// <summary>
        /// Carga la lista de clientes desde la base de datos
        /// </summary>
        private async void CargarClientesAsync()
        {
            try
            {
                var clientes = await _clienteService.ObtenerClientesAsync();
                
                _todosLosClientes.Clear();
                Clientes.Clear();
                
                foreach (var cliente in clientes)
                {
                    _todosLosClientes.Add(cliente);
                    Clientes.Add(cliente);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al cargar clientes: {ex.Message}";
            }
        }

        /// <summary>
        /// Carga la lista de expedientes NO cerrados desde la base de datos
        /// NO los muestra inicialmente - se cargan cuando se selecciona un cliente
        /// </summary>
        private async void CargarExpedientesAsync()
        {
            try
            {
                var expedientes = await _expedienteService.ObtenerExpedientesAsync();
                
                _todosLosExpedientes.Clear();
                
                // Filtrar y guardar solo expedientes NO cerrados
                foreach (var expediente in expedientes.Where(e => e.Estado?.ToLower() != "cerrado"))
                {
                    _todosLosExpedientes.Add(expediente);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al cargar expedientes: {ex.Message}";
            }
        }

        /// <summary>
        /// Actualiza los horarios disponibles según la fecha seleccionada
        /// </summary>
        private async void ActualizarHorariosDisponibles()
        {
            if (Fecha.HasValue)
            {
                try
                {
                    var horarios = await _citaService.ObtenerHorariosDisponiblesAsync(Fecha.Value);
                    
                    HorariosDisponibles.Clear();
                    foreach (var horario in horarios)
                    {
                        HorariosDisponibles.Add(horario);
                    }

                    // Si el horario actual ya no está disponible, limpiarlo
                    if (!string.IsNullOrEmpty(Horario) && !HorariosDisponibles.Contains(Horario))
                    {
                        Horario = null;
                    }

                    // Si no hay horario seleccionado y hay horarios disponibles, seleccionar el primero
                    if (string.IsNullOrEmpty(Horario) && HorariosDisponibles.Count > 0)
                    {
                        Horario = HorariosDisponibles[0];
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Error al cargar horarios: {ex.Message}";
                }
            }
        }

        /// <summary>
        /// Filtra la lista de clientes según el texto de búsqueda
        /// Permite buscar por: nombre, apellidos, nombre completo, y DNI
        /// </summary>
        private void FiltrarClientes()
        {
            if (string.IsNullOrWhiteSpace(BuscarCliente))
            {
                Clientes.Clear();
                foreach (var cliente in _todosLosClientes)
                {
                    Clientes.Add(cliente);
                }
            }
            else
            {
                string busqueda = BuscarCliente.Trim().ToUpper();
                var clientesFiltrados = _todosLosClientes
                    .Where(c => 
                        (c.Nombre != null && c.Nombre.ToUpper().Contains(busqueda)) ||
                        (c.Apellidos != null && c.Apellidos.ToUpper().Contains(busqueda)) ||
                        (c.NombreCompleto != null && c.NombreCompleto.ToUpper().Contains(busqueda)) ||
                        (c.Dni != null && c.Dni.ToUpper().Contains(busqueda)))
                    .ToList();

                Clientes.Clear();
                foreach (var cliente in clientesFiltrados)
                {
                    Clientes.Add(cliente);
                }
            }
        }

        /// <summary>
        /// Filtra y carga los expedientes del cliente seleccionado
        /// </summary>
        private void FiltrarExpedientesPorCliente()
        {
            Expedientes.Clear();
            ExpedienteSeleccionado = null;
            
            // Cargar expedientes del cliente seleccionado
            foreach (var expediente in _todosLosExpedientes.Where(e => e.IdCliente == ClienteSeleccionado.IdCliente))
            {
                Expedientes.Add(expediente);
            }

            // Notificar cambios en propiedades calculadas
            OnPropertyChanged(nameof(NoHayExpedientes));
            OnPropertyChanged(nameof(ExpedientesHabilitado));
            OnPropertyChanged(nameof(PlaceholderExpediente));
        }

        /// <summary>
        /// Filtra la lista de expedientes NO cerrados según el texto de búsqueda (solo por código)
        /// Solo funciona si hay un cliente seleccionado
        /// </summary>
        private void FiltrarExpedientes()
        {
            if (ClienteSeleccionado == null)
            {
                // Si no hay cliente, no hacer nada
                return;
            }

            if (string.IsNullOrWhiteSpace(BuscarExpediente))
            {
                FiltrarExpedientesPorCliente();
            }
            else
            {
                string busqueda = BuscarExpediente.Trim().ToUpper();
                var expedientesFiltrados = _todosLosExpedientes
                    .Where(e => 
                        e.IdCliente == ClienteSeleccionado.IdCliente &&
                        e.Codigo != null && e.Codigo.ToUpper().Contains(busqueda))
                    .ToList();

                Expedientes.Clear();
                foreach (var expediente in expedientesFiltrados)
                {
                    Expedientes.Add(expediente);
                }
            }
        }

        /// <summary>
        /// Crea la nueva cita
        /// </summary>
        private async void CrearCita()
        {
            if (ValidarFormulario())
            {
                try
                {
                    ErrorMessage = string.Empty;

                    var nuevaCita = new Cita
                    {
                        IdCliente = ClienteSeleccionado.IdCliente,
                        IdExpediente = ExpedienteSeleccionado?.IdExpediente,
                        Fecha = Fecha.Value,
                        Horario = Horario,
                        Modalidad = Modalidad,
                        Motivo = string.IsNullOrWhiteSpace(Motivo) ? null : Motivo.Trim(),
                        Estado = "programada"
                    };

                    await _citaService.CrearCitaAsync(nuevaCita);

                    CerrarVentanaExito?.Invoke();
                }
                catch (ArgumentException ex)
                {
                    ErrorMessage = ex.Message;
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Error al crear cita: {ex.Message}";
                }
            }
        }

        /// <summary>
        /// Valida el formulario antes de crear la cita
        /// </summary>
        private bool ValidarFormulario()
        {
            string mensajeError;

            bool esValido = CitaValidador.ValidarNuevaCita(
                ClienteSeleccionado,
                Fecha,
                Horario,
                Modalidad,
                Motivo,
                out mensajeError
            );

            if (!esValido)
            {
                ErrorMessage = mensajeError;
            }

            return esValido;
        }

        /// <summary>
        /// Cancela y cierra la ventana
        /// </summary>
        private void Cancelar()
        {
            CerrarVentanaCancelar?.Invoke();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
