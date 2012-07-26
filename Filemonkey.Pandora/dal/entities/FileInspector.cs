using System;
using Memento.Persistence;
using Memento.Persistence.Commons.Annotations;

namespace FileMonkey.Pandora.dal.entities
{
    [Table(Name = "Inspector")]
    public class FileInspector : Inspector
    {
        public enum TypeActions
        {
            MoveSubDir,
            DeleteFiles
        }

        public String Path
        {
            set { Set(value); }
            get { return Get<string>(); }
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

        public FileInspector(): base()
        {
            InspectorType = (int) EnumInspectorType.File;
        }
    }
}
