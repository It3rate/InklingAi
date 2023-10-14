
using System;
using System.Collections;
using System.Collections.Generic;
using Nums.Utils;

namespace Nums.Primitives
{
    public interface IFocal : IMathElement
    {
        long StartTickPosition { get; set; }
        long EndTickPosition { get; set; }
        long DirectedLength { get; }
        ulong Length { get; } // min size 1
        int Direction { get; }
        bool IsUnitPerspective { get; }
        bool IsUnotPerspective { get; }
        long Min { get; }
        long Max { get; }
        Seg Seg { get; set; }

		IFocal Shift(long value);
		Seg ShiftedSeg(long value);
		void ReverseDirection();
        Seg GetSegWithBasis(IFocal basis);
        void SetTicksWithSegBasis(Seg seg, IFocal basis);

        void GrowEnd(long ticks);
        void GrowStart(long ticks);
        void ShrinkEnd(long ticks);
        void ShrinkStart(long ticks);
        void GrowEnd(IFocal other);
        void GrowStart(IFocal other);
        void ShrinkEnd(IFocal other);
        void ShrinkStart(IFocal other);
        void StretchBy(IFocal amount);
        public void SqueezeBy(IFocal amount);

        Seg UnitTRangeIn(IFocal basis);
        IFocal Clone();
    }

    /// <summary>
    /// Focals are a measure of ticks. When used with a basis focal they can represent ratios in the unit or unot directions.
    /// The basis focal sets the zero point and length of the unit. Alone a focal is just a span of ticks.
    /// Boolean or tip/tail operations can happen on a raw focal, but stretching only makes sense with a zero point.
    /// The physical zero point on a raw focal isn't relevant (the tick measure could be long or ulong with no change in relevance).
    /// The values only have signifcance when used relative to a basis. A basis pointing 'left' is a unot basis (a negative unit).
    /// This is a virtual class as there will potentially be polylines of Focals if it makes sense.
    /// </summary>
    public class Focal : IFocal, IEquatable<Focal>
    {
        public MathElementKind Kind => MathElementKind.Focal;
        protected List<long> _positions = new List<long>() { 0, 0 };
        public virtual long StartTickPosition { get => _positions[0]; set => _positions[0] = value; }
        public virtual long EndTickPosition { get => _positions[_positions.Count - 1]; set => _positions[_positions.Count - 1] = value; }

		// Focal values are always added as unit perspective positions, because
		// there is no unit defined that allows the start point to be interpreted as a unot perspective.
		// Focals are pre-number, positions, not value interpretations.
		protected Focal()
		{
        }
		public Focal(long startTickPosition, long endTickPosition)
        {
            StartTickPosition = startTickPosition;
			EndTickPosition = endTickPosition;
        }
        protected Focal(Seg seg, IFocal basis)
        {
            SetTicksWithSegBasis(seg, basis);
        }
        private Focal(long[] positions)
		{
			StartTickPosition = positions[0];
            EndTickPosition = positions[positions.Length - 1];
        }


        public int Direction => StartTickPosition <= EndTickPosition ? 1 : -1;
		public bool IsUnitPerspective => StartTickPosition <= EndTickPosition;
		public bool IsUnotPerspective => StartTickPosition > EndTickPosition;

