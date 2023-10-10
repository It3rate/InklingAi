using Nums.Primitives;

namespace Nums.CoreConcepts.Time
{
	public class MillisecondNumber : Number
    {
        public MillisecondNumber(IFocal focal) : base(MillisecondTimeDomain.MinMax, focal) { }
        public MillisecondNumber(long duration) : this(0, duration) { }
        public MillisecondNumber(long startTime, long duration) : this(new Focal(startTime, startTime + duration)) { }


        public static MillisecondNumber Zero = new MillisecondNumber(new Focal(0,0));
    }
}
