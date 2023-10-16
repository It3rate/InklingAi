namespace Nums.CoreConcepts.CoreTraits;

using Nums.CoreConcepts;
using Nums.Primitives;

public class PositionTrait : Trait
{
    public static PositionTrait Instance { get; } = new PositionTrait();

	public static PositionTrait CreateIn(Knowledge knowledge) => (PositionTrait)knowledge.Brain.AddTrait(new PositionTrait());

    private PositionTrait() : base("Position") { }
}
