using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IvleLapiPortableClient.Models
{
    public class IvleCredential: ILapiModel
    {
        private static const String AUTH_TOKEN_FORMAT =
            "APIKey={0}&AuthToken={1}";
        private static const String TOKEN_FORMAT =
            "APIKey={0}&Token={1}";

        public String ApiKey
        {
            get;
            set;
        }

        public String Token
        {
            get;
            set;
        }

        public IvleCredential(String apiKey)
        {
            this.ApiKey = apiKey;
        }

        public IvleCredential(String apikey, String token)
        {
            this.ApiKey = apikey;
            this.Token = token;
        }

        public String GetUrlWithAuthToken(String url)
        {
            String authParams = String.Format(AUTH_TOKEN_FORMAT, this.ApiKey, this.Token);
            if (!url.Contains(authParams))
            {
                url += authParams;
            }
            return url;
        }

        public String GetUrlWithToken(String url)
        {
            String authParams = String.Format(TOKEN_FORMAT, this.ApiKey, this.Token);
            if (!url.Contains(authParams))
            {
                url += authParams;
            }
            return url;
        }

        public void Build(String jsonString)
        {

        }
    }
}
