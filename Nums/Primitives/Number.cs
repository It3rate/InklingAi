
using System;
using Nums.Utils;

namespace Nums.Primitives
{
    public class Number : IMathElement
	{
		public MathElementKind Kind => MathElementKind.Number;
        public Domain Domain { get; set; }
		public IFocal Focal { get; set; }

		public Trait Trait => Domain.Trait;
        public IFocal BasisFocal => Domain.BasisFocal;

		public long ZeroTick => BasisFocal.StartTickPosition;
        public bool IsUnitPerspective => Domain.IsUnitPerspective;
		public bool IsUnotPerspective => Domain.IsUnotPerspective;
		public int Direction => StartTickPosition <= EndTickPosition ? 1 : -1;

		protected internal Number(Domain domain, IFocal focal)
		{
            Domain = domain;
			Focal = focal;
		}

		protected long StartTickPosition
		{
			get => Focal.StartTickPosition;
			set => Focal.StartTickPosition = value;
		}
		protected long EndTickPosition
		{
			get => Focal.EndTickPosition;
			set => Focal.EndTickPosition = value;
		}
		public long StartTicks
		{
			get => -StartTickPosition + ZeroTick;
			set => StartTickPosition = ZeroTick - value;
		}
		public long EndTicks
		{
			get => EndTickPosition - ZeroTick;
			set => EndTickPosition = value + ZeroTick;
		}
		public long TickCount => EndTickPosition - StartTickPosition;

		public double StartValue
		{
			get => Value.Start; //Domain.ValueFromStartTickPosition(StartTickPosition); // Value.Start is a little less efficient
			set => Value = new Seg(value, Value.End); //StartTicks = Domain.StartTickPositionFrom(value);

		}
		public double EndValue
		{
			get => Value.End; //Domain.ValueFromEndTickPosition(EndTickPosition); // Value.End is a little less efficient
			set => Value = new Seg(Value.Start, value); //EndTicks = Domain.EndTickPositionFrom(value);
		}
		public Seg Value 
		{
			get => Domain.GetValueOf(Focal); //Focal.RangeWithBasis(BasisFocal);
			set => Domain.SetValueOf(Focal, value);
		}
		public Seg ValueInRenderPerspective => new Seg(-StartValue, EndValue);

		public Number SetWith(Number other)
		{
			if (Domain.Id == other.Domain.Id)
			{
				StartTickPosition = other.StartTickPosition;
				EndTickPosition = other.EndTickPosition;
			}
			else
			{
				Value = other.Value;
            }
			return other;
		}

		public long WholeStartValue => (long) StartValue;
		public long WholeEndValue => (long) EndValue;
		public long RemainderStartValue => Domain.IsApproximate ? 0 : Math.Abs(StartTicks % BasisFocal.NonZeroLength);
		public long RemainderEndValue => Domain.IsApproximate ? 0 : Math.Abs(EndTicks % BasisFocal.NonZeroLength);
		public Seg RangeInMinMax => Focal.UnitTRangeIn(Domain.MinMaxFocal); //*

		public Seg FloorRange => new Seg(Math.Ceiling(StartValue), Math.Floor(EndValue));
		public Seg CeilingRange => new Seg(Math.Floor(StartValue), Math.Ceiling(EndValue));
		public Seg RoundedRange => new Seg(Math.Round(StartValue, MidpointRounding.AwayFromZero), Math.Round(EndValue, MidpointRounding.AwayFromZero));
		public Seg RemainderRange => Value - FloorRange;

		public void PlusTick() => EndTickPosition += 1;
		public void MinusTick() => EndTickPosition -= 1;
        public void AddStartTicks(long ticks) => StartTickPosition += ticks;
        public void AddEndTicks(long ticks) => EndTickPosition += ticks;
		public void AddTicks(long startTicks, long endTicks) { StartTickPosition += startTicks; EndTickPosition += endTicks; }

	// Operations with segments and units allow moving the unit around freely, so for example,
        // you can shift a segment by aligning the unit with start or end,
        // and scale in place by moving the unit to left, right or center (equivalent to affine scale, where you move to zero, scale, then move back)
        // need to have overloads that allow shifting the unit temporarily
        public void Add(Number other)
        {
            // todo: eventually all math on Numbers will be in ticks, allowing preservation of precision etc. Requires syncing of basis, domains.
	        Value += other.Value;
        }
        public void Subtract(Number other)
		{
			Value -= other.Value;
        }
        public void Multiply(Number other)
		{
			Value *= other.Value;
        }
        public void Divide(Number other)
		{
			Value /= other.Value;
        }

