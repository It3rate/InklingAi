using Nums.Primitives;
using Nums.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace InkPadTests.CoreTests;

public class SegTests : IDisposable
{
	public SegTests()
	{
	}
	public void Dispose()
	{
	}

	[Fact]
	public void UnotTests()
	{
		var seg = new Seg(-5, 7);
		Assert.Equal(-5, seg.Start, 3);
		Assert.Equal(7, seg.End, 3);
		Assert.Equal(-7, seg.iStart, 3);
		Assert.Equal(5, seg.iEnd, 3);
		Assert.Equal(-5, seg.Complex.Imaginary, 3);
		Assert.Equal(7, seg.Complex.Real, 3);
		var icplx = seg.iComplex;
		Assert.Equal(-7, icplx.Imaginary, 3);
		Assert.Equal(5, icplx.Real, 3);
		seg = new Seg(icplx);
		Assert.Equal(-7, seg.Start, 3);
		Assert.Equal(5, seg.End, 3);
		Assert.Equal(-5, seg.iStart, 3);
		Assert.Equal(7, seg.iEnd, 3);
	}
	[Fact]
	public void LengthTests()
	{
		var seg = new Seg(-5, 5);
		Assert.Equal(10, seg.Length, 3);
		Assert.Equal(10, seg.DirectedLength, 3);
		Assert.Equal(Math.Sqrt(50), seg.Magnitude, 3);
		seg = new Seg(5, -5);
		Assert.Equal(10, seg.Length, 3);
		Assert.Equal(-10, seg.DirectedLength, 3);
		Assert.Equal(Math.Sqrt(50), seg.Magnitude, 3);
		seg = new Seg(5, 5);
		Assert.Equal(0, seg.Length, 3);
		Assert.Equal(0, seg.DirectedLength, 3);
		Assert.Equal(Math.Sqrt(50), seg.Magnitude, 3);
		seg = new Seg(0, 15);
		Assert.Equal(15, seg.Length, 3);
		Assert.Equal(15, seg.DirectedLength, 3);
		Assert.Equal(Math.Sqrt(225), seg.Magnitude, 3);
		seg = new Seg(15, 0);
		Assert.Equal(15, seg.Length, 3);
		Assert.Equal(-15, seg.DirectedLength, 3);
		Assert.Equal(Math.Sqrt(225), seg.Magnitude, 3);
	}
	[Fact]
	public void MulDivTests()
	{
		var left = new Seg(0, 5);
		var right = new Seg(10, 20);
		var mul = left * right;
		Assert.Equal(50, mul.Start, 3);
		Assert.Equal(100, mul.End, 3);
		var div = mul / left;
		Assert.Equal(right.Start, div.Start, 3);
		Assert.Equal(right.End, div.End, 3);
		var div2 = right / left;
		Assert.Equal(2, div2.Start, 3);
		Assert.Equal(4, div2.End, 3);
		var sq = right * right;
		Assert.Equal(400, sq.Start, 3);
		Assert.Equal(300, sq.End, 3);
		var fr = left / right;
		Assert.Equal(-.1, fr.Start, 3);
		Assert.Equal(.2, fr.End, 3);
	}

	[Fact]
	public void Seg_Addition_ReturnsExpectedResult()
	{
		var seg1 = new Seg(1.0, 2.0);
		var seg2 = new Seg(3.0, 4.0);
		var result = seg1 + seg2;
		Assert.Equal(4.0, result.Start);
		Assert.Equal(6.0, result.End);
	}

	[Fact]
	public void Seg_Equals_ReturnsTrueForEqualSegs()
	{
		var seg1 = new Seg(1.0, 2.0);
		var seg2 = new Seg(1.0, 2.0);
		Assert.True(seg1 == seg2);
	}

	[Fact]
	public void Seg_Equals_ReturnsFalseForNonEqualSegs()
	{
		var seg1 = new Seg(1.0, 2.0);
		var seg2 = new Seg(2.0, 3.0);
		Assert.False(seg1 == seg2);
	}

	[Fact]
	public void Seg_IsZero_ReturnsTrueForZeroSeg()
	{
		var seg = Seg.Zero;
		Assert.True(seg.IsZero);
	}

	[Fact]
	public void Seg_IsZero_ReturnsFalseForNonZeroSeg()
	{
		var seg = new Seg(1.0, 2.0);
		Assert.False(seg.IsZero);
	}

	[Fact]
	public void Seg_IsZeroLength_ReturnsTrueForZeroLengthSeg()
	{
		var seg = new Seg(1.0, 1.0);
		Assert.True(seg.IsZeroLength);
	}

