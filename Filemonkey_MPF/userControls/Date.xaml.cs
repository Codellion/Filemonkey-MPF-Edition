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
using System.Windows.Navigation;
using System.Windows.Shapes;
using FileMonkey.Pandora.dal.entities;
using Elemental.pal.userControls.interfaces;

namespace Elemental.pal.userControls
{
    /// <summary>
    /// Lógica de interacción para Date.xaml
    /// </summary>
    public partial class Date : UserControl, RuleControl
    {
        public RuleFile rule { get; set; }

        public Date()
        {
            InitializeComponent();
        }

        public RuleFile GetRule()
        {
            if (rule == null)
                rule = new RuleFile();

            rule.DateFirst = dateFirst.SelectedDate.Value;
            rule.DateLast = dateLast.SelectedDate.Value;

            return rule;
        }

        public void SetRule(RuleFile rule)
        {
            this.rule = rule;

            dateFirst.SelectedDate = this.rule.DateFirst;            
            dateLast.SelectedDate = this.rule.DateLast;            
        }
    }
}
