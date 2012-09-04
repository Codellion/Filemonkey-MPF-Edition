using System;
using Memento.Persistence;
using Memento.Persistence.Commons.Annotations;

namespace FileMonkey.Pandora.dal.entities
{
    [Table(Name = "Inspector")]
    public class ServiceInspector : Inspector
    {
        public String ServiceName
        {
            set { Set(value); }
            get { return Get<string>(); }
        }

        public ServiceInspector() :base()
        {
            InspectorType = (int) EnumInspectorType.Service;
        }
    }
}
