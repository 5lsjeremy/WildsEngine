using System.Threading.Tasks;
using WildsEngine.Tests;
using WildsEngine.Tests.Pooling;
using Xunit;

namespace WildsEngine.Core.Tests
{
    public sealed class CertificationTests
    {
        [Fact]
        public async Task ObjectPoolCertification()
        {
            await ObjectPoolTests.RunAllAsync();
        }

        [Fact]
        public async Task WildsEngineCompositionCertification()
        {
            await WildsEngineCertification.RunAllAsync();
        }
    }
}