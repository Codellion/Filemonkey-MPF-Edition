using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FileMonkey.Picasso
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            App.Home = this;
        }

        private void pnlRastreadores_MouseUp(object sender, MouseButtonEventArgs e)
        {
            App.Inspectors.Show();
            App.Inspectors.Focus();
        }

        private void btnSalir_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void pnlNewRastreador_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var inspDetail = new InspectorDetail();
            inspDetail.ShowDialog();

            this.Focus();
        }

        private void pnlCambiarSonar_MouseUp(object sender, MouseButtonEventArgs e)
        {
            App.SonarActivo = !App.SonarActivo;

            if (!App.SonarActivo)
            {
                lblEstadoSonar.Foreground = Brushes.Red;
                lblEstadoSonar.Content = "Inactivo";
            }
            else
            {
                lblEstadoSonar.Foreground = Brushes.Green;
                lblEstadoSonar.Content = "Activo";
            }
        }

        private void panels_MouseEnter(object sender, MouseEventArgs e)
        {
            var panel = (StackPanel)sender;
            var label = (Label)panel.Children[1];
            label.FontWeight = FontWeights.ExtraBold;

            lblOpciones.Content = label.ToolTip;
        }

        private void panels_MouseLeave(object sender, MouseEventArgs e)
        {
            var panel = (StackPanel)sender;
            var label = (Label)panel.Children[1];
            label.FontWeight = FontWeights.Normal;

            lblOpciones.Content = String.Empty;
        }

        private void imgOpts_MouseEnter(object sender, MouseEventArgs e)
        {
            var imagen = (Image)sender;

            lblOpciones.Content = imagen.DataContext.ToString();
        }

        private void imgOpts_MouseLeave(object sender, MouseEventArgs e)
        {
            lblOpciones.Content = String.Empty;
        }

        private void pnlVerRegistro_MouseUp(object sender, MouseButtonEventArgs e)
        {
            App.RegistryWindow.Show();
            App.RegistryWindow.Focus();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!App.SonarActivo)
            {
                lblEstadoSonar.Foreground = Brushes.Red;
                lblEstadoSonar.Content = "Inactivo";
            }
            else
            {
                lblEstadoSonar.Foreground = Brushes.Green;
                lblEstadoSonar.Content = "Activo";
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
