using Jose;
using Natom.Extensions.Auth.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natom.Extensions.Auth.Helpers
{
    public static class OAuthHelper
    {
        private static byte[] secretKey = new byte[] { 120, 23, 20, 132, 11, 18, 108, 171, 61, 29, 13, 121,
                                                        0, 72, 129, 48, 40, 65, 15, 73, 32, 225, 87, 59,
                                                        117, 153, 47, 99, 89, 123, 143, 209, 12, 48, 98 };


        public static string Encode(AccessToken accessToken)
                                => JWT.Encode(accessToken, secretKey, JwsAlgorithm.HS256);

        public static AccessToken Decode(string accessToken)
                                => JWT.Decode<AccessToken>(accessToken, secretKey, JwsAlgorithm.HS256);
    }
}
