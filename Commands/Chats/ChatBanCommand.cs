using CommandSystem;
using Exiled.Permissions.Extensions;
using System;
using YongAnFrame.Features.Players;

namespace SyncPlugin.Commands.Chats
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class ChatBanCommand : ICommand
    {
        public string Command => "chat_ban";

        public string[] Aliases => ["cb"];

        public string Description => "用于Ban Chat";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = "NO";
            if (sender.CheckPermission("yongan404.chat.ban"))
            {
                if (arguments.Array.Length > 1)
                {
                    FramePlayer? framePlayer = FramePlayer.Get(int.Parse(arguments.Array[1]));
                    //framePlayer.IsChatBan = true;
                    response = "OK";
                    return true;
                }
            }
            else
            {
                response = "请保证你有yongan404.chat.ban权限";
            }

            return false;
        }
    }
}
