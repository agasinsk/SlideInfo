using System;

namespace SlideInfo.App.Models
{
    public class Attachment
    {
        public int Id { get; set; }
        public int DocTypeId { get; set; }
        public virtual DocumentType DocumentType { get; set; }
        public string FileName { get; set; }
        public int UserId { get; set; }
        public string MimeType { get; set; }
        public float FileSize { get; set; }
        public string StoragePath { get; set; }
        public int OrganizationId { get; set; }
        public string FileHash { get; set; }
        public string IpAddress { get; set; }
        public DateTime DateCreated = DateTime.Now;
    }

    public enum DocumentType
    {
        Text, Image
    }
}
