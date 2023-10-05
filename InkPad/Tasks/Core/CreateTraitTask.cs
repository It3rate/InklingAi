using InkPad.CommandEngine;
using Nums.Primitives;

namespace InkPad.Tasks.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class CreateTraitTask : TaskBase, ICreateTask
    {
        public Trait Trait;
        public string Name;
        public override bool IsValid => true;
        public CreateTraitTask(string name = "")
        {
            Name = name;
        }
        public override void RunTask()
        {
            if (Trait == null)
            {
                Trait = Trait.CreateIn(TaskAgent.Brain, Name);
            }
            else
            {
                TaskAgent.Brain.AddTrait(Trait);
            }
        }

        public override void UnRunTask()
        {
            TaskAgent.Brain.RemoveTrait(Trait);
        }
    }
}
