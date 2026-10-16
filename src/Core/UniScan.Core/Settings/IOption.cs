using ObservableCollections;
using R3;
using Shiki.Common.Identity;
using Shiki.Common.Result;
using Shiki.Internationalization.Component;

namespace UniScan.Core.Settings;

public interface IReadOnlyOption
{
    Identifier Identifier { get; }
    
    ITextComponent Name { get; }
    ITextComponent Description { get; }
    
    Observable<Unit> ValueChanged { get; }
    
    Type Type { get; }
    
    object? GetValue();
}

public interface IOption : IReadOnlyOption
{
    BooleanResult<Exception> SetValue(object? value);
}

public interface IReadOnlyOption<T> : IReadOnlyOption
{
    new T GetValue();
}

public interface IOption<T> : IOption, IReadOnlyOption<T>
{
    Observable<Unit> IReadOnlyOption.ValueChanged => Value.AsObservable().Select(_ => Unit.Default).Skip(1);
    
    IBindableReactiveProperty<T> Value { get; }

    Type IReadOnlyOption.Type => typeof(T);

    
    BooleanResult<Exception> IOption.SetValue(object? value) => value is T v ? SetValue(v) : new InvalidOperationException("Given value is not of type " + typeof(T));
    BooleanResult<Exception> SetValue(T value);
    
    object? IReadOnlyOption.GetValue() => GetValue();
    T IReadOnlyOption<T>.GetValue() => Value.Value;
}

public class SettingOption<T> : IOption<T>
{
    public Identifier Identifier { get; }
    public Identifier CategoryIdentifier { get; }
    
    public required ITextComponent Name { get; init;  }
    public required ITextComponent Description { get; init; }

    public IBindableReactiveProperty<T> Value { get; }
    
    public SettingOption(Identifier id, Identifier category, T defaultValue)
    {
        Identifier = id;
        CategoryIdentifier = category;
        Value = new BindableReactiveProperty<T>(defaultValue);
    }
    
    public BooleanResult<Exception> SetValue(T value)
    {
        Value.Value = value;
        return BooleanResult<Exception>.Successful;
    }
}

public class SettingsCategory(Identifier id, ITextComponent name)
{
    public Identifier Id { get; } = id;
    public ITextComponent Name { get; } = name;

    public Identifier? Parent { get; init; }
}