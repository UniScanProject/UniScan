using System;
using Shiki.Common.Identity;

namespace UniScan.Client.App.Core.Platform;

//hack to get correct platform assembly
//without this, android breaks
[AttributeUsage(AttributeTargets.Assembly)]
public class UniScanPlatformAttribute(string id) : Attribute
{
    public Identifier Identifier { get; } = Identifier.CreateInstance(id);
}