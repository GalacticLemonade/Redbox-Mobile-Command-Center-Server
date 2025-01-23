using System;
using Redbox.HAL.MSHALTester;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Redbox.HAL.Client;

namespace Redbox_Mobile_Command_Center_Server.Commands {
    public class ExecuteKioskCommand : ICommand {
        public async Task<string> Run(string[] arguments) {
            // TODO: implement custom commandregistry for this one too, just like how it was before

            switch (arguments[0]) {
                case "move-to-slot":

                    int slot = Int32.Parse(arguments[1]);
                    int deck = Int32.Parse(arguments[2]);

                    using (TesterMoveToSlotExecutor moveToSlotExecutor = new TesterMoveToSlotExecutor(Program.HardwareService, deck, slot)) {
                        moveToSlotExecutor.Run();
                        return moveToSlotExecutor.Results[0].Message;
                    }
                   
            }

            return await Task.FromResult("500");
        }
    }
}
