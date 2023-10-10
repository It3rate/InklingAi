namespace NumbersTests;

using System;
using System.Collections.Generic;
using Nums.Primitives;
using Nums.Utils;

public class DomainTests : IDisposable
{
	private Brain _brain;
	private Trait _trait;
    private Focal _unitFocal;
    private Focal _maxMin;
    private Domain _domain;

    public DomainTests()
    {
        _brain = Brain.ActiveBrain;
        _trait = Trait.CreateIn(_brain, "domain tests");
        _unitFocal = new Focal(-4, 6);
	        _maxMin = new Focal(-54, 46);
	        _domain = new Domain(_trait, _unitFocal, _maxMin);
    }

    public void Dispose()
    {
        _brain.ClearAll();
    }

    [Fact]
	    public void CoreDomainTests()
	    {
		    Assert.Equal(MathElementKind.Domain, _domain.Kind);
        Assert.Equal(100, _domain.MinMaxFocal.DirectedLength);
        Assert.Equal(10, _domain.BasisFocal.DirectedLength);
        Assert.Equal(_domain.BasisFocal.Id, _unitFocal.Id);
        Assert.Equal(_domain.MinMaxFocal.Id, _maxMin.Id);

        //var b = new Complex(6, 4);
        //var mm = new Complex(46, 54);
        //var p = b / mm;

			var len = _domain.MinMaxNum.Length;
        Assert.Equal(10.0, len, MathUtils.Tolerance);

        var f0 = new Focal(10, 20);
        var n0 = _domain.CreateNumber(f0);
        var n1 = _domain.CreateNumber(f0.Clone());
        var n2 = _domain.CreateNumber(f0.Clone());
        Assert.Equal(5, _domain.NumberStore.Count); // includes unit basis and minmax

        var dict = new Dictionary<int, Seg>();

        _domain.SaveNumberValues(dict, _domain.BasisNumber.Id);
        Assert.Equal(4, dict.Count); // does not include unit
        var saved = n1.Focal.EndTickPosition;
        n1.Focal.EndTickPosition = 22;
        Assert.NotEqual(n1.Focal.EndTickPosition, saved);
        Assert.Equal(22, n1.Focal.EndTickPosition);
        _domain.RestoreNumberValues(dict);
        Assert.Equal(saved, n1.Focal.EndTickPosition);

        Assert.True(_domain.IsUnitPerspective);
        _unitFocal.StartTickPosition = 16;
        Assert.False(_domain.IsUnitPerspective);

    }

	    [Fact]
	    public void DomainValueTests()
    {
            _unitFocal = new Focal(0, 10);

	        var num = _domain.CreateNumber(new Focal(30, 40));
	        var r = num.Value;
	        var ffr = _domain.CreateFocalFromRange(r);
        Assert.Equal(ffr, num.Focal);

        num = _domain.CreateNumber(new Focal( -30, 1));
        r = num.Value;
        ffr = _domain.CreateFocalFromRange(r);
        Assert.Equal(ffr, num.Focal);

     //   _unitFocal.Reset(10, -10);
	        //var testFocal = Focal.CreateByValues(0, 6);

     //   num = _domain.CreateNumber(Focal.CreateByValues(30, 40));
	        //r = num.Value;
	        //_domain.SetValueOf(testFocal, r);

     //   Assert.Equal(num.Focal.StartTickPosition, testFocal.StartTickPosition);
     //   Assert.Equal(num.Focal.EndTickPosition, testFocal.EndTickPosition);
     //   ffr = _domain.CreateFocalFromRange(r);
     //   Assert.Equal(ffr, num.Focal);
    }
}
