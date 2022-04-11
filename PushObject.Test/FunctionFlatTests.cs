using System.Threading;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Google.Events.Protobuf.Cloud.Storage.V1;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace PushObject.Test;

public class FunctionFlatTests
{
    private FunctionFlat _sut;
    
    [SetUp]
    public void Setup()
    {
        var idProvider = NSubstitute.Substitute.For<IProjectIdProvider>();
        idProvider.Id.Returns("parlr-342110");
        _sut = new FunctionFlat(new Logger<FunctionFlat>(new LoggerFactory()), idProvider);
    }

    [Test]
    public async Task Test1()
    {
        await _sut.HandleAsync(new CloudEvent(),
            new StorageObjectData() {Bucket = "parlr-raw-data", Name = "avoir.json"}, CancellationToken.None);
    }
}