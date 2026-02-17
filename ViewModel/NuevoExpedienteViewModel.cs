using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Model;
using Model.Mappers;
using ViewModel.Command;
using ViewModel.Services;
using ViewModel.Validadores;

namespace ViewModel
{
    /// <summary>
    /// ViewModel para la ventana de creación de nuevo expediente
    /// </summary>
    public class NuevoExpedienteViewModel : INotifyPropertyChanged
    {
        private readonly ExpedienteService _expedienteService;
        private readonly ClienteService _clienteService;
        private Cliente _clienteSeleccionado;
        private string _titulo;
        private string _descripcion;
        private string _jurisdiccion;
        private string _organo;
        private string _errorMessage;
        private string _buscarCliente;

        /// <summary>
        /// Lista de clientes disponibles para el ComboBox
        /// </summary>
        public ObservableCollection<Cliente> Clientes { get; set; }

        /// <summary>
        /// Lista completa de clientes (para filtrado)
        /// </summary>
        private ObservableCollection<Cliente> _todosLosClientes;

        /// <summary>
        /// Cliente seleccionado
        /// </summary>
        public Cliente ClienteSeleccionado
        {
            get => _clienteSeleccionado;
            set
            {
                _clienteSeleccionado = value;
                OnPropertyChanged(nameof(ClienteSeleccionado));
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
        /// Título del expediente
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
        /// Descripción del expediente
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
        /// Jurisdicción del expediente
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
        /// Órgano judicial del expediente
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
        /// Diccionario de jurisdicciones disponibles
        /// </summary>
        public System.Collections.Generic.Dictionary<string, string> JurisdiccionesDisponibles { get; }

        /// <summary>
        /// Comando para crear el expediente
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
        public NuevoExpedienteViewModel()
        {
            _expedienteService = new ExpedienteService();
            _clienteService = new ClienteService();
            Clientes = new ObservableCollection<Cliente>();
            _todosLosClientes = new ObservableCollection<Cliente>();

            JurisdiccionesDisponibles = JurisdiccionMapper.ObtenerTodas();

            // Seleccionar la primera jurisdicción por defecto (civil)
            if (JurisdiccionesDisponibles.Count > 0)
            {
                Jurisdiccion = JurisdiccionesDisponibles.First().Key;
            }

            CrearCommand = new RelayCommand(CrearExpediente);
            CancelarCommand = new RelayCommand(Cancelar);

            CargarClientesAsync();
        }

        /// <summary>
        /// Carga la lista de clientes desde la base de datos
        /// </summary>
        private async void CargarClientesAsync()
        {
            try
            {
                var clientes = await _clienteService.ObtenerClientesAsync();
                
                Clientes.Clear();
                _todosLosClientes.Clear();
                
                foreach (var cliente in clientes)
                {
                    Clientes.Add(cliente);
                    _todosLosClientes.Add(cliente);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al cargar clientes: {ex.Message}";
            }
        }

        /// <summary>
        /// Filtra la lista de clientes según el texto de búsqueda
        /// Permite buscar por: nombre, apellidos, nombre completo (nombre + apellidos), y DNI
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
                        $"{c.Nombre} {c.Apellidos}".ToUpper().Contains(busqueda) ||
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
        /// Crea el nuevo expediente
        /// </summary>
        private async void CrearExpediente()
        {
            if (ValidarFormulario())
            {
                try
                {
                    ErrorMessage = string.Empty;

                    var nuevoExpediente = new Expediente
                    {
                        IdCliente = ClienteSeleccionado.IdCliente,
                        Titulo = Titulo.Trim(),
                        Descripcion = string.IsNullOrWhiteSpace(Descripcion) ? null : Descripcion.Trim(),
                        Jurisdiccion = Jurisdiccion,
                        Organo = string.IsNullOrWhiteSpace(Organo) ? null : Organo.Trim(),
                        Apertura = DateTime.Now,
                        Cierre = null,
                        Estado = "abierto"
                    };

                    await _expedienteService.CrearExpedienteAsync(nuevoExpediente);

                    CerrarVentanaExito?.Invoke();
                }
                catch (ArgumentException ex)
                {
                    ErrorMessage = ex.Message;
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Error al crear expediente: {ex.Message}";
                }
            }
        }

        /// <summary>
        /// Valida el formulario antes de crear el expediente
        /// </summary>
        private bool ValidarFormulario()
        {
            string mensajeError;

            bool esValido = ExpedienteValidador.ValidarNuevoExpediente(
                ClienteSeleccionado,
                Titulo,
                Descripcion,
                Jurisdiccion,
                Organo,
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
