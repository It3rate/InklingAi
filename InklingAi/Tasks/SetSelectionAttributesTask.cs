namespace InklingAi.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using InklingAi.Views;
    using InkPad.Adapters;
    using InkPad.CommandEngine;

    public class SetSelectionAttributesTask : TaskBase
    {
        public BrushData NewBrushData;
        public BrushData? PreviousBrushData;
        public VectorImage Target { get; }
        public override bool IsValid => true;
        public SetSelectionAttributesTask(VectorImage target, BrushData newBrushData)
        {
            Target = target;
            NewBrushData = newBrushData;
        }
        public override void RunTask()
        {
            PreviousBrushData = Target.GetBrushDataClone();
            foreach (var item in Target.Selection)
            {
                item.SetWithBrushData(NewBrushData);
            }

            Target.InvalidateVisual();
        }

        public override void UnRunTask()
        {
            if (PreviousBrushData != null)
            {
                foreach (var item in Target.Selection)
                {
                    item.SetWithBrushData(PreviousBrushData);
                }
                Target.InvalidateVisual();
            }
        }
        public override bool MergeDataFrom(TaskBase newTask)
        {
            var result = false;
            return result;
        }
    }
}
