using System.ComponentModel.DataAnnotations;

namespace StudentPortal.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        public string Email { get; set; }
        public string Password { get; set; }

        public string Role { get; set; }

        public int? StudentId { get; set; }
    }
}
