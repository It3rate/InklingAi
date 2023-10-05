using Nums.Primitives;

namespace NumbersTests
{
    using Nums.RefPrimitives;
    using Nums.Utils;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;


    public class NumberTests : IDisposable
    {
	    private Brain _brain;
	    private Trait _trait;
        private FocalRef _unitFocal;
	    private FocalRef _maxMin;
	    private Domain _domain;

	    public NumberTests()
        {
            _brain = Brain.ActiveBrain;
            _trait = Trait.CreateIn(_brain, "number tests");
            _unitFocal = FocalRef.CreateByValues(_trait, 0, 10);
		    _maxMin = FocalRef.CreateByValues(_trait, -1000, 1010);
		    _domain = new Domain(_trait, _unitFocal, _maxMin);
	    }

        public void Dispose()
        {
            _brain.ClearAll();
        }

	    [Fact]
	    public void UnitChangePositionTests()
	    {
		    var n0 = _domain.CreateNumber(0, 20, true);
		    var n1 = _domain.CreateNumber(20, 0, true);
		    var n2 = _domain.CreateNumber(-30, 20, true);
		    var n3 = _domain.CreateNumber(-20, -30, true);
		    Assert.Equal(0, n0.StartValue);
		    Assert.Equal(2, n0.EndValue);
		    Assert.Equal(-2, n1.StartValue);
		    Assert.Equal(0, n1.EndValue);
		    Assert.Equal(3, n2.StartValue);
		    Assert.Equal(2, n2.EndValue);
		    Assert.Equal(2, n3.StartValue);
		    Assert.Equal(-3, n3.EndValue);
		    _unitFocal.EndTickPosition = 20;
		    Assert.Equal(0, n0.StartValue);
		    Assert.Equal(1, n0.EndValue);
		    Assert.Equal(-1, n1.StartValue);
		    Assert.Equal(0, n1.EndValue);
		    Assert.Equal(1.5, n2.StartValue);
		    Assert.Equal(1, n2.EndValue);
		    Assert.Equal(1, n3.StartValue);
		    Assert.Equal(-1.5, n3.EndValue);
		    _unitFocal.EndTickPosition = -20;
		    Assert.Equal(1, n0.StartValue);
		    Assert.Equal(0, n0.EndValue);
		    Assert.Equal(0, n1.StartValue);
		    Assert.Equal(-1, n1.EndValue);
		    Assert.Equal(1, n2.StartValue);
		    Assert.Equal(1.5, n2.EndValue);
		    Assert.Equal(-1.5, n3.StartValue);
		    Assert.Equal(1, n3.EndValue);
		    _unitFocal.StartTickPosition = -10; // unot perspective
		    Assert.Equal(3, n0.StartValue);
		    Assert.Equal(-1, n0.EndValue);
		    Assert.Equal(1, n1.StartValue);
		    Assert.Equal(-3, n1.EndValue);
		    Assert.Equal(3, n2.StartValue);
		    Assert.Equal(2, n2.EndValue);
		    Assert.Equal(-2, n3.StartValue);
		    Assert.Equal(1, n3.EndValue);
		    _unitFocal.StartTickPosition = 2000; // unot perspective
            _unitFocal.EndTickPosition = -2000; // forces things to about the middle
            Assert.Equal(-0.495, n0.StartValue);
            Assert.Equal(0.5, n0.EndValue);
            Assert.Equal(-0.5, n1.StartValue);
            Assert.Equal(0.495, n1.EndValue);
            Assert.Equal(-0.495, n2.StartValue);
            Assert.Equal(0.5075, n2.EndValue);
            Assert.Equal(-0.5075, n3.StartValue);
            Assert.Equal(0.505, n3.EndValue);
        }
	    [Fact]
	    public void UnitChangeValueTests()
	    {
		    var n0 = _domain.CreateNumber(new NumRange(0, 20), true);
		    var n1 = _domain.CreateNumber(new NumRange(20, 0), true);
		    var n2 = _domain.CreateNumber(new NumRange(-30, 20), true);
		    var n3 = _domain.CreateNumber(new NumRange(-20, -30), true);
		    Assert.Equal(0, n0.StartValue);
		    Assert.Equal(20, n0.EndValue);
		    Assert.Equal(20, n1.StartValue);
		    Assert.Equal(0, n1.EndValue);
		    Assert.Equal(-30, n2.StartValue);
		    Assert.Equal(20, n2.EndValue);
		    Assert.Equal(-20, n3.StartValue); 
		    Assert.Equal(-30, n3.EndValue);
		    _unitFocal.StartTickPosition = 100; // unot perspective
		    _unitFocal.EndTickPosition = 0; 
		    Assert.Equal(1, n0.StartValue);
		    Assert.Equal(1, n0.EndValue);
		    Assert.Equal(-1, n1.StartValue);
		    Assert.Equal(3, n1.EndValue);
		    Assert.Equal(1, n2.StartValue);
		    Assert.Equal(-2, n2.EndValue);
		    Assert.Equal(-4, n3.StartValue);
		    Assert.Equal(-1, n3.EndValue);
        }

