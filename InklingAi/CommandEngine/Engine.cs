namespace InklingAi.CommandEngine;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InkPad.CommandEngine;

public class Engine
{
    public static Engine Instance { get; } = new();

    public CommandStack Stack { get; } = new();
    Runner Runner { get; } = new();

    private Engine(){}


}
