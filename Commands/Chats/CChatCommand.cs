using CommandSystem;
using Exiled.API.Features;
using System;
using YongAnFrame.Extensions;
using YongAnFrame.Features.UI.Enums;
using YongAnFrame.Features.UI.Texts;

namespace SyncPlugin.Commands.Chats
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class CChatCommand : ICommand
    {
        public string Command => "cchat";

        public string[] Aliases => ["cc"];

        public string Description => "";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Array.Length == 2)
            {
                if (Player.TryGet(sender, out Player player))
                {
                    foreach (var player1 in Player.Get(player.Role.Side))
                    {
                        player1.ToFPlayer().UI.ChatList.Add(new ChatText(arguments.Array[1], 10, ChatType.All, player.ToFPlayer()));
                    }
                    response = "OK";
                    return true;
                }
            }
            response = "NO";
            return false;
        }
    }
}