		public virtual IFocal Shift(long value) => new Focal(_positions.Select(p => p + value).ToArray());
		public virtual Seg ShiftedSeg(long value) => new Seg(StartTickPosition + value, EndTickPosition + value);
		public virtual Seg GetSegWithBasis(IFocal basis)
        {
            var div = ShiftedSeg(-basis.StartTickPosition) / basis.ShiftedSeg(-basis.StartTickPosition);
            return basis.IsUnitPerspective ? div : new Seg(div.End, div.Start);
        }
        public virtual void SetTicksWithSegBasis(Seg seg, IFocal basis)
        {
            var mult = seg * basis.ShiftedSeg(-basis.StartTickPosition);
			var start = basis.IsUnitPerspective ? mult.Start : mult.End;
			var end = basis.IsUnitPerspective ? mult.End : mult.Start;
            StartTickPosition = (long)Math.Round(start + basis.StartTickPosition, MidpointRounding.AwayFromZero);
            EndTickPosition = (long)Math.Round(end + basis.StartTickPosition, MidpointRounding.AwayFromZero);
        }
        public virtual void ReverseDirection()
		{
            _positions.Reverse();
		}
        public virtual void StretchBy(IFocal amount) => Seg *= amount.Seg;
        public virtual void SqueezeBy(IFocal amount) => Seg /= amount.Seg;
		public virtual Seg UnitTRangeIn(IFocal basis)
        {
            var dl = basis.DirectedLength == 0 ? 1 : basis.DirectedLength;
            var len = (double)Math.Abs(dl);
            var start = (StartTickPosition - basis.StartTickPosition) / len;
            var end = (EndTickPosition - basis.StartTickPosition) / len;
            return new Seg(start, end);
        }

		public virtual Seg Seg
        {
            get => new Seg(StartTickPosition, EndTickPosition);
            set { StartTickPosition = (long)value.Start; EndTickPosition = (long)value.End; }
        }

        public virtual ulong Length => (ulong)Math.Abs(EndTickPosition - StartTickPosition);
		public virtual long DirectedLength => EndTickPosition - StartTickPosition;

        public virtual long Min => _positions.Min();
        public virtual long Max => _positions.Max();

        public void GrowEnd(long ticks) => EndTickPosition += ticks * Direction;
        public void GrowStart(long ticks) => StartTickPosition -= ticks * Direction;
        public void ShrinkEnd(long ticks) => EndTickPosition -= ticks * Direction;
        public void ShrinkStart(long ticks) => StartTickPosition += ticks * Direction;

        public void GrowEnd(IFocal other) => EndTickPosition += other.DirectedLength * Direction;
        public void GrowStart(IFocal other) => StartTickPosition -= other.DirectedLength * Direction;
        public void ShrinkEnd(IFocal other) => EndTickPosition -= other.DirectedLength * Direction;
        public void ShrinkStart(IFocal other) => StartTickPosition += other.DirectedLength * Direction;


        public virtual IFocal Clone() => new Focal(_positions.ToArray());

		public static IFocal CreateZeroFocal(long ticks) => new Focal(0, ticks);
		public static IFocal CreateBalancedFocal(long halfTicks) => new Focal(-halfTicks, halfTicks);

        public static IFocal ZeroFocal { get; } = new Focal(0, 0);
        public static IFocal UnitFocal { get; } = new Focal(0, 1);
        public static IFocal UnotFocal { get; } = new Focal(0, -1);
        public static IFocal MinMaxFocal { get; } = new Focal(long.MinValue, long.MaxValue);
        public static IFocal UpMaxFocal { get; } = new Focal(0, long.MaxValue);
		public static IFocal InvalidFocal { get; } = new Focal(long.MaxValue, long.MaxValue); // just a marker for invalid

		public static long MinPosition(IFocal p, IFocal q) => Math.Min(p.Min, q.Min);
        public static long MaxPosition(IFocal p, IFocal q) => Math.Max(p.Max, q.Max);
        public static long MinStart(IFocal p, IFocal q) => Math.Min(p.StartTickPosition, q.StartTickPosition);
        public static long MaxStart(IFocal p, IFocal q) => Math.Max(p.StartTickPosition, q.StartTickPosition);
        public static long MinEnd(IFocal p, IFocal q) => Math.Min(p.EndTickPosition, q.EndTickPosition);
        public static long MaxEnd(IFocal p, IFocal q) => Math.Max(p.EndTickPosition, q.EndTickPosition);
        public static IFocal Overlap(IFocal p, IFocal q)
        {
	        var start = Math.Max(p.Min, q.Min);
	        var end = Math.Min(p.Max, q.Max);
	        return (start >= end) ? new Focal(0, 0) : new Focal(start, end);
        }
        public static IFocal Extent(IFocal p, IFocal q)
        {
	        var start = Math.Min(p.Min, q.Min);
	        var end = Math.Max(p.Max, q.Max);
	        return new Focal(start, end);
        }


