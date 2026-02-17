using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ViewModel;
using View.Windows;

namespace View.Views
{
    /// <summary>
    /// Code-behind para ExpedientesView.xaml.
    /// Vista principal para la gestión de expedientes del despacho de abogados
    /// </summary>
    public partial class ExpedientesView : UserControl
    {
        private readonly ExpedienteViewModel _viewModel;

        /// <summary>
        /// Constructor que inicializa la vista y configura el ViewModel
        /// </summary>
        public ExpedientesView()
        {
            InitializeComponent();

            // Crear e instanciar el ViewModel
            _viewModel = new ExpedienteViewModel();

            // Asignar Actions
            _viewModel.VentanaNuevoExpediente = VentanaNuevoExpediente;
            _viewModel.ConfirmarEliminar = ConfirmarEliminar;
            _viewModel.IrAActuaciones = IrAActuacionesExpediente;

            // Asignar DataContext
            this.DataContext = _viewModel;
        }

        /// <summary>
        /// Abre la ventana modal para crear un nuevo expediente
        /// </summary>
        private void VentanaNuevoExpediente()
        {
            var ventana = new NuevoExpedienteWindow();
            
            // ShowDialog devuelve true si se guardó correctamente
            if (ventana.ShowDialog() == true)
            {
                // Recargar la lista de expedientes
                _viewModel.InicializarAsync();
            }
        }

        /// <summary>
        /// Muestra un diálogo de confirmación antes de eliminar un expediente
        /// </summary>
        /// <param name="info">Tupla con el ID y código del expediente a eliminar</param>
        private void ConfirmarEliminar((int IdExpediente, string Codigo) info)
        {
            var resultado = MessageBox.Show(
                $"¿Está seguro que desea eliminar el expediente '{info.Codigo}'?",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (resultado == MessageBoxResult.Yes)
            {
                // Confirmar eliminación en el ViewModel
                _viewModel.ConfirmarEliminarExpediente(info.IdExpediente);
            }
        }

        /// <summary>
        /// Controla la visibilidad del placeholder del campo de búsqueda
        /// </summary>
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            placeholderBuscar.Visibility = string.IsNullOrEmpty(txtBuscar.Text) 
                ? Visibility.Visible 
                : Visibility.Collapsed;
        }

        /// <summary>
        /// Navega a la vista de Actuaciones con el expediente seleccionado como filtro
        /// </summary>
        /// <param name="codigoExpediente">Código del expediente para filtrar actuaciones</param>
        private void IrAActuacionesExpediente(string codigoExpediente)
        {
            if (!string.IsNullOrEmpty(codigoExpediente))
            {
                // Obtener MainWindow y cambiar vista pasando el código del expediente como parámetro
                var mainWindow = Window.GetWindow(this) as MainWindow;
                mainWindow?.CambiarVista("Actuaciones", codigoExpediente);
            }
        }

        /// <summary>
        /// Inicializa el color de fondo del ComboBox de Estado al cargar
        /// </summary>
        private void CbEstado_Loaded(object sender, RoutedEventArgs e)
        {
            ActualizarColorEstado();
        }

        /// <summary>
        /// Aplica el color de fondo al ComboBox de Estado según su valor
        /// </summary>
        private void CbEstado_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ActualizarColorEstado();
        }

        /// <summary>
        /// Actualiza el color de fondo del ComboBox de Estado según el valor seleccionado
        /// </summary>
        private void ActualizarColorEstado()
        {
            if (cbEstado != null && !string.IsNullOrEmpty(_viewModel.Estado))
            {
                var backgroundColor = ObtenerColorPorEstado(_viewModel.Estado);
                AplicarColorAComboBox(cbEstado, backgroundColor);
            }
        }

        /// <summary>
        /// Obtiene el color de fondo correspondiente al estado del expediente
        /// </summary>
        private SolidColorBrush ObtenerColorPorEstado(string estado)
        {
            switch (estado.ToLower())
            {
                case "abierto":
                case "en_tramite":
                    return new SolidColorBrush(Color.FromRgb(169, 221, 169)); // Verde pastel #A9DDA9

                case "en_espera":
                case "archivado":
                    return new SolidColorBrush(Color.FromRgb(255, 228, 163)); // Naranja pastel #FFE4A3

                case "cerrado":
                    return new SolidColorBrush(Color.FromRgb(255, 179, 171)); // Rojo pastel #FFB3AB

                default:
                    return Brushes.White;
            }
        }

        /// <summary>
        /// Aplica un color de fondo al ComboBox y sus elementos internos del template.
        /// Necesario porque el template de WPF sobrescribe el Background del ComboBox.
        /// </summary>
        private void AplicarColorAComboBox(ComboBox comboBox, SolidColorBrush backgroundColor)
        {
            // Aplicar color al ComboBox principal
            comboBox.Background = backgroundColor;
            comboBox.Foreground = Brushes.Black;
            comboBox.ApplyTemplate();

            // Colorear el ToggleButton del ComboBox para que el color de fondo sea visible incluso cuando no está desplegado
            var toggleButton = BuscarElementoEnArbolVisual<System.Windows.Controls.Primitives.ToggleButton>(comboBox);
            if (toggleButton != null) toggleButton.Background = backgroundColor;

            // Colorear el borde del ComboBox para que el color de fondo sea visible incluso cuando no está desplegado
            var border = BuscarElementoEnArbolVisual<Border>(comboBox);
            if (border != null) border.Background = backgroundColor;
        }

        /// <summary>
        /// Busca recursivamente un elemento hijo en el árbol visual por tipo.
        /// 
        /// WPF no expone acceso directo a los elementos internos del template de un control.
        /// Este método recorre el árbol visual para encontrar elementos específicos como
        /// ToggleButton o Border dentro de controles complejos como ComboBox.
        /// 
        /// Es la forma estándar en WPF para acceder a elementos del template.
        /// </summary>
        /// <typeparam name="T">Tipo de elemento WPF a buscar (ToggleButton, Border, etc.)</typeparam>
        /// <param name="parent">Elemento padre donde iniciar la búsqueda</param>
        /// <returns>Primer elemento encontrado del tipo especificado, o null si no existe</returns>
        private T BuscarElementoEnArbolVisual<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            // Recorrer todos los hijos visuales del elemento
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var hijo = VisualTreeHelper.GetChild(parent, i);
                
                // Si encontramos el tipo buscado, devolverlo
                if (hijo is T elementoEncontrado)
                    return elementoEncontrado;

                // Buscar recursivamente en los hijos (profundidad primero)
                var resultado = BuscarElementoEnArbolVisual<T>(hijo);
                if (resultado != null)
                    return resultado;
            }

            return null;
        }
    }
}
