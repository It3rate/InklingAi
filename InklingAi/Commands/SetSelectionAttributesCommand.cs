namespace InklingAi.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using InklingAi.Tasks;
    using InklingAi.Views;
    using InkPad.Adapters;
    using InkPad.CommandEngine;
    using Nums.CoreConcepts.Time;

    public class SetSelectionAttributesCommand : CommandBase
    {
        public SetSelectionAttributesTask? SetSelectionAttributesTask;
        private VectorImage _target;
        private BrushData _newBrushData;

        public SetSelectionAttributesCommand(VectorImage target, BrushData newBrushData)
        {
            _target = target;
            _newBrushData = newBrushData;
        }

        public override void Execute()
        {
            base.Execute();
            if (SetSelectionAttributesTask == null)
            {
                SetSelectionAttributesTask = new SetSelectionAttributesTask(_target, _newBrushData);
            }
            AddTaskAndRun(SetSelectionAttributesTask);
        }

        public override void Unexecute()
        {
            base.Unexecute();
        }

        public override void Update(MillisecondNumber currentTime, MillisecondNumber deltaTime)
        {
        }

        public override void Completed()
        {
        }
    }
}