        [Fact]
	    public void CoreNumberTests()
	    {
		    var f0 = FocalRef.CreateByValues(_trait, 0, 20);
		    var n0 = _domain.CreateNumber(f0);
		    var f1 = FocalRef.CreateByValues(_trait, 0, 30);
		    var n1 = _domain.CreateNumber(f1);
		    var f2 = FocalRef.CreateByValues(_trait, -32, 0);
		    var n2 = _domain.CreateNumber(f2);
		    var f3 = FocalRef.CreateByValues(_trait, -50, 45);
		    var n3 = _domain.CreateNumber(f3);
		    var f4 = FocalRef.CreateByValues(_trait, 50, -45);
		    var n4 = _domain.CreateNumber(f4);
		    var f5 = FocalRef.CreateByValues(_trait, 53, 69);
		    var n5 = _domain.CreateNumber(f5);

		    Assert.Equal(MathElementKind.Number, n0.Kind);
		    var n0b = n0.Clone();
		    Assert.Equal(n0b, n0);
		    n0b.Add(n3);
		    Assert.NotEqual(n0, n0b);

		    Assert.Equal(_unitFocal.AbsLengthInTicks, n0.AbsBasisTicks);
		    Assert.Equal(1, n0.Direction);
		    Assert.Equal(-1, n4.Direction);
		    Assert.Equal(n1.DomainId, n0.Domain.Id);
		    Assert.Equal(n0.StartValue, n1.StartValue, MathUtils.Tolerance);
		    Assert.Equal(n1.EndValue, n1.Focal.LengthInTicks / (double) n1.BasisTicks);
		    Assert.Equal(0, n3.RemainderStartValue, MathUtils.Tolerance);
		    Assert.Equal(5, n3.RemainderEndValue, MathUtils.Tolerance);
		    Assert.Equal(2, n2.RemainderStartValue, MathUtils.Tolerance);
		    Assert.Equal(0, n2.RemainderEndValue, MathUtils.Tolerance);

		    Assert.Equal(3, n2.CeilingRange.Start, MathUtils.Tolerance);
		    Assert.Equal(0, n2.CeilingRange.End, MathUtils.Tolerance);
		    Assert.Equal(5, n3.FloorRange.Start, MathUtils.Tolerance);
		    Assert.Equal(4, n3.FloorRange.End, MathUtils.Tolerance);
		    Assert.Equal(-5, n5.RoundedRange.Start, MathUtils.Tolerance);
		    Assert.Equal(7, n5.RoundedRange.End, MathUtils.Tolerance);
		    Assert.Equal(-0.3, n5.RemainderRange.Start, MathUtils.Tolerance);
		    Assert.Equal(0.9, n5.RemainderRange.End, MathUtils.Tolerance);

		    Assert.True(n3.IsUnitPerspective);
		    Assert.True(_domain.BasisNumber.IsUnitPerspective);
		    Assert.True(n4.IsUnitPerspective);
		    Assert.False(n4.IsUnotPerspective);
		    Assert.Equal(0.472636, n3.RangeInMinMax.Start, MathUtils.Tolerance);
		    Assert.Equal(0.519900, n3.RangeInMinMax.End, MathUtils.Tolerance);

		    n1.Subtract(n2);
		    Assert.Equal(new NumRange(-3.2, 3), n1.Value);
		    n2.Add(n3);
		    Assert.Equal(new NumRange(5, 4.5), n3.Value);
		    n3.Multiply(n4);
		    Assert.Equal(new NumRange(-45, 4.8), n3.Value);
		    n4.Divide(n5);
		    Assert.Equal(new NumRange(-0.8, -0.1), n4.Value);
	    }
    }

}
