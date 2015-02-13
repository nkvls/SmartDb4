using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Web;

namespace SmartDb4.Attributes
{
    public class FileTypesAttribute : ValidationAttribute
    {
        private readonly List<string> _types;

        public FileTypesAttribute(string types)
        {
            _types = types.Split(',').ToList();
        }

        public override bool IsValid(object value)
        {
            if (value == null) return true;

            var newObj = value as IEnumerable<HttpPostedFileBase>;
            foreach (var item in newObj)
            {
                if(item == null)
                    continue;   //that means there is no file uploaded

                var extension = Path.GetExtension(item.FileName);
                if (extension != null)
                {
                    var fileExt = extension.Substring(1);
                    if (!_types.Contains(fileExt, StringComparer.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public override string FormatErrorMessage(string name)
        {
            return string.Format("Invalid file type. Only the following types {0} are supported.", String.Join(", ", _types));
        }
    }
}