using System;
using System.Collections.Generic;
using System.Reflection;
using TwitchChatConnect.Client;
using TwitchChatConnect.Data;
using UnityEngine;

namespace TwitchChatConnect.HLAPI
{
    public class TwitchCommandHandler
    {
        public delegate void CommandHandler<T>(T template) where T : TwitchCommandPayload;
        private Dictionary<string, Action<TwitchChatCommand>> _registers { get; } = new Dictionary<string, Action<TwitchChatCommand>>();

        public void Register<T>(string command, CommandHandler<T> template) where T : TwitchCommandPayload
        {
            string prefix = TwitchChatClient.instance.CommandPrefix;
            if (!command.StartsWith(prefix)) command = prefix + command;

            _registers.Add(command, (chatCommand) => template(ReadChatCommand<T>(chatCommand)));
        }

        public void ProcessCommand(TwitchChatCommand chatCommand)
        {
            if (!_registers.ContainsKey(chatCommand.Command)) Debug.LogWarning("Command invalid");
            else _registers[chatCommand.Command](chatCommand);
        }

        private T ReadChatCommand<T>(TwitchChatCommand command) where T : TwitchCommandPayload
        {
            T template = Activator.CreateInstance<T>();
            Type templateType = template.GetType();

            templateType.BaseType.GetProperty("Command", BindingFlags.Public | BindingFlags.Instance).SetValue(template, command);            

            foreach (PropertyInfo property in template.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if(property.GetCustomAttribute(typeof(TwitchCommandPropertyAttribute), true) is TwitchCommandPropertyAttribute attribute)
                {
                    if (command.Parameters.Length <= attribute.Position)
                    {
                        Debug.LogWarning($"The {nameof(T)} command requires the {property.Name} argument.");
                        return default;
                    }

                    string value = command.Parameters[attribute.Position];
                    if (property.PropertyType == typeof(string)) property.SetValue(template, value);
                    else if (property.PropertyType == typeof(int) && int.TryParse(value, out int floatValue)) property.SetValue(template, floatValue);
                    else if (property.PropertyType == typeof(float) && float.TryParse(value, out float intValue)) property.SetValue(template, intValue);
                    else Debug.LogWarning($"The {nameof(T)} command property {property.Name} invalid type, require {property.PropertyType}");
                }                
            }

            templateType.GetMethod("Initialize", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Invoke(template, null);
            return template;
        }
    }
}
