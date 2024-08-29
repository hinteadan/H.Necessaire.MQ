using H.Necessaire.MQ.Abstractions;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Reflection;

namespace H.Necessaire.MQ.Core
{
    public static class HmqEventExtensions
    {
        static readonly string[] externalSourceNames = new string[] { "PersistentStore" };

        public static HmqEvent ToHmqEvent<T>(this T data, params Note[] attributes)
        {
            Type dataType = data?.GetType() ?? typeof(T);

            return
                new HmqEvent
                {
                    ID = Guid.NewGuid(),

                    HappenedAt = DateTime.UtcNow,

                    Assembly = dataType.Assembly.FullName,
                    Type = dataType.FullName,
                    Name = dataType.Name,

                    Attributes = attributes.NullIfEmpty(),

                    Data = data,
                };
        }

        public static HmqEvent Clone(this HmqEvent hmqEvent)
        {
            if (hmqEvent is null)
                return null;

            return
                new HmqEvent
                {
                    ID = hmqEvent.ID,
                    Assembly = hmqEvent.Assembly,
                    Type = hmqEvent.Type,
                    Attributes = hmqEvent.Attributes.ToArrayNullIfEmpty(),
                    Data = hmqEvent.Data,
                    HappenedAt = hmqEvent.HappenedAt,
                    Name = hmqEvent.Name,
                    RaisedBy = hmqEvent.RaisedBy,
                };
        }

        public static HmqEvent MarkAsPersisted(this HmqEvent hmqEvent)
        {
            if (hmqEvent is null)
                return null;

            hmqEvent.Attributes = hmqEvent.Attributes.AddOrReplace(
                $"{true}".NoteAs("IsPersisted"),
                "PersistentStore".NoteAs("Source")
            );

            return hmqEvent;
        }

        public static bool IsExternal(this HmqEvent hmqEvent)
        {
            return hmqEvent?.Attributes?.Get("Source", ignoreCase: true)?.In(externalSourceNames, (val, key) => val.Is(key)) == true;
        }

        public static bool IsInternal(this HmqEvent hmqEvent)
        {
            return (hmqEvent?.Attributes?.Get("Source")).IsEmpty() || !hmqEvent.IsExternal();
        }

        public static HmqActorIdentity ToIdentityOnly<T>(this T identityHolder) where T : class, ImAnHmqActorIdentity
        {
            if (identityHolder is null)
                return null;

            return
                new HmqActorIdentity
                {
                    ID = identityHolder.ID,
                    IdentityAttributes = identityHolder.IdentityAttributes,
                };
        }

        public static Type FindDataType(this HmqEvent hmqEvent)
        {
            if (hmqEvent?.Data is null)
                return null;

            Type type = null;

            new Action(() =>
            {

                type = Type.GetType($"{hmqEvent.Type}, {hmqEvent.Assembly}");

            }).TryOrFailWithGrace(onFail: ex => type = null);

            if (type != null)
                return type;

            Assembly[] allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            Assembly targetAssembly = allAssemblies.FirstOrDefault(a => a.FullName.Is(hmqEvent.Assembly));

            if (targetAssembly != null)
            {
                new Action(() =>
                {

                    type
                        = targetAssembly.GetType(hmqEvent.Type)
                        ??
                        targetAssembly.GetTypes()?.FirstOrDefault(t => t.FullName.Is(hmqEvent.Type))
                        ??
                        targetAssembly.GetTypes()?.FirstOrDefault(t => t.Name.Is(hmqEvent.Type))

                        ;

                }).TryOrFailWithGrace(onFail: ex => type = null);

                if (type != null)
                    return type;
            }

            Type[] allTypes = allAssemblies.SelectMany(a => a.GetTypes() ?? Array.Empty<Type>()).ToArray();

            new Action(() =>
            {

                type
                    = allTypes.FirstOrDefault(t => t.FullName.Is(hmqEvent.Type))
                    ??
                    allTypes.FirstOrDefault(t => t.Name.Is(hmqEvent.Type))
                    ;

            }).TryOrFailWithGrace(onFail: ex => type = null);

            return type;
        }

        public static HmqEvent ToWellTypedEventData(this HmqEvent hmqEvent)
        {
            if (hmqEvent?.Data is null)
                return hmqEvent;

            if (!(hmqEvent.Data is JToken))
                return hmqEvent;

            Type dataType = hmqEvent.FindDataType();

            if (dataType is null)
                return hmqEvent;

            hmqEvent.Data = (hmqEvent.Data as JToken).ToObject(dataType);

            return hmqEvent;
        }
    }
}
