using Nums.Primitives;

namespace InkPad.CommandEngine
{
    using InkPad.Agents;
	using InkPad.Commands;
	using InkPad.Elements;
    using InkPad.Pads;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public interface ITask
    {
	    int Id { get; }
	    IStackCommand? Command { get; }
        TaskTimer Timer { get; }
        bool IsValid { get; }
	    void RunTask();
	    void UnRunTask();

        //void OnAddedToCommand(IStackCommand command);
    }

    public interface ICreateTask : ITask
    {
    }
    public interface IChangeTask : ITask
    {
    }
    public interface IPointTask : ITask
    {
        int PointKey { get; }
        IPoint IPoint { get; }
    }


}
