using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CommonDataIntegrationPatterns.Correlation
{
    class MedicalRecordEntity : Entity
    {
        public Guid PatientId { get; set; }

        public DateTime RecordedOn { get; set; }

        public string Description { get; set; }
    }
}
