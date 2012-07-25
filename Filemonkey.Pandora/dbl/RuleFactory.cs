
using System;
using FileMonkey.Pandora.dal.entities;

namespace Pandora.dbl
{
    public class RuleFactory
    {
        public static RuleFile GetFileRule(RuleFile.TypeFileRule type)
        {
            var res = new RuleFile {RuleType = (int) type};

            return res;
        }
    }
}
