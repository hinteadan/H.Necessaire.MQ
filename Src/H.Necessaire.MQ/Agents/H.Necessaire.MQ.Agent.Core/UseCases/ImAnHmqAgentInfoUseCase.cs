using H.Necessaire.MQ.Agent.Abstractions;
using System.Threading.Tasks;

namespace H.Necessaire.MQ.Agent.Core.UseCases
{
    public interface ImAnHmqAgentInfoUseCase
    {
        Task<ImAnHmqAgentInfo> GetPublicAgentInfo();
        Task<ImAnHmqAgentInfo> GetFullAgentInfo();
    }
}
