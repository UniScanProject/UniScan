using System;

namespace UniScan.Client.App.UI.ConnectionMethod;

[AttributeUsage(AttributeTargets.Class)]
public class ConnectionMethodFactoryViewModelAttribute(string displayName) : Attribute
{
    public string DisplayName { get; } = displayName;
}