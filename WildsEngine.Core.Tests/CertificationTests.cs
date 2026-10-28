using System.Threading.Tasks;
using WildsEngine.Tests;
using WildsEngine.Tests.Logging;
using WildsEngine.Tests.Pooling;
using WildsEngine.Tests.Registry;
using Xunit;

namespace WildsEngine.Core.Tests
{
    public sealed class CertificationTests
    {
        [Fact]
        public async Task ObjectPoolCertification()
        {
            TestLog.Initialize();

            TestLog.Section(
                "ObjectPool Certification");

            TestLog.Write(
                "[START] ObjectPool certification");

            await ObjectPoolTests.RunAllAsync();

            TestLog.Pass(
                "ObjectPool certification");
        }

        [Fact]
        public async Task RegistryCertification()
        {
            TestLog.Initialize();

            TestLog.Section(
                "Registry Certification");

            TestLog.Write(
                "[START] Registry certification");

            await RegistryTests.RunAllAsync();

            TestLog.Pass(
                "Registry certification");
        }

        [Fact]
        public void TaskRegistryCertification()
        {
            TestLog.Initialize();

            TestLog.Section(
                "Task Registry Certification");

            TestLog.Write(
                "[START] Task registry certification");

            TaskRegistryTests.RunAll();

            TestLog.Pass(
                "Task registry certification");
        }

        [Fact]
        public async Task WildsEngineCompositionCertification()
        {
            TestLog.Initialize();

            TestLog.Section(
                "WildsEngine Composition Certification");

            TestLog.Write(
                "[START] WildsEngine composition certification");

            await WildsEngineCertification.RunAllAsync();

            TestLog.Pass(
                "WildsEngine composition certification");
        }

        [Fact]
        public async Task EndToEndProcessingCertification()
        {
            TestLog.Initialize();

            TestLog.Section(
                "End-to-End Processing Certification");

            TestLog.Write(
                "[START] End-to-end processing certification");

            await EndToEndProcessingTests.RunAllAsync();

            TestLog.Pass(
                "End-to-end processing certification");
        }
    }
}