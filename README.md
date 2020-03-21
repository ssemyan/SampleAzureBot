# SampleBot

Bot Framework v4 bot sample showing echo and integration with QnA Maker. This bot was created using the [Bot Framework v4 SDK Templates for Visual Studio](https://marketplace.visualstudio.com/items?itemName=BotBuilder.botbuilderv4)
and following the instructions from the tutorial [Create and deploy a basic bot](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-tutorial-basic-deploy?view=azure-bot-service-4.0&tabs=csharp)
in the [Azure Bot Service Documentation](https://docs.microsoft.com/en-us/azure/bot-service/?view=azure-bot-service-4.0)

## Prerequisites

- [.NET Core SDK](https://dotnet.microsoft.com/download) version 2.1

  ```bash
  # determine dotnet version
  dotnet --version
  ```

  [QnA Maker Setup](https://www.qnamaker.ai/)

  Follow the instructions here to create a QnA maker. Once your QnA Maker app is published, select the SETTINGS tab, and scroll down to Deployment details. 
  
  Copy the following values from the Postman HTTP example request.

  ```
  POST /knowledgebases/<knowledge-base-id>/generateAnswer
  Host: <your-hostname>  // NOTE - this is a URL ending in /qnamaker.
  Authorization: EndpointKey <qna-maker-resource-key>
  ```

  Then add the values from above to your *appsetting.json* file:

  ```
  {
    "MicrosoftAppId": "",
    "MicrosoftAppPassword": "",
  
    "QnAKnowledgebaseId": "knowledge-base-id",
    "QnAAuthKey": "qna-maker-resource-key",
    "QnAEndpointHostName": "your-hostname" 
  }
  ```


## To run the sample

- Run the bot from a terminal or from Visual Studio

### From a terminal

  ```bash
  # run the bot
  dotnet run
  ```

### From Visual Studio

  - Launch Visual Studio
  - File -> Open -> Project/Solution
  - Navigate to `SampleBot` folder
  - Select `SampleBot.csproj` file
  - Press `F5` to run the project

## Testing the bot using Bot Framework Emulator

[Bot Framework Emulator](https://github.com/microsoft/botframework-emulator) is a desktop application that allows bot developers to test and debug their bots on localhost or running 
remotely through a tunnel.

- Install the Bot Framework Emulator version 4.5.0 or greater from [here](https://github.com/Microsoft/BotFramework-Emulator/releases)

### Connect to the bot using Bot Framework Emulator

- Launch Bot Framework Emulator
- File -> Open Bot
- Enter your Bot URL of `http://localhost:3978/api/messages`

## Deploy the bot to Azure

### Set up Azure Services

You can use the *create_azure_services.sh* bash script in the *DeploymentTemplates* directory of this project to quickly set up all the required services in Azure. 

Otherwise you can use the ARM Templates from the *DeploymentTemplates* directory of this project. 

Once the App Service is created, you can publish the app by right-clicking on the project, choose Publish, then select the App Service created above. 

To learn more about deploying a bot to Azure, see [Deploy your bot to Azure](https://aka.ms/azuredeployment) for a complete list of deployment instructions 

## Test the bot in Azure

To test the bot in Azure, navigate in the portal to the Bot Service created above and choose the "Test in Web Chat" blade.

## Further reading

- [Bot Framework Documentation](https://docs.botframework.com)
- [Bot Basics](https://docs.microsoft.com/azure/bot-service/bot-builder-basics?view=azure-bot-service-4.0)
- [Activity processing](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-concept-activity-processing?view=azure-bot-service-4.0)
- [Azure Bot Service Introduction](https://docs.microsoft.com/azure/bot-service/bot-service-overview-introduction?view=azure-bot-service-4.0)
- [Azure Bot Service Documentation](https://docs.microsoft.com/azure/bot-service/?view=azure-bot-service-4.0)
- [.NET Core CLI tools](https://docs.microsoft.com/en-us/dotnet/core/tools/?tabs=netcore2x)
- [Azure CLI](https://docs.microsoft.com/cli/azure/?view=azure-cli-latest)
- [Azure Portal](https://portal.azure.com)
- [Language Understanding using LUIS](https://docs.microsoft.com/en-us/azure/cognitive-services/luis/)
- [Channels and Bot Connector Service](https://docs.microsoft.com/en-us/azure/bot-service/bot-concepts?view=azure-bot-service-4.0)
