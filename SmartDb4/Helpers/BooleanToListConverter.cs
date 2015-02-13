using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SmartDb4.Helpers
{
    public class BooleanToListConverter
    {
        public static IEnumerable<SelectListItem> SelectList(string defaultText = null, string defaultTrue = "True", string defaultFalse = "False")
        {
            var list = new List<SelectListItem>
        {
            new SelectListItem {Text = defaultTrue, Value = "True", Selected = true},
            new SelectListItem {Text = defaultFalse, Value = "False"}
        };

            if (defaultText != null)
            {
                list.Insert(0, new SelectListItem
                {
                    Text = defaultText,
                    Value = string.Empty
                });
            }

            return list.OrderBy(x=>x.Text);
        }
    }
}