using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace SlideInfo.App.Models
{
    public class MessengerUser
    {
        public string AppUserId { get; set; }

        public string ConnectionId { get; set; }

        public MessengerStatus Status { get; set; }
    }

    public enum MessengerStatus
    {
        Active, Inactive, Away
    }
}
