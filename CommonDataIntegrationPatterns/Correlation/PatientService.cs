using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonDataIntegrationPatterns.Correlation
{
    public class PatientService : IPatientService
    {
        public PatientService(Guid id)
        {
            this.patient = dataContext.Patients.Find(id);
        }

        public Guid AddMedicalRecord(MedicalRecord medicalRecord)
        {
            // If the patient already has this medical record
            // Return its correlation ID
            Guid? correlationId = this.patient.MedicalRecords.SingleOrDefault(r => r.Id == medicalRecord.Id)?.CorrelationId;
            if (correlationId.HasValue)
            {
                return correlationId.Value;
            }

            // Otherwise, generate a new correlation ID
            // Persist the medical record to the data context
            medicalRecord.CorrelationId = Guid.NewGuid();
            dataContext.MedicalRecords.Add(medicalRecord);
            dataContext.SaveChanges();

            return medicalRecord.CorrelationId;
        }

        public Patient Retrieve()
        {
            return this.patient;
        }

        private DataContext dataContext = new DataContext();
        private Patient patient;
    }
}
