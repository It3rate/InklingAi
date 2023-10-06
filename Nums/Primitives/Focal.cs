using Nums.Utils;

namespace Nums.Primitives
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class Focal : FocalBase
    {
        public static Focal Empty { get; } = new Focal(0, 0);

        public Focal(long startTickPosition, long endTickPosition)
        {
	        StartTickPosition = startTickPosition;
	        EndTickPosition = endTickPosition;
        }
	    public static Focal CreateByValues(long startPosition, long endPosition)
	    {
		    var result = new Focal(startPosition, endPosition);
		    return result;
	    }

	    public override IFocal Clone()
	    {
		    return CreateByValues(StartTickPosition, EndTickPosition);
	    }

	    public static IFocal CreateZeroFocal(long ticks) { return Focal.CreateByValues(0, ticks); }
        public static IFocal CreateBalancedFocal(long halfTicks) { return Focal.CreateByValues(-halfTicks, halfTicks); }
        private static IFocal _minMaxFocal = Focal.CreateByValues(long.MinValue, long.MaxValue);
        public static IFocal MinMaxFocal => _minMaxFocal;
        private static IFocal _upMaxFocal = Focal.CreateByValues(0, long.MaxValue);
        public static IFocal UpMaxFocal => _upMaxFocal;

    }
 }
