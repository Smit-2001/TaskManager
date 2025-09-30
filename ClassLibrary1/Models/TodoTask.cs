using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagerShared.Models
{
    public class TodoTask
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public string Category { get; set; } = "General"; // Work, Personal, Study

        [Required]
        public string Priority { get; set; } = "Medium"; // Low, Medium, High

        public DateTime DueDate { get; set; } = DateTime.Now.AddDays(1);

        public bool IsCompleted { get; set; }
    }
}
