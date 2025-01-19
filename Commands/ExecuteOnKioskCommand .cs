using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redbox_Mobile_Command_Center_Server.Commands {
    public class ExecuteOnKioskCommand : ICommand {
        public async Task<string> Run(string[] arguments) {
            if (arguments.Length == 0) {
                return "402\r\nError: No command specified for kiosk execution.";
            }

            string innerCommandName = arguments[0];
            string[] innerArguments = arguments.Skip(1).ToArray();

            if (CommandRegistry.Commands.TryGetValue(innerCommandName, out ICommand innerCommand)) {
                Console.WriteLine($"Executing command '{innerCommandName}' on kiosk with arguments: {string.Join(" ", innerArguments)}");
                return await innerCommand.Run(innerArguments);
            }
            else {
                return $"400\r\nError: Unknown command '{innerCommandName}' for kiosk execution.";
            }
        }
    }
}
