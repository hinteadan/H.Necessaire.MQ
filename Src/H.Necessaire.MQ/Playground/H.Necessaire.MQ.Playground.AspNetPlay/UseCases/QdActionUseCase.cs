using H.Necessaire.Runtime;
using System.Threading.Tasks;

namespace H.Necessaire.MQ.Playground.AspNetPlay.UseCases
{
    public class QdActionUseCase : UseCaseBase
    {
        public async Task<OperationResult> Enqueue(QdAction qdAction)
        {
            (await EnsureAuthenticationAndPermissions(new PermissionClaim { IDTag = "Enqueue", MinimumRequiredLevel = PermissionLevel.Create })).ThrowOnFailOrReturn();

            return OperationResult.Fail("Not yet implemented");
        }
    }
}
