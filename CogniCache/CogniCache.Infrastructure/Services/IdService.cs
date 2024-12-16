using CogniCache.Domain.Services;
using Sqids;

namespace CogniCache.Infrastructure.Services
{
    public class IdService : IIdService
    {
        private readonly SqidsEncoder<long> _squid;

        public IdService()
        {
            _squid = new SqidsEncoder<long>(new()
            {
                Alphabet = "ABCEDFGHIJ0123456789",
                MinLength = 6,
            });
        }

        public string Generate(long number)
        {
            return _squid.Encode(number);
        }
    }
}
