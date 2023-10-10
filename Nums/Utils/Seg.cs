using Nums.Primitives;
using System;
using System.Numerics;
using static System.Net.Mime.MediaTypeNames;

namespace Nums.Utils;

public struct Seg : IEquatable<Seg?>
{
    public static readonly Seg Empty = new Seg(new Complex(double.NaN, double.NaN));
    public static readonly Seg Zero = new Seg(0.0, 0.0);
    public static readonly Seg Unit = new Seg(0.0, 1.0);
    public static readonly Seg Unot = new Seg(1.0, 0.0);
    public static readonly Seg Umid = new Seg(-0.5, 0.5);
    public static readonly Seg MaxRange = new Seg(double.MaxValue, double.MaxValue);
    public static readonly Seg MinRange = new Seg(double.MinValue, double.MinValue);
    public static readonly double Tolerance = 0.000000001;

    private Complex mComplex; // a complex segment is always stored in unit space, start to end is -Imaginary to +Real.
	public double Start { get => mComplex.Imaginary; set => mComplex = new Complex(mComplex.Real, value); }
	public double End { get => mComplex.Real; set => mComplex = new Complex(value, mComplex.Imaginary); }
	public double iStart { get => -mComplex.Real; set => mComplex = new Complex(-value, mComplex.Imaginary); }
	public double iEnd { get => -mComplex.Imaginary; set => mComplex = new Complex(mComplex.Real, -value); }

	public float StartF => (float)Start;
	public float EndF => (float)End;
	public float iStartF => (float)iStart;
	public float iEndF => (float)iEnd;

	public Complex Complex
	{
		get => mComplex;
		set => mComplex = value;
	}
	public Complex iComplex
	{
		get => new Complex(iEnd, iStart);
		set => mComplex = new Complex(-value.Real, -value.Imaginary);
	}
	public Seg(int start, int end)
    {
        mComplex = new Complex(end, start); // real, imag
    }
    public Seg(double start, double end)
    {
		mComplex = new Complex(end, start); // real, imag
    }
	public Seg(Seg value)
	{
		mComplex = new Complex(value.End, value.Start); // real, imag
	}
	public Seg(Complex value)
	{
        mComplex = value;
	}

    public Seg Clone() => new Seg(Start, End);

    public double Min => Start >= End ? End : Start;
    public double Max => End >= Start ? End : Start;
    public float MinF => (float)Min;
    public float MaxF => (float)Max;
    public double Length => Seg.AbsLength(this);
    public double DirectedLength => Seg.DirLength(this);
    public double Magnitude => mComplex.Magnitude;// Math.Sqrt(End * End + Start * Start);
    public Seg Conjugate() => Seg.Conjugate(this);
    public Seg Reciprocal() => Seg.Reciprocal(this);
    public Seg Normalize() => Seg.Normalize(this);
    public Seg NormalizeTo(Seg value) => Seg.NormalizeTo(this, value);
    public Seg ClampInner() => Seg.ClampInner(this);
    public Seg ClampOuter() => Seg.ClampOuter(this);
    public Seg Round() => Seg.Round(this);
    public Seg PositiveDirection() => PositiveDirection(this);
    public Seg NegativeDirection() => NegativeDirection(this);

    public bool IsZero => End == 0 && Start == 0;
    public bool IsZeroLength => (End == Start);
    public bool IsForward => End - Start > Tolerance;
    public bool IsBackward => Start - End > Tolerance;
    public bool IsPoint => Length < Tolerance;
    public double Direction => End >= Start ? 1.0 : -1.0;

    // because everything is segments, can add 'prepositions' (before, after, between, entering, leaving, near etc)
    public bool IsWithin(Seg value) => Start >= value.Start && End <= value.End;
    public bool IsBetween(Seg value) => Start > value.Start && End < value.End;
    public bool IsBefore(Seg value) => Start < value.Start && End < value.Start;
    public bool IsAfter(Seg value) => Start > value.End && End > value.End;
    public bool CoversBeginningOf(Seg value) => Start <= value.Start && End >= value.Start;
    public bool CoversEndingOf(Seg value) => Start <= value.End && End >= value.End;
    public bool IsTouching(Seg value) => (Start >= value.Start && Start <= value.End) || (End >= value.Start && End <= value.End);
    public bool IsNotTouching(Seg value) => !IsTouching(value);


	public static Seg operator +(Seg a, double value) => new Seg(a.Complex + value);// new Seg(a.Start + value, a.End + value);
	public static Seg operator -(Seg a, double value) => new Seg(a.Complex - value);// a.Start - value, a.End - value);
	public static Seg operator *(Seg a, double value) => new Seg(a.Complex * value);// a.Start * value, a.End * value);
	public static Seg operator /(Seg a, double value) => new Seg(a.Complex / value);// value == 0 ? double.MaxValue : a.Start / value, value == 0 ? double.MaxValue : a.End / value);

	//public static double Magnitude(Seg value) => Math.Sqrt(value.End * value.End + value.Start * value.Start);
	public static Seg PositiveDirection(Seg value) => new Seg(value.Min, value.Max);
    public static Seg NegativeDirection(Seg value) => new Seg(value.Max, value.Min);
    public static Seg Negate(Seg value) => -value;
    public static Seg Add(Seg left, Seg right) => left + right;
    public static Seg Subtract(Seg left, Seg right) => left - right;
    public static Seg Multiply(Seg left, Seg right) => left * right;
    public static Seg Divide(Seg dividend, Seg divisor) => dividend / divisor;

