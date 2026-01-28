using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace StudentPortal.Models
{
    public class Student
    {
        [Key]
        public int StudentId { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Mobile { get; set; }

        public string? Gender { get; set; }
        public string? Course { get; set; }

        public DateTime? DateOfBirth { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? PhotoPath { get; set; }

        [NotMapped]
        public IFormFile? Photo { get; set; }

        public StudentParent? StudentParent { get; set; }
    }
}
