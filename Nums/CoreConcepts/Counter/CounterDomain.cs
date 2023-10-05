using Nums.Primitives;

namespace Nums.CoreConcepts.Counter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class CounterDomain : Domain
    {
	    private CounterDomain(IFocal basisFocal, IFocal maxFocal) : base(new CounterTrait(), basisFocal, maxFocal) { }

	    public static CounterDomain UpDomain { get; } = new CounterDomain(Focal.CreateZeroFocal(1), Focal.CreateByValues(0, long.MaxValue));

        public static CounterDomain UpDownDomain { get; } = new CounterDomain(Focal.CreateZeroFocal(1), Focal.MinMaxFocal);
    }
}
