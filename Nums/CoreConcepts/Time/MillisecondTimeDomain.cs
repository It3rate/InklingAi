using Nums.Primitives;

namespace Nums.CoreConcepts.Time
{
	public class MillisecondTimeDomain : Domain
    {
	    private MillisecondTimeDomain(Trait trait, IFocal basis, IFocal minMax) : base(trait, "MillisecondTimeDomain",  basis, minMax)
        {
        }
	    public static MillisecondTimeDomain MinMax { get; } = new MillisecondTimeDomain(new TimeTrait(), new Focal(0, 1000), Focal.MinMaxFocal);
    }
}
