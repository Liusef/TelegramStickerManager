using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReunionApp.Runners;

public abstract class CommandRunner
{
    public List<CommandOutput> Outputs { get; set; }

    public abstract Task RunCommandsAsync();
}
