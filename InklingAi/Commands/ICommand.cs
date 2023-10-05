namespace InklingAi.Commands;

using InklingAi.Commands.Tasks;
using InklingAi.Elements;
using InklingAi.Pads;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

public interface ICommand
{
    Pad Pad { get; }
    PadKind PadKind { get; }
    ICommandStack Stack { get; set; }

    int CommandKey { get; }
    List<ITask> Tasks { get; }
    bool IsContinuous { get; }
    bool IsRetainedCommand { get; }

    IElement ElementAt(int key);

    DateTime StartTime { get; }
    DateTime EndTime { get; }
    TimeSpan Duration { get; }
    bool IsActive { get; }

    void Execute();
    void Update(SKPoint point);
    void Unexecute();
    void Completed();

    void AddTask(ITask task);
    void AddTasks(params ITask[] tasks);
    void AddTaskAndRun(ITask task);
    void AddTasksAndRun(params ITask[] tasks);
    //void RunToEnd();

    // event EventHandler OnExecute;
    // event EventHandler OnUpdate;
    // event EventHandler OnUnexecute;
    // event EventHandler OnCompleted;

    ICommand Duplicate();
    bool TryMergeWith(ICommand command);
    // temporal elements: start time, end time etc
}
