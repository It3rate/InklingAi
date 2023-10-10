using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Nums.Utils;

namespace Nums.Primitives
{
    /// <summary>
    /// Traits are measurable properties on objects. They are conceptual measurable categories (like distance, time, temperature) 
    /// without containing units or measurements.
    /// They can be composed of multiple traits (like mph or dollars/day or lwh) but do not need to be. 
    /// Terminal traits are traits that can't be broken into subtraits.
    /// </summary>
    public class Trait : IMathElement
	{
        public MathElementKind Kind => MathElementKind.Trait;

        public string Name { get; } = "";

        public readonly Dictionary<string, Domain> DomainStore = new Dictionary<string, Domain>();

        public Trait(string name)
        {
	        Name = name;
        }

        public static Trait CreateIn(Brain brain, string name) => brain.AddTrait(new Trait(name));


	    public Domain AddDomain(string name, IFocal basis, IFocal minMax)
	    {
		    return new Domain(this, name, basis, minMax);
	    }
	    public Domain AddDomain(string name, long basisTicks)
	    {
		    return AddDomain(name, new Focal(0, basisTicks), Focal.MinMaxFocal);
	    }

        public IEnumerable<Domain> Domains()
	    {
		    foreach (var domain in DomainStore.Values)
		    {
			    yield return domain;
		    }
	    }

        public Domain? DomainByName(string name)
	    {
		    DomainStore.TryGetValue(name, out var result);
		    return result;
        }

        public override bool Equals(object? obj) => obj is Trait other && Equals(other);
        public bool Equals(IMathElement? other) => other is Trait && Equals(other);
        public bool Equals(Trait? value) => Name.Equals(value?.Name);
        public override int GetHashCode() => Name.GetHashCode();
    }
}
