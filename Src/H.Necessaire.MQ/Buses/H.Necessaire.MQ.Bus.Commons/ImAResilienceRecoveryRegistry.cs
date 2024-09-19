using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace H.Necessaire.MQ.Bus.Commons
{
    internal interface ImAResilienceRecoveryRegistry
    {
        void RegisterResilienceTask(Func<Task> resilienceTask);
        void UnregisterResilienceTask(Func<Task> resilienceTask);
        IEnumerable<Func<Task>> StreamAll();
    }
}
