using Microsoft.ServiceBus.Messaging;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace CommonDataIntegrationPatterns.Broadcast
{
    public class MyPlugin : IPlugin
    {
        #region Secure/Unsecure Configuration Setup
        private string _secureConfig = null;
        private string _unsecureConfig = null;

        public MyPlugin(string unsecureConfig, string secureConfig)
        {
            _secureConfig = secureConfig;
            _unsecureConfig = unsecureConfig;
        }
        #endregion

        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracer = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = factory.CreateOrganizationService(context.UserId);

            Entity entity = (Entity)context.InputParameters["Target"];
            Task.Run(async () => await Broadcast(entity));
        }

        private async Task Broadcast(Entity entity)
        {
            var client = TopicClient.CreateFromConnectionString(connectionString, topicName);
            var message = new BrokeredMessage(JsonConvert.SerializeObject(entity));

            await client.SendAsync(message);
        }

        private readonly string connectionString = "<ConnectionString>";
        private readonly string topicName = "<TopicName>";
    }
}
