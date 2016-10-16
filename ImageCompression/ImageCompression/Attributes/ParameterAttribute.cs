using System;
using JetBrains.Annotations;

namespace ImageCompression.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class ParameterAttribute : Attribute
    {
        public ParameterAttribute([NotNull] string parameterName, object defaultValue = null)
        {
            ParameterName = parameterName;
            DefaultValue = defaultValue;
        }

        [NotNull]
        public string ParameterName { get; private set; }

        [CanBeNull]
        public object DefaultValue { get; private set; }
    }
}
