using System;
using Nums.Primitives;
using Nums.RefPrimitives;
using Nums.Utils;

namespace NumbersTests
{

    public class FocalTests : IDisposable
    {
	    private Brain _brain;
        private Trait _trait;

	    public FocalTests()
        {
            _brain = Brain.ActiveBrain;
            _trait = Trait.CreateIn(_brain, "numbers tests");
        }

        public void Dispose()
        {
            _brain.ClearAll();
        }

        [Fact]
	    public void BasisSignTests()
	    {
		    IFocal fb = FocalRef.CreateByValues(_trait, 0, 10);
		    IFocal fn = FocalRef.CreateByValues(_trait, 20, 30);
		    IFocal ftest = FocalRef.CreateByValues(_trait, 0, 0);

		    // focal [0,10]  fn [20, 30]
            Assert.Equal(new NumRange(-2, 3), fn.GetRangeWithBasis(fb, false));
            ftest.SetWithRangeAndBasis(new NumRange(-2, 3), fb, false);
            Assert.Equal(ftest.StartTickPosition, fn.StartTickPosition);
            Assert.Equal(ftest.EndTickPosition, fn.EndTickPosition);

            Assert.Equal(new NumRange(-20, 30), fn.GetRangeWithBasis(fb, true));
            ftest.SetWithRangeAndBasis(new NumRange(-20, 30), fb, true);
            Assert.Equal(ftest.StartTickPosition, fn.StartTickPosition);
            Assert.Equal(ftest.EndTickPosition, fn.EndTickPosition);


            fb.EndTickPosition = -10; // focal [0,-10]  fn [40, 30]

            Assert.Equal(new NumRange(3, -2), fn.GetRangeWithBasis(fb, false));
		    ftest.SetWithRangeAndBasis(new NumRange(3, -2), fb, false);
		    Assert.Equal(ftest.StartTickPosition, fn.StartTickPosition);
		    Assert.Equal(ftest.EndTickPosition, fn.EndTickPosition);

		    Assert.Equal(new NumRange(30, -20), fn.GetRangeWithBasis(fb, true));
		    ftest.SetWithRangeAndBasis(new NumRange(30, -20), fb, true);
		    Assert.Equal(ftest.StartTickPosition, fn.StartTickPosition);
		    Assert.Equal(ftest.EndTickPosition, fn.EndTickPosition);


            fn.StartTickPosition = 40; // focal [0,-10],  fn [40, 30]

            Assert.Equal(new NumRange(3, -4), fn.GetRangeWithBasis(fb, false));
            ftest.SetWithRangeAndBasis(new NumRange(3, -4), fb, false);
            Assert.Equal(ftest.StartTickPosition, fn.StartTickPosition);
            Assert.Equal(ftest.EndTickPosition, fn.EndTickPosition);

            Assert.Equal(new NumRange(30, -40), fn.GetRangeWithBasis(fb, true));
            ftest.SetWithRangeAndBasis(new NumRange(30, -40), fb, true);
            Assert.Equal(ftest.StartTickPosition, fn.StartTickPosition);
            Assert.Equal(ftest.EndTickPosition, fn.EndTickPosition);


            fb.EndTickPosition = 10; // focal [0,10],  fn [40, 30]

            Assert.Equal(new NumRange(-4, 3), fn.GetRangeWithBasis(fb, false));
            ftest.SetWithRangeAndBasis(new NumRange(-4, 3), fb, false);
            Assert.Equal(ftest.StartTickPosition, fn.StartTickPosition);
            Assert.Equal(ftest.EndTickPosition, fn.EndTickPosition);

            Assert.Equal(new NumRange(-40, 30), fn.GetRangeWithBasis(fb, true));
            ftest.SetWithRangeAndBasis(new NumRange(-40, 30), fb, true);
            Assert.Equal(ftest.StartTickPosition, fn.StartTickPosition);
            Assert.Equal(ftest.EndTickPosition, fn.EndTickPosition);
        }
	    [Fact]
	    public void ReciprocalTests()
	    {
		    IFocal f0 = FocalRef.CreateByValues(_trait, 0, 10);
		    IFocal f1 = FocalRef.CreateByValues(_trait, 132, 287);
		    Assert.Equal(-13.2, f1.GetRangeWithBasis(f0, false).Start);
		    Assert.Equal(28.7, f1.GetRangeWithBasis(f0, false).End);
		    Assert.Equal(-130, f1.GetRangeWithBasis(f0, true).Start);
		    Assert.Equal(290, f1.GetRangeWithBasis(f0, true).End);
	    }

