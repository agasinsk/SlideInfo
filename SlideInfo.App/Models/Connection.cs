using System;

namespace SlideInfo.App.Models
{
    public class Connection
    {
        public string Id { get; set; }
        public string UserAgent { get; set; }
        public bool IsConnected { get; set; }

        public DateTime LastActive { get; set; }
    }
}

