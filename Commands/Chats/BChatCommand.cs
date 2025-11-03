using CommandSystem;
using Exiled.API.Features;
using System;
using YongAnFrame.Extensions;
using YongAnFrame.Features.Players;
using YongAnFrame.Features.UI.Enums;
using YongAnFrame.Features.UI.Texts;

namespace SyncPlugin.Commands.Chats
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class BChatCommand : ICommand
    {
        public string Command => "bchat";

        public string[] Aliases => ["bc"];

        public string Description => "";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Array.Length == 2)
            {
                if (Player.TryGet(sender, out Player player))
                {
                    FramePlayer framePlayer = player.ToFPlayer();

                    //if (framePlayer.IsChatBan)
                    //{
                    //    response = "无法发送聊天，你已收到临时聊天禁令";
                    //    return false;
                    //}

                    foreach (var item in FramePlayer.List)
                    {
                        item.UI.ChatList.Add(new ChatText(arguments.Array[1], 10, ChatType.All, framePlayer));
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
