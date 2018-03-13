using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CommonDataIntegrationPatterns.Correlation
{
    [ServiceContract]
    public interface IPatientService
    {
        [OperationContract] Guid AddMedicalRecord(MedicalRecord medicalRecord);

        [OperationContract] Patient Retrieve();
    }
}
