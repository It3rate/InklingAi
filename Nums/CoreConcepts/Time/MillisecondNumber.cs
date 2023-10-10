using Nums.Primitives;

namespace Nums.CoreConcepts.Time
{
	public class MillisecondNumber : Number
    {
        public MillisecondNumber(IFocal focal) : base(MillisecondTimeDomain.Instance, focal) { }
        public MillisecondNumber(long durationTicks) : this(0, durationTicks) { }
        public MillisecondNumber(long startTick, long durationTicks) : this(new Focal(startTick, startTick + durationTicks)) { }

        public double DelaySeconds => Value.Start / 1000;
        public long DelayMS => (long)Value.Start;
        public double DurationSeconds => Value.DirectedLength / 1000;
        public long DurationMS => (long)Value.DirectedLength;
        public long TickCount => (long)Focal.Length;


        public static MillisecondNumber Zero = new MillisecondNumber(new Focal(0,0));
    }
}
