using Microsoft.ServiceBus.Messaging;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using Newtonsoft.Json;
using System;
using System.Activities;

namespace MessageQueueing
{
    public class MyWorkflow : CodeActivity
    {
        protected override void Execute(CodeActivityContext executionContext)
        {
            ITracingService tracer = executionContext.GetExtension<ITracingService>();
            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            try
            {
                Entity entity = null;
                var client = QueueClient.CreateFromConnectionString(connectionString, queueName);

                client.OnMessage(message =>
                {
                    entity = JsonConvert.DeserializeObject<Entity>(message.GetBody<string>());
                });
            }
            catch (Exception e)
            {
                throw new InvalidPluginExecutionException(e.Message);
            }
        }

        private readonly string connectionString = "<ConnectionString>";
        private readonly string queueName = "<QueueName>";
    }
}
