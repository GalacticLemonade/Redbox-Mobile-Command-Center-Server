using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redbox_Mobile_Command_Center_Server {
    public interface ICommand {
        Task<string> Run(string[] arguments);
    }
}
