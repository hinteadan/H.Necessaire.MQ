using H.Necessaire.Dapper;
using H.Necessaire.MQ.Abstractions;

namespace H.Necessaire.MQ.Runtime.SqlServer.Concrete.Storage
{
    internal partial class SqlServerHmqEventReActionStorageService
    {
        const string table = "H.Necessaire.HMQ.HmqEventReactionLog";

        static readonly SqlMigration[] sqlMigrations = new SqlMigration[] {
            new SqlMigration
            {
                ResourceIdentifier = "HmqEventReactionLog",
                VersionNumber = new VersionNumber(1, 0),
                 SqlCommand = $@"
CREATE TABLE [dbo].[{table}]
(
    [{nameof(HmqEventReactionLogSqlEntry.ID)}] [uniqueidentifier] NOT NULL,
    [{nameof(HmqEventReactionLogSqlEntry.AsOf)}] [datetime2](7) NOT NULL,
    [{nameof(HmqEventReactionLogSqlEntry.AsOfTicks)}] [bigint] NOT NULL,
    [{nameof(HmqEventReactionLogSqlEntry.EventJson)}] [nvarchar](MAX) NULL,
    [{nameof(HmqEventReactionLogSqlEntry.EventID)}] [uniqueidentifier] NULL,
    [{nameof(HmqEventReactionLogSqlEntry.EventHappenedAt)}] [datetime2](7) NULL,
    [{nameof(HmqEventReactionLogSqlEntry.EventHappenedAtTicks)}] [bigint] NULL,
    [{nameof(HmqEventReactionLogSqlEntry.ActorIdentityJson)}] [nvarchar](MAX) NULL,
    [{nameof(HmqEventReactionLogSqlEntry.ActorID)}] [nvarchar](450) NULL,
    [{nameof(HmqEventReactionLogSqlEntry.OperationResultJson)}] [nvarchar](MAX) NULL,
    [{nameof(HmqEventReactionLogSqlEntry.IsSuccessful)}] [bit] NULL,

    {nameof(HmqEvent.ID).PrintColumnAsPrimaryKeyConstraintSqlScriptOn(table)}
)
ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

{nameof(HmqEventReactionLogSqlEntry.AsOf).PrintColumnIndexCreationSqlScriptOn(table)}
{nameof(HmqEventReactionLogSqlEntry.AsOfTicks).PrintColumnIndexCreationSqlScriptOn(table)}
{nameof(HmqEventReactionLogSqlEntry.EventID).PrintColumnIndexCreationSqlScriptOn(table)}
{nameof(HmqEventReactionLogSqlEntry.EventHappenedAt).PrintColumnIndexCreationSqlScriptOn(table)}
{nameof(HmqEventReactionLogSqlEntry.EventHappenedAtTicks).PrintColumnIndexCreationSqlScriptOn(table)}
{nameof(HmqEventReactionLogSqlEntry.ActorID).PrintColumnIndexCreationSqlScriptOn(table)}
{nameof(HmqEventReactionLogSqlEntry.IsSuccessful).PrintColumnIndexCreationSqlScriptOn(table)}
",
            }
        };
    }
}