	[Fact]
	public void Seg_IsZeroLength_ReturnsFalseForNonZeroLengthSeg()
	{
		var seg = new Seg(1.0, 2.0);
		Assert.False(seg.IsZeroLength);
	}
	[Fact]
	public void Addition_ReturnsExpectedResult()
	{
		var c = new Complex(4, 3) + new Complex(2, 1);
		var result = new Seg(3, 4) + new Seg(1, 2);
		Assert.Equal(c.Imaginary, result.Start, 3);
		Assert.Equal(c.Real, result.End, 3);
	}

	[Fact]
	public void Subtraction_ReturnsExpectedResult()
	{
		var c = new Complex(4, 3) - new Complex(2, 1);
		var result = new Seg(3, 4) - new Seg(1, 2);
		Assert.Equal(c.Imaginary, result.Start, 3);
		Assert.Equal(c.Real, result.End, 3);
	}

	[Fact]
	public void Multiplication_ReturnsExpectedResult()
	{
		var c = new Complex(2, 1) * new Complex(4, 3);
		var result = new Seg(1, 2) * new Seg(3, 4);
		Assert.Equal(c.Imaginary, result.Start, 3);
		Assert.Equal(c.Real, result.End, 3);
	}

	[Fact]
	public void Division_ReturnsExpectedResult()
	{
		var c = new Complex(2, 1) / new Complex(4, 3);
		var result = new Seg(1, 2) / new Seg(3, 4);
		Assert.Equal(c.Imaginary, result.Start, 3);
		Assert.Equal(c.Real, result.End, 3);
	}

	[Fact]
	public void Equality_ReturnsTrueForEqualSegs()
	{
		Assert.True(new Seg(1, 2) == new Seg(1, 2));
	}

	[Fact]
	public void Equality_ReturnsFalseForNonEqualSegs()
	{
		Assert.False(new Seg(1, 2) == new Seg(2, 3));
	}

	[Fact]
	public void Inequality_ReturnsTrueForNonEqualSegs()
	{
		Assert.True(new Seg(1, 2) != new Seg(2, 3));
	}

	[Fact]
	public void Inequality_ReturnsFalseForEqualSegs()
	{
		Assert.False(new Seg(1, 2) != new Seg(1, 2));
	}

	[Fact]
	public void IsZero_ReturnsTrueForZeroSeg()
	{
		Assert.True(Seg.Zero.IsZero);
	}

	[Fact]
	public void IsZero_ReturnsFalseForNonZeroSeg()
	{
		Assert.False(new Seg(1, 2).IsZero);
	}

	[Fact]
	public void IsZeroLength_ReturnsTrueForZeroLengthSeg()
	{
		Assert.True(new Seg(1, 1).IsZeroLength);
	}

	[Fact]
	public void IsZeroLength_ReturnsFalseForNonZeroLengthSeg()
	{
		Assert.False(new Seg(1, 2).IsZeroLength);
	}

	[Fact]
	public void IsForward_ReturnsTrueForForwardSeg()
	{
		Assert.True(new Seg(1, 2).IsForward);
	}

	[Fact]
	public void IsForward_ReturnsFalseForBackwardSeg()
	{
		Assert.False(new Seg(2, 1).IsForward);
	}

	[Fact]
	public void IsBackward_ReturnsTrueForBackwardSeg()
	{
		Assert.True(new Seg(2, 1).IsBackward);
	}

	[Fact]
	public void IsBackward_ReturnsFalseForForwardSeg()
	{
		Assert.False(new Seg(1, 2).IsBackward);
	}

	[Fact]
	public void IsPoint_ReturnsTrueForPointSeg()
	{
		Assert.True(new Seg(1, 1).IsPoint);
	}

	[Fact]
	public void IsPoint_ReturnsFalseForNonPointSeg()
	{
		Assert.False(new Seg(1, 2).IsPoint);
	}

	[Fact]
	public void IsWithin_ReturnsTrueForWithinSeg()
	{
		Assert.True(new Seg(2, 3).IsWithin(new Seg(1, 4)));
	}

	[Fact]
	public void IsWithin_ReturnsFalseForNotWithinSeg()
	{
		Assert.False(new Seg(1, 4).IsWithin(new Seg(2, 3)));
	}
	[Fact]
	public void IsBetween_ReturnsTrueForBetweenSeg()
	{
		Assert.True(new Seg(2, 3).IsBetween(new Seg(1, 4)));
	}

	[Fact]
	public void IsBetween_ReturnsFalseForNotBetweenSeg()
	{
		Assert.False(new Seg(1, 4).IsBetween(new Seg(2, 3)));
	}

	[Fact]
	public void IsBefore_ReturnsTrueForBeforeSeg()
	{
		Assert.True(new Seg(1, 2).IsBefore(new Seg(3, 4)));
	}

