using Application.ViewModels.ShortUrl;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infra.MapperConfigs
{
    public class MapperConfig : Profile
    {
        public MapperConfig() { MappingAccount(); }
        public void MappingAccount()
        {
            CreateMap<ShortUrl, ShortUrlAddVM>().ReverseMap();
            CreateMap<ShortUrl, ShortUrlVM>().ReverseMap();
        }

    }
}
