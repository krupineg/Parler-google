using System.Threading;
using System.Threading.Tasks;
using PushObject.Model;

namespace PushObject.Flat
{
    public interface IPusher
    {
        Task PushAsync(Verb verb, long verbIndex, CancellationToken cancellationToken);
    }
}