        [Fact]
	    public void RangeToUnitTests()
	    {
		    IFocal fb = FocalRef.CreateByValues(_trait, 0, 10);
		    IFocal fn = FocalRef.CreateByValues(_trait, 20, 30);
		    IFocal testFocal = FocalRef.CreateByValues(_trait, 0, 0);
		    var range = fn.GetRangeWithBasis(fb, false);
            testFocal.SetWithRangeAndBasis(range, fb, false);
            Assert.Equal(testFocal.StartTickPosition, fn.StartTickPosition);
            Assert.Equal(testFocal.EndTickPosition, fn.EndTickPosition);

        }

	    [Fact]
        public void CoreFocalTests()
        {
            IFocal f = FocalRef.CreateByValues(_trait, 150, 250);
            Assert.Equal(MathElementKind.Focal, f.Kind);
            Assert.Equal(150, f.StartTickPosition);
            Assert.Equal(250, f.EndTickPosition);
            Assert.Equal(100, f.LengthInTicks);
            Assert.Equal(1, f.Direction);
            Assert.Equal(100, f.NonZeroLength);
            Assert.Equal(100, f.AbsLengthInTicks);
            var f2 = f.Clone();
            Assert.NotEqual(f.Id, f2.Id);
            Assert.Equal(f2.StartTickPosition, f.StartTickPosition);
            Assert.Equal(f.EndTickPosition, f2.EndTickPosition);

            Assert.Equal(new NumRange(0, 1), f2.GetRangeWithBasis(f, false));

            f2.StartTickPosition = 450;
            Assert.Equal(450, f2.StartTickPosition);
            Assert.Equal(250, f2.EndTickPosition);
            Assert.Equal(-200, f2.LengthInTicks);
            Assert.Equal(-1, f2.Direction);
            Assert.Equal(-200, f2.NonZeroLength);
            Assert.Equal(200, f2.AbsLengthInTicks);

            // f:[150,250] f2[450,250]
            Assert.Equal(new NumRange(-3, 1), f2.GetRangeWithBasis(f, false));
            Assert.Equal(new NumRange(-3, 1), f.RangeAsBasis(f2));
            Assert.Equal(new NumRange(-1, 1.5), f2.RangeAsBasis(f));
            Assert.Equal(new NumRange(-1, 1.5), f.GetRangeWithBasis(f2, false));
            // f:[150,250] f2[500,250]
            f2.StartTickPosition = 500;
            Assert.Equal(new NumRange(-3.5, 1), f2.GetRangeWithBasis(f, false));
            Assert.Equal(new NumRange(-3.5, 1), f.RangeAsBasis(f2));
            Assert.Equal(new NumRange(-1, 1.4), f2.RangeAsBasis(f));
            Assert.Equal(new NumRange(-1, 1.4), f.GetRangeWithBasis(f2, false));

            // f:[150,250] f2[-150,250]
            f2.StartTickPosition = -150;
            Assert.Equal(new NumRange(3, 1), f2.GetRangeWithBasis(f, false));
            Assert.Equal(new NumRange(-0.75, 1), f.GetRangeWithBasis(f2, false));

            // f:[150,250] f2[-150,-150]
            f2.EndTickPosition = -150;
            Assert.Equal(0, f2.LengthInTicks);
            Assert.Equal(1, f2.NonZeroLength);
            Assert.Equal(0, f2.AbsLengthInTicks);

        }
    }
}
