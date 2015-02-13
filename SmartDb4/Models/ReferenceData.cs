using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Permissions;

namespace SmartDb4.Models
{

    public class ReferenceData
    {

    }

    [Table("Nomination")]
    public class Nomination
    {
        [Key]
        public int NominationId { get; set; }
        [MaxLength(200)]
        public string NominationName { get; set; }
    }

    [Table("Classification")]
    public class Classification
    {
        [Key]
        public int ClassificationId { get; set; }
        [MaxLength(200)]
        [DisplayName("Classification")]
        public string ClassificationName { get; set; }

        public virtual ICollection<Project> Projects { get; set; } 
    }

    [Table("FundingResponsibility")]
    public class FundingResponsibility
    {
        [Key]
        public int FundingResponsibilityId { get; set; }
        [MaxLength(200)]
        [DisplayName("Funding Responsibility")]
        public string FundingResponsibilityName { get; set; }
    }

    [Table("SexualOrientation")]
    public class SexualOrientation
    {
        [Key]
        public int SexualOrientationId { get; set; }
        [MaxLength(200)]
        [DisplayName("Sexual Orientation")]
        public string SexualOrientationName { get; set; }
    }

    [Table("Ethnicity")]
    public class Ethnicity
    {
        [Key]
        public int EthnicityId { get; set; }
        [MaxLength(200)]
        [DisplayName("Ethnicity")]
        public string EthnicityName { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; }
    }

    [Table("ReferralType")]
    public class ReferralType
    {
        [Key]
        public int ReferralTypeId { get; set; }
        [MaxLength(100)]
        [DisplayName("Ethnicity")]
        public string ReferralTypeName { get; set; }
    }

    [Table("UserType")]
    public class UserType
    {
        [Key]
        public int UserTypeId { get; set; }
        [MaxLength(200)]
        [DisplayName("User Type")]
        public string UserTypeName { get; set; }

        public ICollection<UserProfile> UserProfiles { get; set; } 
    }

    [Table("LivingArea")]
    public class LivingArea
    {
        [Key]
        public int LivingAreaId { get; set; }
        [MaxLength(200)]
        [DisplayName("Living Area")]
        public string LivingAreaName { get; set; }
    }

    [Table("MemberRole")]
    public class MemberRole
    {
        [Key]
        public int MemberRoleId { get; set; }
        [MaxLength(200)]
        [DisplayName("Member Role")]
        public string MemberRoleName { get; set; }
    }
    [Table("Gender")]
    public class Gender
    {
        [Key]
        public int GenderId { get; set; }
        [MaxLength(50)]
        [DisplayName("Gender")]
        public string GenderName { get; set; }
    }

    [Table("Notes")]
    public class Notes
    {
        [Key]
        public int NotesId { get; set; }

        [ForeignKey("Member")]
        public int MemberId { get; set; }

        [MaxLength(2000)]
        [DisplayName("Notes")]
        public string NotesDesc { get; set; }

        [DisplayName("Broadcast as Alert")]
        public bool BroadCastAsAlert { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        [DisplayName("Date")]
        public DateTime? AlertValidDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        [DisplayName("Date")]
        public DateTime? NotesCreateDate { get; set; }

        public string NotesCreatedBy { get; set; }

        public virtual Member Member { get; set; }

        [NotMapped]
        public bool IsActiveAlert { get; set; }
    }

    [Table("AdminAlert")]
    public class AdminAlert
    {
        [Key]
        public int AdminAlertId { get; set; }

        [ForeignKey("Member")]
        public int MemberId { get; set; }

        [MaxLength(500)]
        [DisplayName("Desc")]
        public string AlertDesc { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        [DisplayName("Date")]
        public DateTime? AlertValidDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        [DisplayName("Date")]
        public DateTime? AlertCreatedOn { get; set; }

        public string AlertCreatedBy { get; set; }

        public int AlertType { get; set; }

        public virtual Member Member { get; set; }
    }

    [Table("AgeBracket")]
    public class AgeBracket
    {
        [Key]
        public int AgeBracketId { get; set; }

        [MaxLength(100)]
        public string AgeBracketText { get; set; }

        public int SortOrder { get; set; }
    }


    [Table("GroupByClause")]
    public class GroupByClause
    {
        [Key]
        public int GroupByClauseId { get; set; }
         [MaxLength(100)]
        public string GroupByClauseText { get; set; }
        public int SortOrder { get; set; }
    }

    [NotMapped]
    public class ReportDataModel
    {
        //search data
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "required")]
        public DateTime DateFrom { get; set; }
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "required")]
        public DateTime DateTo { get; set; }
        public int NominationId { get; set; }
        public int ProjectId { get; set; }
        public string AgeBracketText { get; set; }
        public int GenderId { get; set; }
        public int EthnicityId { get; set; }
        public int FundingResponsibilityId { get; set; }
        public string GroupByClause { get; set; }


        public int GroupByClauseId { get; set; }
        public int AgeBracketId { get; set; }
        //return data
        public string GenderName { get; set; }
        public string ProjectName { get; set; }
        public string EthnicityName { get; set; }
        public string FundingResponsibilityName { get; set; }
        public string MemberName { get; set; }
        public string NominationName { get; set; }
        public string Agency { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string TimeAtSmart { get; set; }
        public DateTime? ExitDate { get; set; }
        public string ReasonOfLeaving { get; set; }
        public int TotalMembers { get; set; }
        public bool IncludeLeavingDetails { get; set; }
    }


    [NotMapped]
    public class ReportDataDisplayModel
    {
        public string ProjectName { get; set; }
        public string MemberName { get; set; }
        public string Nomination { get; set; }
        public string Agency { get; set; }
        public string Gender { get; set; }
        public string Ethnicity { get; set; }
        public string AgeGroup { get; set; }
        public string FundingResponsibility { get; set; }
        public string TimeAtSmart { get; set; }
        public string StartDate { get; set; }
        public int TotalCount { get; set; }
        public string DateOfLeaving { get; set; }
        public string ReasonOfLeaving { get; set; }
    }

    [NotMapped]
    public class MemberAttendanceForView
    {
        public int ProjectId { get; set; }
        public int AttendanceId { get; set; }
        public List<MemberAttendance> MemberAttendances { get; set; }
        public Attendance Attendance { get; set; }
        public virtual ICollection<Project> Projects { get; set; }
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
        
        [NotMapped]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DateForCurrentWeek { get; set; }
    }

    [NotMapped]
    public class ProjectMembership
    {
        public List<Project> Projects { get; set; }
        public bool IsChecked { get; set; }

    }

    //public class ProjectMap : EntityTypeConfiguration<Project>
    //{
    //    public ProjectMap()
    //    {
    //        //// Primary Key
    //        //this.HasKey(t => t.UserTypeId);

    //        //// Properties
    //        //this.Property(t => t.UserTypeName)
    //        //    .IsRequired()
    //        //    .HasMaxLength(100);
    //        //this.Property(x => x.TestDesc).HasColumnName("TestDesc").IsOptional().HasMaxLength(600);
            
    //        //// Table & Column Mappings
    //        //ToTable("UserType");
    //        //Property(t => t.UserTypeId).HasColumnName("Id");
    //        //Property(t => t.UserTypeName).HasColumnName("Name");

    //    }
    //}
}