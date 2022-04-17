using System.Threading;
using System.Threading.Tasks;
using Google.Events.Protobuf.Cloud.Storage.V1;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using PushObject.Flat;

namespace PushObject.Test;
using System.Collections.Generic;

public class FunctionFlatTests
{
    private HandlerFlat _sut;
    
    [SetUp]
    public void Setup()
    {
        var a = new List<int>();
        string.Join(',', a);
        var idProvider = NSubstitute.Substitute.For<IProjectIdProvider>();
        idProvider.Id.Returns("parlr-342110");
        _sut = new HandlerFlat(new Logger<HandlerFlat>(new LoggerFactory()), idProvider);
    }

    [Test]
    public async Task Test1()
    {
        await _sut.HandleAsync(
            "parlr-raw-data",
        "avoir.json",
            1,
            CancellationToken.None);
    }
}