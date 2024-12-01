using System.Collections.Generic;

namespace TodoApp.Models
{
    public class ColourTheme
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required List<Colour> Colours { get; set; } = new List<Colour>();
        public int? UserId { get; set; }
        public bool SystemDefined { get; set; } = false;
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
    }
}
