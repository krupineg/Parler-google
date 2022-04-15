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
            var poll = Poll().ConfigureAwait(false);
            await Publish();
            var result = await poll;
            return result.Single();
        }

        private async Task Publish()
        {
            var origin = Guid.NewGuid();
            _requests.Add(origin);
            _logger.LogInformation($"preparing index request with origin: {origin}");
            TopicName topicName = TopicName.FromProjectTopic(_projectIdProvider.Id, RequestTopic);
            var publisher = await PublisherClient.CreateAsync(topicName);
            
            _logger.LogInformation($"publisher created for topic: {topicName.TopicId}");
            string message = await publisher.PublishAsync(new PubsubMessage()
            {
                Attributes = {{"origin", origin.ToString()}}
            });
            _logger.LogInformation($"message sent with id: {message}");
        }

        private Task<List<long>> Poll()
        {
            return Task.Run(async () =>
            {
                _logger.LogInformation($"prepare poll");
                var subscriptionName =
                    SubscriptionName.FromProjectSubscription(_projectIdProvider.Id, ResponseTopicSubscription);

                _logger.LogInformation($"created subscription {_projectIdProvider.Id} {ResponseTopicSubscription}");
                var subscriber = await SubscriberClient.CreateAsync(subscriptionName);

                _logger.LogInformation($"created subscriber {subscriber.SubscriptionName}");
                // SubscriberClient runs your message handle function on multiple
                // threads to maximize throughput.

                var result = new List<long>();
                var mre = new ManualResetEvent(false);
                var startTask = subscriber.StartAsync((PubsubMessage message, CancellationToken cancel) =>
                {
                    _logger.LogInformation(
                        $"received message {message.MessageId} {string.Join(",", message.Attributes.Select(x => $"{x.Key} : {x.Value}"))}");
                    if (_requests.Contains(Guid.Parse(message.Attributes["origin"])))
                    {
                        _logger.LogInformation($"origin is correct");
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

                    _logger.LogInformation($"origin is different");
                    return Task.FromResult(SubscriberClient.Reply.Nack);
                }).ConfigureAwait(false);

                _logger.LogInformation($"started task");
                mre.WaitOne(TimeSpan.FromSeconds(WaitingTimeoutSeconds));
                await subscriber.StopAsync(CancellationToken.None);

                await startTask;
                return result;
            });

        }

    }
}