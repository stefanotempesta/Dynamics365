using Microsoft.ServiceBus.Messaging;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonDataIntegrationPatterns.Correlation
{
    internal class EntityMap
    {
        private EntityMap()
        {
        }

        protected Dictionary<(string SystemName, string PrimaryKey), (Type EntityType, Guid EntityId)> map =
            new Dictionary<(string SystemName, string PrimaryKey), (Type EntityType, Guid EntityId)>();

        private static EntityMap _instance;
        public static EntityMap Current => _instance ?? (_instance = new EntityMap());
        
        public T MapToEntity<T>(BrokeredMessage message) where T : Entity, new()
        {
            string systemName = message.Properties["SystemName"] as string;
            string primaryKey = message.Properties["PrimaryKey"] as string;

            T entity = BuildEntity<T>(message);
            map.Add((systemName, primaryKey), (entity.GetType(), entity.Id));

            return entity;
        }

        private T BuildEntity<T>(BrokeredMessage message) where T : Entity, new()
        {
            T entity = new T();
            object obj = JsonConvert.DeserializeObject(message.GetBody<string>());

            // Use reflection to fill entity attributes with property values of the object
            obj.GetType().GetProperties().ToList().ForEach(p =>
            {
                entity.Attributes[p.Name] = p.GetValue(obj);
            });

            return entity;
        }

        public (Type EntityType, Guid EntityId) GetEntityId(string systemName, string primaryKey)
        {
            return map[(systemName, primaryKey)];
        }
    }
}
