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
        public enum EnumInspectorType
        {
            File,
            Service
        }

        [PrimaryKey(Generator = KeyGenerationType.Memento)]
        [Field(Name = "InspectorId")]
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

        [Field(Name = "Push")]
        public Boolean? EnablePushNotification
        {
            set { Set(value); }
            get { return Get<Boolean?>(); }
        }

        public int? InspectorType
        {
            set { Set(value); }
            get { return Get<int?>(); }
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
