namespace Nums.CoreConcepts.CoreTraits;

using Nums.CoreConcepts;
using Nums.Primitives;

public class CounterTrait : Trait
{
    public static CounterTrait CreateIn(Knowledge knowledge) => (CounterTrait)knowledge.Brain.AddTrait(new CounterTrait());

    public CounterTrait() : base("Counter") { }
}
