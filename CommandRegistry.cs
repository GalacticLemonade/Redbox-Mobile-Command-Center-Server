using Redbox_Mobile_Command_Center_Server.Commands;
using Redbox_Mobile_Command_Center_Server.KioskCommands;
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
                { "test-comm", new TestCommCommand() },
                { "ping-kiosk", new PingKioskCommand() },
                { "execute-on-kiosk", new ExecuteOnKioskCommand() }
            };
        }
    }
}
