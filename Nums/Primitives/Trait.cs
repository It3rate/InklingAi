using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Nums.Utils;

namespace Nums.Primitives
{
    /// <summary>
    /// Traits are measurable properties on objects. They can be composed of multiple domains (like mph or dollars/day or lwh) but do not need to be.
    /// </summary>
    public class Trait : IMathElement
	{
		public Brain? Brain { get; internal set; }
        public MathElementKind Kind => MathElementKind.Trait;
        public int Id { get; internal set; }
        public int CreationIndex => Id - (int)Kind - 1;
        
        public virtual string Name { get; }

        public readonly Dictionary<int, long> PositionStore = new Dictionary<int, long>(4096);
        public readonly Dictionary<int, IFocal> FocalStore = new Dictionary<int, IFocal>();
        public readonly Dictionary<int, Domain> DomainStore = new Dictionary<int, Domain>();

        protected internal Trait()
        {
        }
        private Trait(string name)
        {
	        Name = name;
        }

        public static Trait CreateIn(Brain brain, string name) => brain.AddTrait(new Trait(name));

	    public Domain AddDomain(IFocal basis, IFocal minMax)
	    {
		    return new Domain(this, basis, minMax);
	    }
	    public Domain AddDomain(long basisTicks)
	    {
		    return AddDomain(Focal.CreateZeroFocal(basisTicks), Focal.MinMaxFocal);
	    }
        public IEnumerable<Domain> Domains()
	    {
		    foreach (var domain in DomainStore.Values)
		    {
			    yield return domain;
		    }
	    }

        public Domain? DomainAt(int index)
	    {
		    var id = index + (int)MathElementKind.Domain;
		    DomainStore.TryGetValue(id, out var result);
		    return result;
	    }
	    public IFocal? FocalAt(int index)
	    {
		    var id = index + (int)MathElementKind.Focal;
		    FocalStore.TryGetValue(id, out var result);
		    return result;
	    }
	}
}
