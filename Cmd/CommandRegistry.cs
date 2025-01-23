using Redbox_Mobile_Command_Center_Server.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redbox_Mobile_Command_Center_Server {
    public static class CommandRegistry {
        public static readonly Dictionary<string, ICommand> Commands;

        static CommandRegistry() {
            Commands = new Dictionary<string, ICommand>
            {
                { "ping-kiosk", new PingKioskCommand() },
                { "execute-command", new ExecuteKioskCommand() },
            };
        }
    }
}
