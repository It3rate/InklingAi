namespace InkPad.CommandEngine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using InkPad.Agents;
    using Nums.CoreConcepts.Time;
    using Nums.Primitives;

    public interface ICommandStack
    {
        bool CanUndo { get; }
        int UndoSize { get; }
        bool CanRedo { get; }
        int RedoSize { get; }
        bool CanRepeat { get; }

        void Do(ICommand command);
        ICommand? PreviousCommand();
        void Update(MillisecondNumber currentTime, MillisecondNumber deltaTime);

        bool Undo();
        void UndoAll();
        void UndoToIndex(int index);
        bool Redo();
        void RedoAll();
        void RedoToIndex(int index);
        void Clear();
    }
}
