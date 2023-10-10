
namespace Nums.CoreConcepts.Time;

using Nums.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Fps60TimeDomain : Domain
{
	    public Fps60TimeDomain(Knowledge knowledge) : base(
		    knowledge.TimeTrait, 
		    new Focal(0, 60), // 60 ticks per second
		    new Focal(0, long.MaxValue)) { } // no negative values
}
