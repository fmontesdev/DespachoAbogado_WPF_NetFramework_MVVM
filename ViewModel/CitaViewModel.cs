using Model;
using Model.Mappers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ViewModel.Command;
using ViewModel.Services;
using ViewModel.Validadores;

namespace ViewModel
{
    /// <summary>
    /// ViewModel para la vista principal de gestión de citas (CitasView).
    /// Maneja la lista de citas, selección y operaciones CRUD
    /// </summary>
    public class CitaViewModel : INotifyPropertyChanged
    {
        private readonly CitaService _citaService;
        private readonly ClienteService _clienteService;
        private readonly ExpedienteService _expedienteService;
        private Cita _citaSeleccionada;
        private string _nombreCompletoCliente;
        private string _codigoExpediente;
        private DateTime? _fecha;
        private string _horario;
        private string _modalidad;
        private string _motivo;
        private string _estado;
        private string _errorMessage;
        private string _buscar;
        private List<Cita> _todasLasCitas;

        /// <summary>
        /// Colección observable de citas para el DataGrid.
        /// Se actualiza automáticamente en la interfaz de usuario
        /// </summary>
        public ObservableCollection<Cita> Citas { get; set; }

        /// <summary>
        /// Lista de clientes disponibles para el ComboBox
        /// </summary>
        public ObservableCollection<Cliente> Clientes { get; set; }

        /// <summary>
        /// Lista de expedientes disponibles para el ComboBox
        /// </summary>
        public ObservableCollection<Expediente> Expedientes { get; set; }

        /// <summary>
        /// Cliente seleccionado (para crear nueva cita)
        /// </summary>
        public Cliente ClienteSeleccionado { get; set; }

        /// <summary>
        /// Expediente seleccionado (opcional para crear nueva cita)
        /// </summary>
        public Expediente ExpedienteSeleccionado { get; set; }

        /// <summary>
        /// Cita seleccionada en el DataGrid.
        /// Al seleccionar una cita, se cargan sus datos en los campos
        /// </summary>
        public Cita CitaSeleccionada
        {
            get => _citaSeleccionada;
            set
            {
                _citaSeleccionada = value;
                OnPropertyChanged(nameof(CitaSeleccionada));

                // Cargar datos en los campos cuando se selecciona una cita
                if (_citaSeleccionada != null)
                {
                    NombreCompletoCliente = _citaSeleccionada.Cliente?.NombreCompleto ?? string.Empty;
                    CodigoExpediente = _citaSeleccionada.Expediente?.Codigo ?? "Sin expediente";
                    Fecha = _citaSeleccionada.Fecha;
                    // No asignar Horario aquí, se asignará después de cargar los horarios disponibles
                    Modalidad = _citaSeleccionada.Modalidad;
                    Motivo = _citaSeleccionada.Motivo;
                    Estado = _citaSeleccionada.Estado;
                    ClienteSeleccionado = _citaSeleccionada.Cliente;
                    ExpedienteSeleccionado = _citaSeleccionada.Expediente;
                }
                else
                {
                    LimpiarFormulario();
                }
            }
        }

        /// <summary>
        /// Nombre completo del cliente de la cita seleccionada
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
        /// Código del expediente de la cita seleccionada
        /// </summary>
        public string CodigoExpediente
        {
            get => _codigoExpediente;
            set
            {
                _codigoExpediente = value;
                OnPropertyChanged(nameof(CodigoExpediente));
            }
        }

        /// <summary>
        /// Fecha de la cita enlazada al DatePicker de edición
        /// </summary>
        public DateTime? Fecha
        {
            get => _fecha;
            set
            {
                _fecha = value;
                OnPropertyChanged(nameof(Fecha));
                // Actualizar horarios disponibles cuando cambia la fecha
                if (_fecha.HasValue)
                {
                    // Llamar al método async sin await (fire-and-forget controlado)
                    var _ = CargarHorariosDisponiblesAsync();
                }
            }
        }

        /// <summary>
        /// Horario de la cita enlazado al ComboBox de edición
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
        /// Modalidad de la cita enlazada al ComboBox de edición
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
        /// Motivo de la cita enlazado al TextBox de edición
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
        /// Estado de la cita enlazado al ComboBox de edición
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
        /// Criterio de búsqueda global para filtrar citas
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
        /// Total de citas registradas en la colección
        /// </summary>
        public int TotalCitas => Citas?.Count ?? 0;

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
        /// Lista de horarios disponibles para el ComboBox
        /// </summary>
        public ObservableCollection<string> HorariosDisponibles { get; set; }

