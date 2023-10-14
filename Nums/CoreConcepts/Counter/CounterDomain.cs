
namespace Nums.CoreConcepts.Counter;

using Nums.Primitives;
using Nums.CoreConcepts.CoreTraits;

public class CounterDomain : Domain
{
    private CounterDomain(string name, IFocal basisFocal, IFocal maxFocal) : base(new CounterTrait(), name, basisFocal, maxFocal) { }

    public static CounterDomain UpDomain { get; } = new CounterDomain("CounterUpDomain", new Focal(0, 1), new Focal(0, long.MaxValue));

    public static CounterDomain UpDownDomain { get; } = new CounterDomain("CounterUpDownDomain", new Focal(0, 1), Focal.MinMaxFocal);
}
