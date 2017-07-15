using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SlideInfo.App.Models.SlideViewModels
{
    public class PropertiesViewModel
    {
        public string Name { get; set; }
        public IOrderedEnumerable<KeyValuePair<string,string>> Properties { get; set; }

        public PropertiesViewModel(string name, IOrderedEnumerable<KeyValuePair<string, string>> properties)
        {
            Name = name;
            Properties = properties;
        }
    }
}
