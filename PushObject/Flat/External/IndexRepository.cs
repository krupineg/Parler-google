using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.PubSub.V1;
using Microsoft.Extensions.Logging;

namespace PushObject.Flat.External
{
    internal sealed class IndexRepository : IIndexRepository
    {
        private const string RequestTopic = "parlr-increment";
        private const string ResponseTopicSubscription = "parlr-increment-response-sub";
        private readonly ILogger<IndexRepository> _logger;
        private readonly IProjectIdProvider _projectIdProvider;
        private readonly HashSet<Guid> _requests;
        private readonly int WaitingTimeoutSeconds;

        public IndexRepository(IProjectIdProvider projectIdProvider, ILogger<IndexRepository> logger)
        {
            WaitingTimeoutSeconds = int.Parse(Environment.GetEnvironmentVariable("INDEX_RESPONSE_TIMEOUT_SECONDS") ?? "300");
            _projectIdProvider = projectIdProvider;
            _logger = logger;
            _requests = new HashSet<Guid>();
        }

        public async Task<long> ObtainIndex()
        {
            var poll = Poll();
            await Publish();
            var result = await poll;
            return result.Single();
        }

        private async Task Publish()
        {
            var origin = Guid.NewGuid();
            _logger.LogInformation($"preparing index request with origin: {origin}");
            TopicName topicName = TopicName.FromProjectTopic(_projectIdProvider.Id, RequestTopic);
            var publisher = await PublisherClient.CreateAsync(topicName);
            
            _logger.LogInformation($"publisher created for topic: {topicName.TopicId}");
            string message = await publisher.PublishAsync(new PubsubMessage()
            {
                Attributes = {{"origin", origin.ToString()}}
            });
            _logger.LogInformation($"message sent with id: {message}");
            _requests.Add(origin);
        }

        private async Task<List<long>> Poll()
        {
            var subscriptionName =
                SubscriptionName.FromProjectSubscription(_projectIdProvider.Id, ResponseTopicSubscription);
            var subscriber = await SubscriberClient.CreateAsync(subscriptionName);
            // SubscriberClient runs your message handle function on multiple
            // threads to maximize throughput.
            
            var result = new List<long>();
            var mre = new ManualResetEventSlim(false);
            var startTask = subscriber.StartAsync((PubsubMessage message, CancellationToken cancel) =>
            {
                if (_requests.Contains(Guid.Parse(message.Attributes["origin"])))
                {
                    try
                    {
                        result.Add(long.Parse(message.Attributes["value"]));
                        return Task.FromResult(SubscriberClient.Reply.Ack);
                    }
                    finally
                    {
                        mre.Set();
                    }
                }

                return Task.FromResult(SubscriberClient.Reply.Nack);
            });
            // Run for 5 seconds.
            mre.Wait(TimeSpan.FromSeconds(WaitingTimeoutSeconds));
            await subscriber.StopAsync(CancellationToken.None);
            // Lets make sure that the start task finished successfully after the call to stop.
            await startTask;
            return result;
        }

    }
}