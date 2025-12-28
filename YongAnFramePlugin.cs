using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Features;
using HarmonyLib;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.Reflection;
using YongAnFrame.Features;
using YongAnFrame.Features.Players;
using YongAnFrame.Features.Roles;

namespace YongAnFrame
{
    /// <summary>
    /// 插件的驱动
    /// </summary>
    public sealed class YongAnFramePlugin : Plugin<Config, Translation>
    {
        private static YongAnFramePlugin? instance;
        /// <summary>
        /// 获取<seealso cref="YongAnFramePlugin"/>单例
        /// </summary>
        public static YongAnFramePlugin Instance
        {
            get
            {
                if (instance is null)
                {
                    throw new InvalidCastException("YongAnFramePlugin实例无效");
                }
                return instance;
            }
        }

        public string ConnectionString => $"server={Config.MySqlServer}; User Id = {Config.MySqlUser} ;Password = {Config.MySqlPassword} ;Database = {Config.MySqlDatabaseName};Charset = utf8";


        /// <summary>
        /// 获取<seealso cref="HarmonyLib.Harmony"/>实例
        /// </summary>
        public Harmony Harmony { get; } = new Harmony("YongAnFrame.Harmony");

        ///<inheritdoc/>
        public override PluginPriority Priority => PluginPriority.Higher;

        ///<inheritdoc/>
        public override void OnEnabled()
        {
            instance = this;
            Log.Info("\r\n __  __     ______     __   __     ______     ______     __   __    \r\n/\\ \\_\\ \\   /\\  __ \\   /\\ \"-.\\ \\   /\\  ___\\   /\\  __ \\   /\\ \"-.\\ \\   \r\n\\ \\____ \\  \\ \\ \\/\\ \\  \\ \\ \\-.  \\  \\ \\ \\__ \\  \\ \\  __ \\  \\ \\ \\-.  \\  \r\n \\/\\_____\\  \\ \\_____\\  \\ \\_\\\\\"\\_\\  \\ \\_____\\  \\ \\_\\ \\_\\  \\ \\_\\\\\"\\_\\ \r\n  \\/_____/   \\/_____/   \\/_/ \\/_/   \\/_____/   \\/_/\\/_/   \\/_/ \\/_/ \r\n                                                                    \r\n ______   ______     ______     __    __     ______                 \r\n/\\  ___\\ /\\  == \\   /\\  __ \\   /\\ \"-./  \\   /\\  ___\\                \r\n\\ \\  __\\ \\ \\  __<   \\ \\  __ \\  \\ \\ \\-./\\ \\  \\ \\  __\\                \r\n \\ \\_\\    \\ \\_\\ \\_\\  \\ \\_\\ \\_\\  \\ \\_\\ \\ \\_\\  \\ \\_____\\              \r\n  \\/_/     \\/_/ /_/   \\/_/\\/_/   \\/_/  \\/_/   \\/_____/              \r\n                                                                    \r\n ");
            PathManager.CheckPath();
            FramePlayer.SubscribeStaticEvents();
            CustomRolePlus.SubscribeStaticEvents();
            Harmony.PatchAll();
            base.OnEnabled();
        }

        ///<inheritdoc/>
        public override void OnDisabled()
        {
            instance = null;
            FramePlayer.UnsubscribeStaticEvents();
            CustomRolePlus.UnsubscribeStaticEvents();
            Harmony.UnpatchAll();
            base.OnDisabled();
        }

        ///<inheritdoc/>
        public override void OnRegisteringCommands()
        {
            Dictionary<Type, List<ICommand>> dictionary = [];
            Type[] types = Assembly.GetTypes();
            foreach (Type type in types)
            {
                if (type.GetInterface("ICommand") != typeof(ICommand) || Config.DisableCommand.Contains(type.Name) || !Attribute.IsDefined(type, typeof(CommandHandlerAttribute)))
                {
                    continue;
                }

                foreach (CustomAttributeData customAttributesDatum in type.GetCustomAttributesData())
                {
                    try
                    {
                        if (customAttributesDatum.AttributeType != typeof(CommandHandlerAttribute))
                        {
                            continue;
                        }

                        Type type2 = (Type)customAttributesDatum.ConstructorArguments[0].Value;
                        ICommand command = GetCommand(type) ?? ((ICommand)Activator.CreateInstance(type));
                        if (typeof(ParentCommand).IsAssignableFrom(type2))
                        {
                            if (GetCommand(type2) is not ParentCommand parentCommand)
                            {
                                if (!dictionary.TryGetValue(type2, out var value))
                                {
                                    dictionary.Add(type2, [command]);
                                }
                                else
                                {
                                    value.Add(command);
                                }
                            }
                            else
                            {
                                parentCommand.RegisterCommand(command);
                            }

                            continue;
                        }

                        try
                        {
                            if (type2 == typeof(RemoteAdminCommandHandler))
                            {
                                CommandProcessor.RemoteAdminCommandHandler.RegisterCommand(command);
                            }
                            else if (type2 == typeof(GameConsoleCommandHandler))
                            {
                                GameCore.Console.ConsoleCommandHandler.RegisterCommand(command);
                            }
                            else if (type2 == typeof(ClientCommandHandler))
                            {
                                QueryProcessor.DotCommandHandler.RegisterCommand(command);
                            }
                        }
                        catch (ArgumentException ex)
                        {
                            if (ex.Message.StartsWith("An"))
                            {
                                Log.Error("Command with same name has already registered! Command: " + command.Command);
                            }
                            else
                            {
                                Log.Error($"An error has occurred while registering a command: {ex}");
                            }
                        }

                        Commands[type2][type] = command;
                    }
                    catch (Exception arg)
                    {
                        Log.Error($"An error has occurred while registering a command: {arg}");
                    }
                }
            }

            foreach (KeyValuePair<Type, List<ICommand>> item in dictionary)
            {
                ParentCommand parentCommand2 = GetCommand(item.Key) as ParentCommand;
                foreach (ICommand item2 in item.Value)
                {
                    parentCommand2.RegisterCommand(item2);
                }
            }
        }
    }
}
