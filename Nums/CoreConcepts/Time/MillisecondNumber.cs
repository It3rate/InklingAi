using Nums.Primitives;

namespace Nums.CoreConcepts.Time
{
	public class MillisecondNumber : Number
    {
        public override Domain Domain
        {
	        get => MillisecondTimeDomain.MinMax;
	        set { }
        }

        protected MillisecondNumber(IFocal focal) : base(focal) { }

        public static MillisecondNumber Create(long duration, bool addToStore = false) => Create(0, duration, addToStore);
	    
	    public static MillisecondNumber Create(long startTime, long duration, bool addToStore = false)
        {
	        var focal = new Focal(-startTime, startTime + duration);
	        var result = new MillisecondNumber(focal);
	        Knowledge.Instance.MillisecondTimeDomain.AddNumber(result, addToStore);
	        return result;
        }

        public static MillisecondNumber Zero(bool addToStore = false) => new MillisecondNumber(new Focal(0,0));
    }
}
