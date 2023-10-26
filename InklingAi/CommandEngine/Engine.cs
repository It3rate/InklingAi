namespace InklingAi.CommandEngine;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InklingAi.ViewModels;
using InkPad.CommandEngine;

public class Engine
{
    public static Engine Instance { get; } = new();

    public CommandStack Stack { get; } = new();
    public Runner Runner { get; } = new();

    public KeyboardData KeyboardData => KeyboardData.Instance;

    private Engine(){}


}
