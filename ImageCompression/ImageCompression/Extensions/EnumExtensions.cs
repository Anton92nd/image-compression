using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace ImageCompression.Extensions
{
    public static class EnumExtensions
    {
        [NotNull]
        public static string GetText<T>([NotNull] this T element)
        {
            return element.ToString();
        }

        public static IEnumerable<T> GetEnumValues<T>([NotNull] this Type enumType)
        {
            if (!enumType.IsEnum)
                throw new Exception(string.Format("'{0}' must be enum", enumType.Name));
            return Enum.GetValues(enumType).Cast<T>();
        }
    }
}
