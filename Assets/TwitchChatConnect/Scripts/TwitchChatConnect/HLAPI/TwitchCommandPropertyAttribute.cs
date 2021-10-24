using System;

namespace TwitchChatConnect.HLAPI
{
    [AttributeUsage(AttributeTargets.Property)]
    public class TwitchCommandPropertyAttribute : Attribute
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
