using System.Threading.Tasks;

namespace PushObject.Flat
{
    public interface IIndexRepository
    {
        Task<long> ObtainIndex();
    }
}