using H.Necessaire.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace H.Necessaire.MQ.Playground.AspNetPlay.Security
{
    public class EnvironmentVariablesIronMenProviderResource : ImTheIronManProviderResource
    {
        static readonly Lazy<Dictionary<UserInfo, string>> ironMenWithCredentials = new Lazy<Dictionary<UserInfo, string>>(EnsureIronMen);
        static Dictionary<UserInfo, string> IronMenWithCredentials => ironMenWithCredentials.Value;

        public Task<UserInfo[]> GetIronMenByIds(params Guid[] ids)
        {
            return IronMenWithCredentials.Keys.ToArray().AsTask();
        }

        public Task<string> GetPasswordFor(Guid ironManuserID)
        {
            return IronMenWithCredentials.SingleOrDefault(x => x.Key.ID == ironManuserID).Value.AsTask();
        }

        public Task<UserInfo[]> SearchIronMen(UserInfoSearchCriteria searchCriteria)
        {
            IEnumerable<UserInfo> result = IronMenWithCredentials.Keys;

            if (searchCriteria?.IDs?.Any() == true)
                result = result.Where(x => x.ID.In(searchCriteria.IDs));

            if (searchCriteria?.Usernames?.Any() == true)
                result = result.Where(x => x.Username.In(searchCriteria.Usernames, (item, key) => (item.Is(key))));

            return result.ToArray().AsTask();
        }

        static Dictionary<UserInfo, string> EnsureIronMen()
        {
            Dictionary<UserInfo, string> result = new Dictionary<UserInfo, string>();

            int index = -1;

            do
            {
                index++;

                string user = Environment.GetEnvironmentVariable($"H.Necessaire.Security.IronMan.{index}.User");
                if (user.IsEmpty())
                    break;
                string pass = Environment.GetEnvironmentVariable($"H.Necessaire.Security.IronMan.{index}.Pass");
                if (pass.IsEmpty())
                    break;

                result.Add(UserInfo.BuildIronMan(id: Guid.NewGuid(), userName: user, displayName: $"IronMan {index}"), pass);
            }
            while (true);

            return result;
        }
    }
}
