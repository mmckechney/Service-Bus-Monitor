using System;
using Microsoft.ApplicationInsights;
using Microsoft.ServiceBus;
using Microsoft.Azure.WebJobs.Host;
using System.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.ServiceBus.Messaging;
public static void Run(TimerInfo myTimer, TraceWriter log)
{
    log.Info($"C# Timer trigger function executed at: {DateTime.Now}");
    string telemetryKey = System.Configuration.ConfigurationManager.AppSettings["TelemetryKey"];

    var connStrings = System.Configuration.ConfigurationManager.ConnectionStrings["ServiceBusConnectionString"];
    string conn = connStrings.ConnectionString;

    string queueNames = System.Configuration.ConfigurationManager.AppSettings["QueueNames"];
    string topicNames = System.Configuration.ConfigurationManager.AppSettings["TopicNames"];
    string subNames = System.Configuration.ConfigurationManager.AppSettings["SubscriptionNames"];
    var splitter = new char[] { ',', ';', ':', '|' };

    log.Verbose($"ServiceBusConnectionString: {conn}");

    TelemetryClient telemetry = new TelemetryClient();
    telemetry.InstrumentationKey = telemetryKey;

    NamespaceManager mgr = NamespaceManager.CreateFromConnectionString(conn);

    //Iterate through any defined queues
    if (!string.IsNullOrWhiteSpace(queueNames))
    {
        var queues = queueNames.Split(splitter);
        queues.ToList().ForEach(async queueName =>
        {

            if (await mgr.QueueExistsAsync(queueName))
            {
                QueueDescription queue = await mgr.GetQueueAsync(queueName);

                var counts = queue.MessageCountDetails;
                log.Info($"{queueName} message Counts: Active={counts.ActiveMessageCount}; Deadletter={counts.DeadLetterMessageCount}");

                telemetry.TrackMetric($"{queueName}-Active-Length", counts.ActiveMessageCount);
                telemetry.TrackMetric($"{queueName}-Deadletter-Length", counts.DeadLetterMessageCount);

            }

        });
    }

    if (!string.IsNullOrWhiteSpace(topicNames) && !string.IsNullOrWhiteSpace(subNames))
    {

        var topics = topicNames.Split(splitter);
        var subs = subNames.Split(splitter);
        topics.ToList().ForEach(topicName =>
        {
            subs.ToList().ForEach(async subName =>
            {
                if (await mgr.SubscriptionExistsAsync(topicName, subName))
                {
                    SubscriptionDescription subDesc = await mgr.GetSubscriptionAsync(topicName, subName);

                    var counts = subDesc.MessageCountDetails;
                    log.Info($"{topicName}/{subName} message Counts: Active={counts.ActiveMessageCount}; Deadletter={counts.DeadLetterMessageCount}");

                    telemetry.TrackMetric($"{topicName}-{subName}-Active-Length", counts.ActiveMessageCount);
                    telemetry.TrackMetric($"{topicName}-{subName}-Deadletter-Length", counts.DeadLetterMessageCount);

                }



            });

        });

        telemetry.Flush();
    }
}