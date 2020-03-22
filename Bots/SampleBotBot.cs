using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Linq;
using Microsoft.Bot.Builder.AI.QnA;

namespace SampleBot.Bots
{
    public class SampleBotBot : ActivityHandler
    {
        public QnAMaker SampleBotQnA { get; private set; }

        public SampleBotBot(QnAMakerEndpoint endpoint)
        {
            // connects to QnA Maker endpoint for each turn
            SampleBotQnA = new QnAMaker(endpoint);
        }

        // This handles the incoming message
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
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

            // Otherwise just process the response like normal
            else
            {
                // Send the echo text
                var replyText = $"Echo: {turnContext.Activity.Text}";
                await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);

                // Send the result from the Q&A maker
                await AccessQnAMaker(turnContext, cancellationToken);
            }
        }

        // This is the welcome message when a new user joins
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            // Use a suggested action to let the user click on suggested starting places when starting the chat
            var reply = MessageFactory.Text("Welcome to the Sample Bot. You can ask questions or start with suggested actions below:");
            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    // Do not add the response to the chat history
                    new CardAction() { Title = "What is the sound of one hand clapping?", Type = ActionTypes.PostBack, Value = "clap" },
                    
                    // Add the response to the chat history
                    new CardAction() { Title = "Why is the sky blue?", Type = ActionTypes.ImBack, Value = "Why is the sky blue?" },
                },
            };
        
            // Could be multiple people added so respond to each
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(reply, cancellationToken);

                    //await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }

        // This is the call to the QnA maker
        private async Task AccessQnAMaker(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var results = await SampleBotQnA.GetAnswersAsync(turnContext);
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
