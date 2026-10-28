using System.Threading.Tasks;

namespace WildsEngine.Core.Tests
{
    internal static class Program
    {
        public static Task Main(string[] args)
        {
            // xUnit is the canonical WildsEngine.Core test runner.
            //
            // Run certifications from this project with:
            //
            //     dotnet test -c Debug
            //     dotnet test -c Release
            //
            // CertificationTests owns the certification entry points.
            return Task.CompletedTask;
        }
    }
}