        // bool ops
        // Q. Should direction be preserved in a bool operation?
        public static IFocal[] Never(IFocal p)
        {
	        return new IFocal[0];
        }
        public static IFocal[] Not(IFocal p)
        {
	        // If p starts at the beginning of the time frame and ends at the end, A is always true and the "not A" relationship is empty
	        if (p.StartTickPosition == 0 && p.EndTickPosition == long.MaxValue)
	        {
		        return new IFocal[] { };
	        }
	        // If p starts at the beginning of the time frame and ends before the end, the "not A" relationship consists of a single interval from p.EndTickPosition + 1 to the end of the time frame
	        else if (p.StartTickPosition == 0)
	        {
		        return new IFocal[] { new Focal(p.EndTickPosition + 1, long.MaxValue) };
	        }
	        // If p starts after the beginning of the time frame and ends at the end, the "not A" relationship consists of a single interval from the beginning of the time frame to p.StartTickPosition - 1
	        else if (p.EndTickPosition == long.MaxValue)
	        {
		        return new IFocal[] { new Focal(0, p.StartTickPosition - 1) };
	        }
	        // If p starts and ends within the time frame, the "not A" relationship consists of two intervals: from the beginning of the time frame to p.StartTickPosition - 1, and from p.EndTickPosition + 1 to the end of the time frame
	        else
	        {
		        return new IFocal[] { new Focal(0, p.StartTickPosition - 1), new Focal(p.EndTickPosition + 1, long.MaxValue) };
	        }
        }
        public static IFocal[] Transfer(IFocal p)
        {
	        return new IFocal[] { p.Clone() };
        }
        public static IFocal[] Always(IFocal p)
        {
	        return new IFocal[] { Focal.MinMaxFocal.Clone()};
        }

