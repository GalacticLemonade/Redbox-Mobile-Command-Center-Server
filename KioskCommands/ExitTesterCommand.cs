using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redbox_Mobile_Command_Center_Server.KioskCommands {
    public class ExitTesterCommand : ICommand {
        public async Task<string> Run(string[] arguments) {
            string halResponse = await HALConnection.SendHALCommandAsync("SERVICE diagnostic-mode status: false");
            halResponse = await Task.FromResult(halResponse);

            List<string> responseLines = Program.SplitByCRLF(halResponse);

            if (responseLines.Count > 0 && responseLines[0].StartsWith("203")) {
                Console.WriteLine("Response code 203 received.");
            }
            else {
                Console.WriteLine("Invalid response.");
                return "402";
            }

            halResponse = await HALConnection.SendHALCommandAsync("JOB execute-immediate-base64 statement: 'IEFJUlhDSEdSIEZBTk9ODQogVkVORERPT1IgQ0xPU0UNCiBHUklQUEVSIFJFTlQNCiBHUklQUEVSIFJFVFJBQ1QNCiBTRU5TT1IgUElDS0VSLU9GRg0KIFJPTExFUiBTVE9QDQogUklOR0xJR0hUIE9GRg0KIENMRUFSDQo='");
            halResponse = await Task.FromResult(halResponse);

            responseLines = Program.SplitByCRLF(halResponse);

            if (responseLines.Count > 1 && responseLines[1].StartsWith("203")) {
                Console.WriteLine("Response code 203 received.");
            }
            else {
                Console.WriteLine("Invalid response.");
                return "402";
            }

            return "200";
        }
    }
}
