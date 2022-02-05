using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReunionApp.Controllers;

public abstract class CommandController
{
    public List<CommandOutput> Outputs { get; set; }

    public abstract Task RunCommandsAsync();
}
