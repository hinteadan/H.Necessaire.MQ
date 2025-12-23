using H.Necessaire.MQ.Agent.Abstractions;
using H.Necessaire.Runtime;
using System.Threading.Tasks;

namespace H.Necessaire.MQ.Agent.Core.UseCases.Concrete
{
    internal class HmqAgentInfoUseCase : UseCaseBase, ImAnHmqAgentInfoUseCase
    {
        #region Construct
        ImAnHmqAgentInfoProvider hmqAgentInfoProvider;
        public override void ReferDependencies(ImADependencyProvider dependencyProvider)
        {
            base.ReferDependencies(dependencyProvider);

            hmqAgentInfoProvider = dependencyProvider.Get<ImAnHmqAgentInfoProvider>();
        }
        #endregion Construct

        public async Task<ImAnHmqAgentInfo> GetPublicAgentInfo()
        {
            return await hmqAgentInfoProvider.GetPublicAgentInfo();
        }

        public async Task<ImAnHmqAgentInfo> GetFullAgentInfo()
        {
            (await EnsureAuthenticationAndPermissions(new PermissionClaim { IDTag = "infra", MinimumRequiredLevel = PermissionLevel.Read })).ThrowOnFail();

            return await hmqAgentInfoProvider.GetFullAgentInfo();
        }
    }
}
