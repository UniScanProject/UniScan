using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using Avalonia.Data.Converters;
using DotNetty.Codecs;

namespace UniScan.Client.App.UI.Conversion;

// public static class NumericConverters<T>
// where T : struct, INumber<T>, IComparable<T>, IEquatable<T>
// {
//     public static IMultiValueConverter GreaterThan =>
//         new FuncMultiValueConverter<T, bool>(v => v is [var lhs, var rhs, ..] && lhs > rhs);
//     
//     public static IMultiValueConverter LessThan =>
//         new FuncMultiValueConverter<T, bool>(v => v is [var lhs, var rhs, ..] && lhs < rhs);
//         
//     public static IMultiValueConverter GreaterThanOrEqual =>
//         new FuncMultiValueConverter<T, bool>(v => v is [var lhs, var rhs, ..] && lhs >= rhs);
//     
//     public static IMultiValueConverter LessThanOrEqual =>
//         new FuncMultiValueConverter<T, bool>(v => v is [var lhs, var rhs, ..] && lhs <= rhs);
// }

//THIS WAS SO FUCKING STUPID TO GET WORKING I NEVER WANT TO DEAL WITH THAT AGAIN HOLY FUCK
public abstract class NumericConverters<T>
    where T : struct, INumber<T>
{
    public static Converter GreaterThan => new Converter(Operator.GreaterThan);
    public static Converter LessThan => new Converter(Operator.LessThan);
    public static Converter GreaterThanOrEqualTo => new Converter(Operator.GreaterThanOrEqual);
    public static Converter LessThanOrEqualTo => new Converter(Operator.LessThanOrEqual);

    public enum Operator
    {
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual
    }

    public class Converter(Operator op) : IValueConverter, IMultiValueConverter
    {
        private bool Eval(T lhs, T rhs) => op switch
        {
            Operator.GreaterThan        => lhs > rhs,
            Operator.LessThan           => lhs < rhs,
            Operator.GreaterThanOrEqual => lhs >= rhs,
            Operator.LessThanOrEqual    => lhs <= rhs,
            _                           => false
        };

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is T lhs)
            {
                if (parameter is string str && T.TryParse(str, CultureInfo.InvariantCulture, out T rhs))
                    return Eval(rhs, lhs);
                
                if (parameter is T rhs2)
                    return Eval(lhs, rhs2);
            }

            return false;
        }
        
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture) => values is [T lhs, T rhs, ..] && Eval(lhs, rhs);

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}

public class ByteConverters : NumericConverters<byte>;

public class SByteConverters : NumericConverters<sbyte>;

public class UInt16Converters : NumericConverters<ushort>;

public class UShortConverters : UInt16Converters;

public class Int16Converters : NumericConverters<short>;

public class ShortConverters : Int16Converters;

public class UInt32Converters : NumericConverters<uint>;

public class UIntConverters : UInt32Converters;

public class Int32Converters : NumericConverters<int>;

public class IntConverters : Int32Converters;

public class UInt64Converters : NumericConverters<ulong>;

public class ULongConverters : UInt64Converters;

public class Int64Converters : NumericConverters<long>;

public class LongConverters : Int64Converters;

public class UInt128Converters : NumericConverters<UInt128>;

public class Int128Converters : NumericConverters<Int128>;

public class DecimalConverters : NumericConverters<decimal>;

public class DoubleConverters : NumericConverters<double>;

public class FloatConverters : NumericConverters<float>;

public class SingleConverters : FloatConverters;

public class HalfConverters : NumericConverters<Half>;