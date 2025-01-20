using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redbox_Mobile_Command_Center_Server.KioskCommands {
    public class TestCommCommand : ICommand {
        public async Task<string> Run(string[] arguments) {
            string halResponse = await HALConnection.SendHALCommandAsync("SERVICE test-comm");

            string halResponse2 = await Task.FromResult(halResponse);

            List<string> responseLines = Program.SplitByCRLF(halResponse2);

            if (responseLines.Count > 1 && responseLines[1].StartsWith("203")) {
                Console.WriteLine("Response code 203 received.");
            }
            else {
                Console.WriteLine("Invalid response.");
                return "402";
            }

            return responseLines[0];
        }
    }
}
