using System;
using JetBrains.Annotations;

namespace ImageCompression.Exceptions
{
    public class ExtensionNotSupportedException : Exception
    {
        public ExtensionNotSupportedException([NotNull] string message) : base(message)
        {
        }
    }
}
