using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redbox_Mobile_Command_Center_Server.KioskCommands {
    public class GetKioskIDCommand : ICommand {
        public async Task<string> Run(string[] arguments) {
            string halResponse = await HALConnection.SendHALCommandAsync("SERVICE get-kiosk-id");

            return await Task.FromResult(halResponse);
        }
    }
}
