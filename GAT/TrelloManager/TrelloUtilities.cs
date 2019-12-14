using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Linq;
using System.Net;

namespace GAT.TrelloManager
{
    internal static class TrelloUtilities
    {
        internal static TrelloInformation CreateDefaultTrelloInformation(string cardName, string cardBody)
        {
            try
            {
                TrelloInformation information = new TrelloInformation();
                information.CardName = cardName;
                information.CardBody = cardBody;
                information.APIKey = "9238e7214691ddce15623742e4132120";
                information.TokenKey = "9038b05247993df35698cc203abe0b01cc093f962cc6d8d5089a9f464b9686cd";

                return information;
            }
            catch (Exception error)
            {
                Tools.LogWriter.WriteLog(error.Message);
                return null;
            }
        }

        internal static string MakeAndPostTrelloCard(TrelloInformation information)
        {
            try
            {
                string boardName = "GAT";
                string listName = "Emails não lidos";
                string boardId = GetBoardId(information, boardName);
                string listId = GetListId(information, listName, boardId);
                string cardId = CreateAndReturnCard(information, listId);
                return cardId;
            }
            catch (Exception error)
            {
                Tools.LogWriter.WriteLog(error.Message);
                return null;
            }
        }

        internal static bool MoveCardToReadEmail(TrelloInformation information, string cardId, string boardId, string newListId)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                RestClient client = new RestClient("https://api.trello.com");
                RestRequest request = new RestRequest($@"/1/cards/{cardId}/idList?value={newListId}&idBoard={boardId}c&" +
                                                      $"key={information.APIKey}&token={information.TokenKey}", Method.PUT);
                IRestResponse response = client.Execute(request);
                if (response.StatusDescription.ToUpper() == "OK")
                    return true;
                else
                    return false;
            }
            catch (Exception error)
            {
                Tools.LogWriter.WriteLog(error.Message);
                return false;
            }
        }


        internal static string GetListId(TrelloInformation information, string trelloListName, string boardId)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                RestClient client = new RestClient("https://api.trello.com");

                RestRequest request = new RestRequest($@"/1/boards/{boardId}/lists?cards=none&card_fields=all&filter=open&fields=all&" +
                                                      $@"key={information.APIKey}&token={information.TokenKey}", Method.GET);
                IRestResponse response = client.Execute(request);
                JArray returnedJson = (JArray)JsonConvert.DeserializeObject(response.Content);
                JToken[] returnedJsonArray = returnedJson.Children().ToArray();
                foreach (JToken trelloList in returnedJsonArray)
                {
                    JObject listObject = JObject.Parse(trelloList.ToString());
                    string listName = (string)listObject["name"];
                    if (listName == trelloListName)
                        return (string)listObject["id"];
                }
                return null;
            }
            catch (Exception error)
            {
                Tools.LogWriter.WriteLog(error.Message);
                return null;
            }
        }

        internal static string GetBoardId(TrelloInformation information, string trelloBoardName)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                RestClient client = new RestClient("https://api.trello.com");

                RestRequest request = new RestRequest($@"/1/members/me/boards?key={information.APIKey}&token={information.TokenKey}", 
                                                      Method.GET);
                IRestResponse response = client.Execute(request);
                JArray returnedJson = (JArray)JsonConvert.DeserializeObject(response.Content);
                JToken[] returnedJsonArray = returnedJson.Children().ToArray();
                foreach (JToken trelloBoard in returnedJsonArray)
                {
                    JObject boardObject = JObject.Parse(trelloBoard.ToString());
                    string boardName = (string)boardObject["name"];
                    if (boardName == trelloBoardName)
                        return (string)boardObject["id"];
                }

                return null;
            }
            catch (Exception error)
            {
                Tools.LogWriter.WriteLog(error.Message);
                return null;
            }
        }

        private static string CreateAndReturnCard(TrelloInformation information, string listId)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                RestClient client = new RestClient("https://api.trello.com");

                RestRequest request = new RestRequest($@"/1/cards?name={information.CardName}&desc={information.CardBody}&" +
                            $@"idList={listId}&keepFromSource=all&key={information.APIKey}&token={information.TokenKey}", Method.POST);

                IRestResponse response = client.Execute(request);
                if (response.StatusDescription.ToUpper() == "OK")
                {
                    JObject cardJObject = JObject.Parse(response.Content);
                    string cardId = (string)cardJObject["id"];
                    return cardId;
                }
                else
                    return null;
            }
            catch (Exception error)
            {
                Tools.LogWriter.WriteLog(error.Message);
                return null;
            }
        }
    }
}
