namespace InklingAi.Commands.EditCommands
{
    using InklingAi.Elements;
    using InklingAi.Pads;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public interface IDraggableCommand : ICommand
    {
        IPoint DraggablePoint { get; }
        bool HasDraggablePoint { get; }
    }
    public class EditCommand : CommandBase
    {
        new CommandStack<EditCommand> Stack { get; set; }
        public EditCommand(Pad pad) : base(pad) { }
    }
}
