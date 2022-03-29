using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natom.Extensions.Logger.Entities.Discord.DTO
{
    public class reqSendMessage
    {
        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("username")]
        public string UserName { get; set; }
    }
}
