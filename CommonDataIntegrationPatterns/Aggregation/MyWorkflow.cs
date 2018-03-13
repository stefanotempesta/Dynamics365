using Microsoft.ServiceBus.Messaging;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;

namespace CommonDataIntegrationPatterns.Correlation
{
    public class MyWorkflow : CodeActivity
    {
        protected override void Execute(CodeActivityContext executionContext)
        {
            ITracingService tracer = executionContext.GetExtension<ITracingService>();
            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            var client = SubscriptionClient.CreateFromConnectionString(connectionString, topicName, subscriptionName);
            client.OnMessage(message => {
                Entity entity = EntityMap.Current.MapToEntity<Entity>(message);

                var exists = service.Retrieve(entity.LogicalName, entity.Id, columnSet: new ColumnSet(new[] { "Id" })) != null;
                if (exists)
                    service.Update(entity);
                else
                    service.Create(entity);
            });
        }

        private readonly string connectionString = "<ConnectionString>";
        private readonly string topicName = "<TopicName>";
        private readonly string subscriptionName = "<SubscriptionName>";
    }
}
