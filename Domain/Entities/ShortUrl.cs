using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ShortUrl : BaseEntity
    {
        
        public required string OriginalUrl { get; set; }
        [MaxLength(100)]
        public required string ShortenedUrl { get; set; }
        public required string Team { get; set; }
        public required string Level { get; set; }
        public string? Title { get; set; }

    }
}