        /// <summary>
        /// Diccionario de modalidades disponibles para el ComboBox. Convierte enums de BD a texto amigable para la UI
        /// </summary>
        public Dictionary<string, string> ModalidadesDisponibles { get; }

        /// <summary>
        /// Diccionario de estados disponibles para el ComboBox. Convierte enums de BD a texto amigable para la UI
        /// </summary>
        public Dictionary<string, string> EstadosDisponibles { get; }

        /// <summary>
        /// Comando para crear una nueva cita
        /// </summary>
        public ICommand NuevoCommand { get; }

        /// <summary>
        /// Comando para editar la cita seleccionada
        /// </summary>
        public ICommand EditarCommand { get; }

        /// <summary>
        /// Comando para eliminar la cita seleccionada
        /// </summary>
        public ICommand EliminarCommand { get; }

        /// <summary>
        /// Comando para limpiar los filtros de búsqueda
        /// </summary>
        public ICommand LimpiarBusquedaCommand { get; }

        /// <summary>
        /// Constructor que inicializa los servicios, comandos y carga los datos iniciales
        /// </summary>
        public CitaViewModel()
        {
            _citaService = new CitaService();
            _clienteService = new ClienteService();
            _expedienteService = new ExpedienteService();
            Citas = new ObservableCollection<Cita>();
            Clientes = new ObservableCollection<Cliente>();
            Expedientes = new ObservableCollection<Expediente>();
            _todasLasCitas = new List<Cita>();
            HorariosDisponibles = new ObservableCollection<string>();

            // Inicializar diccionarios para ComboBox
            ModalidadesDisponibles = ModalidadCitaMapper.ObtenerTodas();
            EstadosDisponibles = EstadoCitaMapper.ObtenerTodos();

            // Inicializar Commands
            NuevoCommand = new RelayCommand(NuevaCita);
            EditarCommand = new RelayCommand(EditarCita);
            EliminarCommand = new RelayCommand(EliminarCita);
            LimpiarBusquedaCommand = new RelayCommand(LimpiarBusqueda);

            // Cargar citas, clientes y expedientes al inicializar
            InicializarAsync();
        }

        /// <summary>
        /// Inicializa la carga de datos de forma asíncrona y selecciona la primera cita
        /// </summary>
        public async void InicializarAsync()
        {
            await CargarCitasAsync();
            await CargarClientesAsync();
            await CargarExpedientesAsync();
            SeleccionarPrimero();
        }

