namespace Restate.Sdk;

/// <summary>
///     A typed key for reading and writing durable state in virtual objects and workflows.
/// </summary>
public readonly record struct StateKey<T>(string Name);