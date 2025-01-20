using Redbox_Mobile_Command_Center_Server.KioskCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redbox_Mobile_Command_Center_Server.Commands {
    public class ExecuteOnKioskCommand : ICommand {
        // Internal registry for kiosk-specific commands
        private static readonly Dictionary<string, ICommand> KioskCommands = new Dictionary<string, ICommand> {
            { "test-comm", new TestCommCommand() },
            { "get-kiosk-id", new GetKioskIDCommand() },
            { "tester-startup", new TesterStartupCommand() }
        };

        public async Task<string> Run(string[] arguments) {
            if (arguments.Length == 0) {
                return "Error: No command specified for kiosk execution.";
            }

            string innerCommandName = arguments[0];
            string[] innerArguments = arguments.Skip(1).ToArray();

            if (KioskCommands.TryGetValue(innerCommandName, out ICommand innerCommand)) {
                Console.WriteLine($"Executing kiosk command '{innerCommandName}' with arguments: {string.Join(" ", innerArguments)}");
                return await innerCommand.Run(innerArguments);
            }
            else {
                return $"Error: Unknown kiosk command '{innerCommandName}'.";
            }
        }
    }
}
