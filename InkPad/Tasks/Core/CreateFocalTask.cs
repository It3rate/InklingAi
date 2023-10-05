using InkPad.CommandEngine;
using Nums.Primitives;

namespace InkPad.Tasks.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Nums.RefPrimitives;

    public class CreateFocalTask : TaskBase, ICreateTask
    {
        public IFocal CreatedFocal;

        private Trait Trait { get; }
        public long StartPosition { get; }
        public long EndPosition { get; }

        public override bool IsValid => true;

        public CreateFocalTask(long startPosition, long endPosition)
        {
            StartPosition = startPosition;
            EndPosition = endPosition;
        }
        public CreateFocalTask(Trait trait, long startPosition, long endPosition)
        {
            Trait = trait;
            StartPosition = startPosition;
            EndPosition = endPosition;
        }
        public override void RunTask()
        {
            if (CreatedFocal == null)
            {
                if (Trait == null)
                {
                    CreatedFocal = Focal.CreateByValues(StartPosition, EndPosition);
                }
                else
                {
                    CreatedFocal = FocalRef.CreateByValues(Trait, StartPosition, EndPosition);
                }
            }

            Trait?.FocalStore.Add(CreatedFocal.Id, CreatedFocal);
        }

        public override void UnRunTask()
        {
            Trait?.FocalStore.Remove(CreatedFocal.Id);
        }
    }
}