	[Fact]
	public void IsBefore_ReturnsFalseForNotBeforeSeg()
	{
		Assert.False(new Seg(3, 4).IsBefore(new Seg(1, 2)));
	}

	[Fact]
	public void IsAfter_ReturnsTrueForAfterSeg()
	{
		Assert.True(new Seg(3, 4).IsAfter(new Seg(1, 2)));
	}

	[Fact]
	public void IsAfter_ReturnsFalseForNotAfterSeg()
	{
		Assert.False(new Seg(1, 2).IsAfter(new Seg(3, 4)));
	}

	[Fact]
	public void CoversBeginningOf_ReturnsTrueForBeginningSeg()
	{
		Assert.True(new Seg(1, 2).CoversBeginningOf(new Seg(2, 3)));
	}

	[Fact]
	public void CoversBeginningOf_ReturnsFalseForNotBeginningSeg()
	{
		Assert.False(new Seg(2, 3).CoversBeginningOf(new Seg(1, 2)));
	}

	[Fact]
	public void CoversEndingOf_ReturnsTrueForEndingSeg()
	{
		Assert.True(new Seg(3, 4).CoversEndingOf(new Seg(2, 3)));
	}

	[Fact]
	public void IsEnding_ReturnsFalseForNotEndingSeg()
	{
		Assert.False(new Seg(2, 3).CoversEndingOf(new Seg(3, 4)));
	}

	[Fact]
	public void IsTouching_ReturnsTrueForTouchingSeg()
	{
		Assert.True(new Seg(2, 3).IsTouching(new Seg(3, 4)));
	}

	[Fact]
	public void IsTouching_ReturnsFalseForNotTouchingSeg()
	{
		Assert.False(new Seg(1, 2).IsTouching(new Seg(3, 4)));
	}

	[Fact]
	public void IsNotTouching_ReturnsTrueForNotTouchingSeg()
	{
		Assert.True(new Seg(1, 2).IsNotTouching(new Seg(3, 4)));
	}

	[Fact]
	public void IsNotTouching_ReturnsFalseForTouchingSeg()
	{
		Assert.False(new Seg(2, 3).IsNotTouching(new Seg(3, 4)));
	}

	[Fact]
	public void Conjugate_ReturnsExpectedResult()
	{
		var result = new Seg(1, 2).Conjugate();
		Assert.Equal(2, result.End);
		Assert.Equal(-1, result.Start);
	}

	[Fact]
	public void Reciprocal_ReturnsExpectedResultForNonZeroSeg()
	{
		var cr = Complex.Reciprocal(new Complex(2, 1));
		var result = new Seg(1, 2).Reciprocal();
		Assert.Equal(cr.Imaginary, result.Start, 3);
		Assert.Equal(cr.Real, result.End, 3);
	}

	[Fact]
	public void Reciprocal_ReturnsZeroForZeroSeg()
	{
		var result = Seg.Zero.Reciprocal();
		Assert.True(result.IsZero);
	}

	[Fact]
	public void Normalize_ReturnsExpectedResultForNonZeroSeg()
	{
		var result = new Seg(2, 4).Normalize();
		Assert.Equal(0.447, result.Start, 2);
		Assert.Equal(0.894, result.End, 2);
	}

	[Fact]
	public void Normalize_ReturnsUnitForZeroSeg()
	{
		var result = Seg.Zero.Normalize();
		Assert.Equal(Seg.Zero, result);
	}

	[Fact]
	public void Magnitude_ReturnsExpectedResult()
	{
		var cm = new Complex(20, 6).Magnitude;
		var m = new Seg(6, 20).Magnitude;
		Assert.Equal(cm, m, 4);
	}
	[Fact]
	public void NormalizeTo_ReturnsExpectedResult()
	{
		var cx = new Complex(20, 6);
		var cxnt = new Complex(3, 2);
		var m = cx.Magnitude;

		var result = new Seg(6, 20).NormalizeTo(new Seg(2, 3));
		Assert.Equal(2.777, result.Start, 2);
		Assert.Equal(2.298, result.End, 2);
	}

	[Fact]
	public void ClampInner_ReturnsExpectedResult()
	{
		var result = new Seg(1.5, 2.5).ClampInner();
		Assert.Equal(1, result.Start);
		Assert.Equal(2, result.End);
	}

	[Fact]
	public void ClampOuter_ReturnsExpectedResult()
	{
		var result = new Seg(1.5, 2.5).ClampOuter();
		Assert.Equal(2, result.Start);
		Assert.Equal(3, result.End);
	}

	[Fact]
	public void Round_ReturnsExpectedResult()
	{
		var result = new Seg(1.5, 2.5).Round();
		Assert.Equal(2, result.Start);
		Assert.Equal(3, result.End);
	}

}


