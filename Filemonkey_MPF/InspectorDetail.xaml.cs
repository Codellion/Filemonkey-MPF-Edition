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
    /// Lógica de interacción para InspectorDetail.xaml
    /// </summary>
    public partial class InspectorDetail : Window
    {
        public FileInspector Inspector { get; set; }

        public Boolean OptionPressed { set; get; }
        
        public InspectorDetail()
        {
            InitializeComponent();
            Inspector = new FileInspector { Path = "Seleccione una carpeta para rastrear" };

            App.ActualInspectorDetail = this;
            this.Owner = App.Home;
        }

        public InspectorDetail(FileInspector inspector)
        {
            InitializeComponent();
            this.Inspector = inspector;

            App.ActualInspectorDetail = this;            
            this.Owner = App.Inspectors;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtName.Text = Inspector.Name;
            txtPath.Text = Inspector.Path;
            slPeriod.Value = Inspector.CheckPeriod.HasValue?Inspector.CheckPeriod.Value : 0;
            txtPeriod.Text = GetPeriodText((int)slPeriod.Value);

            if (Inspector.Action == (int) FileInspector.TypeActions.MoveSubDir)
            {
                rbtMoveSubDir.IsChecked = true;

                if(!String.IsNullOrEmpty(Inspector.SubDirAction))
                {
                    txtPathAction.Text = Inspector.SubDirAction;
                }
            }
            else
            {
                rbtDeleteFiles.IsChecked = true;                
            }

            if (Inspector.InspectorId.HasValue)
            {
                rbtMoveSubDir.IsEnabled = false;
                rbtDeleteFiles.IsEnabled = false;
            }

            if(!Inspector.EnablePushNotification.HasValue)
            {
                Inspector.EnablePushNotification = false;
            }

            if(Inspector.EnablePushNotification.Value)
            {
                imgPush.Source = new BitmapImage(new Uri(@"images/pushover_on.png", UriKind.Relative));
            }
                
            RulesRefresh(null);
        }

        private void RulesRefresh(RuleFile rule_, Boolean added = false)
        {
            List<RuleFile> lstRules;

            if (rule_ == null)
            {
                Inspector.Rules.Value.ToList().ForEach(SetImageRule);
                lstRules = Inspector.Rules.Value.ToList();
            }
            else
            {

                lstRules = (List<RuleFile>)lstVRules.ItemsSource ?? new List<RuleFile>();

                if (added)
                {
                    lstRules.Add(rule_);
                }
                else
                    lstRules.Remove(rule_);
            }

            lstVRules.ItemsSource = null;
            lstVRules.ItemsSource = lstRules;
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

        private void SetImageRule(RuleFile rule)
        {
            if (rule.RuleType != null)
                switch ((RuleFile.TypeFileRule)rule.RuleType)
                {
                    case RuleFile.TypeFileRule.Date:
                        rule.ImagePath = @"/images/icon_ruleFileDate.png";
                        break;
                    case RuleFile.TypeFileRule.Extension:
                        rule.ImagePath = @"/images/icon_ruleFileExtension.png";
                        break;
                    case RuleFile.TypeFileRule.FileName:
                        rule.ImagePath = @"/images/icon_ruleFileName.png";
                        break;
                }
        }

        private void lstVRules_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lstVRules.SelectedIndex != -1)
            {
                var ruleSelected = (RuleFile)lstVRules.SelectedValue;

                var ruleDetail = new NewRule(ruleSelected);
                ruleDetail.ShowDialog();

                if(ruleDetail.Rule != null)
                {
                    ruleSelected = ruleDetail.Rule;

                    var lstRules = (List<RuleFile>)lstVRules.ItemsSource;

                    lstVRules.ItemsSource = null;
                    lstVRules.ItemsSource = lstRules;
                }
            }
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
            if (string.IsNullOrWhiteSpace(txtPath.Text) || "Seleccione una carpeta para rastrear".Equals(txtPath.Text))
            {
                error += "La ruta es obligatoria. \n";
            }
            if (rbtMoveSubDir.IsChecked.HasValue && rbtMoveSubDir.IsChecked.Value)
            {
                if (string.IsNullOrWhiteSpace(txtPathAction.Text) || "Seleccione la carpeta destino de los ficheros".Equals(txtPathAction.Text))
                {
                    error += "La ruta de destino es obligatoria. \n";
                }
            }

            if ((Inspector.Rules.Value == null) || 
                (Inspector.Rules.Value != null && Inspector.Rules.Value.Count == 0))
            {
                error += "Se debe de especificar alguna regla. \n";
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
                    Inspector.Path = txtPath.Text;

                    if (rbtMoveSubDir.IsChecked.HasValue && rbtMoveSubDir.IsChecked.Value)
                    {
                        Inspector.Action = (int)FileInspector.TypeActions.MoveSubDir;
                        Inspector.SubDirAction = txtPathAction.Text;
                    }
                    else
                    {
                        Inspector.Action = (int)FileInspector.TypeActions.DeleteFiles;
                        Inspector.SubDirAction = String.Empty;
                    }

                    Inspector.CheckPeriod = (int) slPeriod.Value;

                    var servInsp = db.CreatePersistenceService<FileInspector>() as IPersistence<FileInspector>;

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

        private void pnlChoosePath_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SetFileBrowser(txtPath);
        }

        private void SetFileBrowser(TextBlock control)
        {
            var selectFolderDialog = new FolderBrowserDialog {ShowNewFolderButton = true};

            if (selectFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                control.Text = selectFolderDialog.SelectedPath;
            }
        }

        private void pnlNewRule_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var newRule = new NewRule();
            newRule.ShowDialog();

            if (newRule.Rule != null)
            {
                RuleFile rule = newRule.Rule;

                Inspector.Rules.Value.Add(rule);

                RulesRefresh(rule, true);
            }
        }

        private void pnlDeleteRule_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (lstVRules.SelectedIndex != -1)
            {
                var ruleSelected = (RuleFile)lstVRules.SelectedValue;

                Inspector.Rules.Value.Remove(ruleSelected);

                RulesRefresh(ruleSelected);
            }
            else
            {
                MessageBox.Show("Por favor, seleccione una regla de la lista");
            }
        }

        private void pnlChoosePathAction_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SetFileBrowser(txtPathAction);
        }

        private void rbtMoveSubDir_Checked(object sender, RoutedEventArgs e)
        {
            pnlChoosePathActionTot.Visibility = System.Windows.Visibility.Visible;
        }

        private void rbtDeleteFiles_Checked(object sender, RoutedEventArgs e)
        {
            pnlChoosePathActionTot.Visibility = System.Windows.Visibility.Hidden;
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
