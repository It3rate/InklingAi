namespace Nums.CoreConcepts.CoreTraits;

using Nums.CoreConcepts;
using Nums.Primitives;

public class PositionTrait : Trait
{
    public static PositionTrait CreateIn(Knowledge knowledge) => (PositionTrait)knowledge.Brain.AddTrait(new PositionTrait());

    public PositionTrait() : base("Position") { }
}
