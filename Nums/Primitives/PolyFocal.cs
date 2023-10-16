namespace Nums.Primitives;

using Nums.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// A series of ordered positions on a single line, the end point of one focal is the start point of the next one.
/// </summary>
public class PolyFocal : Focal
{
    public long[] Positions
    {
        get => _positions.ToArray();
        set
        {
            if (value.Length < 2) throw new ArgumentException("Polyfocal must have at least two points.");
            _positions.Clear();
            _positions.AddRange(value);
        }
    }
    public int Count => _positions.Count;

    public PolyFocal(params long[] positions)
    {
        _positions.AddRange(positions);
    }

    public void AddPosition(long position)
    {
        _positions.Add(position);
    }
    public void AddPositions(long[] positions)
    {
        _positions.AddRange(positions);
    }
    public void RemovePosition()
    {
        if (_positions.Count > 2)
        {
            _positions.RemoveAt(_positions.Count - 1);
        }
    }
    public void Clear()
    {
        _positions.Clear();
    }

	public IFocal FocalAt(int index)
	{
		return (index >= 0 && index < Count - 1) ? new Focal(_positions[index], _positions[index + 1]) : Focal.InvalidFocal;
	}
	public IFocal PositionAt(int index)
	{
		return (index >= 0 && index < Count) ? new Focal(_positions[index], _positions[index]) : Focal.InvalidFocal;
	}

	public override long DirectedLength => Positions.Select(x => x).Sum();
    public override ulong Length => (ulong)_positions.Zip(_positions.Skip(1), (a, b) => Math.Abs(a - b)).Sum();

    public override void StretchBy(IFocal amount)
    {
        var seg = amount.Seg;
        for (var i = 0; i < _positions.Count; i++)
        {
            var pseg = new Seg(0, _positions[i]);
            _positions[i] = (long)(pseg * seg).End;
        }
    }

    public override void SqueezeBy(IFocal amount)
    {
        var seg = amount.Seg;
        for (var i = 0; i < _positions.Count; i++)
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
