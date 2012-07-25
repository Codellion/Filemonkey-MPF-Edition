using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Elemental.pal.userControls;
using FileMonkey.Pandora.dal.entities;
using Elemental.pal.userControls.interfaces;

namespace FileMonkey.Picasso
{
    /// <summary>
    /// Lógica de interacción para NewRule.xaml
    /// </summary>
    public partial class NewRule : Window
    {
        public RuleFile Rule { get; set; }
        private bool _isSaved;

        public NewRule()
        {
            InitializeComponent();

            Owner = App.ActualInspectorDetail;
        }

        public NewRule(RuleFile rule)
        {
            InitializeComponent();
            Rule = rule;

            rbtDate.IsEnabled = false;
            rbtExtension.IsEnabled = false;
            rbtName.IsEnabled = false;

            Owner = App.ActualInspectorDetail;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {            
            txtName.Text = Rule.RuleName;

            if (Rule.RuleType != null)
                switch ((RuleFile.TypeFileRule) Rule.RuleType)
                {
                    case RuleFile.TypeFileRule.Date:
                        rbtDate.IsChecked = true;
                        break;
                    case RuleFile.TypeFileRule.Extension:
                        rbtExtension.IsChecked = true;
                        break;
                    case RuleFile.TypeFileRule.FileName:
                        rbtName.IsChecked = true;
                        break;
                }

            var rControl = (RuleControl)pnlExtensionControl.Children[0];
            rControl.SetRule(Rule);
        }

        private void rbtDate_Checked(object sender, RoutedEventArgs e)
        {
            pnlExtensionControl.Children.Clear();
            pnlExtensionControl.Children.Add(new Date());
        }

        private void rbtExtension_Checked(object sender, RoutedEventArgs e)
        {
            pnlExtensionControl.Children.Clear();
            pnlExtensionControl.Children.Add(new Extension());
        }

        private void rbtName_Checked(object sender, RoutedEventArgs e)
        {
            pnlExtensionControl.Children.Clear();
            pnlExtensionControl.Children.Add(new Name());
        }

        private void SaveRule()
        {
            Boolean typeChoose = false;
            var ruleAux = new RuleFile();

            if (rbtDate.IsChecked.HasValue && rbtDate.IsChecked.Value)
            {
                
                ruleAux.RuleType = (int)RuleFile.TypeFileRule.Date;
                ruleAux.ImagePath = @"/images/icon_ruleFileDate.png";
                
                typeChoose = true;
            }
            else if (rbtExtension.IsChecked.HasValue && rbtExtension.IsChecked.Value)
            {
                ruleAux.RuleType = (int)RuleFile.TypeFileRule.Extension;
                ruleAux.ImagePath = @"/images/icon_ruleFileExtension.png";

                typeChoose = true;
            }
            else if (rbtName.IsChecked.HasValue && rbtName.IsChecked.Value)
            {
                ruleAux.RuleType = (int)RuleFile.TypeFileRule.FileName;
                ruleAux.ImagePath = @"/images/icon_ruleFileName.png";

                typeChoose = true;
            }

            if (!typeChoose)
            {
                MessageBox.Show("Necesita especificar el tipo de la regla");
                return;
            }

            var rControl = (RuleControl) pnlExtensionControl.Children[0];
            Rule = rControl.GetRule();

            Rule.RuleType = ruleAux.RuleType;
            Rule.ImagePath = ruleAux.ImagePath;
            Rule.RuleName = txtName.Text;

            if (string.IsNullOrWhiteSpace(Rule.NamePattern) && string.IsNullOrWhiteSpace(Rule.ExtensionPattern)
                && (!Rule.DateFirst.HasValue && !Rule.DateLast.HasValue))
            {
                MessageBox.Show("El detalle de la regla es obligatorio.");
                return;
            }

            _isSaved = true;

            Close();
        }

        private void pnlSaveRule_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SaveRule();
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_isSaved)
            {
                Rule = null;
            }
        }
    }
}
