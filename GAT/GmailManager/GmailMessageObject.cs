using Google.Apis.Gmail.v1.Data;

namespace GAT.GmailManager
{
    internal class GmailMessageObject
    {
        internal Message GmailMessage { get; set; }
        internal string TrelloCardId { get; set; }
    }
}