        public double RatioAt(double pointOnLine)
        {
	        var val = Value;
	        var len = val.Length;
	        return (pointOnLine + val.Start) / len;
        }

		public void ChangeDomain(Domain newDomain)
		{
			if(newDomain != Domain)
			{
				var value = Value;
				Domain = newDomain; 
				Value = value;
			}
		}

		public bool FullyContains(Number toTest, bool includeEndpoints = true) => toTest != null ? Value.FullyContains(toTest.Value, includeEndpoints) : false;
		public Number AlignedDomainCopy(Number toCopy) => AlignToDomain(toCopy, Domain);
		public static Number AlignToDomain(Number target, Domain domain)
		{
			var result = target.Clone();
			result.ChangeDomain(domain);
			return result;
		}

		public void InterpolateFromZero(Number t, Number result) => InterpolateFromZero(this, t, result);
        public void InterpolateFrom(Number source, Number t, Number result) => Interpolate(source, this, t, result);
        public void InterpolateTo(Number target, Number t, Number result) => Interpolate(this, target, t, result);
        public static void InterpolateFromZero(Number target, Number t, Number result)
        {
	        var targetValue = target.Value;
	        var tValue = t.Value;
	        result.StartValue = targetValue.Start * tValue.Start;
	        result.EndValue = targetValue.End * tValue.End;
        }
        public static void InterpolateFromOne(Number target, Number t, Number result)
        {
	        var targetValue = target.Value;
	        var tValue = t.Value;
	        result.StartValue = targetValue.Start * tValue.Start;
	        result.EndValue = (targetValue.End - 1.0) * tValue.End + 1.0;
        }
        public void InterpolateFromOne(Number target, double t)
        {
	        if (target != null)
	        {
		        var targetValue = target.Value;
		        StartValue = targetValue.Start * t;
		        EndValue = (targetValue.End - 1.0) * t + 1.0;
	        }
        }
        public void InterpolateFromOne(Seg range, double t)
        {
	        if (range != null)
	        {
		        StartValue = range.Start * t;
		        EndValue = (range.End - 1.0) * t + 1.0;
	        }
        }
        public static void Interpolate(Number source, Number target, Number t, Number result)
        {
	        var sourceValue = source.Value;
	        var targetValue = target.Value;
            var tValue = t.Value;
            result.StartValue = (targetValue.Start - sourceValue.Start) * tValue.Start + sourceValue.Start;
            result.EndValue = (targetValue.End - sourceValue.End) * tValue.End + sourceValue.End;
        }

        // segment comparison considers unot numbers to also have positive segments in the negative space, and
        // unit numbers to be unot in its negative space. So two unit numbers that have a point on both ends of a range,
        // can be considered an ordinary unot number. eg: [+++>....++>] == [....<---...]
        // This should also handle things like gt lt div0 etc

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

        /*
		function BooleanOperations()
		{
			this.Never = function(a, b) { return 0; };
			this.And = function(a, b) { return a & b; };
			this.LeftandNotRight = function(a, b) { return a & ~b; };
			this.Left = function(a, b) { return a; };
			this.NotLeftAndRight = function(a, b) { return ~a & b; };
			this.Right = function(a, b) { return b; };
			this.Xor = function(a, b) { return a ^ b; };
			this.Or = function(a, b) { return a | b; };
			this.Nor = function(a, b) { return ~(a | b); };
			this.Xnor = function(a, b) { return ~(a ^ b); };
			this.NotRight = function(a, b) { return ~b; };
			this.LeftOrNotRight = function(a, b) { return a | ~b; };// implies
			this.NotLeft = function(a, b) { return ~a; };
			this.RightOrNotLeft = function(a, b) { return ~a | b; }; // implies
			this.Nand = function(a, b) { return ~(a & b); };
			this.Always = function(a, b) { return 1; };
		}

		*/
        // use segments rather than ints
        // convert values to first param's domain's context
        // result in first params's domain

		public NumberSet Never(Number q) => new NumberSet(Domain, Primitives.Focal.Never(Focal, q.Focal));
		public void Never(Number q, NumberSet result) => result.Reset(Primitives.Focal.Never(Focal, q.Focal));

		public NumberSet And(Number q) => new NumberSet(Domain, Primitives.Focal.And(Focal, q.Focal));
		public void And(Number q, NumberSet result) => result.Reset(Primitives.Focal.And(Focal, q.Focal));

