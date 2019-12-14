using GAT.GmailManager;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace GAT
{
    class Program
    {
        static string[] Scopes = { GmailService.Scope.GmailReadonly };
        static string ApplicationName = "Gmail API .NET Quickstart";

        static void Main(string[] args)
        {
            try
            {
                List<Message> Messages = null;
                List<GmailMessageObject> GmailXTrelloMessages = new List<GmailMessageObject>();
                while (true)
                {
                    UserCredential credential;
                    using (FileStream stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
                    {
                        string credPath = "token.json";
                        credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                            GoogleClientSecrets.Load(stream).Secrets,
                            Scopes,
                            "user",
                            CancellationToken.None,
                            new FileDataStore(credPath, true)).Result;
                        Console.WriteLine("Credential file saved to: " + credPath);
                    }

                    GmailService service = new GmailService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = ApplicationName,
                    });


                    GmailUtilities.MessagesList = Messages;
                    GmailUtilities.GmailXTrelloMessagesList = GmailXTrelloMessages;
                    GmailUtilities.ReadMails(service, "lucasgat.sp.25@gmail.com");
                    Messages = GmailUtilities.MessagesList;
                    GmailXTrelloMessages = GmailUtilities.GmailXTrelloMessagesList;

                    UsersResource.LabelsResource.ListRequest request = service.Users.Labels.List("me");
                    IList<Label> labels = request.Execute().Labels;
                    Console.WriteLine("Labels:");
                    if (labels != null && labels.Count > 0)
                    {
                        foreach (var labelItem in labels)
                            Console.WriteLine("{0}", labelItem.Name);
                    }
                    else
                    {
                        Console.WriteLine("No labels found.");
                    }
                }

            }
            catch (Exception error)
            {
                Tools.LogWriter.WriteLog(error.Message);
            }
            Task.Delay(1000);
        }
    }
}