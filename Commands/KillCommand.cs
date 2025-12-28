using CommandSystem;
using Exiled.API.Features;
using PlayerStatsSystem;
using System;

namespace SyncPlugin.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class KillCommand : ICommand
    {
        public string Command => "killme";

        public string[] Aliases => ["kill", "s"];

        public string Description => "自杀指令";
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (Player.TryGet(sender, out Player player))
            {
                player.Kill(new CustomReasonDamageHandler("都死了啦，都你害啦，死亡原因自杀"));
                response = "Ok";
                return true;
            }
            response = "No";

            return false;
        }
    }
}
