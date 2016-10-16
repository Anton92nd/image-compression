using System;
using JetBrains.Annotations;

namespace ImageCompression.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class TextAttribute : Attribute
    {
        public string Text { get; private set; }

        public TextAttribute([NotNull] string text)
        {
            Text = text;
        }
    }
}
