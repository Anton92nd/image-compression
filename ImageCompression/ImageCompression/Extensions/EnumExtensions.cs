using System;
using System.Collections.Generic;
using System.Linq;
using ImageCompression.Attributes;
using JetBrains.Annotations;

namespace ImageCompression.Extensions
{
    public static class EnumExtensions
    {
        [NotNull]
        public static string GetText<T>([NotNull] this T element)
        {
            if (!typeof(T).IsEnum)
                throw new Exception(string.Format("Type '{0}' is not enum", typeof(T).Name));
            var memberInfo = typeof(T).GetMember(element.ToString()).First();
            var attr = memberInfo.GetCustomAttributes(typeof(TextAttribute), false).Cast<TextAttribute>().Single();
            return attr.Text;
        }

        [CanBeNull]
        public static ParameterAttribute GetParameter<T>([NotNull] this T element)
        {
            if (!typeof(T).IsEnum)
                throw new Exception(string.Format("Type '{0}' is not enum", typeof(T).Name));
            var memberInfo = typeof(T).GetMember(element.ToString()).First();
            var attr = memberInfo.GetCustomAttributes(typeof(ParameterAttribute), false).Cast<ParameterAttribute>().SingleOrDefault();
            return attr;
        }


        [NotNull]
        public static IEnumerable<T> GetEnumValues<T>([NotNull] this Type enumType)
        {
            if (!enumType.IsEnum)
                throw new Exception(string.Format("'{0}' must be enum", enumType.Name));
            return Enum.GetValues(enumType).Cast<T>();
        }
    }
}
