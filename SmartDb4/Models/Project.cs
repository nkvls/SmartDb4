using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartDb4.Helpers;

namespace SmartDb4.Models
{
    [Table("Project")]
    public class Project // : SelectOption
    {
        [Key]
        public int ProjectId { get; set; }

        [Required(ErrorMessage = ErrorMessages.RequiredString)]
        [StringLength(200, ErrorMessage = ErrorMessages.MaxLen200)]
        [DisplayName("Name")]
        public string ProjectName { get; set; }

        [MaxLength(2000)]
        [StringLength(2000, ErrorMessage = ErrorMessages.MaxLen2000)]
        public string Description { get; set; }

        //[MaxLength(250)]
        //public string TestDesc { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        [DisplayName("Start Date")]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        [DisplayName("End Date")]
        public DateTime? EndDate { get; set; }

        [DisplayName("Max Participants")]
        //[Range(1, 6, ErrorMessage = ErrorMessages.Between1To6)]
        public int MaxNoOfParticipants { get; set; }

        [ForeignKey("Classification")]
        public int ClassificationId { get; set; }

        [Required(ErrorMessage = ErrorMessages.RequiredString)]
        [ForeignKey("Supervisor")]
        public int SupervisorId { get; set; }

        [MaxLength(100)]
        public string CreatedBy { get; set; }

        public DateTime CreatedOn { get; set; }

        [MaxLength(100)]
        public string ModifiedBy { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public virtual UserProfile Supervisor { get; set; }
        public virtual Classification Classification { get; set; }
        public virtual ICollection<Member> Members { get; set; }
        public virtual ICollection<Attendance> Attendances { get; set; }


        [NotMapped]
        public bool AssignProjectToMemberNow { get; set; }

        [NotMapped]
        public bool ProjectAlreadyAssigned { get; set; }

        [NotMapped]
        public bool CanEdit { get; set; }
    }
}