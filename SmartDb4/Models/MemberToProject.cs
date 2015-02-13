using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartDb4.Models
{
    public class MemberToProject
    {
        [Key]
        public int MemberToProjectId { get; set; }

        public int ProjectId { get; set; }

        public int MemberId { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime AssignDate { get; set; }

        public int MemberRoleId { get; set; }

        public decimal MemberRate { get; set; }

        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; }

        [ForeignKey("MemberId")]
        public virtual Member Member { get; set; }

        [ForeignKey("MemberRoleId")]
        public virtual MemberRole MemberRole { get; set; }

        [MaxLength(100)]
        public string CreatedBy { get; set; }

        public DateTime CreatedOn { get; set; }

        [MaxLength(100)]
        public string ModifiedBy { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public bool IsAssigned { get; set; }

        [NotMapped]
        public bool MemberAlreadyAssigned { get; set; }
    }

    [NotMapped]
    public class MembersToProjectCollection
    {
        public Project Project { get; set; }
        public List<CutDownVersionOfMembers> AllCutdownMembers { get; set; }
        public List<Member> AllMembers { get; set; }
        public List<MemberRole> Role { get; set; }
    }

    [NotMapped]
    public class CutDownVersionOfMembers
    {
        public int MemberId { get; set; }
        public int ProjectId { get; set; }
        public int MemberToProjectId { get; set; }

        [DisplayName("Name")]
        public string MemberName { get; set; }

        public bool MemberAlreadyExists { get; set; }
        public bool IsAssigned { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        [DisplayName("Assign Date")]
        public DateTime AssignDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.##}")]
        public decimal Rate { get; set; }

        public int RoleId { get; set; }
        public MemberRole Role { get; set; }
    }
}