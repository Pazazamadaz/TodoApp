namespace TodoApp.Models
{
    public class ColourTheme
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Colours { get; set; }
        int? UserId { get; set; }
        bool SysDefined { get; set; } = false;
        bool IsDefault { get; set; } = false;
        bool IsActive { get; set; } = false;
    }
}
