using System.Diagnostics.CodeAnalysis;

namespace Nuages.TaskRunner;

// ReSharper disable once ClassNeverInstantiated.Global
[ExcludeFromCodeCoverage]
public class RunnableTaskDefinition
{
    public string AssemblyQualifiedName { get; set; } = string.Empty;
    public object? Payload { get; set; }
    public string? UserId { get; set; }
}