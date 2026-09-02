using System.Collections.Generic;
using Shiki.Common.Identity;
using UniScan.Client.App.Views.SSR;

namespace UniScan.Client.App.UI.ServersideRendering;

public interface IUISlotRegistry
{
    public IUISlotControlViewModel? Get(Identifier id);
    
    public void Add(IUISlotControlViewModel control);
    public void Add(Identifier id, IUISlotControlViewModel control);
    
    public void Remove(IUISlotControlViewModel control);
    public void Remove(Identifier id, out IUISlotControlViewModel? control);
}

public class UISlotRegistry : IUISlotRegistry
{
    private readonly Dictionary<Identifier, IUISlotControlViewModel> _controls = [];
    
    public IUISlotControlViewModel? Get(Identifier id) => _controls.TryGetValue(id, out IUISlotControlViewModel? control) ? control : null;

    public void Add(IUISlotControlViewModel control) => Add(control.Identifier, control);

    public void Add(Identifier id, IUISlotControlViewModel control) => _controls[control.Identifier] = control;
    

    public void Remove(IUISlotControlViewModel control) => _controls.Remove(control.Identifier);

    public void Remove(Identifier id, out IUISlotControlViewModel? control) => _controls.Remove(id, out control);
}