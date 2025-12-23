using H.Necessaire.MQ.Agent.Abstractions;
using System;
using System.Threading.Tasks;

namespace H.Necessaire.MQ.Agent.Core.Concrete
{
    internal class HmqAgentInfoProvider : ImAnHmqAgentInfoProvider, ImADependency
    {
        static readonly Lazy<string> lazyAgentID = new Lazy<string>(GetAgentID);
        static string AgentID => lazyAgentID.Value;
        static readonly TimeSpan agentInfoCacheValidity = TimeSpan.FromMinutes(30);
        ImACacher<ImAnHmqAgentInfo> agentInfoCache;
        public void ReferDependencies(ImADependencyProvider dependencyProvider)
        {
            agentInfoCache = dependencyProvider.GetCacher<ImAnHmqAgentInfo>();
        }

        public async Task<ImAnHmqAgentInfo> GetPublicAgentInfo()
        {
            return
                await agentInfoCache.GetOrAdd(
                    "PublicAgentInfo",
                    async cacheKey
                        => (await BuildPublicAgentInfo())
                        .ToCacheableItem(cacheKey, agentInfoCacheValidity)
                        .And(x => x.IsSlidingExpirationDisabled = true)
                );
        }

        public async Task<ImAnHmqAgentInfo> GetFullAgentInfo()
        {
            return
                await agentInfoCache.GetOrAdd(
                    "FullAgentInfo", 
                    async cacheKey 
                        => (await BuildFullAgentInfo())
                        .ToCacheableItem(cacheKey, agentInfoCacheValidity)
                        .And(x => x.IsSlidingExpirationDisabled = true)
                );
        }

        async Task<ImAnHmqAgentInfo> BuildPublicAgentInfo()
        {
            return
                new HmqAgentInfo
                {
                    ID = AgentID,
                }
                ;
        }

        async Task<ImAnHmqAgentInfo> BuildFullAgentInfo()
        {
            return
                new HmqAgentInfo
                {
                    ID = AgentID,
                }
                .And(agentInfo => {
                    HSafe.Run(() => agentInfo.IdentityAttributes = Note.GetEnvironmentInfo());
                    HSafe.Run(() => agentInfo.IdentityAttributes = agentInfo.IdentityAttributes.AppendProcessInfo());
                })
                ;
        }

        static string GetAgentID()
        {
            return
                HSafe.Run(() => System.Net.Dns.GetHostName().NullIfEmpty()).Return().NullIfEmpty()
                ?? HSafe.Run(() => Environment.MachineName).Return().NullIfEmpty()
                ?? $"UnknownHmqAgentMachineID-{Guid.NewGuid()}-AsOf-{DateTime.UtcNow.PrintTimeStampAsIdentifier()}"
                ;
        }
    }
}