    public static Seg operator -(Seg value) => new Seg(-value.mComplex);//-value.Start, -value.End);
    public static Seg operator +(Seg left, Seg right) => new Seg(left.mComplex + right.mComplex);// Start + right.Start, left.End + right.End);
    public static Seg operator -(Seg left, Seg right) => new Seg(left.mComplex - right.mComplex);// left.Start - right.Start, left.End - right.End);
	public static Seg operator *(Seg left, Seg right) => new Seg(left.mComplex * right.mComplex);//
	//left.Start * right.End + left.End * right.Start, 
 //       left.End * right.End - left.Start * right.Start);
    public static Seg operator /(Seg left, Seg right) => new Seg(left.mComplex / right.mComplex);
    //{
    //    double real1 = left.End;
    //    double imaginary1 = left.Start;
    //    double real2 = right.End;
    //    double imaginary2 = right.Start;
    //    if (Math.Abs(imaginary2) < Math.Abs(real2))
    //    {
    //        double num = imaginary2 / real2;
    //        return new Seg((imaginary1 - real1 * num) / (real2 + imaginary2 * num), (real1 + imaginary1 * num) / (real2 + imaginary2 * num));
    //    }
    //    double num1 = real2 / imaginary2;
    //    return new Seg((-real1 + imaginary1 * num1) / (imaginary2 + real2 * num1), (imaginary1 + real1 * num1) / (imaginary2 + real2 * num1));
    //}

    public static double DirLength(Seg value) => value.End - value.Start;
    public static double AbsLength(Seg value) => Math.Abs(value.End - value.Start);

    public static Seg Conjugate(Seg a) => new Seg(Complex.Conjugate(a.mComplex));// new Seg(a.End, -a.Start);
    public static Seg Reciprocal(Seg value) => new Seg(Complex.Reciprocal(value.mComplex));// value.End == 0.0 && value.Start == 0.0 ? Seg.Zero : Seg.Unit / value;
public static Seg Normalize(Seg value) 
    {
        if (value.IsZeroLength) { return Seg.Zero; }
        double mag = value.Magnitude;
        return new Seg(value.Start / mag, value.End / mag);
    }
public static Seg NormalizeTo(Seg from, Seg to) => from.Normalize() * to;

    public static Seg ClampInner(Seg value) => new Seg(Math.Floor(Math.Abs(value.Start)) * Math.Sign(value.Start), Math.Floor(Math.Abs(value.End)) * Math.Sign(value.End));
    public static Seg ClampOuter(Seg value) => new Seg(Math.Ceiling(Math.Abs(value.Start)) * Math.Sign(value.Start), Math.Ceiling(Math.Abs(value.End)) * Math.Sign(value.End));
    public static Seg Round(Seg value) => new Seg(Math.Round(value.Start, MidpointRounding.AwayFromZero), Math.Round(value.End, MidpointRounding.AwayFromZero));

    public static double Abs(Seg value) => Complex.Abs(value.mComplex);
    //{
    // if (double.IsInfinity(value.End) || double.IsInfinity(value.Start))
    //  return double.PositiveInfinity;
    // double num1 = Math.Abs(value.End);
    // double num2 = Math.Abs(value.Start);
    // if (num1 > num2)
    // {
    //  double num3 = num2 / num1;
    //  return num1 * Math.Sqrt(1.0 + num3 * num3);
    // }
    // if (num2 == 0.0)
    //  return num1;
    // double num4 = num1 / num2;
    // return num2 * Math.Sqrt(1.0 + num4 * num4);
    //}

    public static Seg Pow(Seg value, Seg power) => new Seg(Complex.Pow(value.mComplex, power.mComplex));
    //{
    //    if (power == Seg.Zero)
    //        return Seg.Unit;
    //    if (value == Seg.Zero)
    //        return Seg.Zero;
    //    double real1 = value.End;
    //    double imaginary1 = value.Start;
    //    double real2 = power.End;
    //    double imaginary2 = power.Start;
    //    double num1 = Seg.Abs(value);
    //    double num2 = Math.Atan2(imaginary1, real1);
    //    double num3 = real2 * num2 + imaginary2 * Math.Log(num1);
    //    double num4 = Math.Pow(num1, real2) * Math.Pow(Math.E, -imaginary2 * num2);
    //    return new Seg(num4 * Math.Sin(num3), num4 * Math.Cos(num3));
    //}
    public static Seg Pow(Seg value, double power) => new Seg(Complex.Pow(value.mComplex, power));// => Seg.Pow(value, new Seg(0.0, power));
    private static Seg Scale(Seg value, double factor) => new Seg(value.Start * factor, value.End * factor);// factor * value.Start, factor * value.End);

    public bool IsSameDirection(Seg range) => Math.Abs(range.Direction + Direction) > Tolerance;
    public bool FullyContains(Seg toTest, bool includeEndpoints = true)
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



    public static bool operator ==(Seg left, Seg right) => left.mComplex == right.mComplex;// left.End == right.End && left.Start == right.Start;
    public static bool operator !=(Seg left, Seg right) => left.mComplex != right.mComplex; // left.End != right.End || left.Start != right.Start;

    public override bool Equals(object? other) => other is Seg seg && Equals(seg);
    public bool Equals(Seg? value) => mComplex.Equals(value?.mComplex);
    public override int GetHashCode() => mComplex.GetHashCode();

    public override string ToString() => $"[{Start:0.00}->{End:0.00}]";
}
