using System.Threading.Tasks;

namespace H.Necessaire.MQ.Agent.Abstractions
{
    public interface ImAnHmqAgentInfoProvider
    {
        Task<ImAnHmqAgentInfo> GetPublicAgentInfo();
        Task<ImAnHmqAgentInfo> GetFullAgentInfo();
    }
}
