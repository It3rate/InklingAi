using Nums.Utils;

namespace Nums.Primitives
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class Relation : IMathElement
    {
	    public MathElementKind Kind => MathElementKind.Relation;
	    public int Id { get; set; }
	    public int CreationIndex => Id - (int)Kind - 1;

	    public Brain Brain { get; }

        // the domain basis here may need to allow one to be exponential and the the other not? 
        // Or maybe this is always with the numbers/transforms.
	    public Domain UnitDomain { get; }
	    public Domain UnotDomain { get; }

        public Number Relatedness { get; set; } // angle of relation between source and Repeat, like the dot product. Determines 'perpendicularness' of axis. Can be non linear.


        public override bool Equals(object? obj) => obj is Relation other && Equals(other);
        public bool Equals(IMathElement? other) => other is Relation && Equals(other);
        public bool Equals(Relation? value) => Id.Equals(value?.Id);
        public override int GetHashCode() => Id.GetHashCode();
    }
}
