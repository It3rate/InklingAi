namespace Nums.Composites;

using Nums.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class PolyFocal : FocalBase
{
    public static Focal Empty { get; } = new Focal(0, 0);

    public override long StartTickPosition
    {
        get => _positions[0];
        set => _positions[0] = value;
    }
    public override long EndTickPosition
    {
        get => _positions[_positions.Length - 1];
        set => _positions[_positions.Length - 1] = value;
    }

    private long[] _positions;
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
        _positions = positions;
    }

    public override IFocal Clone()
    {
        return new PolyFocal(Positions);
    }

}
