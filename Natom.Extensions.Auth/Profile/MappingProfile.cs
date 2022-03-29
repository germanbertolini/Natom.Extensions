using AutoMapper;
using Natom.Extensions.Auth.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natom.Extensions.Auth.Profile
{
    public static class MappingProfile
    {
        public static MapperConfiguration Build()
                            => new MapperConfiguration(cfg =>
                                {
                                    cfg.CreateMap<AccessToken, AccessToken>().ReverseMap();
                                    cfg.CreateMap<AccessToken, AccessTokenWithPermissions>().ReverseMap();
                                });
    }
}
