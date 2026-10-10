using System;
using Shiki.Common.Identity;

namespace UniScan.Client.App.Core.Platform;

//hack to get correct platform assembly
//without this, android breaks
[AttributeUsage(AttributeTargets.Assembly)]
public class UniScanPlatformAttribute(string id, string displayName) : Attribute
{
    public Identifier Identifier { get; } = Identifier.CreateInstance(id);
    public string DisplayName { get; } = displayName;
}