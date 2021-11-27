using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using TwitchChatConnect.Data;

namespace TwitchChatConnect.HLAPI
{
    public class TwitchCommandHandler
    {
        public delegate void CommandHandler<T>(T template) where T : TwitchCommandPayload;

        public TwitchCommandLogger Logger { get; private set; } = new TwitchCommandLogger();

        private Dictionary<string, Action<TwitchChatCommand>> _registers { get; } = new Dictionary<string, Action<TwitchChatCommand>>();
        private string _prefix { get; }

        public TwitchCommandHandler(string prefix)
        {
            _prefix = prefix;
        }

        public bool Register<T>(string command, CommandHandler<T> template) where T : TwitchCommandPayload
        {
            if (!command.StartsWith(_prefix)) command = _prefix + command;

            if (_registers.ContainsKey(command))
            {
                Logger.Error($"A record already exists for this command [{command}]");
                return false;
            }

            _registers.Add(command, (chatCommand) =>
            {
                T payload = ParsePayload<T>(chatCommand);
                if (payload != null)
                {
                    template(payload);
                    Logger.Info($"Execute Command:{chatCommand.Command}");
                }
            });
            return true;
        }

        public bool ProcessCommand(TwitchChatCommand chatCommand)
        {
            if (!_registers.ContainsKey(chatCommand.Command))
            {
                Logger.Warning($"The command {chatCommand.Command} is not registered.");
                return false;
            }

            _registers[chatCommand.Command](chatCommand);
            Logger.Info($"Process Command:{chatCommand.Command}");
            return true;
        }

        private T ParsePayload<T>(TwitchChatCommand command) where T : TwitchCommandPayload
        {
            try
            {
                T template = Activator.CreateInstance<T>();
                Type templateType = template.GetType();

                templateType.BaseType.GetProperty("Command", BindingFlags.Public | BindingFlags.Instance).SetValue(template, command);

                HashSet<TwitchCommandPropertyAttribute> requiredProperties = new HashSet<TwitchCommandPropertyAttribute>();

                TwitchCommandPropertyAttribute[] attributes = (TwitchCommandPropertyAttribute[])templateType.GetCustomAttributes(typeof(TwitchCommandPropertyAttribute), true);
                foreach (TwitchCommandPropertyAttribute attribute in attributes)
                    if (attribute.Required)
                        requiredProperties.Add(attribute);

                PropertyInfo[] properties = template.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (PropertyInfo property in properties)
                {
                    if (property.GetCustomAttribute(typeof(TwitchCommandPropertyAttribute), true) is TwitchCommandPropertyAttribute attribute)
                    {
                        if (command.Parameters.Length <= attribute.Position)
                        {
                            Logger.Error($"{nameof(T)} command requires the {property.Name} argument.");
                            return default;
                        }

                        string value = command.Parameters[attribute.Position];
                        if (property.PropertyType == typeof(string))
                        {
                            property.SetValue(template, value);
                        }
                        else if (property.PropertyType == typeof(int) &&
                                 int.TryParse(Regex.Replace(value, "[^A-Za-z0-9 -]", ""), out int intValue))
                        {
                            property.SetValue(template, intValue);
                        }
                        else if (property.PropertyType == typeof(float) &&
                                 float.TryParse(value.Replace(",", CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator).Replace("f", ""), out float floatValue))
                        {
                            property.SetValue(template, floatValue);
                        }
                        else
                        {
                            Logger.Error($"{nameof(T)} command property {property.Name} invalid type, require {property.PropertyType}");
                        }
                        
                        requiredProperties.RemoveWhere(x => x.Position == attribute.Position);
                    }
                }

                if (requiredProperties.Count > 0)
                {
                    string requiredPropertiesString = "";
                    foreach (TwitchCommandPropertyAttribute attribute in requiredProperties)
                        requiredPropertiesString += $"{attribute.Position}";

                    Logger.Error($"{nameof(T)} required properties position {requiredPropertiesString}");
                }

                templateType.GetMethod("Initialize", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Invoke(template, null);
                return template;
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return default;
            }
        }
    }
}