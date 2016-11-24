using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using ContosoBotPrototype.Models;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using Microsoft.WindowsAzure.MobileServices;
using ContosoBotPrototype.DataModels;
using Microsoft.ProjectOxford.Emotion;
using Microsoft.ProjectOxford.Emotion.Contract;

namespace ContosoBotPrototype
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                StateClient stateClient = activity.GetStateClient();
                BotData userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);
                HttpClient client = new HttpClient();
                // declared variables
                var userInput = activity.Text;
                string inputCurrency = "";
                string outputCurrency = "";
                string errormsg = "";
                double result = 0;
                //string loginname = "a";
                bool validcurr = false;
                bool exchangerequest = false;
                bool clearData = false;
                bool setCurr = false;
                bool defex = false;
                bool login = false;
                bool checkBal = false;
                bool reg = false;
                bool del = false;
                bool trans = false;
                bool pic = false;
                if (activity.Type == ActivityTypes.Message && activity.Attachments?.Any() == true)
                {
                    var responseMsg = "";
                    var photoUrl = activity.Attachments[0].ContentUrl;
                    var photoclient = new HttpClient();
                    var photoStream = await photoclient.GetStreamAsync(photoUrl);
                    Scores smileScores;
                    double smilePecentage;
                    const string emotionApiKey = "4d0605f9e44b405084c8ac7c4b291dc6";
                    EmotionServiceClient emotionServiceClient = new EmotionServiceClient(emotionApiKey);

                    try
                    {
                        Emotion[] emotionResult = await emotionServiceClient.RecognizeAsync(photoStream);
                        smileScores = emotionResult[0].Scores;
                        smilePecentage = Math.Ceiling(smileScores.Happiness * 100);
                        responseMsg =  smilePecentage + "% Smile!";
                        Activity reply = activity.CreateReply($"You have a " + responseMsg + " Does that mean you were " + smilePecentage + "% happy with our service? Let us know at 09-CONTOSO or email us feedback@contoso.com");
                        await connector.Conversations.ReplyToActivityAsync(reply);
                    }
                    catch (Exception e)
                    {
                        responseMsg = "Sorry, we were unable to detect your emotions. Please try another photo.";
                        Activity reply = activity.CreateReply(responseMsg);
                        await connector.Conversations.ReplyToActivityAsync(reply);

                    }

                    // Return reply
                    
                    pic = true;
                }
                if (userInput.Length > 6 && userInput.Substring(0,6).ToLower() == "delete")
                {
                    string[] dellist = new string[3];
                    dellist = (userInput.Split());
                    try
                    {
                        string temp = dellist[1];
                        string temp2 = dellist[2];
                    }catch (Exception dele)
                    {
                        Activity reply = activity.CreateReply($"Please include both username and password along with this command");
                        await connector.Conversations.ReplyToActivityAsync(reply);
                    }
                    string delloginname = dellist[1];
                    string delpassword = dellist[2];
                    List<myTable> nowrecords = await AzureManager.AzureManagerInstance.getRecords();
                    bool valid = false;
                    
                    foreach (myTable r in nowrecords)
                    {
                        if(r.userName == delloginname && r.passWord == delpassword)
                        {
                            valid = true;
                            if(r.Balance == 0)
                            {
                                await AzureManager.AzureManagerInstance.deleteRecord(r);
                                Activity reply = activity.CreateReply($"Account has been deleted");
                                await connector.Conversations.ReplyToActivityAsync(reply);
                            }
                            else
                            {
                                Activity reply = activity.CreateReply($"We cannot delete that account as it still has money left in it");
                                await connector.Conversations.ReplyToActivityAsync(reply);
                            }
                            
                        }
                    }

                    if(!valid)
                    {
                        Activity reply = activity.CreateReply($"Incorrect username/password, please try again");
                        await connector.Conversations.ReplyToActivityAsync(reply);
                    }

                    del = true;
                }
                if (userInput.Length >= 8 && userInput.Substring(0,8).ToLower() == "register")
                {
                    string[] reglist = new string[3];
                    reglist = (userInput.Split());
                    try
                    {
                        string temp = reglist[1];
                        string temp2 = reglist[2];
                    }
                    catch (Exception rege)
                    {
                        Activity reply = activity.CreateReply($"Please include both username and password along with this command");
                        await connector.Conversations.ReplyToActivityAsync(reply);
                    }
                    string regloginname = reglist[1];
                    string regpassword = reglist[2];
                    List<myTable> records = await AzureManager.AzureManagerInstance.getRecords();
                    bool duplicate = false;
                    foreach(myTable record in records)
                    {
                        if (record.userName == regloginname)
                        {
                            duplicate = true;
                        }
                    }

                    if (duplicate)
                    {
                        Activity reply = activity.CreateReply($"Sorry, this username already exists please try another one");
                        await connector.Conversations.ReplyToActivityAsync(reply);
                    }else
                    {
                        myTable newRecord = new myTable()
                        {
                            userName = regloginname,
                            passWord = regpassword,
                            Balance = 0.00
                        };

                        await AzureManager.AzureManagerInstance.addRecord(newRecord);
                        Activity reply = activity.CreateReply($"Register Success");
                        await connector.Conversations.ReplyToActivityAsync(reply);
                    }

                    reg = true;
                }
                
                if(userInput.Length >5 && userInput.Substring(0,5).ToLower() == "login")
                {
                    login = true;
                    string[] list = new string[3];
                    list = (userInput.Split());
                    try
                    {
                        string temp = list[1];
                        string temp2 = list[2];
                    }
                    catch (Exception loge)
                    {
                        Activity reply = activity.CreateReply($"Please include both username and password along with this command");
                        await connector.Conversations.ReplyToActivityAsync(reply);
                    }
                    string loginname = list[1];
                    string password = list[2];
                    List<myTable> getrecords = await AzureManager.AzureManagerInstance.getRecords();
                    bool exists = false;
                    foreach (myTable record in getrecords)
                    {
                        if (record.userName == loginname)
                        {
                            exists = true;
                        }
                    }
                    if (!exists)
                    {
                        Activity reply = activity.CreateReply($"This username does not exist, please try again");
                        await connector.Conversations.ReplyToActivityAsync(reply);
                    }
                    else
                    {
                        List<string> records = await AzureManager.AzureManagerInstance.getPassWord(loginname);
                        string output = records[0];

                        if (password == output && exists)
                        {
                            userData.SetProperty<string>("LoginName", loginname);
                            userData.SetProperty<bool>("LoginStatus", true);
                            await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                            Activity reply = activity.CreateReply($"Login success");
                            await connector.Conversations.ReplyToActivityAsync(reply);
                        }
                        else
                        {
                            Activity reply = activity.CreateReply($"Incorrect password please try again");
                            await connector.Conversations.ReplyToActivityAsync(reply);
                        }
                    }
                    
                }

                if (userInput.ToLower() == "my balance")
                {
                    checkBal = true;
                    bool loggedIn = userData.GetProperty<bool>("LoginStatus");
                    if (loggedIn == true)
                    {
                        string clientLogin = userData.GetProperty<string>("LoginName");
                        List<double> clientBalance = await AzureManager.AzureManagerInstance.getBalance(clientLogin);
                        double cash = clientBalance[0];
                        Activity reply = activity.CreateReply($"You currently have $" + cash + " in your account");
                        await connector.Conversations.ReplyToActivityAsync(reply);
                    }else
                    {
                        Activity reply = activity.CreateReply($"Please log in first");
                        await connector.Conversations.ReplyToActivityAsync(reply);
                    }
                }

                if(userInput.Length > 9 && userInput.Substring(0,8).ToLower() == "transfer" && userData.GetProperty<bool>("LoginStatus") == true)
                {
                    string[] command = new string[4];
                    command = userInput.Split();
                    try
                    {
                        string temp = command[1];
                        string temp2 = command[3];
                    }
                    catch (Exception transe)
                    {
                        Activity reply = activity.CreateReply($"The correct format for this command should be 'Transfer $(amount) to (account username)'");
                        await connector.Conversations.ReplyToActivityAsync(reply);
                    }
                    double amount = double.Parse(command[1].Substring(1));
                    string targetAcc = command[3];
                    string srcAcc = userData.GetProperty<string>("LoginName");
                    List<myTable> transrecords = await AzureManager.AzureManagerInstance.getRecords();
                    bool targetExist = false;
                    
                    /*foreach(var item in transrecords)
                    {
                        //item.Balance = item.Balance + 10;
                        await AzureManager.AzureManagerInstance.updateRecord(item);
                        Activity reply = activity.CreateReply(item.userName);
                        await connector.Conversations.ReplyToActivityAsync(reply);
                    }*/
                    foreach(myTable t in transrecords)
                    {
                        if (t.userName == targetAcc)
                        {
                            targetExist = true;
                            t.Balance += amount;
                            await AzureManager.AzureManagerInstance.updateRecord(t);
                        }
                    }
                    if (targetExist)
                    {
                        foreach(myTable t in transrecords)
                        {
                            if(t.userName == srcAcc)
                            {
                                t.Balance = t.Balance - amount;
                                await AzureManager.AzureManagerInstance.updateRecord(t);
                                Activity reply = activity.CreateReply($"Successfully transferred $" + amount + " to " + targetAcc);
                                await connector.Conversations.ReplyToActivityAsync(reply);
                            }
                        }
                    }


                    if (!targetExist)
                    {
                        Activity reply = activity.CreateReply($"Target account does not exist");
                        await connector.Conversations.ReplyToActivityAsync(reply);
                    }
                    
                    trans = true;
                }
                else if(userInput.Length > 9 && userInput.Substring(0, 8).ToLower() == "transfer" && userData.GetProperty<bool>("LoginStatus") != true)
                {
                    Activity reply = activity.CreateReply($"Please log in first");
                    await connector.Conversations.ReplyToActivityAsync(reply);
                    trans = true;
                }

                if (userInput.Length >= 5 && userInput.Substring(0,5).ToLower() == "clear")
                {
                    clearData = true;
                    Activity reply = activity.CreateReply($"User data has been cleared");
                    await stateClient.BotState.DeleteStateForUserAsync(activity.ChannelId, activity.From.Id);
                    await connector.Conversations.ReplyToActivityAsync(reply);
                    
                }

                if (userInput.Length >= 12 && userInput.Substring(0,12).ToLower() == "set currency")
                {
                    setCurr = true;
                    string defaultCurrency = userInput.Substring(13, 3).ToUpper();
                    userData.SetProperty<string>("DefaultCurrency", defaultCurrency);
                    await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                    Activity reply = activity.CreateReply($"User default currency has been set to " + defaultCurrency);
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }

                if (userInput.Length >= 11 && userInput.Substring(0,11).ToLower() == "exchange to")
                {
                    defex = true;
                    string targetCurrency = userInput.Substring(12, 3);
                    string currentUnit = userData.GetProperty<string>("DefaultCurrency"); //gets preset default currency
                    if (currentUnit == null)
                    {
                        Activity reply = activity.CreateReply($"Please set a default currency first");
                        await connector.Conversations.ReplyToActivityAsync(reply);
                    }else
                    {
                        userInput = "exchange rate " + currentUnit + " to " + targetCurrency;
                        
                    }
                }
                
                if (userInput.Length > 17 && userInput.Substring(0,13).ToLower() == "exchange rate")
                {
                    exchangerequest = true;
                    inputCurrency = userInput.Substring(14, 3).ToUpper();
                    outputCurrency = userInput.Substring(21, 3).ToUpper();
                    if (inputCurrency != "NZD" && inputCurrency != "USD" && inputCurrency != "AUD" && inputCurrency != "EUR" && inputCurrency != "CNY")
                    {
                        goto invalidcurr;
                        
                    }

                    else if (outputCurrency != "NZD" && outputCurrency != "USD" && outputCurrency != "EUR" && outputCurrency != "AUD" && outputCurrency != "CNY")
                    {
                        validcurr = false;
                    }
                    else
                    {
                        validcurr = true;
                    }

                
                    string currencyApi = await client.GetStringAsync(new Uri("http://api.fixer.io/latest?base="+inputCurrency));
                    CurrencyObjects.RootObject rootObject;

                    rootObject = JsonConvert.DeserializeObject<CurrencyObjects.RootObject>(currencyApi);

                    switch (outputCurrency)
                    {
                        case "NZD":
                            result = rootObject.rates.NZD;
                            
                            break;
                        case "USD":
                            result = rootObject.rates.USD;
                            
                            break;
                        case "AUD":
                            result = rootObject.rates.AUD;
                            
                            break;
                        case "EUR":
                            result = rootObject.rates.EUR;
                            
                            break;
                        case "CNY":
                            result = rootObject.rates.CNY;
                            
                            break;
                        default:
                            
                            break;
                    }
                    
                }
                
                if (userInput.ToLower()=="other services")
                {
                    Activity replyToConversation = activity.CreateReply("For other services such as loan or mortgage please head to our website");
                    replyToConversation.Recipient = activity.From;
                    replyToConversation.Type = "message";
                    replyToConversation.Attachments = new List<Attachment>();

                    List<CardImage> cardImages = new List<CardImage>();
                    cardImages.Add(new CardImage(url: "https://i.imgur.com/dfsOvah.png"));

                    List<CardAction> cardButtons = new List<CardAction>();
                    CardAction plButton = new CardAction()
                    {
                        Value = "http://www.asb.co.nz",
                        Type = "openUrl",
                        Title = "Click to visit website"
                    };
                    cardButtons.Add(plButton);

                    ThumbnailCard plCard = new ThumbnailCard()
                    {
                        Title = "Contoso Bank",
                        Subtitle = "For more services please visit our website",
                        Images = cardImages,
                        Buttons = cardButtons
                    };

                    Attachment plAttachment = plCard.ToAttachment();
                    replyToConversation.Attachments.Add(plAttachment);
                    await connector.Conversations.SendToConversationAsync(replyToConversation);

                    return Request.CreateResponse(HttpStatusCode.OK);

                }
                // return our reply to the user

                if (!exchangerequest && !validcurr && !clearData && !setCurr && !defex &&!login &&!checkBal &&!reg &&!del &&!trans &&!pic)
                {
                    Activity reply = activity.CreateReply($"Hi, my service commands include my balance, login, register, transfer, exchange rate and other services. Please double check your commands if you encounter an error and make sure to include username and password in appropriate commands. You can also send us a photo of yourself to give us feedback");
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }
                if (exchangerequest && validcurr)
                {
                    Activity reply = activity.CreateReply($"Currently for every 1 " + inputCurrency + " you can get " + result + " " + outputCurrency + errormsg);
                    await connector.Conversations.ReplyToActivityAsync(reply);
                    exchangerequest = false;
                }
            invalidcurr:
                validcurr = false;
                if (exchangerequest && !validcurr)
                {
                    Activity reply = activity.CreateReply($"Oops, something went wrong");
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }
                
                
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}