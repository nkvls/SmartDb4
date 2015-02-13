using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SmartDb4.Models
{
    public class SearchParameters
    {
        [Display(Name = "Search By Terrritory")]
        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }
        public int NominationId { get; set; }
        public int ProjectId { get; set; }
        public int AgeBracket { get; set; }
        public int AgeBracketId { get; set; }
        public int GenderId { get; set; }
        public int EthnicityId { get; set; }
        public int FundingResponsibilityId { get; set; }
        public string GroupByClause { get; set; }


    }
}