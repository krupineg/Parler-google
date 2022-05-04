using System.Threading;
using System.Threading.Tasks;
using Google.Events.Protobuf.Cloud.Storage.V1;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using PushObject.Flat;
using PushObject.Model;

namespace PushObject.Test;
using System.Collections.Generic;

public class FunctionFlatTests
{
    private HandlerFlat _sut;
    private IPusher _pusher;

    [SetUp]
    public void Setup()
    {
        var a = new List<int>();
        string.Join(',', a);
        _pusher = Substitute.For<IPusher>();
        _sut = new HandlerFlat(new Logger<HandlerFlat>(new LoggerFactory()), _pusher);
    }

    [Test]
    public async Task Test1()
    {
        await _sut.HandleAsync(
            "parlr-raw-data",
        "avoir.json",
            1,
            CancellationToken.None);
        await _pusher.Received(1).PushAsync(Arg.Any<Verb>(), Arg.Any<long>(), Arg.Any<CancellationToken>());
    }
}