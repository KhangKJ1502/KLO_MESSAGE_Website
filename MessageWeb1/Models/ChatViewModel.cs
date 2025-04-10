namespace MessageWeb1.Models
{
    public class ChatViewModel
    {
        public string CurrentUser { get; set; } = string.Empty;
        public string ToUser { get; set; } = string.Empty;
        public List<Message> Messages { get; set; } = new List<Message>();
    }
}
