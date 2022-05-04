using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.BigQuery.V2;
using Google.Cloud.Storage.V1;
using Google.Events.Protobuf.Cloud.Storage.V1;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PushObject.Model;
using PushObject.Model.V2;

namespace PushObject.Flat
{
    public class HandlerFlat
    {
        private readonly BigQueryClient _bigQueryClient;
        private readonly ILogger<HandlerFlat> _logger;
        private readonly IPusher _pusher;
        private readonly StorageClient _storageClient;

        public HandlerFlat(ILogger<HandlerFlat> logger, IPusher pusher)
        {
            _logger = logger;
            _pusher = pusher;
            _storageClient = StorageClient.Create(GoogleCredential.GetApplicationDefault());
        }

        public async Task HandleAsync(string bucket, string name, long verbIndex, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Storage bucket: {bucket}");
            _logger.LogInformation($"Object being handled: {name}");

            await using var stream = new MemoryStream();
            await _storageClient.DownloadObjectAsync(bucket, name, stream,
                cancellationToken: cancellationToken);
            stream.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(stream);
            var str = await reader.ReadToEndAsync().ConfigureAwait(false);
            var verb = JsonConvert.DeserializeObject<Verb>(str);
            await _pusher.PushAsync(verb, verbIndex, cancellationToken).ConfigureAwait(false);
            _logger.LogInformation($"Object was handled successfully: {name}");
        }
    }
}