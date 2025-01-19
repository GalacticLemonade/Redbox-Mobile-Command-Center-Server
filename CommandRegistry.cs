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
            //explicitly initialize the dictionary
            Commands = new Dictionary<string, ICommand>();

            //register commands
            Commands.Add("ping-kiosk", new PingKioskCommand());
        }
    }
}
