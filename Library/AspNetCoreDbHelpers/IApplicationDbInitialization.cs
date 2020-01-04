using Microsoft.Extensions.Configuration;

namespace DbHelpers
{
    public interface IApplicationDbInitialization
    {
        void Init();
    }
}
