using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SlideInfo.App.Models
{
    public class Property
    {
        public int Id { get; set; }
        public int SlideId { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }

        public Property()
        {   
        }

        public Property(int slideId, string key, string value)
        {
            SlideId = slideId;
            Key = key;
            Value = value;
        }
    }
}
