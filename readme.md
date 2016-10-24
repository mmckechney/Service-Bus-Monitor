## Azure Service Bus Monitoring with Application Insights

This Azure Function App is a sample how to monitor an Azure Service Bus leveraging Application Insights. 
It will iterate through each queue and topic/subscription, creating custom metrics of Active Message count and DeadLetter Message count for each.

The function is timer triggered and will poll the queue length every 10 sec. To change this adjust the timerTrigger binding in function.json according to your needs.
Please note that this sample just illustrates the concept and is not a fully configurable solution. It is by no means intended for being used in a production scenario as-is!


### Deployment Steps

#### 1. Create Application Insights Instance
If you don't already have an Application Insights instance you want to send the data to:
- Navigate to the Azure portal [https://portal.azure.com](https://portal.azure.com). 
- Next, login and navigate to the Application Insights blade and select _"+Add"_
- Complete the guided setup of your Application Insights instance

From your Application insights blade, click into the instance that you will be using, select the _Overview_ blade and copy the *Instrumentation Key*, you will need that later

#### 2. Create an Azure Function App
- Navigate to [https://functions.azure.com](https://functions.azure.com) and click on _Get Started_ button (at the time of this writing, it's at the bottom of the page)
- Login, name your app and select a region
- Click _Create_ 
- You will then be redirected to the new function page in the Azure portal

#### 3. Configure the App Service 
  - At the bottom left of the Functions blade, there is a link for _Function App Settings_, click that, then click _Configure App Settings_
  - On the Applications Settings blade, add the following App Settings:
    - Key: _TelemetryKey_ ; Value: _the instrumentation key copied from Application insights_
    - Key: _QueueNames_ ; Value: _Comma delimited list of service bus queues names_
    - Key: _TopicNames_ ; Value: _Comma delimited list of service bus topic names_
    - Key: _SubsciptionNames_ ; Value: _Commad delimited list of topic subscription names_
  - Add the following to Connection Settings:
    - Key: _ServiceBusConnectionString_ ; Value: _Connection string to your service bus_
  - Click the _Save_ button to save these settings

#### 4. Deploy your function
- In the Function app blade, click _"+ New function_
- Select _"Empty - C#"_ as the template
- Enter the name of your new function and click _Create_
- In the _Develop_ view /  _Code_ section, click the link for _View Files_
- Click the upload icon and locate your files in the file picker window
- Select the __function.json__, __project.json__, and __run.csx__ files and click _Open_
- Click the _Save_ button to save your function. Compilation will start automatically
- To ensure the function is runing properly, scroll down to the _Logs_ window and you should see a "Compilation succeeded" message. If not, troubleshoot according to the log message

#### 5. Monitor in Application Insights
The function will create 2 custom metrics for each queue and topic/subscription with the format of {name}-Active-Length and {name}-Deadletter-Length
- In the Azure portal, navigate to your Application insights instance
- Select _Metrics Explorer_ and either add a new metric chart or edit an existing one. 
- Scroll down to CUSTOM metrics and select metric(s) you want to add to the chart. 
- The current metric length should now be displayed in the chart.


__Optionally, you can also create an alert for specific threshold values of your new metric.__

Thanks to [Simon Schwingel](https://www.linkedin.com/in/simon-schwingel-b602869a/en) for kickstarting this code!

