using System;
using Memento.Persistence.Commons;
using Memento.Persistence.Commons.Annotations;
using Memento.Persistence;

namespace FileMonkey.Pandora.dal.entities
{
    public class RuleFile: Entity
    {
        public enum TypeRuleFilaDate
        {
            Period,
            Dfirst,
            AfterDfirst,
            BeforeDfirst,
            Dlast,
            AfterDlast,
            BeforeDlast
        }

        public enum TypeFileRule
        {
            Date,
            Extension,
            FileName
        }

        [PrimaryKey(Generator = KeyGenerationType.Memento)]
        public int? RuleFileId
        {
            set { Set(value); }
            get { return Get<int?>(); }
        }

        [Field(Name = "Name")]
        public String RuleName
        {
            set { Set(value); }
            get { return Get<string>(); }
        }

        [Field]
        public int? RuleType
        {
            set { Set(value); }
            get { return Get<int?>(); }
        }

        public DateTime? DateFirst
        {
            set { Set(value); }
            get { return Get<DateTime?>(); }
        }
        public DateTime? DateLast
        {
            set { Set(value); }
            get { return Get<DateTime?>(); }
        }

        public String ExtensionPattern
        {
            set { Set(value); }
            get { return Get<String>(); }
        }

        public String NamePattern
        {
            set { Set(value); }
            get { return Get<String>(); }
        }

        [Relation("Rules", RelationType.Reference)]
        public Reference<Inspector> Inspector { set; get; }

        [Transient]
        public String ImagePath { get; set; }
    }
}