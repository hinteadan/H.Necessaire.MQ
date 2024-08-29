using H.Necessaire.Dapper;
using H.Necessaire.MQ.Abstractions;

namespace H.Necessaire.MQ.Runtime.SqlServer.Concrete.Storage
{
    internal partial class SqlServerHmqEventStorageService
    {
        const string table = "H.Necessaire.HMQ.HmqEvent";

        static readonly SqlMigration[] sqlMigrations = new SqlMigration[] {
            new SqlMigration
            {
                ResourceIdentifier = "HmqEvent",
                VersionNumber = new VersionNumber(1, 0),
                SqlCommand = $@"
CREATE TABLE [dbo].[{table}]
(
    [{nameof(HmqEventSqlEntry.ID)}] [uniqueidentifier] NOT NULL,
    [{nameof(HmqEventSqlEntry.HappenedAt)}] [datetime2](7) NOT NULL,
    [{nameof(HmqEventSqlEntry.HappenedAtTicks)}] [bigint] NOT NULL,
    [{nameof(HmqEventSqlEntry.RaisedByJson)}] [nvarchar](MAX) NULL,
    [{nameof(HmqEventSqlEntry.RaisedByID)}] [nvarchar](450) NULL,
    [{nameof(HmqEventSqlEntry.Name)}] [nvarchar](450) NULL,
    [{nameof(HmqEventSqlEntry.Type)}] [nvarchar](450) NULL,
    [{nameof(HmqEventSqlEntry.Assembly)}] [nvarchar](450) NULL,
    [{nameof(HmqEventSqlEntry.AttributesJson)}] [nvarchar](MAX) NULL,
    [{nameof(HmqEventSqlEntry.DataJson)}] [nvarchar](MAX) NULL,

    {nameof(HmqEvent.ID).PrintColumnAsPrimaryKeyConstraintSqlScriptOn(table)}
)
ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

{nameof(HmqEventSqlEntry.HappenedAt).PrintColumnIndexCreationSqlScriptOn(table)}
{nameof(HmqEventSqlEntry.RaisedByID).PrintColumnIndexCreationSqlScriptOn(table)}
{nameof(HmqEventSqlEntry.Name).PrintColumnIndexCreationSqlScriptOn(table)}
{nameof(HmqEventSqlEntry.Type).PrintColumnIndexCreationSqlScriptOn(table)}
{nameof(HmqEventSqlEntry.Assembly).PrintColumnIndexCreationSqlScriptOn(table)}
",
            }
        };
    }
}
