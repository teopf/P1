using System;

namespace Data
{
    [Serializable]
    public class ChatData
    {
        public string SenderName;
        public string MessageContent;
        // public DateTime Timestamp; // 필요 시 추가

        public ChatData(string sender, string content)
        {
            SenderName = sender;
            MessageContent = content;
        }
    }
}
