namespace InklingAi.Commands.Tasks;

using InklingAi.Pads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface ITask
{
    ICommand Command { get; }
    Pad Pad { get; }
    PadKind PadKind { get; }
    int TaskKey { get; }
    bool IsValid { get; }
    void RunTask();
    void UnRunTask();
    //void OnAddedToCommand(ICommand command);
}
