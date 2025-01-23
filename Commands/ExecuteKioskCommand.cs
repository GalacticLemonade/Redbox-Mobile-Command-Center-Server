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
                case "exit-tester":
                    Console.WriteLine("Cleanup hardware");
                    Program.HardwareService.ExecuteServiceCommand("SERVICE diagnostic-mode status: false");
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine(" AIRXCHGR FANON");
                    stringBuilder.AppendLine(" VENDDOOR CLOSE");
                    stringBuilder.AppendLine(" GRIPPER RENT");
                    stringBuilder.AppendLine(" GRIPPER RETRACT");
                    stringBuilder.AppendLine(" SENSOR PICKER-OFF");
                    stringBuilder.AppendLine(" ROLLER STOP");
                    stringBuilder.AppendLine(" RINGLIGHT OFF");
                    stringBuilder.AppendLine(" CLEAR");
                    Program.HardwareService.ExecuteImmediateProgram(Encoding.ASCII.GetBytes(stringBuilder.ToString()), out HardwareJob _);
                    break;
                   
            }

            return await Task.FromResult("500");
        }
    }
}
