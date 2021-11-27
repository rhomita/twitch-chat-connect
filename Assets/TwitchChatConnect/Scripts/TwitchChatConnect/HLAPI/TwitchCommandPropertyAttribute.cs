using System;

namespace TwitchChatConnect.HLAPI
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class TwitchCommandPropertyAttribute : Attribute
    {
        public int Position { get; }
        public bool Required { get; }

        public TwitchCommandPropertyAttribute(int position, bool required = true)
        {
            Position = position;
            Required = required;
        }
    }
}