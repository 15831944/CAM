﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAM
{
    public interface IHasProcessCommands
    {
        List<ProcessCommand> ProcessCommands { get; set; }
    }
}
