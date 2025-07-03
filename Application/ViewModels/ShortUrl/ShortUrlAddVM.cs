using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.ShortUrl
{
    public class ShortUrlAddVM
    {
        public required string OriginalUrl { get; set; }
        public required string Team { get; set; }
        public required string Level { get; set; }
        public string? Title { get; set; }
    }
}
