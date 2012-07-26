using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Serialization;
using Filemonkey.Pandora.dbl;

namespace FileMonkey.Picasso
{
    /// <summary>
    /// Lógica de interacción para Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
            App.ConfigurarOpciones = this;
            Owner = App.Home;
        }

        private void panels_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var panel = (StackPanel)sender;
            var label = (Label)panel.Children[1];
            label.FontWeight = FontWeights.ExtraBold;
        }

        private void panels_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var panel = (StackPanel)sender;
            var label = (Label)panel.Children[1];
            label.FontWeight = FontWeights.Normal;
        }

        private void pnlSaveSettings_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var opciones = new FmSettings
                               {
                                   PushoverUserKey = txtPushUserKey.Text, 
                                   PushoverDeviceName = txtPushDeviceName.Text
                               };

            Stream data = new FileStream("Settings.xml", FileMode.Create);

            var marshall = new XmlSerializer(typeof(FmSettings));

            marshall.Serialize(data, opciones);

            App.Opciones = opciones;

            Close();
        }

        private void pnlCancel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtPushUserKey.Text = App.Opciones.PushoverUserKey;
            txtPushDeviceName.Text = App.Opciones.PushoverDeviceName;
        }
    }
}
