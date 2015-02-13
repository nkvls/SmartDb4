using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SmartDb4.Attributes
{
    public class FileSizeAttribute : ValidationAttribute
    {
        private readonly int _maxSize;

        public FileSizeAttribute(int maxSize)
        {
            _maxSize = maxSize;
        }
        public override bool IsValid(object value)
        {
            if (value == null) return true;

            var newObj = value as IEnumerable<HttpPostedFileBase>;

            foreach (var item in newObj)
            {
                if (item.ContentLength >= _maxSize)
                {
                    return false;
                }
            }

            return true;
        }

        public override string FormatErrorMessage(string name)
        {
            return string.Format("The File size should not exceed {0}", _maxSize);
        }
    }
}