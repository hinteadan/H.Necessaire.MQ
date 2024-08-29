using Dapper;
using H.Necessaire;
using H.Necessaire.Dapper;
using H.Necessaire.MQ.Abstractions;
using H.Necessaire.Runtime.SqlServer;
using H.Necessaire.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace H.Necessaire.MQ.Runtime.SqlServer.Concrete.Storage
{
    internal partial class SqlServerHmqEventReActionStorageService : DapperStorageServiceBase<Guid, HmqEventReactionLog, HmqEventReactionLogSqlEntry, HmqEventReActionFilter>
    {
        #region Construct
        public SqlServerHmqEventReActionStorageService() : base(connectionString: null, tableName: table, databaseName: "H.Necessaire.HMQ.Core") { }
        protected override Task<SqlMigration[]> GetAllMigrations() => sqlMigrations.AsTask();

        public override void ReferDependencies(ImADependencyProvider dependencyProvider)
        {
            connectionString = dependencyProvider.GetRuntimeConfig()?.Get("SqlConnections")?.Get("DefaultConnectionString")?.ToString();
            connectionStringWithoutDatabase = connectionString?.WithoutDatabase();
            databaseName = dependencyProvider.GetRuntimeConfig()?.Get("SqlConnections")?.Get("DatabaseNames")?.Get("HMQRegistry")?.ToString();
            if (!databaseName.IsEmpty())
                connectionString = connectionStringWithoutDatabase.WithDatabase(databaseName);

            base.ReferDependencies(dependencyProvider);
        }
        #endregion

        protected override ISqlFilterCriteria[] ApplyFilter(HmqEventReActionFilter filter, DynamicParameters sqlParams)
        {
            List<ISqlFilterCriteria> result = new List<ISqlFilterCriteria>();

            if (filter?.IDs?.Any() ?? false)
            {
                result.Add(new SqlFilterCriteria(columnName: nameof(HmqEventReactionLogSqlEntry.ID), parameterName: nameof(filter.IDs), @operator: "IN"));
            }

            if (filter?.From != null)
            {
                sqlParams.Add($"{nameof(filter.From)}Ticks", filter.From.Value.Ticks);
                result.Add(new SqlFilterCriteria(columnName: nameof(HmqEventReactionLogSqlEntry.AsOf), parameterName: $"{nameof(filter.From)}Ticks", @operator: ">="));
            }

            if (filter?.To != null)
            {
                sqlParams.Add($"{nameof(filter.To)}Ticks", filter.To.Value.Ticks);
                result.Add(new SqlFilterCriteria(columnName: nameof(HmqEventReactionLogSqlEntry.AsOf), parameterName: $"{nameof(filter.To)}Ticks", @operator: "<="));
            }

            if (filter?.EventIDs?.Any() ?? false)
            {
                result.Add(new SqlFilterCriteria(columnName: nameof(HmqEventReactionLogSqlEntry.EventID), parameterName: nameof(filter.EventIDs), @operator: "IN"));
            }

            if (filter?.EventsThatHappenedFrom != null)
            {
                sqlParams.Add($"{nameof(filter.EventsThatHappenedFrom)}Ticks", filter.To.Value.Ticks);
                result.Add(new SqlFilterCriteria(columnName: nameof(HmqEventReactionLogSqlEntry.EventHappenedAtTicks), parameterName: $"{nameof(filter.EventsThatHappenedFrom)}Ticks", @operator: ">="));
            }

            if (filter?.EventsThatHappenedTo != null)
            {
                sqlParams.Add($"{nameof(filter.EventsThatHappenedTo)}Ticks", filter.To.Value.Ticks);
                result.Add(new SqlFilterCriteria(columnName: nameof(HmqEventReactionLogSqlEntry.EventHappenedAtTicks), parameterName: $"{nameof(filter.EventsThatHappenedTo)}Ticks", @operator: "<="));
            }

            if (filter?.ActorIDs?.Any() == true)
            {
                result.Add(new SqlFilterCriteria(columnName: nameof(HmqEventReactionLogSqlEntry.ActorID), parameterName: nameof(filter.ActorIDs), @operator: "IN"));
            }

            if (filter?.IsSuccessful != null)
            {
                result.Add(new SqlFilterCriteria(columnName: nameof(HmqEventReactionLogSqlEntry.IsSuccessful), parameterName: nameof(filter.IsSuccessful.Value), @operator: "="));
            }

            return result.ToArray();
        }
    }

    internal class HmqEventReactionLogSqlEntry : SqlEntryBase, IGuidIdentity
    {
        public Guid ID { get; set; }
        public DateTime AsOf { get; set; }
        public long AsOfTicks { get; set; }

        public string EventJson { get; set; }
        public Guid EventID { get; set; }
        public DateTime EventHappenedAt { get; set; }
        public long EventHappenedAtTicks { get; set; }

        public string ActorIdentityJson { get; set; }
        public string ActorID { get; set; }

        public string OperationResultJson { get; set; }
        public bool IsSuccessful { get; set; }
    }

    internal class HmqEventReactionLogSqlEntryMapper : SqlEntityMapperBase<HmqEventReactionLog, HmqEventReactionLogSqlEntry>
    {
        static HmqEventReactionLogSqlEntryMapper() => new HmqEventReactionLogSqlEntryMapper().RegisterMapper();

        public override HmqEventReactionLogSqlEntry MapEntityToSql(HmqEventReactionLog entity)
        {
            return
                base
                .MapEntityToSql(entity)
                .And(x =>
                {
                    x.AsOfTicks = entity.AsOf.Ticks;
                    x.EventHappenedAtTicks = entity.EventHappenedAt.Ticks;
                    x.EventJson = entity.Event?.ToJsonObject();
                    x.ActorIdentityJson = entity.ActorIdentity?.ToJsonObject();
                    x.OperationResultJson = entity.OperationResult?.ToJsonObject();
                })
                ;
        }

        public override HmqEventReactionLog MapSqlToEntity(HmqEventReactionLogSqlEntry sqlEntity)
        {
            return
                base
                .MapSqlToEntity(sqlEntity)
                .And(x =>
                {
                    x.AsOf = new DateTime(sqlEntity.AsOfTicks);
                    x.Event = sqlEntity.EventJson?.JsonToObject<HmqEvent>();
                    x.Event.HappenedAt = new DateTime(sqlEntity.EventHappenedAtTicks);
                    x.ActorIdentity = sqlEntity.ActorIdentityJson?.JsonToObject<HmqActorIdentity>();
                    x.OperationResult = sqlEntity.OperationResultJson?.JsonToObject<OperationResult>();
                })
                ;
        }
    }

}
