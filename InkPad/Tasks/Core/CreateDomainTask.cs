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
        public Domain Domain;

        public Trait Trait { get; }
        public IFocal BasisFocal { get; }
        public IFocal MinMax { get; }

        public override bool IsValid => true;

        public CreateDomainTask(Trait trait, IFocal basisFocal, IFocal minMax)
        {
            Trait = trait;
            BasisFocal = basisFocal;
            MinMax = minMax;
        }
        public override void RunTask()
        {
            if (Domain == null)
            {
                Domain = new Domain(Trait, BasisFocal, MinMax);
            }
            else
            {
                Trait.DomainStore.Add(Domain.Id, Domain);
            }
            TaskAgent.Workspace.AddDomains(false, Domain);
        }

        public override void UnRunTask()
        {
            TaskAgent.Workspace.RemoveDomains(false, Domain);
            Trait.DomainStore.Remove(Domain.Id);
        }
    }
}
