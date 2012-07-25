using System;
using System.Collections.Generic;
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
using log4net.Appender;
using log4net;
using System.IO;
using FileMonkey.Picasso.helpers;

namespace FileMonkey.Picasso
{
    /// <summary>
    /// Lógica de interacción para Log.xaml
    /// </summary>
    public partial class Log : Window
    {
        public Log()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshLog();
        }

        public void RefreshLog()
        {
            // Formateamos la salida de HTML a XAML
            FlowDocument document = txtLogger.Document;            
            String text = HtmlToXamlConverter.ConvertHtmlToXaml(App.Registry, false);

            var tr = new TextRange(document.ContentStart, document.ContentEnd);
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(text)))
            {
                tr.Load(ms, DataFormats.Xaml);
            }
        }

        private void txtLogger_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (chkBlockVisor.IsChecked != null && !chkBlockVisor.IsChecked.Value)
            {
                txtLogger.ScrollToEnd();
            }
        }
    }
}
