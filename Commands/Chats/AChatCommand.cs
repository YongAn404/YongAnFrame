using CommandSystem;
using Exiled.API.Features;
using System;
using System.Linq;
using YongAnFrame.Features.Players;
using YongAnFrame.Features.UI.Enums;
using YongAnFrame.Features.UI.Texts;

namespace SyncPlugin.Commands.Chats
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class AChatCommand : ICommand
    {
        public string Command => "achat";

        public string[] Aliases => ["ac"];

        public string Description => "";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Array.Length >= 2)
            {
                if (Player.TryGet(sender, out Player player))
                {
                    foreach (var player1 in FramePlayer.List.Where((p) => { return p.ExPlayer.RemoteAdminAccess; }))
                    {
                        player1.UI.MessageList.Add(new MessageText(arguments.Array[1], 30, MessageType.Feedback));
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
