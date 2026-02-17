using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ViewModel;

namespace View.Windows
{
    /// <summary>
    /// Lógica de interacción para NuevoExpedienteWindow.xaml
    /// </summary>
    public partial class NuevoExpedienteWindow : Window
    {
        private NuevoExpedienteViewModel _viewModel;

        public NuevoExpedienteWindow()
        {
            InitializeComponent();
            _viewModel = new NuevoExpedienteViewModel();
            DataContext = _viewModel;

            // Asignar acciones para cerrar la ventana (más simple que eventos)
            _viewModel.CerrarVentanaExito = () => { DialogResult = true; Close(); };
            _viewModel.CerrarVentanaCancelar = () => { DialogResult = false; Close(); };

            // Suscribirse al evento Loaded para configurar el TextBox interno del ComboBox
            Loaded += (s, e) =>
            {
                // Buscar el TextBox editable dentro del ComboBox
                var textBoxCliente = cbCliente.Template.FindName("PART_EditableTextBox", cbCliente) as TextBox;
                if (textBoxCliente != null)
                {
                    textBoxCliente.TextChanged += ComboBox_TextChanged;
                }

                // Actualizar visibilidad inicial de placeholders
                ActualizarVisibilidadPlaceholder(txtTitulo, placeholderTitulo);
                ActualizarVisibilidadPlaceholder(txtDescripcion, placeholderDescripcion);
                ActualizarVisibilidadPlaceholder(txtOrgano, placeholderOrgano);
            };
        }

        /// <summary>
        /// Maneja el cambio de texto en el ComboBox editable de Cliente
        /// </summary>
        private void ComboBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                // El TextBox interno del ComboBox tiene nombre "PART_EditableTextBox"
                if (cbCliente.Template.FindName("PART_EditableTextBox", cbCliente) == textBox)
                {
                    ActualizarVisibilidadPlaceholder(cbCliente, placeholderCliente);
                }
            }
        }

        /// <summary>
        /// Maneja el cambio de selección en el ComboBox de Cliente
        /// </summary>
        private void CbCliente_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ActualizarVisibilidadPlaceholder(cbCliente, placeholderCliente);
        }

        /// <summary>
        /// Maneja el cambio de texto en los TextBox para actualizar placeholders
        /// </summary>
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            if (textBox == txtTitulo)
                ActualizarVisibilidadPlaceholder(txtTitulo, placeholderTitulo);
            else if (textBox == txtDescripcion)
                ActualizarVisibilidadPlaceholder(txtDescripcion, placeholderDescripcion);
            else if (textBox == txtOrgano)
                ActualizarVisibilidadPlaceholder(txtOrgano, placeholderOrgano);
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
        private void CbCliente_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Selecciona todo el texto del ComboBox editable
            if (cbCliente.IsEditable && cbCliente.Template.FindName("PART_EditableTextBox", cbCliente) is TextBox textBox)
            {
                textBox.SelectAll();
            }
        }

        /// <summary>
        /// Filtra los clientes según el texto escrito
        /// </summary>
        private void CbCliente_KeyUp(object sender, KeyEventArgs e)
        {
            // Obtener el criterio de búsqueda del ComboBox
            string criterioCliente = cbCliente.Text;

            // Delegar el filtrado al ViewModel
            _viewModel.BuscarCliente = criterioCliente;

            // Abrir el dropdown
            cbCliente.IsDropDownOpen = true;

            // Restaurar el texto (importante para evitar que se borre)
            cbCliente.Text = criterioCliente;

            // Poner el cursor al final del texto
            if (cbCliente.Template.FindName("PART_EditableTextBox", cbCliente) is TextBox textBox)
            {
                textBox.SelectionStart = criterioCliente.Length;
            }
        }
    }
}
