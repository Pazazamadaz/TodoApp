namespace TodoApp.Models
{
    public class ColourTheme
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Colours { get; set; }
        public int? UserId { get; set; }
        public bool SysDefined { get; set; } = false;
        public bool IsDefault { get; set; } = false;
        public bool IsActive { get; set; } = false;
    }
}
