using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FileMonkey.Pandora.dal.entities;

namespace Pandora.dpl
{
    public class InspectorHelper
    {
        public String ImagePath { get; set; }
        public int CountRuleType { get; set; }
        public RuleFile.TypeFileRule Type {get; set;}        
    }
}
