using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonDataIntegrationPatterns.Correlation
{
    class HospitalServer
    {
        public void ReceiveMedicalRecord()
        {
            var client = SubscriptionClient.CreateFromConnectionString(connectionString, topicName, subscriptionName);
            client.OnMessageAsync(async message =>
            {
                MedicalRecord medicalRecord = JsonConvert.DeserializeObject<MedicalRecord>(message.GetBody<string>());
                Patient patient = new PatientService(medicalRecord.PatientId).Retrieve();

                if (PatientExists(patient) && !MedicalRecordExists(medicalRecord))
                {
                    await SaveMedicalRecord(medicalRecord);
                }
            });
        }

        private bool PatientExists(Patient patient)
        {
            // Cannot find by ID as it may be different in each hospital
            // Search patient by biographical information
            return dataContext.Patients.FirstOrDefault(p =>
                p.FirstName == patient.FirstName &&
                p.LastName == patient.LastName &&
                p.DateOfBirth == patient.DateOfBirth &&
                p.Gender == patient.Gender
            ) != null;
        }

        private bool MedicalRecordExists(MedicalRecord medicalRecord)
        {
            // Cannot find by ID as it may be different in each hospital
            // Use correlation ID
            return dataContext.MedicalRecords
                .SingleOrDefault(r => r.CorrelationId == medicalRecord.CorrelationId) != null;
        }

        private async Task SaveMedicalRecord(MedicalRecord medicalRecord)
        {
            dataContext.MedicalRecords.Add(medicalRecord);
            await dataContext.SaveChangesAsync();
        }

        private DataContext dataContext = new DataContext();

        private readonly string connectionString = "<ConnectionString>";
        private readonly string topicName = "<TopicName>";
        private readonly string subscriptionName = "<SubscriptionName>";
    }
}
