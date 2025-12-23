using H.Necessaire.MQ.Agent.Abstractions;
using H.Necessaire.MQ.Agent.Core.UseCases;
using H.Necessaire.Runtime.Integration.AspNetCore;
using Microsoft.AspNetCore.Mvc;

namespace H.Necessaire.MQ.Agent.Host.AspNetCore.Controllers
{
    [Route("/")]
    [ApiController]
    public class HmqAgentInfoController(ImAnHmqAgentInfoUseCase useCase) : ControllerBase, ImAnHmqAgentInfoUseCase
    {
        readonly ImAnHmqAgentInfoUseCase useCase = useCase;

        [Route(""), HttpGet]
        public async Task<ImAnHmqAgentInfo> GetPublicAgentInfo()
        {
            return await useCase.GetPublicAgentInfo();
        }

        [Route("details"), HttpGet]
        public async Task<ImAnHmqAgentInfo> GetFullAgentInfo()
        {
            return await useCase.GetFullAgentInfo();
        }
    }
}
