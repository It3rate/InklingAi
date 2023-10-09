using System;
using Nums.Primitives;
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
		    IFocal fb = Focal.CreateByValues( 0, 10);
		    IFocal fn = Focal.CreateByValues(20, 30);
		    IFocal ftest = Focal.CreateByValues(0, 0);

		    // focal [0,10]  fn [20, 30]
            Assert.Equal(new Seg(2, 3), fn.GetSegBasis(fb));
            ftest.SetSegBasis(new Seg(2, 3), fb);
            Assert.Equal(ftest.StartTickPosition, fn.StartTickPosition);
            Assert.Equal(ftest.EndTickPosition, fn.EndTickPosition);

            //Assert.Equal(new Seg(-20, 30), fn.GetSegBasis(fb));
            //ftest.SetSegBasis(new Seg(-20, 30), fb);
            //Assert.Equal(ftest.StartTickPosition, fn.StartTickPosition);
            //Assert.Equal(ftest.EndTickPosition, fn.EndTickPosition);


            fb.EndTickPosition = -10; // focal [0,-10]  fn [40, 30]

            Assert.Equal(new Seg(-3, -2), fn.GetSegBasis(fb));
		    ftest.SetSegBasis(new Seg(-3, -2), fb);
		    Assert.Equal(ftest.StartTickPosition, fn.StartTickPosition);
		    Assert.Equal(ftest.EndTickPosition, fn.EndTickPosition);

			//Assert.Equal(new Seg(30, -20), fn.GetSegWithBasis(fb, true));
			//ftest.SetSegWithBasis(new Seg(30, -20), fb, true);
			//Assert.Equal(ftest.StartTickPosition, fn.StartTickPosition);
			//Assert.Equal(ftest.EndTickPosition, fn.EndTickPosition);

			fb = new Focal(0, -10);
			fn = new Focal(40, 30);

			Assert.Equal(new Seg(-3, -4), fn.GetSegBasis(fb));
            ftest.SetSegBasis(new Seg(-3, -4), fb);
            Assert.Equal(40, fn.StartTickPosition);
            Assert.Equal(30, fn.EndTickPosition);

			//Assert.Equal(new Seg(30, -40), fn.GetSegWithBasis(fb, true));
			//ftest.SetSegWithBasis(new Seg(30, -40), fb, true);
			//Assert.Equal(ftest.StartTickPosition, fn.StartTickPosition);
			//Assert.Equal(ftest.EndTickPosition, fn.EndTickPosition);


			fb = new Focal(0, 10);
			fn = new Focal(40, 30);

			Assert.Equal(new Seg(4, 3), fn.GetSegBasis(fb));
            ftest.SetSegBasis(new Seg(4, 3), fb);
            Assert.Equal(40, fn.StartTickPosition);
            Assert.Equal(30, fn.EndTickPosition);

            //Assert.Equal(new Seg(-40, 30), fn.GetSegWithBasis(fb, true));
            //ftest.SetSegWithBasis(new Seg(-40, 30), fb, true);
            //Assert.Equal(ftest.StartTickPosition, fn.StartTickPosition);
            //Assert.Equal(ftest.EndTickPosition, fn.EndTickPosition);
        }
	    [Fact]
	    public void ReciprocalTests()
	    {
		    IFocal f0 = Focal.CreateByValues(0, 10);
		    IFocal f1 = Focal.CreateByValues(132, 287);
		    Assert.Equal(13.2, f1.GetSegBasis(f0).Start);
		    Assert.Equal(28.7, f1.GetSegBasis(f0).End);

			Assert.Equal(0.02875, f0.GetSegBasis(f1).Start, 3);
			Assert.Equal(-0.0132, f0.GetSegBasis(f1).End, 3);
		}

        [Fact]
	    public void RangeToUnitTests()
	    {
		    IFocal fb = Focal.CreateByValues(0, 10);
		    IFocal fn = Focal.CreateByValues(20, 30);
		    IFocal testFocal = Focal.CreateByValues(0, 0);
		    var range = fn.GetSegBasis(fb);
            testFocal.SetSegBasis(range, fb);
            Assert.Equal(testFocal.StartTickPosition, fn.StartTickPosition);
            Assert.Equal(testFocal.EndTickPosition, fn.EndTickPosition);

        }

	    [Fact]
        public void CoreFocalTests()
        {
            IFocal f = new Focal(150, 250);
            Assert.Equal(MathElementKind.Focal, f.Kind);
            Assert.Equal(150, f.StartTickPosition);
            Assert.Equal(250, f.EndTickPosition);
            Assert.Equal(100, f.DirectedLength);
            Assert.Equal(1, f.Direction);
            Assert.Equal(100, f.NonZeroLength);
            Assert.Equal(100, f.Length);
            var f2 = f.Clone();
            Assert.NotEqual(f.Id, f2.Id);
            Assert.Equal(f2.StartTickPosition, f.StartTickPosition);
            Assert.Equal(f.EndTickPosition, f2.EndTickPosition);

            Assert.Equal(new Seg(0, 1), f2.GetSegBasis(f));

            f2.StartTickPosition = 450;
            Assert.Equal(450, f2.StartTickPosition);
            Assert.Equal(250, f2.EndTickPosition);
            Assert.Equal(-200, f2.DirectedLength);
            Assert.Equal(-1, f2.Direction);
            Assert.Equal(-200, f2.NonZeroLength);
            Assert.Equal(200, f2.Length);

            // f:[150,250] f2[450,250]
            Assert.Equal(new Seg(-3, 1), f2.GetSegBasis(f));
            //Assert.Equal(new Seg(-3, 1), f.GetSegBasis(f2));
            //Assert.Equal(new Seg(-1, 1.5), f2.GetSegBasis(f));
            //Assert.Equal(new Seg(-1, 1.5), f.GetSegBasis(f2));
            // f:[150,250] f2[500,250]
            f2.StartTickPosition = 500;
            Assert.Equal(new Seg(-3.5, 1), f2.GetSegBasis(f));
            //Assert.Equal(new Seg(-3.5, 1), f.GetSegBasis(f2));
            //Assert.Equal(new Seg(-1, 1.4), f2.GetSegBasis(f));
            Assert.Equal(new Seg(-1, 1.4), f.GetSegBasis(f2));

            // f:[150,250] f2[-150,250]
            f2.StartTickPosition = -150;
            Assert.Equal(new Seg(3, 1), f2.GetSegBasis(f));
            Assert.Equal(new Seg(-0.75, 1), f.GetSegBasis(f2));

            // f:[150,250] f2[-150,-150]
            f2.EndTickPosition = -150;
            Assert.Equal(0, f2.DirectedLength);
            Assert.Equal(1, f2.NonZeroLength);
            Assert.Equal(0, f2.Length);

        }
    }
}
