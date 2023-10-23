
namespace InkPad.CommandEngine.BrainEngine;

using InkPad.Agents;
using InkPad.CommandEngine;
using InkPad.Elements;
using Nums.CoreConcepts.Time;
using Nums.Primitives;

// selection range - segment, first, all, not, direction
// motion range
// motion type (ideally as a range) prepend, postpend, scale, or, and, not...
// repeat range
// duration range
// termination range & bool
//WorkspaceKind WorkspaceKind { get; }
// Commands happen over time, can be repeatable, appendable, and can be evaluated to check for termination.
// Commands are more or less formulas.
public interface IBrainCommand : ICommand
{
    IAgent Agent { get; set; }
    Brain Brain { get; }
    Workspace Workspace { get; }
}
public interface IDraggableCommand : IBrainCommand
{
    IPoint DraggablePoint { get; }
    bool HasDraggablePoint { get; }
}