using System.ComponentModel.DataAnnotations;

namespace StudentPortal.Models
{
    public class StudentParent
    {
        [Key]
        public int ParentId { get; set; }

        public int StudentId { get; set; }

        public string? FatherName { get; set; }
        public string? FatherMobile { get; set; }

        public string? MotherName { get; set; }
        public string? MotherMobile { get; set; }

        public string? Address { get; set; }

        public Student Student { get; set; }
    }
}
