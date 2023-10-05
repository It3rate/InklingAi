using System;
using Nums.Primitives;

namespace Nums.Utils
{
	public struct NumRange
    {
        public static readonly NumRange Empty = new NumRange(0.0, 1.0, true);
        public static readonly NumRange Zero = new NumRange(0.0, 0.0);
        public static readonly NumRange Unit = new NumRange(0.0, 1.0);
        public static readonly NumRange Unot = new NumRange(1.0, 0.0);
        public static readonly NumRange Umid = new NumRange(-0.5, 0.5);
        public static readonly NumRange MaxRange = new NumRange(double.MaxValue, double.MaxValue);
        public static readonly NumRange MinRange = new NumRange(double.MinValue, double.MinValue);
        public static readonly double Tolerance = 0.000000001;

        public double Start { get; set; }
        public double End { get; set; }
        private readonly bool _hasValue;

        public float StartF => (float)Start;
        public float EndF => (float)End;

        public NumRange(int start, int end)
        {
            _hasValue = true;
            Start = start;
            End = end;
        }
        public NumRange(double start, double end)
        {
            _hasValue = true;
            Start = start;
            End = end;
        }
        public NumRange(NumRange value)
        {
            _hasValue = true;
            Start = value.Start;
            End = value.End;
        }

        private NumRange(double start, double end, bool isEmpty) // empty ctor
        {
            _hasValue = !isEmpty;
            Start = start;
            End = end;
        }

        public bool IsEmpty => _hasValue;

        public NumRange Clone() => new NumRange(Start, End);

        public double Min => Start >= End ? End : Start;
        public double Max => End >= Start ? End : Start;
        public float MinF => (float)Min;
        public float MaxF => (float)Max;
        public double Length => NumRange.AbsLength(this);
        public double DirectedLength() => NumRange.DirectedLength(this);
        public double AbsLength() => NumRange.AbsLength(this);
        public NumRange Conjugate() => NumRange.Conjugate(this);
        public NumRange Reciprocal() => NumRange.Reciprocal(this);
        public NumRange Square() => NumRange.Square(this);
        public NumRange Normalize() => NumRange.Normalize(this);
        public NumRange NormalizeTo(NumRange value) => NumRange.NormalizeTo(this, value);
        public NumRange ClampInner() => NumRange.ClampInner(this);
        public NumRange ClampOuter() => NumRange.ClampOuter(this);
        public NumRange Round() => NumRange.Round(this);
        public NumRange PositiveDirection() => PositiveDirection(this);
        public NumRange NegativeDirection() => NegativeDirection(this);

        public bool IsZero => End == 0 && Start == 0;
        public bool IsZeroLength => (End == Start);
        public bool IsForward => End - Start > Tolerance;
        public bool IsBackward => Start - End > Tolerance;
        public bool IsPoint => Length < Tolerance;
        public double Direction => End >= Start ? 1.0 : -1.0;

        // because everything is segments, can add 'prepositions' (before, after, between, entering, leaving, near etc)
        public bool IsWithin(NumRange value) => Start >= value.Start && End <= value.End;
        public bool IsBetween(NumRange value) => Start > value.Start && End < value.End;
        public bool IsBefore(NumRange value) => Start < value.Start && End < value.Start;
        public bool IsAfter(NumRange value) => Start > value.End && End > value.End;
        public bool IsBeginning(NumRange value) => Start <= value.Start && End > value.Start;
        public bool IsEnding(NumRange value) => Start >= value.End && End > value.End;
        public bool IsTouching(NumRange value) => (Start >= value.Start && Start <= value.End) || (End >= value.Start && End <= value.End);
        public bool IsNotTouching(NumRange value) => !IsTouching(value);


        public static NumRange operator +(NumRange a, double value) => new NumRange(a.Start + value, a.End + value);
        public static NumRange operator -(NumRange a, double value) => new NumRange(a.Start - value, a.End - value);
        public static NumRange operator *(NumRange a, double value) => new NumRange(a.Start * value, a.End * value);
        public static NumRange operator /(NumRange a, double value) => new NumRange(value == 0 ? double.MaxValue : a.Start / value, value == 0 ? double.MaxValue : a.End / value);

        public static NumRange PositiveDirection(NumRange value) => new NumRange(value.Min, value.Max);
        public static NumRange NegativeDirection(NumRange value) => new NumRange(value.Max, value.Min);
        public static NumRange Negate(NumRange value) => -value;
        public static NumRange Add(NumRange left, NumRange right) => left + right;
        public static NumRange Subtract(NumRange left, NumRange right) => left - right;
        public static NumRange Multiply(NumRange left, NumRange right) => left * right;
        public static NumRange Divide(NumRange dividend, NumRange divisor) => dividend / divisor;