        public static IFocal[] Never(IFocal p, IFocal q)
        {
            return new IFocal[0];
        }
        public static IFocal[] And(IFocal p, IFocal q)
        {
	        var overlap = Overlap(p, q);
	        return (overlap.DirectedLength == 0) ? new IFocal[0] : new IFocal[] {overlap};
        }
        public static IFocal[] B_Inhibits_A(IFocal p, IFocal q)
        {
	        if (p.EndTickPosition < q.StartTickPosition - 1 || q.EndTickPosition < p.StartTickPosition - 1)
	        {
		        return new IFocal[] { p };
	        }
	        else
	        {
		        return new IFocal[] { new Focal(p.StartTickPosition, q.StartTickPosition - 1) };
	        }
        }
        public static IFocal[] Transfer_A(IFocal p, IFocal q)
        {
	        return new IFocal[] { p };
        }
        public static IFocal[] A_Inhibits_B(IFocal p, IFocal q)
        {
	        if (p.EndTickPosition < q.StartTickPosition - 1 || q.EndTickPosition < p.StartTickPosition - 1)
	        {
		        return new IFocal[] { q };
	        }
	        else
	        {
		        return new IFocal[] { new Focal(q.StartTickPosition, p.StartTickPosition - 1) };
	        }
        }
        public static IFocal[] Transfer_B(IFocal p, IFocal q)
        {
	        return new IFocal[] { q };
        }
        public static IFocal[] Xor(IFocal p, IFocal q)
        {
            // Return the symmetric difference of the two input segments as a new array of segments
            List<IFocal> result = new List<IFocal>();
            IFocal[] andResult = And(p, q);
            if (andResult.Length == 0)
            {
                // If the segments do not intersect, return the segments as separate non-overlapping segments
                result.Add(p);
                result.Add(q);
            }
            else
            {
                // If the segments intersect, return the complement of the intersection in each segment
                IFocal[] complement1 = Nor(p, andResult[0]);
                IFocal[] complement2 = Nor(q, andResult[0]);
                result.AddRange(complement1);
                result.AddRange(complement2);
            }
            return result.ToArray();
        }
        public static IFocal[] Or(IFocal p, IFocal q)
        {
	        var overlap = Overlap(p, q);
	        return (overlap.DirectedLength == 0) ? new IFocal[] {p, q} : new IFocal[] {Extent(p, q)};
        }
        public static IFocal[] Nor(IFocal p, IFocal q)
        {
            // Return the complement of the union of the two input IFocals as a new array of IFocals
            List<IFocal> result = new List<IFocal>();
            IFocal[] orResult = Or(p, q);
            if (orResult.Length == 0)
            {
                // If the IFocals do not overlap, return both IFocals as separate non-overlapping IFocals
                result.Add(p);
                result.Add(q);
            }
            else
            {
                // If the IFocals overlap, return the complement of the union in each IFocal
                IFocal[] complement1 = Nor(p, orResult[0]);
                IFocal[] complement2 = Nor(q, orResult[0]);
                result.AddRange(complement1);
                result.AddRange(complement2);
            }
            return result.ToArray();
        }
        public static IFocal[] Xnor(IFocal p, IFocal q)
        {
            // Return the complement of the symmetric difference of the two input IFocals as a new array of IFocals
            List<IFocal> result = new List<IFocal>();
            IFocal[] xorResult = Xor(p, q);
            if (xorResult.Length == 0)
            {
                // If the IFocals are equal, return p as a single IFocal
                result.Add(p);
            }
            else
            {
                // If the IFocals are not equal, return the complement of the symmetric difference in each IFocal
                IFocal[] complement1 = Nor(p, xorResult[0]);
                IFocal[] complement2 = Nor(q, xorResult[0]);
                result.AddRange(complement1);
                result.AddRange(complement2);
            }
            return result.ToArray();
        }
        public static IFocal[] Not_B(IFocal p, IFocal q)
        {
	        return Not(q);
        }
        public static IFocal[] B_Implies_A(IFocal p, IFocal q)
        {
	        if (p.EndTickPosition < q.StartTickPosition - 1 || q.EndTickPosition < p.StartTickPosition - 1)
	        {
		        return new IFocal[] { };
	        }
	        else
	        {
		        return new IFocal[] { new Focal(MaxStart(p, q), MinEnd(p, q)) };
	        }
        }
        public static IFocal[] Not_A(IFocal p, IFocal q)
        {
	        return Not(p);
        }
        public static IFocal[] A_Implies_B(IFocal p, IFocal q)
        {
	        if (p.EndTickPosition < q.StartTickPosition - 1 || q.EndTickPosition < p.StartTickPosition - 1)
	        {
		        return new IFocal[] { };
	        }
	        else
	        {
		        return new IFocal[] { new Focal(MaxStart(p, q), MinEnd(p, q)) };
	        }
        }
        public static IFocal[] Nandx(IFocal p, IFocal q)
        {
            // Return the complement of the intersection of the two input IFocals as a new array of IFocals
            List<IFocal> result = new List<IFocal>();
            IFocal[] andResult = And(p, q);
            if (andResult.Length == 0)
            {
                // If the IFocals do not intersect, return the union of the IFocals as a single IFocal
                result.Add(new Focal(MinStart(p, q), MaxEnd(p, q)));
            }
            else
            {
                // If the IFocals intersect, return the complement of the intersection in each IFocal
                IFocal[] complement1 = Nor(p, andResult[0]);
                IFocal[] complement2 = Nor(q, andResult[0]);
                result.AddRange(complement1);
                result.AddRange(complement2);
            }
            return result.ToArray();
        }
        public static IFocal[] Nand(IFocal p, IFocal q)
        {
            IFocal[] result;
            var overlap = Overlap(p, q);
            if(overlap.DirectedLength == 0)
            {
                result = new IFocal[] { Focal.MinMaxFocal.Clone() };
            }
            else
            {
                result = new IFocal[]
                { 
                    new Focal(long.MinValue, overlap.StartTickPosition),
                    new Focal(overlap.EndTickPosition, long.MaxValue)
                };
            }
            return result;
        }
        public static IFocal[] Always(IFocal p, IFocal q)
        {
            return new IFocal[] { Focal.MinMaxFocal.Clone() };
        }


