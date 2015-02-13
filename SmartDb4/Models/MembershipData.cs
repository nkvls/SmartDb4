using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;
using System.Web.Mvc;
using SmartDb4.Attributes;
using SmartDb4.Extensions;
using SmartDb4.Helpers;

namespace SmartDb4.Models
{
    [Table("Member")]
    public class Member
    {
        public Member()
        {
            Notes = new List<Notes>();
        }

        [Key]
        public int MemberId { get; set; }

        [NotMapped]
        public int Age
        {
            get
            {
                if (DateOfBirth == null) return 0;
                var today = DateTime.Today;
                var age = today.Year - DateOfBirth.Value.Year;
                if (DateOfBirth.Value > today.AddYears(-age))
                    age--;

                return age;
            }
        }
        //[NotMapped]
        //public bool IsAlertValid
        //{
        //    get
        //    {
        //        //if (AlertValidDate.HasValue)
        //        //    return AlertValidDate.Value.CompareTo(DateTime.Today) >= 0;
        //        return true;
        //    }
        //}

        
        [NotMapped]
        public int? GroupedEthnicityId { get; set; }
        [NotMapped]
        public IEnumerable<SelectListItem> GroupedEthnicityOptions { get; set; }

        [NotMapped]
        public string SearchText { get; set; }
        [NotMapped]
        public int MemberRoleId { get; set; }
        [NotMapped]
        public ProjectMembership ProjectMembership { get; set; }

        //*************************************************************************
        //APPLICANT DETAILS
        //*************************************************************************
        [Required(ErrorMessage = ErrorMessages.RequiredString)]
        [StringLength(100, ErrorMessage = ErrorMessages.MaxLen100)]
        [DisplayName("Name")]
        public string MemberName { get; set; }

        [Required(ErrorMessage = ErrorMessages.RequiredString)]
        [StringLength(200, ErrorMessage = ErrorMessages.MaxLen200)]
        public string Address1 { get; set; }

        [MaxLength(200)]
        [StringLength(200, ErrorMessage = ErrorMessages.MaxLen200)]
        public string Address2 { get; set; }

        [Required(ErrorMessage = ErrorMessages.RequiredString)]
        [StringLength(100, ErrorMessage = ErrorMessages.MaxLen100)]
        public string City { get; set; }

        [MaxLength(50)]
        [DisplayName("Post Code")]
        [StringLength(20, ErrorMessage = ErrorMessages.MaxLen20)]
        public string PostCode { get; set; }

        [MaxLength(50)]
        [DisplayName("Home Tel")]
        [DataType(DataType.PhoneNumber)]
        //[RegularExpression(RegXConstants.RegExPhoneNo, ErrorMessage = ErrorMessages.InvalidPhoneNoMessage)]
        [StringLength(50, ErrorMessage = ErrorMessages.MaxLen50)]
        //http://www.authorcode.com/how-to-use-regular-expression-for-validating-phone-numbers-in-net/
        public string HomeTel { get; set; }

        //[Required(ErrorMessage = ErrorMessages.RequiredString)]
        [StringLength(100, ErrorMessage = ErrorMessages.MaxLen100)]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [MaxLength(50)]
        [DisplayName("Mobile Tel")]
        [StringLength(50, ErrorMessage = ErrorMessages.MaxLen50)]
        public string MobileTel { get; set; }

        [ForeignKey("Gender")]
        public int GenderId { get; set; }
        
