namespace InkPad.CommandEngine;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nums.CoreConcepts.Time;

public interface IStackCommand
{
    int Id { get; }
    ICommandStack Stack { get; set; }

    // todo: rather than time commands, all commands will be formulas that exit when their stop condition is met.
    MillisecondNumber LiveTimeSpan { get; set; }
    long DurationMS { get; }
    long DefaultDelay { get; set; }
    long DefaultDuration { get; set; }

    bool AppendElements();
    bool RemoveElements();
    bool IsMergableWith(IStackCommand command);
    bool TryMergeWith(IStackCommand command);

    int RepeatCount { get; } // Number - this will be Number once default traits are in.
    int RepeatIndex { get; } // Number - this will be Number once default traits are in.

    bool IsActive { get; }
    bool IsContinuous { get; }
    bool CanUndo { get; }
    bool IsRetainedCommand { get; }

    bool IsRepeatable();
    bool IsComplete();

    void Execute();
    void Update(MillisecondNumber currentTime, MillisecondNumber deltaTime);
    void Completed();
    bool Evaluate();
    void Unexecute();

    List<ITask> Tasks { get; }
    void AddTask(ITask task);
    void AddTasks(params ITask[] tasks);
    void AddTaskAndRun(ITask task);
    void AddTasksAndRun(params ITask[] tasks);
    //void RunToEnd();

    // event EventHandler OnExecute;
    // event EventHandler OnUpdate;
    // event EventHandler OnUnexecute;
    // event EventHandler OnCompleted;

    IStackCommand Duplicate();
}
