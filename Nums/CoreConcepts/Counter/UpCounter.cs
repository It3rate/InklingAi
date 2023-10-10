using Nums.Primitives;

namespace Nums.CoreConcepts.Counter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class UpCounter : Number
    {
	    public UpCounter() : base(CounterDomain.UpDomain, new Focal(0, 0))
	    {
	    }

	    public long AddOne()
	    {
		    Focal.EndTickPosition += 1;
		    return Focal.EndTickPosition;
	    }

	    public void Reset()
	    {
		    Focal.EndTickPosition = 0;
	    }
    }
}
