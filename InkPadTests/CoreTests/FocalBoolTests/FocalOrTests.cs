using Nums.Primitives;

namespace NumbersTests.CoreTests.FocalBoolTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    
    public class FocalOrTests
    {
        [Fact]
        public void NoOverlapTest()
        {
            IFocal p = new Focal(10, 20);
            IFocal q = new Focal(30, 40);
            IFocal[] result = Focal.Or(p, q);
            Assert.Equal(2, result.Length);
            Assert.Equal(p, result[0]);
            Assert.Equal(q, result[1]);
        }

        [Fact]
        public void OverlapTest()
        {
            IFocal p = new Focal(10, 20);
            IFocal q = new Focal(15, 25);
            IFocal[] result = Focal.Or(p, q);
            Assert.Equal(1, result.Length);
            Assert.Contains(new Focal(10, 25), result);
        }
        [Fact]
        public void OverlapTestOrder1()
        {
            IFocal p = new Focal(15, 25);
            IFocal q = new Focal(10, 20);
            IFocal[] result = Focal.Or(p, q);
            Assert.Equal(1, result.Length);
            Assert.Contains(new Focal(10, 25), result);
        }
        [Fact]
        public void OverlapTestOrder2()
        {
            IFocal p = new Focal(25, 15);
            IFocal q = new Focal(10, 20);
            IFocal[] result = Focal.Or(p, q);
            Assert.Equal(1, result.Length);
            Assert.Contains(new Focal(10, 25), result);
        }
        [Fact]
        public void OverlapTestOrder3()
        {
            IFocal p = new Focal(25, 15);
            IFocal q = new Focal(20, 10);
            IFocal[] result = Focal.Or(p, q);
            Assert.Equal(1, result.Length);
            Assert.Contains(new Focal(10, 25), result);
        }
        [Fact]
        public void OverlapTestDirection()
        {
            IFocal p = new Focal(15, 25);
            IFocal q = new Focal(20, 10);
            IFocal[] result = Focal.Or(p, q);
            Assert.Equal(1, result.Length);
            Assert.Contains(new Focal(10, 25), result);
        }

    }

}
