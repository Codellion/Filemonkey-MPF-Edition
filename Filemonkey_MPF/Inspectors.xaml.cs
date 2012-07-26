using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FileMonkey.Pandora.dal.entities;
using Memento.Persistence;
using Memento.Persistence.Interfaces;
using Pandora.dpl;

namespace FileMonkey.Picasso
{
    /// <summary>
    /// Lógica de interacción para Inspectors.xaml
    /// </summary>
    public partial class Inspectors : Window
    {
        public Inspectors()
        {
            InitializeComponent();

            Owner = App.Home;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InspectorsRefresh(null);
        }

        private void InspectorsRefresh(Inspector inspector, Boolean added = false)
        {
            var lstInspectors = new List<Inspector>();

            if (inspector == null)
            {
                IPersistence<FileInspector> servInspF = new Persistence<FileInspector>();

                lstInspectors.AddRange(servInspF.GetEntities(new FileInspector(){Enable = null}));
                lstInspectors.ForEach(FillRulesAux);

                IPersistence<ServiceInspector> servInspV = new Persistence<ServiceInspector>();

                lstInspectors.AddRange(servInspV.GetEntities());
            }
            else
            {
                lstInspectors = (List<Inspector>)lstVInspectors.ItemsSource;

                if (added)
                {
                    FillRulesAux(inspector);
                    lstInspectors.Add(inspector);
                }
                else
                    lstInspectors.Remove(inspector);                
            }

            lstVInspectors.ItemsSource = null;
            lstVInspectors.ItemsSource = lstInspectors;
        }

        private void FillRulesAux(Inspector fInspector)
        {
            var inspector = fInspector as FileInspector;

            if (!inspector.Enable.HasValue || !inspector.CheckPeriod.HasValue)
                return; 

            inspector.ImageEnable = inspector.Enable.Value ? @"/Resources/play.png" : @"/Resources/pausa.png";

            inspector.CheckPeriodText = InspectorDetail.GetPeriodText(inspector.CheckPeriod.Value) + " ";

            inspector.RulesAux = new List<InspectorHelper>(3);

            var queryRule = from rule in inspector.Rules.Value
                             where rule.RuleType.Equals((int)RuleFile.TypeFileRule.FileName)
                             select rule;

            var inspHelper = new InspectorHelper
                                 {
                                     CountRuleType = queryRule.Count(),
                                     Type = RuleFile.TypeFileRule.FileName,
                                     ImagePath = @"/images/icon_ruleFileName.png"
                                 };

            inspector.RulesAux.Add(inspHelper);

            queryRule = from rule in inspector.Rules.Value
                        where rule.RuleType.Equals((int)RuleFile.TypeFileRule.Extension)
                         select rule;

            inspHelper = new InspectorHelper
                             {
                                 CountRuleType = queryRule.Count(),
                                 Type = RuleFile.TypeFileRule.Extension,
                                 ImagePath = @"/images/icon_ruleFileExtension.png"
                             };

            inspector.RulesAux.Add(inspHelper);

            queryRule = from rule in inspector.Rules.Value
                        where rule.RuleType.Equals((int)RuleFile.TypeFileRule.Date)
                         select rule;

            inspHelper = new InspectorHelper
                             {
                                 CountRuleType = queryRule.Count(),
                                 Type = RuleFile.TypeFileRule.Date,
                                 ImagePath = @"/images/icon_ruleFileDate.png"
                             };

            inspector.RulesAux.Add(inspHelper);
        }

        private void lstVInspectors_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lstVInspectors.SelectedIndex != -1)
            {
                var inspSelected = (FileInspector)lstVInspectors.SelectedValue;

                var inspDetail = new InspectorDetail(inspSelected);
                inspDetail.ShowDialog();

                var lstInspectors = (List<FileInspector>)lstVInspectors.ItemsSource;

                inspSelected = inspDetail.Inspector;

                FillRulesAux(inspSelected);

                lstVInspectors.ItemsSource = null;
                lstVInspectors.ItemsSource = lstInspectors;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            App.Home.Focus();
        }

        private void pnlNewInspector_MouseUp(object sender, MouseButtonEventArgs e)
        {
            pnlNewInspector.ContextMenu.IsOpen = true;
        }

        private void pnlDeleteInspector_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (lstVInspectors.SelectedIndex != -1)
            {
                var inspSelected = (Inspector)lstVInspectors.SelectedValue;

                IPersistence<Inspector> servInsp = new Persistence<Inspector>();

                servInsp.DeleteEntity(inspSelected);
                App.Single.RemoveWork(inspSelected);
                
                InspectorsRefresh(inspSelected);
            }
            else
            {
                MessageBox.Show("Por favor, seleccione un inspector de la lista");
            }    
        }

        private void panels_MouseEnter(object sender, MouseEventArgs e)
        {
            var panel = (StackPanel)sender;
            var label = (Label)panel.Children[1];
            label.FontWeight = FontWeights.ExtraBold;
        }

        private void panels_MouseLeave(object sender, MouseEventArgs e)
        {
            var panel = (StackPanel)sender;
            var label = (Label)panel.Children[1];
            label.FontWeight = FontWeights.Normal;
        }

        private void pnlActDesactInspector_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (lstVInspectors.SelectedIndex != -1)
            {
                var inspSelected = (Inspector)lstVInspectors.SelectedValue;

                if(inspSelected.InspectorType == (int) Inspector.EnumInspectorType.File)
                {
                    IPersistence<FileInspector> servInsp = new Persistence<FileInspector>();
                    inspSelected.Enable = !inspSelected.Enable;

                    servInsp.PersistEntity(inspSelected as FileInspector);
                }

                if (inspSelected.InspectorType == (int)Inspector.EnumInspectorType.Service)
                {
                    IPersistence<ServiceInspector> servInsp = new Persistence<ServiceInspector>();
                    inspSelected.Enable = !inspSelected.Enable;

                    servInsp.PersistEntity(inspSelected as ServiceInspector);
                }
            
                if (inspSelected.Enable.Value)
                {
                    inspSelected.ImageEnable = @"/Resources/play.png";

                    App.Single.UpdateWork(inspSelected);
                }
                else
                {
                    inspSelected.ImageEnable = @"/Resources/pausa.png";

                    App.Single.RemoveWork(inspSelected);
                }

                var lstInspectors = (List<Inspector>)lstVInspectors.ItemsSource;

                lstVInspectors.ItemsSource = null;
                lstVInspectors.ItemsSource = lstInspectors;
            }
            else
            {
                MessageBox.Show("Por favor, seleccione un inspector de la lista");
            }    
        }

        private void mnServiceR_Click(object sender, RoutedEventArgs e)
        {
            var inspector = new ServiceInspector();

            var inspDetail = new ServiceInspectorDetail(inspector);
            inspDetail.ShowDialog();

            inspector = inspDetail.Inspector;

            if (inspector.InspectorId.HasValue)
            {
                InspectorsRefresh(inspector, true);
            }
        }

        private void mnFileR_Click(object sender, RoutedEventArgs e)
        {
            var inspector = new FileInspector { Path = "Seleccione una carpeta para rastrear" };

            var inspDetail = new InspectorDetail(inspector);
            inspDetail.ShowDialog();

            inspector = inspDetail.Inspector;

            if (inspector.InspectorId.HasValue)
            {
                InspectorsRefresh(inspector, true);
            }
        }
    }
}
