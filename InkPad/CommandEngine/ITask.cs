using InkPad.Commands;
using Nums.Primitives;

namespace InkPad.CommandEngine
{
    using InkPad.Agents;
    using InkPad.Pads;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public interface ITask
    {
	    int Id { get; }
	    ICommand Command { get; }
	    IAgent TaskAgent { get; set; }
        TaskTimer Timer { get; }
        bool IsValid { get; }
	    void RunTask();
	    void UnRunTask();
        //Pad Pad { get; }
        //PadKind PadKind { get; }

        //void OnAddedToCommand(ICommand command);
    }

    public interface ICreateTask
    {
    }
}
