using System;
using System.Collections.Generic;
using Memento.Persistence;
using Memento.Persistence.Commons;
using Pandora.dpl;
using Memento.Persistence.Commons.Annotations;

namespace FileMonkey.Pandora.dal.entities
{
    public class Inspector : Entity
    {
        public enum TypeActions
        {
            MoveSubDir,
            DeleteFiles
        }

        [PrimaryKey(Generator = KeyGenerationType.Memento)]
        public int? InspectorId
        {
            set { Set(value); }
            get { return Get<int?>(); }
        }

        public String Name
        {
            set { Set(value); }
            get { return Get<string>(); }
        }
        public String Path
        {
            set { Set(value); }
            get { return Get<string>(); }
        }
        public int? CheckPeriod
        {
            set { Set(value); }
            get { return Get<int?>(); }
        }
        public Boolean? Enable
        {
            set { Set(value); }
            get { return Get<Boolean?>(); }
        }
        public int? Action
        {
            set { Set(value); }
            get { return Get<int?>(); }
        }
        public String SubDirAction
        {
            set { Set(value); }
            get { return Get<string>(); }
        }

        [Relation("Inspector", RelationType.Dependences)]
        public Dependences<RuleFile> Rules
        {
            set { Set(value); }
            get { return Get<Dependences<RuleFile>>(); }
        }

        public Inspector()
        {
            Enable = true;
        }

        [Transient]
        public String CheckPeriodText { get; set; }
        [Transient]
        public String ImageEnable { get; set; }
        [Transient]
        public IList<InspectorHelper> RulesAux { get; set; }
    }
}
