using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartDb4.Helpers;

namespace SmartDb4.Models
{
    [Table("MemberAttendance")]
    public class MemberAttendance
    {
        [Key]
        public int MemberAttendanceId { get; set; }

        [ForeignKey("Attendance")]
        public int AttendanceId { get; set; }

        public bool Monday { get; set; }
        public bool Tuesday { get; set; }
        public bool Wednesday { get; set; }
        public bool Thursday { get; set; }
        public bool Friday { get; set; }
        public bool Saturday { get; set; }
        public bool Sunday { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.##}")]
        [RegularExpression(RegXConstants.RegExDecimal, ErrorMessage = ErrorMessages.InvalidRate)]
        public decimal MondayRate { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.##}")]
        [RegularExpression(RegXConstants.RegExDecimal, ErrorMessage = ErrorMessages.InvalidRate)]
        public decimal TuesdayRate { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.##}")]
        [RegularExpression(RegXConstants.RegExDecimal, ErrorMessage = ErrorMessages.InvalidRate)]
        public decimal WednesdayRate { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.##}")]
        [RegularExpression(RegXConstants.RegExDecimal, ErrorMessage = ErrorMessages.InvalidRate)]
        public decimal ThursdayRate { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.##}")]
        [RegularExpression(RegXConstants.RegExDecimal, ErrorMessage = ErrorMessages.InvalidRate)]
        public decimal FridayRate { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.##}")]
        [RegularExpression(RegXConstants.RegExDecimal, ErrorMessage = ErrorMessages.InvalidRate)]
        public decimal SaturdayRate { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.##}")]
        [RegularExpression(RegXConstants.RegExDecimal, ErrorMessage = ErrorMessages.InvalidRate)]
        public decimal SundayRate { get; set; }

        [MaxLength(100)]
        public string CreatedBy { get; set; }

        public DateTime CreatedOn { get; set; }

        [MaxLength(100)]
        public string ModifiedBy { get; set; }

        public DateTime? ModifiedOn { get; set; }

        //[ForeignKey("Attendance")]
        //public int AttendanceId { get; set; }


        public virtual Attendance Attendance { get; set; }

        [ForeignKey("Member")]
        public int MemberId { get; set; }


        public virtual Member Member { get; set; }

        [NotMapped]
        [DisplayName("Project")]
        public string ProjectName { get; set; }

        //public int ProjectId { get; set; }

        //[ForeignKey("ProjectId")]
        //public virtual Project Project { get; set; }

        [NotMapped]
        public string MondayDate { get; set; }
        [NotMapped]
        public string TuesdayDate { get; set; }
        [NotMapped]
        public string WednesdayDate { get; set; }
        [NotMapped]
        public string ThursdayDate { get; set; }
        [NotMapped]
        public string FridayDate { get; set; }
        [NotMapped]
        public string SaturdayDate { get; set; }
        [NotMapped]
        public string SundayDate { get; set; }
    }
}