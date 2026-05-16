namespace SynchronizationTool.Demo.Dto
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Password { get; set; } = null!; // в реальном проекте не возвращайте пароль, но для CRUD оставим
    }
    public class UserCreateUpdateDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