        public static NumRange operator -(NumRange value) => new NumRange(-value.Start, -value.End);
        public static NumRange operator +(NumRange left, NumRange right) => new NumRange(left.Start + right.Start, left.End + right.End);
        public static NumRange operator -(NumRange left, NumRange right) => new NumRange(left.Start - right.Start, left.End - right.End);
        public static NumRange operator *(NumRange left, NumRange right) => new NumRange(left.Start * right.End + left.End * right.Start, left.End * right.End - left.Start * right.Start);
        public static NumRange operator /(NumRange left, NumRange right)
        {
            double real1 = left.End;
            double imaginary1 = left.Start;
            double real2 = right.End;
            double imaginary2 = right.Start;
            if (Math.Abs(imaginary2) < Math.Abs(real2))
            {
                double num = imaginary2 / real2;
                return new NumRange((imaginary1 - real1 * num) / (real2 + imaginary2 * num), (real1 + imaginary1 * num) / (real2 + imaginary2 * num));
            }
            double num1 = real2 / imaginary2;
            return new NumRange((-real1 + imaginary1 * num1) / (imaginary2 + real2 * num1), (imaginary1 + real1 * num1) / (imaginary2 + real2 * num1));
        }

        public static double DirectedLength(NumRange value) => value.End + value.Start;
        public static double AbsLength(NumRange value) => Math.Abs(value.End + value.Start);

        public static NumRange Conjugate(NumRange a) => new NumRange(a.End, -a.Start);
        public static NumRange Reciprocal(NumRange value) => value.End == 0.0 && value.Start == 0.0 ? NumRange.Zero : NumRange.Unit / value;
        public static NumRange Square(NumRange a) => new NumRange(a.Start * a.Start + (a.End * a.End) * -1, 0); // value * value;
        public static NumRange Normalize(NumRange value) => value.IsZeroLength ? new NumRange(0.5, 0.5) : value / value;
        public static NumRange NormalizeTo(NumRange from, NumRange to) => from.Normalize() * to;

        public static NumRange ClampInner(NumRange value) => new NumRange(Math.Floor(Math.Abs(value.Start)) * Math.Sign(value.Start), Math.Floor(Math.Abs(value.End)) * Math.Sign(value.End));
        public static NumRange ClampOuter(NumRange value) => new NumRange(Math.Ceiling(Math.Abs(value.Start)) * Math.Sign(value.Start), Math.Ceiling(Math.Abs(value.End)) * Math.Sign(value.End));
        public static NumRange Round(NumRange value) => new NumRange(Math.Round(value.Start), Math.Round(value.End));

        public static double Abs(NumRange value)
        {
	        if (double.IsInfinity(value.End) || double.IsInfinity(value.Start))
		        return double.PositiveInfinity;
	        double num1 = Math.Abs(value.End);
	        double num2 = Math.Abs(value.Start);
	        if (num1 > num2)
	        {
		        double num3 = num2 / num1;
		        return num1 * Math.Sqrt(1.0 + num3 * num3);
	        }
	        if (num2 == 0.0)
		        return num1;
	        double num4 = num1 / num2;
	        return num2 * Math.Sqrt(1.0 + num4 * num4);
        }
        
        public static NumRange Pow(NumRange value, NumRange power)
        {
            if (power == NumRange.Zero)
                return NumRange.Unit;
            if (value == NumRange.Zero)
                return NumRange.Zero;
            double real1 = value.End;
            double imaginary1 = value.Start;
            double real2 = power.End;
            double imaginary2 = power.Start;
            double num1 = NumRange.Abs(value);
            double num2 = Math.Atan2(imaginary1, real1);
            double num3 = real2 * num2 + imaginary2 * Math.Log(num1);
            double num4 = Math.Pow(num1, real2) * Math.Pow(Math.E, -imaginary2 * num2);
            return new NumRange(num4 * Math.Sin(num3), num4 * Math.Cos(num3));
        }
        public static NumRange Pow(NumRange value, double power) => NumRange.Pow(value, new NumRange(0.0, power));
        private static NumRange Scale(NumRange value, double factor) => new NumRange(factor * value.Start, factor * value.End);

        public bool IsSameDirection(NumRange range) => Math.Abs(range.Direction + Direction) > Tolerance;
        public bool FullyContains(NumRange toTest, bool includeEndpoints = true)
        {
	        bool result = false;
	        if (IsSameDirection(toTest))
	        {
		        var pd = PositiveDirection();
		        var pdTest = toTest.PositiveDirection();
		        result = includeEndpoints ? pdTest.IsWithin(pd) : pdTest.IsBetween(pd);
            }
	        return result;
        }



        public static bool operator ==(NumRange left, NumRange right) => left.End == right.End && left.Start == right.Start;
        public static bool operator !=(NumRange left, NumRange right) => left.End != right.End || left.Start != right.Start;
        public override bool Equals(object obj)
        {
            return obj is NumRange other && Equals(other);
        }
        public bool Equals(NumRange value)
        {
            return Start.Equals(value.Start) && End.Equals(value.End);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Start.GetHashCode();
                hashCode = (hashCode * 397) ^ End.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString() => $"[{Start:0.00}->{End:0.00}]";
    }
}