        //  0000	Never			0			FALSE
        //  0001	Both            A ^ B       AND
        //  0010	Only A          A ^ !B      A AND NOT B
        //  0011	A Maybe B       A           A
        //  0100	Only B			!A ^ B      NOT A AND B
        //  0101	B Maybe A       B           B
        //  0110	One of          A xor B     XOR
        //  0111	At least one    A v B       OR
        //  1000	No one          A nor B     NOR
        //  1001	Both or no one  A XNOR B    XNOR
        //  1010	A or no one		!B          NOT B
        //  1011	Not B alone     A v !B      A OR NOT B
        //  1100	B or no one		!A          NOT A
        //  1101	Not A alone		!A v B      NOT A OR B
        //  1110	Not both        A nand B    NAND
        //  1111	Always			1			TRUE

        public long Never(long a) { return 0; } // Null
        public long Transfer(long a) { return a; }
        public long Not(long a) { return ~a; }
        public long Identity(long a) { return -1; }

        public long Never(long a, long b) { return 0; } // Null
        public long And(long a, long b) { return a & b; }
        public long B_Inhibits_A(long a, long b) { return a & ~b; } // inhibition, div a/b. ab`
        public long Transfer_A(long a, long b) { return a; }                   // transfer
        public long A_Inhibits_B(long a, long b) { return ~a & b; } // inhibition, div b/a. a`b
        public long Transfer_B(long a, long b) { return b; }                   // transfer
        public long Xor(long a, long b) { return a ^ b; }
        public long Or(long a, long b) { return a | b; }
        public long Nor(long a, long b) { return ~(a | b); }
        public long Xnor(long a, long b) { return ~(a ^ b); } // equivalence, ==. (xy)`
        public long Not_B(long a, long b) { return ~b; } // complement
        public long B_Implies_A(long a, long b) { return a | ~b; } // implication (Not B alone)
        public long Not_A(long a, long b) { return ~a; } // complement
        public long A_Implies_B(long a, long b) { return b | ~a; } // implication (Not A alone)
        public long Nand(long a, long b) { return ~(a & b); }
        public long Always(long a, long b) { return -1; }

        public long Equals(long a, long b) { return ~(a ^ b); } // xnor
        public long NotEquals(long a, long b) { return a ^ b; } // xor
        public long GreaterThan(long a, long b) { return ~a & b; } // b > a
        public long LessThan(long a, long b) { return a & ~b; } // a > b
        public long GreaterThanOrEqual(long a, long b) { return ~a | b; } // a implies b
        public long LessThanOrEqual(long a, long b) { return a | ~b; } // b implies a


        public override bool Equals(object? obj) => obj is Focal other && Equals(other);
        public bool Equals(IMathElement? other) => other is Focal focal && Equals(focal);
        public bool Equals(Focal? value) => value != null && StartTickPosition.Equals(value?.StartTickPosition) && EndTickPosition.Equals(value?.EndTickPosition);
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = StartTickPosition.GetHashCode();
                hashCode = (hashCode * 397) ^ EndTickPosition.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString() => $"[{StartTickPosition}->{EndTickPosition}]";

    }
}