		public NumberSet B_Inhibits_A(Number q) => new NumberSet(Domain, Primitives.Focal.B_Inhibits_A(Focal, q.Focal));
		public void B_Inhibits_A(Number q, NumberSet result) => result.Reset(Primitives.Focal.B_Inhibits_A(Focal, q.Focal));

		public NumberSet Transfer_A(Number q) => new NumberSet(Domain, Primitives.Focal.Transfer_A(Focal, q.Focal));
		public void Transfer_A(Number q, NumberSet result) => result.Reset(Primitives.Focal.Transfer_A(Focal, q.Focal));

		public NumberSet A_Inhibits_B(Number q) => new NumberSet(Domain, Primitives.Focal.A_Inhibits_B(Focal, q.Focal));
		public void A_Inhibits_B(Number q, NumberSet result) => result.Reset(Primitives.Focal.A_Inhibits_B(Focal, q.Focal));

		public NumberSet Transfer_B(Number q) => new NumberSet(Domain, Primitives.Focal.Transfer_B(Focal, q.Focal));
		public void Transfer_B(Number q, NumberSet result) => result.Reset(Primitives.Focal.Transfer_B(Focal, q.Focal));

		public NumberSet Xor(Number q) => new NumberSet(Domain, Primitives.Focal.Xor(Focal, q.Focal));
		public void Xor(Number q, NumberSet result) => result.Reset(Primitives.Focal.Xor(Focal, q.Focal));

		public NumberSet Or(Number q) => new NumberSet(Domain, Primitives.Focal.Or(Focal, q.Focal));
		public void Or(Number q, NumberSet result) => result.Reset(Primitives.Focal.Or(Focal, q.Focal));

		public NumberSet Nor(Number q) => new NumberSet(Domain, Primitives.Focal.Nor(Focal, q.Focal));
		public void Nor(Number q, NumberSet result) => result.Reset(Primitives.Focal.Nor(Focal, q.Focal));

		public NumberSet Xnor(Number q) => new NumberSet(Domain, Primitives.Focal.Xnor(Focal, q.Focal));
		public void Xnor(Number q, NumberSet result) => result.Reset(Primitives.Focal.Xnor(Focal, q.Focal));

		public NumberSet Not_B(Number q) => new NumberSet(Domain, Primitives.Focal.Not_B(Focal, q.Focal));
		public void Not_B(Number q, NumberSet result) => result.Reset(Primitives.Focal.Not_B(Focal, q.Focal));

		public NumberSet B_Implies_A(Number q) => new NumberSet(Domain, Primitives.Focal.B_Implies_A(Focal, q.Focal));
		public void B_Implies_A(Number q, NumberSet result) => result.Reset(Primitives.Focal.B_Implies_A(Focal, q.Focal));

		public NumberSet Not_A(Number q) => new NumberSet(Domain, Primitives.Focal.Not_A(Focal, q.Focal));
		public void Not_A(Number q, NumberSet result) => result.Reset(Primitives.Focal.Not_A(Focal, q.Focal));

		public NumberSet A_Implies_B(Number q) => new NumberSet(Domain, Primitives.Focal.A_Implies_B(Focal, q.Focal));
		public void A_Implies_B(Number q, NumberSet result) => result.Reset(Primitives.Focal.A_Implies_B(Focal, q.Focal));

		public NumberSet Nand(Number q) => new NumberSet(Domain, Primitives.Focal.Nand(Focal, q.Focal));
		public void Nand(Number q, NumberSet result) => result.Reset(Primitives.Focal.Nand(Focal, q.Focal));

		public NumberSet Always(Number q) => new NumberSet(Domain, Primitives.Focal.Always(Focal, q.Focal));
		public void Always(Number q, NumberSet result) => result.Reset(Primitives.Focal.Always(Focal, q.Focal));


        public Number Clone() => new Number(Domain, Focal.Clone());

        public override string ToString()
		{
			var v = Value;
			return $"[{-v.Start:0.00}->{v.End:0.00}]";
		}

        public override bool Equals(object? obj) => obj is Number other && Equals(other);
        public bool Equals(IMathElement? other) => other is Number && Equals(other);
        public bool Equals(Number? value) => Domain.Equals(value?.Domain) && Focal.Equals(value?.Focal);

        public override int GetHashCode()
		{
			unchecked
			{
                var hashCode = Domain.GetHashCode();
                hashCode = (hashCode * 397) ^ Focal.GetHashCode();
                return hashCode;
			}
		}
    }
}
