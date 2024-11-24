namespace TodoApp.Models
{
    public class Colour
    {
        public int Id { get; set; }
        public required string ColourProperty { get; set; }
        public required string ColourValue { get; set; }
        public required int ColourThemeId { get; set; }
    }
}
