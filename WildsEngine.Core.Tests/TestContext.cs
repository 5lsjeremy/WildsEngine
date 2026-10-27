using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Objects;

namespace WildsEngine.Core.Tests;

public sealed class TestContext :
    BaseObject,
    IContext
{
    private readonly IComponent _component;

    private readonly Dictionary<string, object?> _variables =
        new Dictionary<string, object?>();

    private readonly Dictionary<string, object?>
        _contextMetadata =
            new Dictionary<string, object?>();

    public TestContext(
        IComponent component,
        string layer)
        : base(layer)
    {
        _component = component
                     ?? throw new ArgumentNullException(
                         nameof(component));
    }

    public override IComponent Component =>
        _component;

    public IReadOnlyDictionary<string, object?> Variables =>
        _variables;

    IReadOnlyDictionary<string, object?> IContext.Metadata =>
        _contextMetadata;

    public void SetVariable(
        string key,
        object? value)
    {
        ValidateKey(key);

        _variables[key] = value;
    }

    public void SetMetadata(
        string key,
        object? value)
    {
        ValidateKey(key);

        _contextMetadata[key] = value;
    }

    private static void ValidateKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException(
                "Context keys cannot be empty.",
                nameof(key));
    }
}