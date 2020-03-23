using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Linq;
using Microsoft.Bot.Builder.AI.QnA;
using System;
using System.IO;

namespace SampleBot.Bots
{
    public class SampleBotBot : ActivityHandler
    {
        private readonly QnAMaker _sampleBotQnA;
        private readonly BotState _userState;
        private readonly BotConfig _botConfig;

        public SampleBotBot(QnAMakerEndpoint endpoint, UserState userState, BotConfig botConfig)
        {
            // connects to QnA Maker endpoint for each turn
            _sampleBotQnA = new QnAMaker(endpoint);

            // set the user state
            _userState = userState;

            // set the config
            _botConfig = botConfig;
        }

        // This handles the incoming message
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // Get the accessors for user state
            var userStateAccessors = _userState.CreateProperty<UserProfile>(nameof(UserProfile));
            var userProfile = await userStateAccessors.GetAsync(turnContext, () => new UserProfile());

            // Check for suggested actions
            var text = turnContext.Activity.Text;
            string response = "";
            switch (text)
            {
                case "clap":
                    response = "It cannot be known.";
                    break;
                case "Why is the sky blue?":
                    response = "Because it is, grasshopper.";
                    break;
            }
            if (!string.IsNullOrEmpty(response))
            {
                await turnContext.SendActivityAsync(MessageFactory.Text(response), cancellationToken);
            }

            // See if the user entered their name
            else if (text.StartsWith("My name is"))
            {
                // Get the name, save it back to the state, and say hello
                userProfile.Name = text.Substring(10).Trim();
                await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
                await turnContext.SendActivityAsync(MessageFactory.Text($"Hello {userProfile.Name}"), cancellationToken);
            }

            // Otherwise just process the response like normal
            else
            {
                // Use the username if set
                string user = string.IsNullOrEmpty(userProfile.Name) ? "Unknown User" : userProfile.Name;

                // Send the echo text
                var replyText = $"Echo from {user}: {turnContext.Activity.Text}";
                await turnContext.SendActivityAsync(MessageFactory.Text(replyText), cancellationToken);

                // Send the result from the Q&A maker
                await AccessQnAMaker(turnContext, cancellationToken);
            }
        }

        // This is the welcome message when a new user joins
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            // Do the welcome as a hero card
            var card = new HeroCard
            {
                Title = "Welcome to the Sample Bot",
                Subtitle = "A sample intended to show how the framework operates",
                Text = @"You can ask free-form questions for the QnA bot or start with suggested actions below. If you tell your name (by typing 'My name is X'), it will be remembered",
                Images = new List<CardImage>() { new CardImage(_botConfig.BaseUrl + "/logo.png") },
                
                Buttons = new List<CardAction>()
                {
                    // Do not add the response to the chat history
                    new CardAction() { Title = "What is the sound of one hand clapping?", Type = ActionTypes.PostBack, Value = "clap" },
                    
                    // Add the response to the chat history
                    new CardAction() { Title = "Why is the sky blue?", Type = ActionTypes.ImBack, Value = "Why is the sky blue?" },
                }
            };
            var reply = MessageFactory.Attachment(card.ToAttachment());

            //// Create a media attachment for the welcome message
            //var imagePath = Path.Combine(Environment.CurrentDirectory, @"Resources\logo.png");
            //var imageData = Convert.ToBase64String(File.ReadAllBytes(imagePath));
            //var imageAttachment = new Attachment
            //{
            //    Name = @"Resources\logo.png",
            //    ContentType = "image/png",
            //    ContentUrl = $"data:image/png;base64,{imageData}",
            //};
            //var reply = MessageFactory.Attachment(new List<Attachment>() { imageAttachment });

            //// Set the text of the reply
            //reply.Text = "Welcome to the Sample Bot. You can ask free-form questions for the QnA bot or start with suggested actions below. If you tell your name (by typing 'My name is X'), it will be remembered:";
            
            //// Use a suggested action to let the user click on suggested starting places when starting the chat
            //reply.SuggestedActions = new SuggestedActions()
            //{
            //    Actions = new List<CardAction>()
            //    {
            //        // Do not add the response to the chat history
            //        new CardAction() { Title = "What is the sound of one hand clapping?", Type = ActionTypes.PostBack, Value = "clap" },
                    
            //        // Add the response to the chat history
            //        new CardAction() { Title = "Why is the sky blue?", Type = ActionTypes.ImBack, Value = "Why is the sky blue?" },
            //    },
            //};

            // Could be multiple people added so respond to each
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(reply, cancellationToken);
                }
            }
        }

        // This is the call to the QnA maker
        private async Task AccessQnAMaker(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var results = await _sampleBotQnA.GetAnswersAsync(turnContext);
            if (results.Any())
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("QnA Maker Returned: " + results.First().Answer), cancellationToken);
            }
            else
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("Sorry, could not find an answer in the Q and A system."), cancellationToken);
            }
        }
    }
}
