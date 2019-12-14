using GAT.TrelloManager;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GAT.GmailManager
{
    internal static class GmailUtilities
    {
        // TODO:
        // Substituir por um Singleton  
        internal static List<Message> MessagesList { get; set; }
        internal static List<GmailMessageObject> GmailXTrelloMessagesList { get; set; }


        internal static void ReadMails(GmailService service, string userId)
        {
            try
            {
                string defaultSubjectValue = "Trello";
                UsersResource.MessagesResource.ListRequest inboxlistRequest = service.Users.Messages.List(userId);
                inboxlistRequest.LabelIds = "INBOX";
                inboxlistRequest.Q = "is:unread";
                inboxlistRequest.IncludeSpamTrash = false;
                ListMessagesResponse emailListResponse = inboxlistRequest.Execute();
                if (emailListResponse != null && emailListResponse.Messages != null)
                {
                    List<Message> newMessageList = new List<Message>();
                    if (MessagesList == null)
                        MessagesList = new List<Message>();

                    foreach (Message email in emailListResponse.Messages)
                    {
                        UsersResource.MessagesResource.GetRequest emailInfoRequest = service.Users.Messages.Get(userId, email.Id);
                        Message emailInfoResponse = emailInfoRequest.Execute();
                        if (emailInfoResponse != null)
                        {
                            string subjectValue = emailInfoResponse.Payload.Headers.Where(header => header.Name == "Subject")
                                                                   .Select(subj => subj.Value).ToArray().First();
                            if (subjectValue.Contains(defaultSubjectValue))
                            {
                                newMessageList.Add(emailInfoResponse);
                                if (MessagesList.Find(x => x.Id == emailInfoResponse.Id) == null)
                                {
                                    string cardBody = emailInfoResponse.Snippet;
                                    TrelloInformation information = TrelloUtilities.CreateDefaultTrelloInformation(subjectValue, cardBody);
                                    string cardId = TrelloUtilities.MakeAndPostTrelloCard(information);
                                    if (cardId != null)
                                    {
                                        GmailMessageObject gmailMessage = CreateGmailObject(emailInfoResponse, cardId);
                                        if (gmailMessage != null)
                                            GmailXTrelloMessagesList.Add(gmailMessage);
                                        Console.WriteLine("Card criado com sucesso! ");
                                    }
                                    else
                                        Console.WriteLine("Não foi possível criar o Card.");
                                }
                            }
                        }
                    }       
                    List<Message> returnedList = MessageListVerifier(newMessageList);
                    if (returnedList != null)
                        MoveEmailToReadCard(returnedList);
                }
            }
            catch (Exception error)
            {
                Tools.LogWriter.WriteLog(error.Message);
            }
        }
        private static List<Message> MessageListVerifier(List<Message> newMessageList)
        {
            try
            {
                if (MessagesList.Count == 0)
                {
                    MessagesList = newMessageList;
                    return null;
                }
                else
                {
                    List<Message> readMessageList = new List<Message>();
                    foreach (Message message in MessagesList)
                    {
                        if (newMessageList.Find(m => m.Id == message.Id) == null)
                            readMessageList.Add(message);
                    }
                    MessagesList = newMessageList;
                    return readMessageList;
                }
            }
            catch (Exception error)
            {
                Tools.LogWriter.WriteLog(error.Message);
                return null;
            }
        }

        private static void MoveEmailToReadCard(List<Message> readMessageList)
        {
            try
            {
                string boardId = TrelloUtilities.GetBoardId(TrelloUtilities.CreateDefaultTrelloInformation(null, null), "GAT");
                string newListId = TrelloUtilities.GetListId(TrelloUtilities.CreateDefaultTrelloInformation(null, null),
                                                             "Emails lidos", boardId);
                foreach (Message readMessage in readMessageList)
                {
                    GmailMessageObject gmailMessage = GmailXTrelloMessagesList.Find(email => email.GmailMessage.Id == readMessage.Id);
                    TrelloUtilities.MoveCardToReadEmail(TrelloUtilities.CreateDefaultTrelloInformation(null, null),
                                                        gmailMessage.TrelloCardId, boardId, newListId);
                    GmailXTrelloMessagesList.Remove(gmailMessage);
                }
            }
            catch (Exception error)
            {
                Tools.LogWriter.WriteLog(error.Message);
            }
        }

        private static GmailMessageObject CreateGmailObject(Message message, string cardId)
        {
            try
            {
                GmailMessageObject gmailMessage = new GmailMessageObject();
                gmailMessage.GmailMessage = message;
                gmailMessage.TrelloCardId = cardId;

                return gmailMessage;
            }
            catch (Exception error)
            {
                Tools.LogWriter.WriteLog(error.Message);
                return null;
            }
        }
    }
}
