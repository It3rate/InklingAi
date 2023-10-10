using Nums.Primitives;

namespace NumbersTests
{
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
        private Focal _unitFocal;
	    private Focal _maxMin;
	    private Domain _domain;

	    public NumberTests()
        {
            _brain = Brain.ActiveBrain;
            _trait = Trait.CreateIn(_brain, "number tests");
			_unitFocal = new Focal(0, 10);
			_maxMin = new Focal(-1000, 1010);
		    _domain = new Domain(_trait, "testDomain", _unitFocal, _maxMin);
	    }

        public void Dispose()
        {
            _brain.ClearAll();
        }

	    [Fact]
	    public void UnitChangePositionTests()
	    {
		    var n0 = _domain.CreateNumber(0, 20);
		    var n1 = _domain.CreateNumber(20, 0);
		    var n2 = _domain.CreateNumber(-30, 20);
		    var n3 = _domain.CreateNumber(-20, -30);
            Assert.True(_unitFocal.IsUnitPerspective);
            Assert.Equal(0, n0.Value.Start); // [0, 10]
		    Assert.Equal(2, n0.Value.End);
		    Assert.Equal(2, n1.Value.Start);
		    Assert.Equal(0, n1.Value.End);
		    Assert.Equal(-3, n2.Value.Start);
		    Assert.Equal(2, n2.Value.End);
		    Assert.Equal(-2, n3.Value.Start);
		    Assert.Equal(-3, n3.Value.End);
		    _unitFocal.EndTickPosition = 20; // [0, 20]
            Assert.True(_unitFocal.IsUnitPerspective);
            Assert.Equal(0, n0.Value.Start);
		    Assert.Equal(1, n0.Value.End);
		    Assert.Equal(1, n1.Value.Start);
		    Assert.Equal(0, n1.Value.End);
		    Assert.Equal(-1.5, n2.Value.Start);
		    Assert.Equal(1, n2.Value.End);
		    Assert.Equal(-1, n3.Value.Start);
		    Assert.Equal(-1.5, n3.Value.End);
		    _unitFocal.EndTickPosition = -20; // [0, -20]
            Assert.True(_unitFocal.IsUnotPerspective);
            Assert.Equal(-1, n0.Value.Start);
		    Assert.Equal(0, n0.Value.End);
		    Assert.Equal(0, n1.Value.Start);
		    Assert.Equal(-1, n1.Value.End);
		    Assert.Equal(-1, n2.Value.Start);
		    Assert.Equal(1.5, n2.Value.End);
		    Assert.Equal(1.5, n3.Value.Start);
		    Assert.Equal(1, n3.Value.End);
		    _unitFocal.StartTickPosition = -10;  // [-10, -20]
            Assert.True(_unitFocal.IsUnotPerspective);
            Assert.Equal(-3, n0.Value.Start);
		    Assert.Equal(-1, n0.Value.End);
		    Assert.Equal(-1, n1.Value.Start);
		    Assert.Equal(-3, n1.Value.End);
		    Assert.Equal(-3, n2.Value.Start);
		    Assert.Equal(2, n2.Value.End);
		    Assert.Equal(2, n3.Value.Start);
		    Assert.Equal(1, n3.Value.End);
		    _unitFocal.StartTickPosition = 2000; // unot perspective
            _unitFocal.EndTickPosition = -2000; // forces things to about the middle
            Assert.Equal(0.495, n0.Value.Start);
            Assert.Equal(0.5, n0.Value.End, 3);
            Assert.Equal(0.5, n1.Value.Start, 3);
            Assert.Equal(0.495, n1.Value.End, 3);
            Assert.Equal(0.495, n2.Value.Start, 3);
            Assert.Equal(0.5075, n2.Value.End, 3);
            Assert.Equal(0.5075, n3.Value.Start, 3);
            Assert.Equal(0.505, n3.Value.End, 3);
        }
	    [Fact]
	    public void UnitChangeValueTests()
	    {
		    var n0 = _domain.CreateNumber(new Seg(0, 20)); // [0, 200]
		    var n1 = _domain.CreateNumber(new Seg(20, 0)); // [200, 0]
		    var n2 = _domain.CreateNumber(new Seg(-30, 20)); // [-300, 200]
		    var n3 = _domain.CreateNumber(new Seg(-20, -30)); // [-200, -300]
		    Assert.Equal(0, n0.Value.Start);
		    Assert.Equal(20, n0.Value.End);
		    Assert.Equal(20, n1.Value.Start);
		    Assert.Equal(0, n1.Value.End);
		    Assert.Equal(-30, n2.Value.Start);
		    Assert.Equal(20, n2.Value.End);
		    Assert.Equal(-20, n3.Value.Start); 
		    Assert.Equal(-30, n3.Value.End);
		    _unitFocal.StartTickPosition = 100; // unot perspective
		    _unitFocal.EndTickPosition = 0; 
		    Assert.Equal(-1, n0.Value.Start);
		    Assert.Equal(1, n0.Value.End);
		    Assert.Equal(1, n1.Value.Start);
		    Assert.Equal(-1, n1.Value.End);
		    Assert.Equal(-1, n2.Value.Start);
		    Assert.Equal(4, n2.Value.End);
		    Assert.Equal(4, n3.Value.Start);
		    Assert.Equal(3, n3.Value.End);
        }
        [Fact]
	    public void CoreNumberTests()
	    {
		    var f0 = new Focal(0, 20);
		    var n0 = _domain.CreateNumber(f0);
		    var f1 = new Focal(0, 30);
		    var n1 = _domain.CreateNumber(f1);
		    var f2 = new Focal(-32, 0);
		    var n2 = _domain.CreateNumber(f2);
		    var f3 = new Focal(-50, 45);
		    var n3 = _domain.CreateNumber(f3);
		    var f4 = new Focal(50, -45);
		    var n4 = _domain.CreateNumber(f4);
		    var f5 = new Focal(53, 69);
		    var n5 = _domain.CreateNumber(f5);

		    Assert.Equal(MathElementKind.Number, n0.Kind);
		    var n0b = n0.Clone();
		    Assert.Equal(n0b, n0);
		    n0b.Add(n3);
		    Assert.NotEqual(n0, n0b);

		    Assert.Equal(_unitFocal.Length, n0.BasisFocal.Length);
		    Assert.Equal(1, n0.Direction);
		    Assert.Equal(-1, n4.Direction);
		    Assert.Equal(n1.Domain.Id, n0.Domain.Id);
		    Assert.Equal(n0.Value.Start, n1.Value.Start, MathUtils.Tolerance);
		    Assert.Equal(n1.Value.End, n1.Focal.DirectedLength / (double) n1.BasisFocal.DirectedLength);

		    Assert.Equal(-3, n2.OuterRounding.Start, MathUtils.Tolerance);
		    Assert.Equal(0, n2.OuterRounding.End, MathUtils.Tolerance);
		    Assert.Equal(-5, n3.InnerRounding.Start, MathUtils.Tolerance);
		    Assert.Equal(4, n3.InnerRounding.End, MathUtils.Tolerance);
		    Assert.Equal(0.3, n5.RemainderRange.Start, MathUtils.Tolerance);
		    Assert.Equal(0.9, n5.RemainderRange.End, MathUtils.Tolerance);

		    Assert.True(n3.IsUnitPerspective);
		    Assert.True(_domain.BasisFocal.IsUnitPerspective);
		    Assert.True(n4.IsUnitPerspective);
		    Assert.False(n4.IsUnotPerspective);
		    Assert.Equal(0.472636, n3.RangeInMinMax.Start, MathUtils.Tolerance);
		    Assert.Equal(0.519900, n3.RangeInMinMax.End, MathUtils.Tolerance);

		    n1.Subtract(n2);
		    Assert.Equal(new Seg(3.2, 3), n1.Value);
		    n2.Add(n3);
		    Assert.Equal(new Seg(-5, 4.5), n3.Value);
		    n3.Multiply(n4);
		    Assert.Equal(new Seg(45, 4.8), n3.Value);
		    n4.Divide(n5);
		    Assert.Equal(new Seg(0.8, -0.1), n4.Value);
	    }
    }

}
