namespace Nums.Primitives;

using Nums.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class PolyFocal : Focal
{
    public long[] Positions
    {
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

    public override void StretchBy(IFocal amount)
    {
        var seg = amount.Seg;
        for (var i = 0; i < _positions.Length; i++)
        {
            var pseg = new Seg(0, _positions[i]);
            _positions[i] = (long)(pseg * seg).End;
        }
    }

    public override void SqueezeBy(IFocal amount)
    {
        var seg = amount.Seg;
        for (var i = 0; i < _positions.Length; i++)
        {
            var pseg = new Seg(0, _positions[i]);
            _positions[i] = (long)(pseg / seg).End;
        }
    }

    // these work with segs, so needs a polyseg, or maybe just array of segs?
    public override Seg Seg
    {
        get { throw new NotImplementedException(); }
        set { throw new NotImplementedException(); }
    }
    public override Seg ShiftedSeg(long value)
    {
        throw new NotImplementedException();
    }
    public override Seg GetSegWithBasis(IFocal basis)
    {
        throw new NotImplementedException();
    }
    public override void SetTicksWithSegBasis(Seg seg, IFocal basis)
    {
        throw new NotImplementedException();
    }
    public override Seg UnitTRangeIn(IFocal basis)
    {
        throw new NotImplementedException();
    }



    public override IFocal Clone()
    {
        return new PolyFocal((long[])Positions.Clone());
    }

}
