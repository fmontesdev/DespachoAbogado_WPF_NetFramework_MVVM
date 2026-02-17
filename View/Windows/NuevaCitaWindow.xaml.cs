using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ViewModel;

namespace View.Windows
{
    public partial class NuevaCitaWindow : Window
    {
        private NuevaCitaViewModel _viewModel;

        public NuevaCitaWindow()
        {
            InitializeComponent();
            _viewModel = new NuevaCitaViewModel();
            DataContext = _viewModel;

            _viewModel.CerrarVentanaExito = () => { DialogResult = true; Close(); };
            _viewModel.CerrarVentanaCancelar = () => { DialogResult = false; Close(); };

            // Suscribirse a cambios en las propiedades del ViewModel
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;

            Loaded += (s, e) =>
            {
                var textBoxCliente = cbCliente.Template.FindName("PART_EditableTextBox", cbCliente) as TextBox;
                if (textBoxCliente != null)
                {
                    textBoxCliente.TextChanged += ComboBoxCliente_TextChanged;
                }

                var textBoxExpediente = cbExpediente.Template.FindName("PART_EditableTextBox", cbExpediente) as TextBox;
                if (textBoxExpediente != null)
                {
                    textBoxExpediente.TextChanged += ComboBoxExpediente_TextChanged;
                }

                ActualizarVisibilidadPlaceholder(txtMotivo, placeholderMotivo);
            };
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Cuando cambia el placeholder, actualizar visibilidad
            if (e.PropertyName == "PlaceholderExpediente" || 
                e.PropertyName == "ExpedientesHabilitado" ||
                e.PropertyName == "ClienteSeleccionado")
            {
                ActualizarVisibilidadPlaceholder(cbExpediente, placeholderExpediente);
            }
        }

        private void ComboBoxCliente_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                if (cbCliente.Template.FindName("PART_EditableTextBox", cbCliente) == textBox)
                {
                    ActualizarVisibilidadPlaceholder(cbCliente, placeholderCliente);
                }
            }
        }

        private void ComboBoxExpediente_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                if (cbExpediente.Template.FindName("PART_EditableTextBox", cbExpediente) == textBox)
                {
                    ActualizarVisibilidadPlaceholder(cbExpediente, placeholderExpediente);
                }
            }
        }

        private void CbCliente_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ActualizarVisibilidadPlaceholder(cbCliente, placeholderCliente);
        }

        private void CbExpediente_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ActualizarVisibilidadPlaceholder(cbExpediente, placeholderExpediente);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            if (textBox == txtMotivo)
                ActualizarVisibilidadPlaceholder(txtMotivo, placeholderMotivo);
        }

        private void ActualizarVisibilidadPlaceholder(TextBox textBox, TextBlock placeholder)
        {
            placeholder.Visibility = string.IsNullOrEmpty(textBox.Text) 
                ? Visibility.Visible 
                : Visibility.Collapsed;
        }

        private void ActualizarVisibilidadPlaceholder(ComboBox comboBox, TextBlock placeholder)
        {
            bool tieneSeleccion = comboBox.SelectedItem != null;
            bool tieneTexto = false;

            // Si el ComboBox es editable, verificar si tiene texto
            if (comboBox.Template.FindName("PART_EditableTextBox", comboBox) is TextBox textBox)
            {
                tieneTexto = !string.IsNullOrEmpty(textBox.Text);
            }

            // Mostrar el placeholder solo si no hay selección ni texto
            placeholder.Visibility = (tieneSeleccion || tieneTexto)
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        private void CbCliente_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (cbCliente.IsEditable && cbCliente.Template.FindName("PART_EditableTextBox", cbCliente) is TextBox textBox)
            {
                textBox.SelectAll();
            }
        }

        private void CbExpediente_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (cbExpediente.IsEditable && cbExpediente.Template.FindName("PART_EditableTextBox", cbExpediente) is TextBox textBox)
            {
                textBox.SelectAll();
            }
        }

        private void CbCliente_KeyUp(object sender, KeyEventArgs e)
        {
            string criterioCliente = cbCliente.Text;
            _viewModel.BuscarCliente = criterioCliente;
            cbCliente.IsDropDownOpen = true;
            cbCliente.Text = criterioCliente;

            if (cbCliente.Template.FindName("PART_EditableTextBox", cbCliente) is TextBox textBox)
            {
                textBox.SelectionStart = criterioCliente.Length;
            }
        }

        private void CbExpediente_KeyUp(object sender, KeyEventArgs e)
        {
            string criterioExpediente = cbExpediente.Text;
            _viewModel.BuscarExpediente = criterioExpediente;
            cbExpediente.IsDropDownOpen = true;
            cbExpediente.Text = criterioExpediente;

            if (cbExpediente.Template.FindName("PART_EditableTextBox", cbExpediente) is TextBox textBox)
            {
                textBox.SelectionStart = criterioExpediente.Length;
            }
        }

        /// <summary>
        /// Bloquea los fines de semana (sábados y domingos) en el DatePicker
        /// </summary>
        public void DatePicker_CalendarOpened(object sender, RoutedEventArgs e)
        {
            var datePicker = sender as DatePicker;
            if (datePicker == null) return;

            // Limpiar fechas bloqueadas anteriores para evitar duplicados
            datePicker.BlackoutDates.Clear();

            // Obtener la fecha de inicio (hoy o fecha seleccionada)
            System.DateTime fechaInicio = datePicker.SelectedDate ?? System.DateTime.Now;
            System.DateTime fechaFin = fechaInicio.AddMonths(6);

            // Bloquear todos los fines de semana en el rango
            for (var fecha = fechaInicio.Date; fecha <= fechaFin; fecha = fecha.AddDays(1))
            {
                if (fecha.DayOfWeek == System.DayOfWeek.Saturday || fecha.DayOfWeek == System.DayOfWeek.Sunday)
                {
                    datePicker.BlackoutDates.Add(new CalendarDateRange(fecha));
                }
            }
        }
    }
}
