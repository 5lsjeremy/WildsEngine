using System.Text.Json.Serialization;

namespace WildsEngine.Core.Tests.Certification
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum WildsQslCertificationStatus
    {
        Passed = 0,
        Failed = 1,
        MissingDependency = 2
    }
}