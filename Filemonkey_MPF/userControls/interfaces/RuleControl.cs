using System;
using FileMonkey.Pandora.dal.entities;

namespace Elemental.pal.userControls.interfaces
{   
    public interface RuleControl
    {
        RuleFile GetRule();
        void SetRule(RuleFile rule);
    }
}