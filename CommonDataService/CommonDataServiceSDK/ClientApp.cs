using Microsoft.CommonDataService.CommonEntitySets;
using Microsoft.CommonDataService.Configuration;
using Microsoft.CommonDataService.ServiceClient.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonDataServiceSDK
{
    public class ClientApp
    {
        public async Task Run()
        {
            var leads = new[]
            {
                new Lead
                {
                    FullName = "John Smith",
                    IsEmailContactAllowed = true,
                    IsPhoneContactAllowed = true,
                    OrganizationName = "Contoso"
                },
                new Lead
                {
                    FullName = "Jane Green",
                    IsEmailContactAllowed = true,
                    IsPhoneContactAllowed = false,
                    OrganizationName = "NewCo"
                }
            };

            await Insert(leads);

            await InsertAndRetrieve(leads);

            await Select();

            await Update(leads);

            await Delete(leads);
        }

        async Task Insert(Lead[] leads)
        {
            using (var client = ConnectionSettings.Instance.CreateClient().Result)
            {
                var import = new DataImport(client);
                await import.Insert(leads);
            }
        }

        async Task InsertAndRetrieve(Lead[] leads)
        {
            using (var client = ConnectionSettings.Instance.CreateClient().Result)
            {
                var import = new DataImport(client);

                // List of retrieved leads after import, with operational result (completed / faulted / exception)
                var result = await import.InsertAndRetrieve(leads);
            }
        }

        async Task Select()
        {
            using (var client = ConnectionSettings.Instance.CreateClient().Result)
            {
                var import = new DataImport(client);

                // List of leads with the given conditions
                var result = await import.Select<Lead>(
                    new[] { "FullName", "OrganizationName" },       // select
                    l => l.IsEmailContactAllowed == true);          // where
            }
        }

        async Task Update(Lead[] leads)
        {
            using (var client = ConnectionSettings.Instance.CreateClient().Result)
            {
                var import = new DataImport(client);
                await import.Update(leads, "PersonName", "Stefano Tempesta");
            }
        }

        async Task Delete(Lead[] leads)
        {
            using (var client = ConnectionSettings.Instance.CreateClient().Result)
            {
                var import = new DataImport(client);
                await import.Delete(leads);
            }
        }
    }
}
