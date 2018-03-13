using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Migration
{
    public class Dynamics365ApiClient : IDisposable
    {
        public Dynamics365ApiClient(string serviceUrl, string clientId, string redirectUrl)
        {
            ServiceUrl = serviceUrl;
            ClientId = clientId;
            RedirectUrl = redirectUrl;

            // Handle API authentication
            HttpMessageHandler messageHandler = new OAuthMessageHandler(this.ServiceUrl, this.ClientId, this.RedirectUrl, new HttpClientHandler());

            // Create an HTTP connection to the API
            _httpClient = new HttpClient(messageHandler)
            {
                BaseAddress = new Uri(this.ServiceUrl),
                Timeout = new TimeSpan(0, 1, 0) // 1 minute
            };
            _httpClient.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
            _httpClient.DefaultRequestHeaders.Add("OData-Version", "4.0");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Retrieve the current API version
            GetWebAPIVersion().GetAwaiter();
        }

        public async Task<Guid> CreateEntityAsync(string entityPluralName, IDictionary<string, object> fields)
        {
            var entityObject = CreateEntityObject(entityPluralName, fields);
            return await SendAsync(HttpMethod.Post, entityObject);
        }

        public async Task UpdateEntityAsync(string entityPluralName, Guid entityId, IDictionary<string, object> fields)
        {
            var entityObject = CreateEntityObject(entityPluralName, fields);
            await SendAsync(new HttpMethod("PATCH"), entityObject, entityId);
        }

        public async Task UpdatePropertyAsync(string entityPluralName, Guid entityId, IDictionary<string, object> fields)
        {
            var entityObject = CreateEntityObject(entityPluralName, fields);
            foreach (var property in entityObject.Properties())
            {
                var propertyObject = new JObject();
                propertyObject.Add("value", property.Value);

                await SendAsync(HttpMethod.Put, propertyObject, entityId, property.Name);
            }
        }

        private JObject CreateEntityObject(string entityPluralName, IDictionary<string, object> fields)
        {
            JObject entityObject = new JObject();
            entityObject.AddAnnotation(entityPluralName);
            foreach (var field in fields)
            {
                entityObject.Add(field.Key, new JValue(field.Value));
            }

            return entityObject;
        }

        private async Task<Guid> SendAsync(HttpMethod method, JObject entity)
        {
            return await SendAsync(method, entity, Guid.Empty);
        }

        private async Task<Guid> SendAsync(HttpMethod method, JObject entity, Guid entityId)
        {
            return await SendAsync(method, entity, entityId, string.Empty);
        }

        private async Task<Guid> SendAsync(HttpMethod method, JObject entity, Guid entityId, string property)
        {
            string apiName = entity.Annotation<string>();
            string requestUri = $"api/data/v{_apiVersion}/{apiName}" +
                (entityId != Guid.Empty ? $"/({entityId.ToString()})" : string.Empty) +
                (property != string.Empty ? $"/{property}" : string.Empty);

            var request = new HttpRequestMessage(method, requestUri)
            {
                Content = new StringContent(entity.ToString(), Encoding.UTF8, "application/json")
            };

            var response = await _httpClient.SendAsync(request);
            if (response.StatusCode != HttpStatusCode.NoContent)
            {
                // HTTP Code 204 = No Content expected, otherwise throw an exception
                throw new ApplicationException(response.Content.ToString());
            }

            // Return the Entity ID
            // For new entities, it is defined in the OData-EntityId header value
            // [Organization URI]/api/data/v8.2/accounts(00000000-0000-0000-0000-000000000001)
            return entityId != Guid.Empty ? entityId : ReadEntityId(response);
        }

        private Guid ReadEntityId(HttpResponseMessage response)
        {
            string entityUri = response.Headers.GetValues("OData-EntityId").First();
            return Guid.Parse(entityUri.Substring(entityUri.IndexOf("(") + 1, 36));
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        // Start with base version and update with actual version later
        private Version _apiVersion = new Version(8, 0);

        private async Task GetWebAPIVersion()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"v{_apiVersion.ToString(2)}/RetrieveVersion");
            var response = await _httpClient.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                JObject version = JsonConvert.DeserializeObject<JObject>(await response.Content.ReadAsStringAsync());

                // Retrieve the actual version available in this CRM organization
                _apiVersion = Version.Parse((string)version.GetValue("Version"));
            }
        }

        public string ServiceUrl { get; set; }
        public string ClientId { get; set; }
        public string RedirectUrl { get; set; }

        private HttpClient _httpClient = null;
    }
}
