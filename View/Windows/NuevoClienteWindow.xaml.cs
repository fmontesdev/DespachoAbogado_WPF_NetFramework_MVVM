using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ViewModel;

namespace View.Windows
{
    /// <summary>
    /// Code-behind para NuevoClienteWindow.xaml.
    /// Ventana modal para crear un nuevo cliente con validación de formulario
    /// </summary>
    public partial class NuevoClienteWindow : Window
    {
        /// <summary>
        /// Constructor que inicializa la ventana y configura el ViewModel
        /// </summary>
        public NuevoClienteWindow()
        {
            InitializeComponent();

            // Crear ViewModel
            var viewModel = new NuevoClienteViewModel();

            // Asignar acciones para cerrar la ventana (más simple que eventos)
            viewModel.CerrarVentanaExito = () => { DialogResult = true; Close(); };
            viewModel.CerrarVentanaCancelar = () => { DialogResult = false; Close(); };

            // Asignar DataContext
            this.DataContext = viewModel;
        }

        /// <summary>
        /// Maneja la visibilidad de los placeholders de los TextBox
        /// </summary>
        /// <param name="sender">TextBox que disparó el evento</param>
        /// <param name="e">Argumentos del evento</param>
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;

            if (textBox != null)
            {
                // Determinar cuál placeholder corresponde
                if (textBox.Name == "txtNombre")
                {
                    placeholderNombre.Visibility = string.IsNullOrEmpty(textBox.Text)
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                }
                else if (textBox.Name == "txtApellidos")
                {
                    placeholderApellidos.Visibility = string.IsNullOrEmpty(textBox.Text)
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                }
                else if (textBox.Name == "txtDni")
                {
                    placeholderDni.Visibility = string.IsNullOrEmpty(textBox.Text)
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                }
                else if (textBox.Name == "txtTelefono")
                {
                    placeholderTelefono.Visibility = string.IsNullOrEmpty(textBox.Text)
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                }
                else if (textBox.Name == "txtEmail")
                {
                    placeholderEmail.Visibility = string.IsNullOrEmpty(textBox.Text)
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                }
                else if (textBox.Name == "txtPoblacion")
                {
                    placeholderPoblacion.Visibility = string.IsNullOrEmpty(textBox.Text)
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                }
                else if (textBox.Name == "txtDireccion")
                {
                    placeholderDireccion.Visibility = string.IsNullOrEmpty(textBox.Text)
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                }
            }
        }
    }
}
