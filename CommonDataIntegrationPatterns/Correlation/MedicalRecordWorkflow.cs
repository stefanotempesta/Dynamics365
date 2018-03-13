using AutoMapper;
using Microsoft.ServiceBus.Messaging;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using Newtonsoft.Json;
using System;
using System.Activities;
using System.Threading.Tasks;

namespace CommonDataIntegrationPatterns.Correlation
{
    public class MedicalRecordWorkflow : CodeActivity
    {
        protected override void Execute(CodeActivityContext executionContext)
        {
            ITracingService tracer = executionContext.GetExtension<ITracingService>();
            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            // Retrieve the medical record entity on which the workflow executed
            // Send the medical record for registration with other external systems
            MedicalRecordEntity medicalRecord = (MedicalRecordEntity)service.Retrieve(context.PrimaryEntityName, context.PrimaryEntityId, new ColumnSet(allColumns: true));
            Task.Run(async () => await RegisterMedicalRecord(medicalRecord));
        }

        private async Task RegisterMedicalRecord(MedicalRecordEntity entity)
        {
            // First map the XRM entity to a Data Contract using AutoMapper
            // Then add the medical record to its patient via a Patient service
            // A correlation ID identifies the medical record associated to its patient uniquely
            Mapper.Initialize(cfg => cfg.CreateMap<MedicalRecordEntity, MedicalRecord>());
            MedicalRecord medicalRecord = Mapper.Map<MedicalRecord>(entity);
            medicalRecord.CorrelationId = new PatientService(medicalRecord.PatientId).AddMedicalRecord(medicalRecord);

            // Create a connection to an Azure Service Bus Topic
            // Serialize the medical record and add the correlation ID as an extra property of the message
            var client = TopicClient.CreateFromConnectionString(connectionString, topicName);
            var message = new BrokeredMessage(JsonConvert.SerializeObject(medicalRecord));
            // Send the message to the Topic
            await client.SendAsync(message);
        }

        private readonly string connectionString = "<ConnectionString>";
        private readonly string topicName = "<TopicName>";
    }
}
