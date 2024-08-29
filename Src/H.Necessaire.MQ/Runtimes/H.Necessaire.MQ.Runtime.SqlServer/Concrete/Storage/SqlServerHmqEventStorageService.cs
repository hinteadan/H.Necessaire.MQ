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
    internal partial class SqlServerHmqEventStorageService : DapperStorageServiceBase<Guid, HmqEvent, HmqEventSqlEntry, HmqEventFilter>
    {
        #region Construct
        public SqlServerHmqEventStorageService() : base(connectionString: null, tableName: table, databaseName: "H.Necessaire.HMQ.Core") { }
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
        protected override ISqlFilterCriteria[] ApplyFilter(HmqEventFilter filter, DynamicParameters sqlParams)
        {
            List<ISqlFilterCriteria> result = new List<ISqlFilterCriteria>();

            if (filter?.IDs?.Any() ?? false)
            {
                result.Add(new SqlFilterCriteria(columnName: nameof(HmqEventSqlEntry.ID), parameterName: nameof(filter.IDs), @operator: "IN"));
            }

            if (filter?.From != null)
            {
                sqlParams.Add($"{nameof(filter.From)}Ticks", filter.From.Value.Ticks);
                result.Add(new SqlFilterCriteria(columnName: nameof(HmqEventSqlEntry.HappenedAtTicks), parameterName: $"{nameof(filter.From)}Ticks", @operator: ">="));
            }

            if (filter?.To != null)
            {
                sqlParams.Add($"{nameof(filter.To)}Ticks", filter.To.Value.Ticks);
                result.Add(new SqlFilterCriteria(columnName: nameof(HmqEventSqlEntry.HappenedAtTicks), parameterName: $"{nameof(filter.To)}Ticks", @operator: "<="));
            }

            if (filter?.Names?.Any() ?? false)
            {
                result.Add(new SqlFilterCriteria(columnName: nameof(HmqEventSqlEntry.Name), parameterName: nameof(filter.Names), @operator: "IN"));
            }

            if (filter?.Types?.Any() ?? false)
            {
                result.Add(new SqlFilterCriteria(columnName: nameof(HmqEventSqlEntry.Type), parameterName: nameof(filter.Types), @operator: "IN"));
            }

            if (filter?.Assemblies?.Any() ?? false)
            {
                result.Add(new SqlFilterCriteria(columnName: nameof(HmqEventSqlEntry.Assembly), parameterName: nameof(filter.Assemblies), @operator: "IN"));
            }

            return result.ToArray();
        }

    }

    internal class HmqEventSqlEntry : SqlEntryBase, IGuidIdentity
    {
        public Guid ID { get; set; }
        public DateTime HappenedAt { get; set; }
        public long HappenedAtTicks { get; set; }
        public string RaisedByJson { get; set; }
        public string RaisedByID { get; set; }


        public string Name { get; set; }

        public string Type { get; set; }

        public string Assembly { get; set; }


        public string AttributesJson { get; set; }


        public string DataJson { get; set; }
    }

    internal class HmqEventSqlEntityMapper : SqlEntityMapperBase<HmqEvent, HmqEventSqlEntry>
    {
        static HmqEventSqlEntityMapper() => new HmqEventSqlEntityMapper().RegisterMapper();

        public override HmqEventSqlEntry MapEntityToSql(HmqEvent entity)
        {
            return
                base
                .MapEntityToSql(entity)
                .And(x =>
                {
                    x.HappenedAtTicks = entity.HappenedAt.Ticks;
                    x.RaisedByJson = entity.RaisedBy?.ToJsonObject();
                    x.AttributesJson = entity.Attributes?.ToJsonArray();
                    x.DataJson = entity.Data?.ToJsonObject();
                })
                ;
        }

        public override HmqEvent MapSqlToEntity(HmqEventSqlEntry sqlEntity)
        {
            return
                base
                .MapSqlToEntity(sqlEntity)
                .And(x =>
                {
                    x.HappenedAt = new DateTime(sqlEntity.HappenedAtTicks);
                    x.RaisedBy = sqlEntity.RaisedByJson?.JsonToObject<HmqActorIdentity>();
                    x.Attributes = sqlEntity.AttributesJson?.DeserializeToNotes();
                    x.Data = sqlEntity.DataJson?.JsonToObject<object>();
                })
                ;
        }
    }
}
