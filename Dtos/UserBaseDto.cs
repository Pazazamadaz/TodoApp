namespace TodoApp.Dtos
{
    public class UserBaseDto
    {
        public string Username { get; set; }
        //public string NormalisedUsername => Username.ToLower();
        public bool IsAdmin { get; set; }
    }
}
