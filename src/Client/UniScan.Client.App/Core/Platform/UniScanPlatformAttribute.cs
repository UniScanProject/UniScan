using System;

namespace UniScan.Client.App.Core.Platform;

//hack to get correct platform assembly
//without this, android breaks
[AttributeUsage(AttributeTargets.Assembly)]
public class UniScanPlatformAttribute : Attribute;