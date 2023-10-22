using Nums.CoreConcepts.CoreTraits;
using Nums.Primitives;

namespace Nums.CoreConcepts.Time
{
    public class MillisecondTimeDomain : Domain
    {
	    public static MillisecondTimeDomain Instance { get; } = new MillisecondTimeDomain();

        private MillisecondTimeDomain() : base(TimeTrait.Instance, "MillisecondTimeDomain", new Focal(0, 1000), Focal.MinMaxFocal)
        {
        }
    }
}
