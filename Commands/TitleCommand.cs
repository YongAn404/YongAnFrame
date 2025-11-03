using CommandSystem;
using Exiled.Permissions.Extensions;
using System;
using YongAnFrame.Features.Players;

namespace SyncPlugin.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class TitleCommand : ICommand
    {
        public string Command => "title";

        public string[] Aliases => ["tit", "ti"];

        public string Description => "title指令";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            PlayerTitle? titleData;
            switch (arguments.Array[1])
            {
                case "new":
                    if (!sender.CheckPermission("yongan404.title.new"))
                    {
                        response = "请保证你有yongan404.title.new权限";
                        return false;
                    }

                    if (arguments.Array[2] == "dynamic")
                    {
                        titleData = new(0, "", "", bool.Parse(arguments.Array[3]), arguments.Array[4]);
                        titleData.Insert(arguments.Array[4]);
                    }
                    else
                    {
                        titleData = new(0, arguments.Array[2], arguments.Array[3], bool.Parse(arguments.Array[4]));
                        titleData.Insert(null);
                    }
                    response = "OK";
                    return true;
                case "up":
                    if (!sender.CheckPermission("yongan404.title.update"))
                    {
                        response = "请保证你有yongan404.title.update权限";
                        return false;
                    }

                    titleData = PlayerTitle.Get(uint.Parse(arguments.Array[2]));
                    if (titleData is null)
                    {
                        response = "无效的称号ID";
                        return false;
                    }

                    switch (arguments.Array[3])
                    {
                        case "name":
                            titleData.Name = arguments.Array[4];
                            break;
                        case "color":
                            titleData.Color = arguments.Array[4];
                            break;
                        case "pro":
                            titleData.IsRank = bool.Parse(arguments.Array[4]);
                            break;
                        case "dc":
                            titleData.SetDynamicCommand(arguments.Array[4]);
                            break;
                    }
                    titleData.Update();
                    response = "OK";
                    return true;
                case "set":
                    if (!sender.CheckPermission("yongan404.title.set"))
                    {
                        response = "请保证你有yongan404.title.set权限";
                        return false;
                    }

                    FramePlayer? framePlayer = FramePlayer.Get(int.Parse(arguments.Array[2]));
                    titleData = PlayerTitle.Get(uint.Parse(arguments.Array[3]));

                    if (framePlayer is null)
                    {
                        response = "无效的玩家ID";
                        return false;
                    }
                    if (titleData is null)
                    {
                        response = "无效的称号ID";
                        return false;
                    }

                    if (titleData.IsRank)
                    {
                        framePlayer.UsingRankTitles = titleData;
                    }
                    else
                    {
                        framePlayer.UsingTitles = titleData;
                    }
                    framePlayer.UpdateShowInfo();
                    response = "OK";
                    return true;
            }
            response = "???";
            return false;
        }
    }
}
