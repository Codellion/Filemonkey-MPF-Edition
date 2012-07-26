using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using FileMonkey.Pandora.dal.entities;
using System.Windows.Forms;
using Memento.Persistence;
using Memento.Persistence.Interfaces;
using MessageBox = System.Windows.MessageBox;

namespace FileMonkey.Picasso
{
    /// <summary>
    /// Lógica de interacción para ServiceInspectorDetail.xaml
    /// </summary>
    public partial class ServiceInspectorDetail : Window
    {
        public ServiceInspector Inspector { get; set; }

        public Boolean OptionPressed { set; get; }
        
        public ServiceInspectorDetail()
        {
            InitializeComponent();
            Inspector = new ServiceInspector();

           // App.ActualInspectorDetail = this;
            this.Owner = App.Home;
        }

        public ServiceInspectorDetail(ServiceInspector inspector)
        {
            InitializeComponent();
            this.Inspector = inspector;

            //App.ActualInspectorDetail = this;            
            this.Owner = App.Inspectors;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtName.Text = Inspector.Name;
            slPeriod.Value = Inspector.CheckPeriod.HasValue?Inspector.CheckPeriod.Value : 0;
            txtPeriod.Text = GetPeriodText((int)slPeriod.Value);

            if(!Inspector.EnablePushNotification.HasValue)
            {
                Inspector.EnablePushNotification = false;
            }

            if(Inspector.EnablePushNotification.Value)
            {
                imgPush.Source = new BitmapImage(new Uri(@"images/pushover_on.png", UriKind.Relative));
            }
        }

        private void slPeriod_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            txtPeriod.Text = GetPeriodText((int)e.NewValue);
            Inspector.CheckPeriod = (int)e.NewValue;
        }

        public static String GetPeriodText(int pValue)
        {
            String result;

            if (pValue == 0)
            {
                result = "5 segundos";
            }            
            else if (pValue < 4)
            {
                result = (pValue * 15).ToString(CultureInfo.InvariantCulture) + " segundos";
            }
            else if (pValue < 11)
            {
                String value = "1";

                switch (pValue)
                {
                    case 5: value = "2";
                        break;
                    case 6: value = "5";
                        break;
                    case 7: value = "10";
                        break;
                    case 8: value = "15";
                        break;
                    case 9: value = "30";
                        break;
                    case 10: value = "45";
                        break;
                }

                result = value + " minutos";
            }
            else
            {
                result = (pValue - 10).ToString(CultureInfo.InvariantCulture) + " horas";
            }

            return result;
        }
    
        private void panels_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var panel = (StackPanel)sender;
            var label = (System.Windows.Controls.Label)panel.Children[1];
            label.FontWeight = FontWeights.ExtraBold;
        }

        private void panels_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var panel = (StackPanel)sender;
            var label = (System.Windows.Controls.Label)panel.Children[1];
            label.FontWeight = FontWeights.Normal;
        }

        private void pnlNewInspector_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!OptionPressed)
                return;

            string error = string.Empty;

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                error += "El nombre es obligatorio. \n";
            }

            if(!string.IsNullOrWhiteSpace(error))
            {
                MessageBox.Show(error);
                return;
            }

            using (var db = new DataContext())
            {
                try
                {
                    Inspector.Name = txtName.Text;
                    Inspector.CheckPeriod = (int) slPeriod.Value;

                    var servInsp = db.CreatePersistenceService<ServiceInspector>() as IPersistence<ServiceInspector>;

                    if (servInsp != null) servInsp.PersistEntity(Inspector);
                    db.SaveChanges();

                    if (Inspector.InspectorId.HasValue)
                    {
                        App.Single.UpdateWork(Inspector);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.InnerException.Message);
                }
            }

            Close();
        }

        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OptionPressed = true;
        }

        private void pnlPushoverMonitor_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Inspector.EnablePushNotification = !Inspector.EnablePushNotification;

            if (Inspector.EnablePushNotification.Value)
            {
                imgPush.Source = new BitmapImage(new Uri(@"images/pushover_on.png", UriKind.Relative));
            }
            else
            {
                imgPush.Source = new BitmapImage(new Uri(@"images/pushover_off.png", UriKind.Relative));
            }
        }
    }
}
