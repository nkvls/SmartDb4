using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartDb4.Models
{
    [Table("Attendance")]
    public class Attendance
    {
        [Key]
        public int AttendanceId { get; set; }

        public int Year { get; set; }
        public int Week { get; set; }

        [MaxLength(100)]
        [DisplayName("Week (in words)")]
        public string WeekString { get; set; }

        [MaxLength(100)]
        public string CreatedBy { get; set; }

        public DateTime CreatedOn { get; set; }

        [MaxLength(100)]
        public string ModifiedBy { get; set; }

        public DateTime? ModifiedOn { get; set; }

        [ForeignKey("Project")]
        public int ProjectId { get; set; }

        public virtual Project Project { get; set; }

        [NotMapped]
        public DateTime FirstDateOfWeek { get; set; }
    }
}