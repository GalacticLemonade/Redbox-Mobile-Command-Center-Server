using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redbox_Mobile_Command_Center_Server.Commands {
    public class PingKioskCommand : ICommand {
        public async Task<string> Run(string[] arguments) {
            return await Task.FromResult("200");
        }
    }
}
