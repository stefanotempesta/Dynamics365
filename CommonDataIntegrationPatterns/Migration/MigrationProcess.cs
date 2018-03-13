using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Migration
{
    public class MigrationProcess
    {
        public async Task Run()
        {
            using (var apiClient = new Dynamics365ApiClient(serviceUrl, clientId, redirectUrl))
            {
                Guid contactId = await CreateContact(apiClient);
                await UpdateContact(apiClient, contactId);
                await UpdateCountry(apiClient, contactId);
            }
        }

        private async Task<Guid> CreateContact(Dynamics365ApiClient apiClient)
        {
            var fields = new Dictionary<string, object>
            {
                ["firstName"] = "Stefano",
                ["lastName"] = "Tempesta",
                ["country"] = "Switzerland"
            };

            Guid entityId = await apiClient.CreateEntityAsync("contacts", fields);
            return entityId;
        }

        private async Task UpdateContact(Dynamics365ApiClient apiClient, Guid contactId)
        {
            var fields = new Dictionary<string, object>
            {
                ["firstName"] = "Stefano",
                ["lastName"] = "Tempesta",
                ["country"] = "Italy"
            };

            await apiClient.UpdateEntityAsync("contacts", contactId, fields);
        }

        private async Task UpdateCountry(Dynamics365ApiClient apiClient, Guid contactId)
        {
            var fields = new Dictionary<string, object>
            {
                ["country"] = "Italy"
            };

            await apiClient.UpdatePropertyAsync("contacts", contactId, fields);
        }

        private const string serviceUrl = "https://mydomain.crm.dynamics.com/";
        private const string clientId = "<app-reg-guid>";
        private const string redirectUrl = "<redirect-URL>";
    }
}
