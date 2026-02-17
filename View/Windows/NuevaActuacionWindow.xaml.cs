using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ViewModel;

namespace View.Windows
{
    /// <summary>
    /// Lógica de interacción para NuevaActuacionWindow.xaml
    /// </summary>
    public partial class NuevaActuacionWindow : Window
    {
        private NuevaActuacionViewModel _viewModel;

        public NuevaActuacionWindow()
        {
            InitializeComponent();
            _viewModel = new NuevaActuacionViewModel();
            DataContext = _viewModel;

            // Asignar acciones para cerrar la ventana (más simple que eventos)
            _viewModel.CerrarVentanaExito = () => { DialogResult = true; Close(); };
            _viewModel.CerrarVentanaCancelar = () => { DialogResult = false; Close(); };

            // Suscribirse al evento Loaded para configurar el TextBox interno del ComboBox
            Loaded += (s, e) =>
            {
                // Buscar el TextBox editable dentro del ComboBox
                var textBoxExpediente = cbExpediente.Template.FindName("PART_EditableTextBox", cbExpediente) as TextBox;
                if (textBoxExpediente != null)
                {
                    textBoxExpediente.TextChanged += ComboBox_TextChanged;
                }

                // Actualizar visibilidad inicial de placeholders
                ActualizarVisibilidadPlaceholder(txtDescripcion, placeholderDescripcion);
            };
        }

        /// <summary>
        /// Maneja el cambio de texto en el ComboBox editable de Expediente
        /// </summary>
        private void ComboBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                // El TextBox interno del ComboBox tiene nombre "PART_EditableTextBox"
                if (cbExpediente.Template.FindName("PART_EditableTextBox", cbExpediente) == textBox)
                {
                    ActualizarVisibilidadPlaceholder(cbExpediente, placeholderExpediente);
                }
            }
        }

        /// <summary>
        /// Maneja el cambio de selección en el ComboBox de Expediente
        /// </summary>
        private void CbExpediente_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ActualizarVisibilidadPlaceholder(cbExpediente, placeholderExpediente);
        }

        /// <summary>
        /// Maneja el cambio de texto en los TextBox para actualizar placeholders
        /// </summary>
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            if (textBox == txtDescripcion)
                ActualizarVisibilidadPlaceholder(txtDescripcion, placeholderDescripcion);
        }

        /// <summary>
        /// Actualiza la visibilidad del placeholder según el estado del TextBox
        /// </summary>
        private void ActualizarVisibilidadPlaceholder(TextBox textBox, TextBlock placeholder)
        {
            placeholder.Visibility = string.IsNullOrEmpty(textBox.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        /// <summary>
        /// Actualiza la visibilidad del placeholder según el estado del ComboBox
        /// </summary>
        private void ActualizarVisibilidadPlaceholder(ComboBox comboBox, TextBlock placeholder)
        {
            bool tieneSeleccion = comboBox.SelectedItem != null;
            bool tieneTexto = false;

            if (comboBox.Template.FindName("PART_EditableTextBox", comboBox) is TextBox textBox)
            {
                tieneTexto = !string.IsNullOrEmpty(textBox.Text);
            }

            placeholder.Visibility = (tieneSeleccion || tieneTexto)
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        /// <summary>
        /// Selecciona todo el texto del ComboBox al hacer clic
        /// </summary>
        private void CbExpediente_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Selecciona todo el texto del ComboBox editable
            if (cbExpediente.IsEditable && cbExpediente.Template.FindName("PART_EditableTextBox", cbExpediente) is TextBox textBox)
            {
                textBox.SelectAll();
            }
        }

        /// <summary>
        /// Filtra los expedientes según el texto escrito
        /// </summary>
        private void CbExpediente_KeyUp(object sender, KeyEventArgs e)
        {
            // Obtener el criterio de búsqueda del ComboBox
            string criterioExpediente = cbExpediente.Text;

            // Delegar el filtrado al ViewModel
            _viewModel.BuscarExpediente = criterioExpediente;

            // Abrir el dropdown
            cbExpediente.IsDropDownOpen = true;

            // Restaurar el texto (importante para evitar que se borre)
            cbExpediente.Text = criterioExpediente;

            // Poner el cursor al final del texto
            if (cbExpediente.Template.FindName("PART_EditableTextBox", cbExpediente) is TextBox textBox)
            {
                textBox.SelectionStart = criterioExpediente.Length;
            }
        }
    }
}
