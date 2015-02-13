using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartDb4.Helpers;

namespace SmartDb4.Models
{
    [Table("UserProfile")]
    public class UserProfile
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [StringLength(50, ErrorMessage = ErrorMessages.MaxLen50)]
        [DisplayName("User Name")]
        public string UserName { get; set; }

        [Required(ErrorMessage = ErrorMessages.RequiredString)]
        [StringLength(100, ErrorMessage = ErrorMessages.MaxLen100)]
        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = ErrorMessages.RequiredString)]
        [StringLength(100, ErrorMessage = ErrorMessages.MaxLen100)]
        [DisplayName("Last Name")]
        public string LastName { get; set; }

        [StringLength(200, ErrorMessage = ErrorMessages.MaxLen200)]
        public string Agency { get; set; }

        [StringLength(100, ErrorMessage = ErrorMessages.MaxLen100)]
        public string Address1 { get; set; }

        [StringLength(100, ErrorMessage = ErrorMessages.MaxLen100)]
        public string Address2 { get; set; }

        [StringLength(50, ErrorMessage = ErrorMessages.MaxLen50)]
        public string City { get; set; }

        [StringLength(20, ErrorMessage = ErrorMessages.MaxLen20)]
        public string PostCode { get; set; }

        [DisplayName("Work Tel")]
        [DataType(DataType.PhoneNumber)]
        [StringLength(50, ErrorMessage = ErrorMessages.MaxLen50)]
        public string WorkTelephone { get; set; }

        [DisplayName("Mobile")]
        [DataType(DataType.PhoneNumber)]
        [StringLength(50, ErrorMessage = ErrorMessages.MaxLen50)]
        public string MobileTelephone { get; set; }

        [Required(ErrorMessage = ErrorMessages.RequiredString)]
        [StringLength(100, ErrorMessage = ErrorMessages.MaxLen100)]
        //[EmailAddress(ErrorMessage = "Invalid Email Address")]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Invalid Email")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required(ErrorMessage = ErrorMessages.RequiredString)]
        [StringLength(50, ErrorMessage = ErrorMessages.MaxLen50)]
        [NotMapped]
        public string Password { get; set; }

        [DisplayName("Relationship")]
        public string RelationshipWithApplicant { get; set; }

        [ForeignKey("UserType")]
        public int UserTypeId { get; set; }

        [MaxLength(100)]
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        [MaxLength(100)]
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        
        public virtual UserType UserType { get; set; }
        public virtual ICollection<Project> Projects { get; set; }

        [DisplayName("Is Historic")]
        public bool IsHistoric { get; set; }
        
        [NotMapped]
        public bool CanEdit { get; set; }
        [NotMapped]
        public string FullName { get; set; }
        [NotMapped]
        public string OldUserNameWhenChangedByAdmin { get; set; }
    }
}