namespace CsvSwan
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal class TypeMapFactory
    {
        private readonly object[] setterBuffer = new object[1];

        private readonly Type type;
        private readonly IReadOnlyList<(int column, MethodInfo setter, Type propertyType)> propertySetters;

        private TypeMapFactory(Type type, IReadOnlyList<(int column, MethodInfo setter, Type propertyType)> propertySetters)
        {
            this.type = type ?? throw new ArgumentNullException(nameof(type));
            this.propertySetters = propertySetters ?? throw new ArgumentNullException(nameof(propertySetters));
        }

        public static TypeMapFactory Create<T>(IReadOnlyList<string> columnHeaders = null)
        {
            // TODO: column headers.

            var type = typeof(T);
            var props = type.GetProperties();

            var setters = new List<(int index, MethodInfo setter, Type propertyType)>();
            var settersUnmapped = new List<(MethodInfo, Type propertyType)>();

            foreach (var prop in props)
            {
                var attr = prop.GetCustomAttribute<CsvColumnOrderAttribute>();

                if (attr == null)
                {
                    settersUnmapped.Add((prop.GetSetMethod(true), prop.PropertyType));
                    continue;
                }

                var val = attr.ColumnIndex;

                setters.Add((val, prop.GetSetMethod(true), prop.PropertyType));
            }

            if (setters.Count > 0)
            {
                return new TypeMapFactory(type, setters);
            }

            return new TypeMapFactory(type, settersUnmapped.Select((x, i) => (i, x.Item1, x.propertyType)).ToList());
        }

        public object Map(Csv.RowAccessor row, IFormatProvider formatProvider)
        {
            var instance = Activator.CreateInstance(type);

            foreach (var setter in propertySetters)
            {
                var propertyType = setter.propertyType;

                object value;

                if (propertyType == typeof(string))
                {
                    value = row.GetString(setter.column);
                }
                else if (propertyType == typeof(short))
                {
                    value = row.GetShort(setter.column, formatProvider);
                }
                else if (propertyType == typeof(int))
                {
                    value = row.GetInt(setter.column, formatProvider);
                }
                else if (propertyType == typeof(long))
                {
                    value = row.GetLong(setter.column, formatProvider);
                }
                else if (propertyType == typeof(double))
                {
                    value = row.GetDouble(setter.column, formatProvider);
                }
                else if (propertyType == typeof(float))
                {
                    value = (float) row.GetDouble(setter.column, formatProvider);
                }
                else if (propertyType == typeof(decimal))
                {
                    value = row.GetDecimal(setter.column, formatProvider);
                }
                else
                {
                    throw new InvalidOperationException($"Cannot map to type {propertyType.FullName} on type {type.FullName}.");
                }

                setterBuffer[0] = value;

                setter.setter.Invoke(instance, setterBuffer);
            }

            return instance;
        }
    }
}