        /// <summary>
        /// Carga la lista de citas desde la base de datos y actualiza la colección observable
        /// </summary>
        private async Task CargarCitasAsync()
        {
            try
            {
                ErrorMessage = string.Empty;

                var citas = await _citaService.ObtenerCitasAsync();
                _todasLasCitas = citas;

                Citas.Clear();
                foreach (var cita in citas)
                {
                    Citas.Add(cita);
                }

                OnPropertyChanged(nameof(TotalCitas));
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al cargar citas: {ex.Message}";
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
        /// Carga la lista de expedientes desde la base de datos
        /// </summary>
        private async Task CargarExpedientesAsync()
        {
            try
            {
                var expedientes = await _expedienteService.ObtenerExpedientesAsync();

                Expedientes.Clear();
                foreach (var expediente in expedientes)
                {
                    Expedientes.Add(expediente);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al cargar expedientes: {ex.Message}";
            }
        }

        /// <summary>
        /// Carga los horarios disponibles según la fecha seleccionada
        /// INCLUYE el horario de la cita actual SOLO si la fecha coincide con la fecha de la cita
        /// </summary>
        private async Task CargarHorariosDisponiblesAsync()
        {
            try
            {
                if (Fecha.HasValue)
                {
                    // Guardar el horario de la cita seleccionada
                    string horarioCitaActual = _citaSeleccionada?.Horario;
                    DateTime? fechaCitaOriginal = _citaSeleccionada?.Fecha;

                    // Obtener horarios disponibles
                    var horarios = await _citaService.ObtenerHorariosDisponiblesAsync(Fecha.Value);
                    
                    // Solo añadir el horario de la cita si la fecha consultada coincide con la fecha original de la cita
                    if (!string.IsNullOrEmpty(horarioCitaActual) && 
                        fechaCitaOriginal.HasValue &&
                        fechaCitaOriginal.Value.Date == Fecha.Value.Date &&
                        !horarios.Contains(horarioCitaActual))
                    {
                        horarios.Add(horarioCitaActual);
                    }

                    // Ordenar y actualizar la colección observable
                    HorariosDisponibles.Clear();
                    foreach (var horario in horarios.OrderBy(h => h))
                    {
                        HorariosDisponibles.Add(horario);
                    }

                    // Asignar el horario SOLO si la fecha no cambió
                    if (!string.IsNullOrEmpty(horarioCitaActual) && 
                        fechaCitaOriginal.HasValue &&
                        fechaCitaOriginal.Value.Date == Fecha.Value.Date)
                    {
                        Horario = horarioCitaActual;
                    }
                    else
                    {
                        // Si cambió la fecha, seleccionar el primer horario disponible
                        if (HorariosDisponibles.Count > 0)
                        {
                            Horario = HorariosDisponibles[0];
                        }
                        else
                        {
                            Horario = null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al cargar horarios: {ex.Message}";
            }
        }

        /// <summary>
        /// Selecciona la primera cita de la lista
        /// </summary>
        private void SeleccionarPrimero()
        {
            if (Citas.Count > 0)
            {
                CitaSeleccionada = Citas[0];
            }
            else
            {
                CitaSeleccionada = null;
            }
        }

        /// <summary>
        /// Mantiene la selección de la cita en el índice especificado.
        /// Si el índice es inválido, selecciona el primero
        /// </summary>
        private void MantieneSeleccion(int indice)
        {
            if (Citas.Count == 0)
            {
                CitaSeleccionada = null;
            }
            else if (indice >= 0 && indice < Citas.Count)
            {
                CitaSeleccionada = Citas[indice];
            }
            else
            {
                SeleccionarPrimero();
            }
        }

        /// <summary>
        /// Selecciona la cita anterior al índice especificado al eliminar
        /// </summary>
        private void SeleccionaAnterior(int indiceEliminado)
        {
            if (Citas.Count == 0)
            {
                CitaSeleccionada = null;
            }
            else if (indiceEliminado == 0)
            {
                CitaSeleccionada = Citas[0];
            }
            else
            {
                int nuevoIndice = indiceEliminado - 1;
                if (nuevoIndice >= Citas.Count)
                {
                    nuevoIndice = Citas.Count - 1;
                }
                CitaSeleccionada = Citas[nuevoIndice];
            }
        }

        /// <summary>
        /// Abre la ventana modal para crear una nueva cita
        /// </summary>
        private void NuevaCita()
        {
            VentanaNuevaCita?.Invoke();
        }

        /// <summary>
        /// Edita la cita seleccionada con los datos de los campos,
        /// validando los datos y guardando los cambios en la base de datos
        /// </summary>
        private async void EditarCita()
        {
            if (CitaSeleccionada == null)
            {
                ErrorMessage = "No hay ninguna cita seleccionada para editar";
            }
            else if (ValidarFormulario())
            {
                try
                {
                    ErrorMessage = string.Empty;

                    int indiceCitaActual = Citas.IndexOf(CitaSeleccionada);

                    // Actualizar propiedades de la cita seleccionada
                    CitaSeleccionada.Fecha = Fecha.Value;
                    CitaSeleccionada.Horario = Horario;
                    CitaSeleccionada.Modalidad = Modalidad;
                    CitaSeleccionada.Motivo = string.IsNullOrWhiteSpace(Motivo) ? null : Motivo.Trim();
                    CitaSeleccionada.Estado = Estado;

                    await _citaService.ActualizarCitaAsync(CitaSeleccionada);

                    await CargarCitasAsync();
                    MantieneSeleccion(indiceCitaActual);
                }
                catch (ArgumentException ex)
                {
                    ErrorMessage = $"{ex.Message}";
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Error al editar cita: {ex.Message}";
                }
            }
        }

        /// <summary>
        /// Solicita confirmación para eliminar la cita seleccionada
        /// </summary>
        private void EliminarCita()
        {
            if (CitaSeleccionada == null)
            {
                ErrorMessage = "No hay ninguna cita seleccionada para eliminar";
                return;
            }

            // Validar que la cita no tenga estado realizada
            if (CitaSeleccionada.Estado?.ToLower() == "realizada")
            {
                ErrorMessage = "No se puede eliminar una cita ya realizada";
                return;
            }

            ErrorMessage = string.Empty;
            string codigoExpediente = CitaSeleccionada.Expediente?.Codigo ?? "Sin expediente";
            ConfirmarEliminar?.Invoke((CitaSeleccionada.IdCita, CitaSeleccionada.Fecha, codigoExpediente));
        }

        /// <summary>
        /// Confirma y ejecuta la eliminación de la cita de la base de datos
        /// </summary>
        public async void ConfirmarEliminarCita(int idCita)
        {
            try
            {
                if (CitaSeleccionada == null || CitaSeleccionada.IdCita != idCita)
                {
                    ErrorMessage = "La cita seleccionada no coincide con la cita a eliminar";
                }
                else
                {
                    int indiceCitaEliminada = Citas.IndexOf(CitaSeleccionada);

                    await _citaService.EliminarCitaAsync(CitaSeleccionada);

                    await CargarCitasAsync();
                    SeleccionaAnterior(indiceCitaEliminada);
                }
            }
            catch (InvalidOperationException ex)
            {
                ErrorMessage = $"{ex.Message}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al eliminar cita: {ex.Message}";
            }
        }

        /// <summary>
        /// Valida los datos del formulario de edición de citas
        /// </summary>
        private bool ValidarFormulario()
        {
            string mensajeError;

            bool esValido = CitaValidador.ValidarCita(
                Fecha,
                Horario,
                Modalidad,
                Motivo,
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
        /// Aplica el filtro de búsqueda global sobre la lista completa de citas
        /// </summary>
        private void AplicarFiltros()
        {
            if (_todasLasCitas != null)
            {
                var citasFiltradas = _todasLasCitas.AsEnumerable();

                if (!string.IsNullOrWhiteSpace(Buscar))
                {
                    string busqueda = Buscar.Trim().ToUpper();
                    citasFiltradas = citasFiltradas
                        .Where(c =>
                            (c.Cliente != null && c.Cliente.Nombre != null && c.Cliente.Nombre.ToUpper().Contains(busqueda)) ||
                            (c.Cliente != null && c.Cliente.Apellidos != null && c.Cliente.Apellidos.ToUpper().Contains(busqueda)) ||
                            (c.Cliente != null && c.Cliente.NombreCompleto != null && c.Cliente.NombreCompleto.ToUpper().Contains(busqueda)) ||
                            (c.Expediente != null && c.Expediente.Codigo != null && c.Expediente.Codigo.ToUpper().Contains(busqueda)) ||
                            (c.Estado != null && EstadoCitaMapper.DeBDaUI(c.Estado).ToUpper().Contains(busqueda))
                        )
                        .ToList();
                }

                Citas.Clear();
                foreach (var cita in citasFiltradas)
                {
                    Citas.Add(cita);
                }

                OnPropertyChanged(nameof(TotalCitas));
                SeleccionarPrimero();
            }
        }

        /// <summary>
        /// Limpia el filtro de búsqueda y muestra todas las citas
        /// </summary>
        private void LimpiarBusqueda()
        {
            Buscar = string.Empty;
        }

        /// <summary>
        /// Limpia los campos del formulario de edición
        /// </summary>
        private void LimpiarFormulario()
        {
            NombreCompletoCliente = string.Empty;
            CodigoExpediente = string.Empty;
            Fecha = null;
            Horario = null;
            Modalidad = null;
            Motivo = string.Empty;
            Estado = null;
            ClienteSeleccionado = null;
            ExpedienteSeleccionado = null;
        }

        /// <summary>
        /// Acción para comunicar con la Vista y abrir la ventana de nueva cita
        /// </summary>
        public Action VentanaNuevaCita { get; set; }

        /// <summary>
        /// Acción para comunicar con la Vista y solicitar confirmación de eliminación
        /// </summary>
        public Action<(int IdCita, DateTime Fecha, string CodigoExpediente)> ConfirmarEliminar { get; set; }

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
