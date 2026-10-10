using System;
using Shiki.Common.Result;
using UniScan.Client.App.Views.SSR;
using UniScan.Client.Core.SSR;
using UniScan.UserInterface;
using UniScan.UserInterface.Nodes.Core.Node;

namespace UniScan.Client.App.UI.ServersideRendering;

public interface IUIViewFactory
{
    Result<object, Exception> CreateView(IUINode node, SSRSlotContext ctx);
}