
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
		    Focal.CreateZeroFocal(60), // 60 ticks per second
		    Focal.CreateZeroFocal(long.MaxValue)) { } // no negative values
}
