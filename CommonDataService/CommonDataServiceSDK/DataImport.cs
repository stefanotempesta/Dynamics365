using Microsoft.CommonDataService;
using Microsoft.CommonDataService.Builders;
using Microsoft.CommonDataService.CommonEntitySets;
using Microsoft.CommonDataService.Configuration;
using Microsoft.CommonDataService.Entities;
using Microsoft.CommonDataService.ServiceClient.Security;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonDataServiceSDK
{
    public class DataImport
    {
        public DataImport(Client client)
        {
            _client = client;
        }
        private Client _client { get; set; }

        public async Task Insert(IList<RelationalEntity> entities)
        {
            await _client
                .CreateRelationalBatchExecuter(RelationalBatchExecutionMode.Transactional)
                .Insert(entities)
                .ExecuteAsync();
        }

        public async Task<IEnumerable<OperationResult<RelationalEntity>>> InsertAndRetrieve(IList<RelationalEntity> entities)
        {
            var output = new OperationResult<RelationalEntity>[entities.Count];
            int index = 0;

            var executer = _client.CreateRelationalBatchExecuter(RelationalBatchExecutionMode.Transactional);

            foreach (var entity in entities)
            {
                executer.InsertAndRetrieveEntity(entity, out output[index++]);
            }

            await executer.ExecuteAsync();

            return output.AsEnumerable();
        }

        public async Task<IEnumerable<T>> Select<T>(string[] selectFields, Func<T, bool> whereClause) where T : TypedRelationalEntity, new()
        {
            var queryBuilder = _client.GetRelationalEntitySet<T>()
                .CreateQueryBuilder();

            var query = queryBuilder
                .Where(e => whereClause(e))
                .Project(p => SelectFields(p, selectFields));

            OperationResult<IReadOnlyList<T>> queryResult = null;
            await _client.CreateRelationalBatchExecuter(RelationalBatchExecutionMode.Transactional)
                .Query<T>(query, out queryResult)
                .ExecuteAsync();

            return queryResult.Result;

            RelatedEntityIncludeBuilder<T> SelectFields(QueryEntityProjectionBuilder<T> builder, string[] fieldNames)
            {
                // Create a continuation builder from the first field from the query builder
                QueryEntityProjectionContinuationBuilder<T> projection = builder.SelectField(f => f[fieldNames[0]]);

                // If more fields are defined, select them using the continuation builder
                for (int i = 1; i < fieldNames.Length; i++)
                {
                    projection = projection.SelectField(f => f[fieldNames[i]]);
                }

                return projection;
            }
        }

        public async Task Update<T>(IEnumerable<T> entities, string fieldName, string newValue) where T : TypedRelationalEntity
        {
            var executer = _client.CreateRelationalBatchExecuter(RelationalBatchExecutionMode.Transactional);
            foreach (var entity in entities)
            {
                var updates = _client.CreateRelationalFieldUpdates<T>();
                updates.Update(e => e[fieldName], newValue);
                
                executer.Update(entity, updates);
            }

            await executer.ExecuteAsync();
        }

        public async Task Delete<T>(IEnumerable<T> entities) where T : TypedRelationalEntity
        {
            var executer = _client.CreateRelationalBatchExecuter(RelationalBatchExecutionMode.Transactional);
            foreach (var entity in entities)
            {
                executer.Delete(entity);
            }

            await executer.ExecuteAsync();
        }
    }
}
