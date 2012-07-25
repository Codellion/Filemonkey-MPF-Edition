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
    /// Lógica de interacción para Name.xaml
    /// </summary>
    public partial class Name : UserControl, RuleControl
    {
        public RuleFile rule { get; set; }

        public Name()
        {
            InitializeComponent();
        }

        public RuleFile GetRule()
        {
            if (rule == null)
                rule = new RuleFile();

            rule.NamePattern = txtNombre.Text;

            return rule;
        }

        public void SetRule(RuleFile rule)
        {
            this.rule = (RuleFile)rule;

            txtNombre.Text = this.rule.NamePattern;
        }
    }
}
