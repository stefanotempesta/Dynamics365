using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CommonDataIntegrationPatterns.Correlation
{
    [DataContract]
    public class Patient
    {
        [DataMember] public Guid Id { get; set; }

        [DataMember] public string FirstName { get; set; }

        [DataMember] public string LastName { get; set; }

        [DataMember] public DateTime DateOfBirth { get; set; }

        [DataMember] public char Gender { get; set; }

        public virtual ICollection<MedicalRecord> MedicalRecords { get; set; }
    }
}
