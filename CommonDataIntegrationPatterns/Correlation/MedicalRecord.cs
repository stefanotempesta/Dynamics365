using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CommonDataIntegrationPatterns.Correlation
{
    [DataContract]
    public class MedicalRecord
    {
        [DataMember] public Guid Id { get; set; }

        [DataMember] public Guid CorrelationId { get; set; }

        [ForeignKey("PatientId")]
        public virtual Patient Patient { get; set; }

        [DataMember] public Guid PatientId { get; set; }

        [DataMember] public DateTime RecordedOn { get; set; }

        [DataMember] public string Description { get; set; }
    }
}
