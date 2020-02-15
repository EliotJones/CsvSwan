namespace CsvSwan
{
    using System;
    using System.Collections.Generic;
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

            var settersByAttribute = new List<(int index, MethodInfo setter, Type propertyType)>();
            var settersByHeader = new List<(int index, MethodInfo setter, Type propertyType)>();
            var settersUnmapped = new List<(int index, MethodInfo setter, Type propertyType)>();

            foreach (var prop in props)
            {
                var attr = prop.GetCustomAttribute<CsvColumnOrderAttribute>();

                // Attributes first.
                if (attr != null || settersByAttribute.Count > 0)
                {
                    if (attr == null)
                    {
                        continue;
                    }

                    settersByAttribute.Add((attr.ColumnIndex, prop.GetSetMethod(true), prop.PropertyType));
                    continue;
                }

                // Then headers
                if (columnHeaders != null)
                {
                    var index = -1;
                    for (var i = 0; i < columnHeaders.Count; i++)
                    {
                        var header = columnHeaders[i];
                        if (string.Equals(header, prop.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            index = i;
                            break;
                        }
                    }

                    if (index < 0)
                    {
                        continue;
                    }

                    settersByHeader.Add((index, prop.GetSetMethod(true), prop.PropertyType));
                    continue;
                }

                settersUnmapped.Add((settersUnmapped.Count, prop.GetSetMethod(true), prop.PropertyType));
            }

            if (settersByAttribute.Count > 0)
            {
                return new TypeMapFactory(type, settersByAttribute);
            }

            if (settersByHeader.Count > 0)
            {
                return new TypeMapFactory(type, settersByHeader);
            }
            
            return new TypeMapFactory(type, settersUnmapped);
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