        public virtual Gender Gender { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        [DisplayName("Date of Birth")]
        public DateTime? DateOfBirth { get; set; }

        //*************************************************************************
        //REFEREE DETAILS
        //*************************************************************************
        [NotMapped]
        public int RefereeId { get; set; }

        [ForeignKey("AgentDetails")]
        public int AgentId { get; set; }

        [MaxLength(200)]
        [StringLength(200, ErrorMessage = ErrorMessages.MaxLen200)]
        [DisplayName("Relationship To Client")]
        public string RelationshipToClient { get; set; }

        public UserProfile AgentDetails { get; set; }

        [NotMapped]
        [StringLength(100, ErrorMessage = ErrorMessages.MaxLen100)]
        public string RefereeName { get; set; }

        [NotMapped]
        [StringLength(100, ErrorMessage = ErrorMessages.MaxLen100)]
        [DisplayName("Agency")]
        public string RefereeAgency { get; set; }

        [NotMapped]
        [StringLength(200, ErrorMessage = ErrorMessages.MaxLen100)]
        [DisplayName("Address 1")]
        public string RefereeAddress1 { get; set; }

        [NotMapped]
        [StringLength(200, ErrorMessage = ErrorMessages.MaxLen100)]
        [DisplayName("Address 2")]
        public string RefereeAddress2 { get; set; }

        [NotMapped]
        [StringLength(100, ErrorMessage = ErrorMessages.MaxLen100)]
        [DisplayName("City")]
        public string RefereeCity { get; set; }

        [NotMapped]
        [MaxLength(50)]
        [DisplayName("Post Code")]
        [StringLength(20, ErrorMessage = ErrorMessages.MaxLen100)]
        public string RefereePostCode { get; set; }

        [NotMapped]
        [MaxLength(50)]
        [DisplayName("Home Tel")]
        [StringLength(50, ErrorMessage = ErrorMessages.MaxLen100)]
        public string RefereeWorkTel { get; set; }

        [NotMapped]
        [MaxLength(100)]
        [DisplayName("Email")]
        [StringLength(100, ErrorMessage = ErrorMessages.MaxLen100)]
        [EmailAddress(ErrorMessage = ErrorMessages.InvaliEmail)]
        public string RefereeEmail { get; set; }

        [NotMapped]
        [MaxLength(50)]
        [DisplayName("Mobile Tel")]
        [StringLength(50, ErrorMessage = ErrorMessages.MaxLen50)]
        public string RefereeMobileTel { get; set; }

        [NotMapped]
        [MaxLength(200)]
        public string RelationshipWithApplicant { get; set; }

        //*************************************************************************
        //OTHER CONTACTS
        //*************************************************************************
        public bool IsKeyOrSocialWorker { get; set; }

        [MaxLength(100)]
        [DisplayName("Name")]
        [StringLength(100, ErrorMessage = ErrorMessages.MaxLen100)]
        public string KeyWorkerName { get; set; }

        [MaxLength(100)]
        [DisplayName("Agency")]
        [StringLength(100, ErrorMessage = ErrorMessages.MaxLen100)]
        public string KeyWorkerAgency { get; set; }

        [MaxLength(200)]
        [DisplayName("Address 1")]
        [StringLength(200, ErrorMessage = ErrorMessages.MaxLen200)]
        public string KeyWorkerAddress1 { get; set; }

        [MaxLength(200)]
        [DisplayName("Address 2")]
        [StringLength(200, ErrorMessage = ErrorMessages.MaxLen200)]
        public string KeyWorkerAddress2 { get; set; }

        [MaxLength(100)]
        [DisplayName("City")]
        [StringLength(100, ErrorMessage = ErrorMessages.MaxLen100)]
        public string KeyWorkerCity { get; set; }

        [MaxLength(50)]
        [DisplayName("Post Code")]
        [StringLength(20, ErrorMessage = ErrorMessages.MaxLen20)]
        public string KeyWorkerPostCode { get; set; }

        [MaxLength(50)]
        [DisplayName("Home Tel")]
        [StringLength(50, ErrorMessage = ErrorMessages.MaxLen50)]
        public string KeyWorkerHomeTel { get; set; }

        [MaxLength(100)]
        [DisplayName("Email")]
        [StringLength(100, ErrorMessage = ErrorMessages.MaxLen100)]
        [EmailAddress(ErrorMessage = ErrorMessages.InvaliEmail)]
        public string KeyWorkerEmail { get; set; }

        [MaxLength(50)]
        [DisplayName("Mobile Tel")]
        [StringLength(50, ErrorMessage = ErrorMessages.MaxLen50)]
        public string KeyWorkerMobileTel { get; set; }


        //*************************************************************************
        //CONSULTANT
        //*************************************************************************
        public bool IsConsultant { get; set; }

        [MaxLength(100)]
        [DisplayName("Name")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters.")]
        public string ConsultantName { get; set; }

        [MaxLength(100)]
        [DisplayName("Agency")]
        [StringLength(100, ErrorMessage = "Agency cannot be longer than 100 characters.")]
        public string ConsultantAgency { get; set; }

        [MaxLength(200)]
        [DisplayName("Address 1")]
        [StringLength(200, ErrorMessage = "Address1 cannot be longer than 200 characters.")]
        public string ConsultantAddress1 { get; set; }

        [MaxLength(200)]
        [DisplayName("Address 2")]
        [StringLength(200, ErrorMessage = "Address2 cannot be longer than 200 characters.")]
        public string ConsultantAddress2 { get; set; }

        [MaxLength(100)]
        [DisplayName("City")]
        [StringLength(100, ErrorMessage = "City cannot be longer than 100 characters.")]
        public string ConsultantCity { get; set; }

        [MaxLength(50)]
        [DisplayName("Post Code")]
        [StringLength(20, ErrorMessage = "PostCode cannot be longer than 20 characters.")]
        public string ConsultantPostCode { get; set; }

        [MaxLength(50)]
        [DisplayName("Home Tel")]
        [StringLength(50, ErrorMessage = "Telephone cannot be longer than 50 characters.")]
        public string ConsultantHomeTel { get; set; }

        [MaxLength(100)]
        [DisplayName("Email")]
        [StringLength(100, ErrorMessage = "Email cannot be longer than 100 characters.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string ConsultantEmail { get; set; }

        [MaxLength(50)]
        [DisplayName("Mobile Tel")]
        [StringLength(50, ErrorMessage = "Telephone cannot be longer than 50 characters.")]
        public string ConsultantMobileTel { get; set; }


        //*************************************************************************
        //CPN
        //*************************************************************************
        public bool IsCpn { get; set; }

        [MaxLength(100)]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters.")]
        [DisplayName("Name")]
        public string CpnName { get; set; }

        [MaxLength(100)]
        [StringLength(100, ErrorMessage = "Agency cannot be longer than 100 characters.")]
        [DisplayName("Agency")]
        public string CpnAgency { get; set; }

        [MaxLength(200)]
        [StringLength(200, ErrorMessage = "Address1 cannot be longer than 200 characters.")]
        [DisplayName("Address 1")]
        public string CpnAddress1 { get; set; }

        [MaxLength(200)]
        [StringLength(200, ErrorMessage = "Address2 cannot be longer than 200 characters.")]
        [DisplayName("Address 2")]
        public string CpnAddress2 { get; set; }

        [MaxLength(100)]
        [DisplayName("City")]
        [StringLength(100, ErrorMessage = "City cannot be longer than 100 characters.")]
        public string CpnCity { get; set; }

        [MaxLength(50)]
        [StringLength(20, ErrorMessage = "Postcode cannot be longer than 20 characters.")]
        [DisplayName("Post Code")]
        public string CpnPostCode { get; set; }

        [MaxLength(50)]
        [DisplayName("Home Tel")]
        [StringLength(50, ErrorMessage = "Telephone cannot be longer than 50 characters.")]
        public string CpnHomeTel { get; set; }

        [MaxLength(100)]
        [DisplayName("Email")]
        [StringLength(100, ErrorMessage = "Email cannot be longer than 100 characters.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string CpnEmail { get; set; }

        [MaxLength(50)]
        [DisplayName("Mobile Tel")]
        [StringLength(50, ErrorMessage = "Phone cannot be longer than 50 characters.")]
        public string CpnMobileTel { get; set; }


        //*************************************************************************
        //OTHER
        //*************************************************************************
        [MaxLength(200)]
        [StringLength(200, ErrorMessage = "Other field cannot be longer than 200 characters.")]
        public string OtherSpecify { get; set; }

        [MaxLength(100)]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters.")]
        [DisplayName("Name")]
        public string OtherName { get; set; }

        [MaxLength(100)]
        [StringLength(100, ErrorMessage = "Agency cannot be longer than 100 characters.")]
        [DisplayName("Agency")]
        public string OtherAgency { get; set; }

        [MaxLength(200)]
        [StringLength(200, ErrorMessage = "Address1 cannot be longer than 200 characters.")]
        [DisplayName("Address 1")]
        public string OtherAddress1 { get; set; }

        [MaxLength(200)]
        [StringLength(200, ErrorMessage = "Address2 cannot be longer than 200 characters.")]
        [DisplayName("Address 2")]
        public string OtherAddress2 { get; set; }

        [MaxLength(100)]
        [DisplayName("City")]
        [StringLength(100, ErrorMessage = "City cannot be longer than 100 characters.")]
        public string OtherCity { get; set; }

        [MaxLength(50)]
        [StringLength(20, ErrorMessage = "Postcode cannot be longer than 20 characters.")]
        [DisplayName("Post Code")]
        public string OtherPostCode { get; set; }

        [MaxLength(50)]
        [DisplayName("Home Tel")]
        [StringLength(50, ErrorMessage = "Telephone cannot be longer than 50 characters.")]
        public string OtherHomeTel { get; set; }

        [MaxLength(100)]
        [DisplayName("Email")]
        [StringLength(100, ErrorMessage = "Email cannot be longer than 100 characters.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string OtherEmail { get; set; }

        [MaxLength(50)]
        [DisplayName("Mobile Tel")]
        [StringLength(50, ErrorMessage = "Telephone cannot be longer than 50 characters.")]
        public string OtherMobileTel { get; set; }


        //*************************************************************************
        //ABOUT YOUR CLIENT / PATIENT
        //*************************************************************************
        [MaxLength(2000)]
        [StringLength(2000, ErrorMessage = "Client mental illness cannot be longer than 2000 characters.")]
        [DisplayName("Client Mental Illness")]
        public string ClientMentalIllness { get; set; }

        [MaxLength(2000)]
        [StringLength(2000, ErrorMessage = "New member Reportable symptom cannot be longer than 2000 characters.")]
        [DisplayName("New member Reportable symptom")]
        public string NewMemberReportableSymptoms { get; set; }

        [MaxLength(2000)]
        [StringLength(2000, ErrorMessage = "Reason for Referring to SMART cannot be longer than 2000 characters.")]
        [DisplayName("Reason for Referring to SMART")]
        public string ReasonForReferringToSmart { get; set; }

        [Required(ErrorMessage = ErrorMessages.RequiredString)]
        public bool CareProgramApproach { get; set; }
        [Required(ErrorMessage = ErrorMessages.RequiredString)]
        public bool CareManagementEligible { get; set; }
        [Required(ErrorMessage = ErrorMessages.RequiredString)]
        public bool RiskAssessmentDone { get; set; }

        //*************************************************************************
        //GP ACKNOWLEDGEMENT
        //*************************************************************************
        public bool GpAcknowledgement { get; set; }
        
        //*************************************************************************
        //MONITORING INFORMATION
        //*************************************************************************
        [ForeignKey("LivingArea")]
        public int LivingAreaId { get; set; }
        
        public LivingArea LivingArea { get; set; }

        [MaxLength(200)]
        [StringLength(200, ErrorMessage = "Other living area cannot be longer than 200 characters.")]
        [DisplayName("Other living area")]
        public string OtherLivingArea { get; set; }

        [ForeignKey("EthnicOrigin")]
        public int EthnicityId { get; set; }

        
        [DisplayName("Ethnic Origin")]
        public Ethnicity EthnicOrigin { get; set; }

        [ForeignKey("ReferralType")]
        public int ReferralTypeId { get; set; }
        [DisplayName("Referral Type")]
        public ReferralType ReferralType { get; set; }

        [MaxLength(200)]
        [StringLength(200, ErrorMessage = "Other Ethnic Origin cannot be longer than 200 characters.")]
        [DisplayName("Other Ethnic Origin")]
        public string OtherEthnicOrigin { get; set; }

        [ForeignKey("SexualOrientation")]
        public int SexualOrientationId { get; set; }

        public SexualOrientation SexualOrientation { get; set; }
        //*************************************************************************
        //YOU AND SMART
        //*************************************************************************
        [MaxLength(2000)]
        [StringLength(2000, ErrorMessage = "Reason Referred To Smart cannot be longer than 2000 characters.")]
        [DisplayName("Reason Referred To Smart")]
        public string ReasonReferredToSmart { get; set; }

        public bool ReturnToEmployment { get; set; }

        [MaxLength(2000)]
        [StringLength(2000, ErrorMessage = "Type Of Employment Interested cannot be longer than 2000 characters.")]
        [DisplayName("Type Of Employment Interested")]
        public string TypeOfEmploymentInterested { get; set; }

        public bool TrainingOrStudy { get; set; }

        [MaxLength(2000)]
        [StringLength(2000, ErrorMessage = "Area Of Training Or Study cannot be longer than 2000 characters.")]
        [DisplayName("Area Of Training Or Study")]
        public string AreaOfTrainingOrStudy { get; set; }

        [MaxLength(2000)]
        [StringLength(2000, ErrorMessage = "Any Other Condition cannot be longer than 2000 characters.")]
        [DisplayName("Any Other Condition")]
        public string AnyOtherCondition { get; set; }


        //*************************************************************************
        //IMPORTANT INFORMATION / REFERRER AGREEMENT
        //*************************************************************************
        [Required(ErrorMessage = ErrorMessages.RequiredString)]
        public bool CompletedAllSection { get; set; }
        [Required(ErrorMessage = ErrorMessages.RequiredString)]
        public bool EnclosedFullRiskAssessment { get; set; }
        public bool? EnclosedCopyOfCpa { get; set; }


        //*************************************************************************
        //STAFF SECTION
        //*************************************************************************
        //[DisplayName("Broadcast as Alert")]
        //public bool BroadCastAsAlert { get; set; }

        //[DataType(DataType.Date)]
        //[DisplayName("Date")]
        //public DateTime? AlertValidDate { get; set; }

        [MaxLength(2000)]
        [StringLength(2000, ErrorMessage = "Vocational Outcome cannot be longer than 2000 characters.")]
        [DisplayName("Vocational Outcome")]
        public string VocationalOutcome { get; set; }

        [MaxLength(2000)]
        [StringLength(2000, ErrorMessage = "Soft Outcome cannot be longer than 2000 characters.")]
        [DisplayName("Soft Outcome")]
        public string SoftOutcome { get; set; }


        //*************************************************************************
        //ADMIN SECTION
        //*************************************************************************
        [NotMapped]
        [MaxLength(2000)]
        [StringLength(2000, ErrorMessage = "Notes cannot be longer than 2000 characters.")]
        [DisplayName("Notes")]
        public string Note { get; set; }

        [NotMapped]
        public IEnumerable<AdminAlert> AdminAlerts { get; set; }

        [NotMapped]
        public string NotesCreatedBy { get; set; }
        [NotMapped]
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        public DateTime NotesCreateDate { get; set; }

        //public int? NotesId { get; set; }
        //[ForeignKey("NotesId")]
        public virtual ICollection<Notes> Notes { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        [DisplayName("Submit Date")]
        public DateTime FormSubmitDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        [DisplayName("Induction Date")]
        public DateTime? InductionDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        [DisplayName("Start Date")]
        public DateTime? StartDate { get; set; }

        [ForeignKey("Nomination")]
        public int? NominationId { get; set; }

        
        [DisplayName("Nomination")]
        public Nomination Nomination { get; set; }

         [ForeignKey("FundingResponsibility")]
        public int? FundingResponsibilityId { get; set; }
       
        [DisplayName("Funding")]
        public FundingResponsibility FundingResponsibility { get; set; }

        [MaxLength(200)]
        [StringLength(200, ErrorMessage = "Other living area cannot be longer than 200 characters.")]
        [DisplayName("Other (Please specify)")]
        public string OtherFundingResponsibility { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        [DisplayName("Exit Date")]
        public DateTime? ExitDate { get; set; }

        [MaxLength(2000)]
        [StringLength(2000, ErrorMessage = "Reason For Leaving cannot be longer than 2000 characters.")]
        [DisplayName("Reason For Leaving")]
        public string ReasonForLeaving { get; set; }

        [MaxLength(2000)]
        [StringLength(2000, ErrorMessage = "Summary Of Risk cannot be longer than 2000 characters.")]
        [DisplayName("Summary Of Risk")]
        public string SummaryOfRisk { get; set; }

        public virtual ICollection<Project> Projects { get; set; }


        //*************************************************************************
        //GENERAL INFORMATION ABOUT THE RECORD
        //*************************************************************************
        [DisplayName("App Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime ApplicationDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime CreatedOn { get; set; }

        [MaxLength(100)]
        public string CreatedBy { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ModifiedOn { get; set; }

        [MaxLength(100)]
        public string ModifiedBy { get; set; }


        [NotMapped]
        [DisplayName("Broadcast as Alert")]
        public bool BroadCastAsAlert { get; set; }
        
        [DisplayName("Member record submitted")]
        public bool IsSubmit { get; set; }

        [DisplayName("IsContractSigned")]
        public bool IsContractSigned { get; set; }

        [NotMapped]
        [DataType(DataType.Date)]
        [DisplayName("Date")]
        public DateTime? AlertValidDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Last Member Review Date")]
        public DateTime? LastMemberReviewDate { get; set; }

        [NotMapped]
        public IEnumerable<MemberAttendance> CurrentWeekAttendances { get; set; }

        [NotMapped]
        //[FileSize(4194304)]
        [FileTypes("pdf,xls,doc,xlsx,docx,txt")]
        public IEnumerable<HttpPostedFileBase> Files { get; set; }

        public virtual ICollection<BinaryFile> BinaryFilesList { get; set; }

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
        public string CreatedByName { get; set; }
    }
}
