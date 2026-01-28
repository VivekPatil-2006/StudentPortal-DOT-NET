using System;
using System.ComponentModel.DataAnnotations;

namespace StudentPortal.Models
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        public string Message { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
