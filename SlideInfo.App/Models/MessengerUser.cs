namespace SlideInfo.App.Models
{
    public class MessengerUser
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public int UnreadMessagesCount { get; set; }
        public string PrivateConversationSubject { get; set; }
    }
}
