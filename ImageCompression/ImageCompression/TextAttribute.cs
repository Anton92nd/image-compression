using System;
using JetBrains.Annotations;

namespace ImageCompression
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field)]
    public class TextAttribute : Attribute
    {
        public string Text { get; private set; }

        public TextAttribute([NotNull] string text)
        {
            Text = text;
        }
    }
}
