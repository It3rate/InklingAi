using Nums.Primitives;

namespace Nums.CoreConcepts.Time
{
	public class TimeTrait : Trait
    {
	    public override string Name => "Time";

	    public static TimeTrait CreateIn(Knowledge knowledge) => (TimeTrait)knowledge.Brain.AddTrait(new TimeTrait());
    }
}
