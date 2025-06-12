using Domain;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.ShortUrl
{
    public class ShortUrlVM : BaseEntity
    {
        public required string OriginalUrl { get; set; }
        public required string ShortenedUrl { get; set; }
        public required string Team { get; set; }
        public required string Level { get; set; }
    }
}
