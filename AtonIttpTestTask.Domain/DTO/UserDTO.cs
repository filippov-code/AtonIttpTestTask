using AtonIttpTestTask.Domain.Models;

namespace AtonIttpTestTask.Domain.DTO
{
    public class UserDTO
    {
        public string Name { get; init; }
        public int Gender { get; init; }
        public DateTime? Birthday { get; init; }
        public bool IsRevoked { get; init; }

        public UserDTO(User user)
        {
            Name = user.Name;
            Gender = user.Gender;
            Birthday = user.Birthday;
            IsRevoked = user.RevokedOn != null;
        }
    }
}
