using InkPad.CommandEngine;
using Nums.Primitives;

namespace InkPad.Tasks.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class CreateDomainTask : TaskBase, ICreateTask
    {
        public Workspace TaskWorkspace;
        public Domain TaskDomain;

        public Trait Trait { get; }
        public IFocal BasisFocal { get; }
        public IFocal MinMax { get; }

        public override bool IsValid => true;

        public CreateDomainTask(Workspace workspace, Trait trait, IFocal basisFocal, IFocal minMax)
        {
            TaskWorkspace = workspace;
            Trait = trait;
            BasisFocal = basisFocal;
            MinMax = minMax;
        }
        public override void RunTask()
        {
            if (TaskDomain == null)
            {
                TaskDomain = new Domain(Trait, BasisFocal, MinMax);
            }
            else
            {
                Trait.DomainStore.Add(TaskDomain.Id, TaskDomain);
            }
            TaskWorkspace.AddDomains(false, TaskDomain);
        }

        public override void UnRunTask()
        {
            TaskWorkspace.RemoveDomains(false, TaskDomain);
            Trait.DomainStore.Remove(TaskDomain.Id);
        }
    }
}
