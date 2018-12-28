using System;
using System.ComponentModel.DataAnnotations;

namespace WatchParty.Database
{
    public class Room
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Url { get; set; }
        public DateTime UtcTime { get; set; } = DateTime.UtcNow;
        public string Iframe { get; set; }
        public string Name { get; set; }
    }
}