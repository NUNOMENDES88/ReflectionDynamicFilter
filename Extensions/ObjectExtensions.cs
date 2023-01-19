using System;
using System.ComponentModel;

namespace ReflectionDynamicFilter.Extensions
{
    public static class ObjectExtensions
    {
        public static Object ParseData(this Object input, Type type)
        {
            if (input.GetType() != type)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                {
                    type = Nullable.GetUnderlyingType(type);
                }
                try
                {
                    return TypeDescriptor.GetConverter(type).ConvertFromInvariantString(input.ToString());
                    //return Convert.ChangeType(input, type);
                }
                catch (Exception)
                {
                    return Activator.CreateInstance(type);
                }
            }
            return input;
        }
    }
}
