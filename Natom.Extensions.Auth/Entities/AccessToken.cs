using Natom.Extensions.Auth.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natom.Extensions.Auth.Entities
{
    public class AccessToken
    {
        [JsonProperty("k")]
        public string Key { get; set; }

        [JsonProperty("s")]
        public string Scope { get; set; }

        [JsonProperty("sid")]
        public string SyncInstanceId { get; set; }

        [JsonProperty("uid")]
        public int? UserId { get; set; }

        [JsonProperty("ufn")]
        public string UserFullName { get; set; }

        [JsonProperty("cid")]
        public int? ClientId { get; set; }

        [JsonProperty("cfn")]
        public string ClientFullName { get; set; }

        [JsonProperty("cat")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("eat")]
        public DateTime? ExpiresAt { get; set; }


        public string ToJwtEncoded() => OAuthHelper.Encode((AccessToken)this.MemberwiseClone());
    }
}
