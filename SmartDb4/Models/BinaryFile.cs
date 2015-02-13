using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using System.Linq;
using System.Web;
using SmartDb4.Attributes;
using SmartDb4.Helpers;

namespace SmartDb4.Models
{
    [Table("BinaryFile")]
    public class BinaryFile
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int BinaryFileId { get; set; }

        [MaxLength(200)]
        [StringLength(200, ErrorMessage = ErrorMessages.MaxLen200)]
        public string FileName { get; set; }

        public string FileExtension { get; set; }

        public byte[] FileContent { get; set; }

        [ForeignKey("Member")]
        public int MemberId { get; set; }

        public virtual Member Member { get; set; }
    }
}