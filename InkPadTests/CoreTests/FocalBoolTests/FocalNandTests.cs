using Nums.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NumbersTests.CoreTests.FocalBoolTests
{
    
    public class FocalNandTests
    {
        [Fact]
        public void NoOverlapTest()
        {
            IFocal p = new Focal(10, 20);
            IFocal q = new Focal(30, 40);
            IFocal[] result = FocalBase.Nand(p, q);
            Assert.Single(result);
            Assert.Equal(Focal.MinMaxFocal, result[0]);
        }

        [Fact]
        public void OverlapTest()
        {
            IFocal p = new Focal(10, 20);
            IFocal q = new Focal(15, 25);
            IFocal[] result = FocalBase.Nand(p, q);
            Assert.Equal(2, result.Length);
            Assert.Contains( new Focal(long.MinValue, 15), result);
            Assert.Contains( new Focal(20, long.MaxValue), result);
        }
        [Fact]
        public void OverlapTestOrder1()
        {
            IFocal p = new Focal(15, 25);
            IFocal q = new Focal(10, 20);
            IFocal[] result = FocalBase.Nand(p, q);
            Assert.Equal(2, result.Length);
            Assert.Contains(new Focal(long.MinValue, 15), result);
            Assert.Contains(new Focal(20, long.MaxValue), result);
        }
        [Fact]
        public void OverlapTestOrder2()
        {
            IFocal p = new Focal(25, 15);
            IFocal q = new Focal(10, 20);
            IFocal[] result = FocalBase.Nand(p, q);
            Assert.Equal(2, result.Length);
            Assert.Contains(new Focal(long.MinValue, 15), result);
            Assert.Contains(new Focal(20, long.MaxValue), result);
        }
        [Fact]
        public void OverlapTestOrder3()
        {
            IFocal p = new Focal(25, 15);
            IFocal q = new Focal(20, 10);
            IFocal[] result = FocalBase.Nand(p, q);
            Assert.Equal(2, result.Length);
            Assert.Contains(new Focal(long.MinValue, 15), result);
            Assert.Contains(new Focal(20, long.MaxValue), result);
        }
        [Fact]
        public void OverlapTestDirection()
        {
            IFocal p = new Focal(15, 25);
            IFocal q = new Focal(20, 10);
            IFocal[] result = FocalBase.Nand(p, q);
            Assert.Equal(2, result.Length);
            Assert.Contains(new Focal(long.MinValue, 15), result);
            Assert.Contains(new Focal(20, long.MaxValue), result);
        }

        [Fact]
        public void qBeforePTest()
        {
            IFocal p = new Focal(20, 30);
            IFocal q = new Focal(10, 15);
            IFocal[] result = FocalBase.Nand(p, q);
            Assert.Single(result);
            Assert.Equal(Focal.MinMaxFocal, result[0]);
        }
    }

}
