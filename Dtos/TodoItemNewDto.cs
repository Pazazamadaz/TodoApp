namespace TodoApp.Dtos
{
    public class TodoItemNewDto
    {
        public string? Title { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsPriority { get; set; }
    }
}
