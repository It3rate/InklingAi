namespace Nums.Composites;

using Nums.Primitives;
using Nums.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class PolyFocal : Focal
{
    public long[] Positions {
        get => _positions;
        set
        {
            if (value.Length < 2) throw new ArgumentException("Polyfocal must have at least two points.");
            _positions = value;
        }
    }

    public PolyFocal(params long[] positions)
    {
        _positions = (long[])positions.Clone();
    }


    public override long DirectedLength => Positions.Select(x => x).Sum();
    public override ulong Length => (ulong)_positions.Zip(_positions.Skip(1), (a, b) => Math.Abs(a - b)).Sum();
    

    // these work with segs, so needs a polyseg, or maybe just array of segs?
    //Seg Seg { get; set; } // 
    //Seg ShiftedSeg(long value);
    //Seg GetSegWithBasis(IFocal basis);
    //void SetTicksWithSegBasis(Seg seg, IFocal basis);
    //void StretchBy(IFocal amount);
    //public void SqueezeBy(IFocal amount);
    //Seg UnitTRangeIn(IFocal basis);



    public override IFocal Clone()
    {
        return new PolyFocal((long[])Positions.Clone());
    }

}
