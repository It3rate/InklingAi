namespace InklingAi.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using InklingAi.ViewModels;
    using InklingAi.Views;
    using InkPad.Adapters;
    using InkPad.CommandEngine;

    public class SetSelectionAttributesTask : TaskBase
    {
        public BrushData NewBrushData;
        public BrushData? PreviousBrushData;
        public PaintableImage Target { get; }
        public override bool IsValid => true;
        public SetSelectionAttributesTask(PaintableImage target, BrushData newBrushData)
        {
            Target = target;
            NewBrushData = newBrushData;
        }
        public override void RunTask()
        {
            PreviousBrushData = Target.GetBrushDataClone();
            Target.SetWithBrushData(NewBrushData);
        }

        public override void UnRunTask()
        {
            if (PreviousBrushData != null)
            {
                Target.SetWithBrushData(PreviousBrushData);
            }
        }
        public override bool MergeDataFrom(TaskBase newTask)
        {
            var result = false;
            return result;
        }
    }
}
