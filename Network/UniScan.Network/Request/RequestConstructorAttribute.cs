namespace UniScan.Network.Request;

/// <summary>
/// Automatically generates a IRequestFactoryConstructable.CreateInstance implementation
/// </summary>
[AttributeUsage(AttributeTargets.Constructor)]
public class RequestConstructorAttribute : Attribute;