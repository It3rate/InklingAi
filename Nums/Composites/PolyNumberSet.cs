namespace Nums.Composites;

using Nums.Primitives;
using System;
using System.Drawing;
using System.Linq;

public class PolyNumberSet
{
	private PolyNumber[] _polynumbers;

	// convinience accessors
	public PolyNumber XValues => _polynumbers[0];
	public PolyNumber YValues => _polynumbers.Length > 0 ? _polynumbers[1] : _polynumbers.Last();
	public PolyNumber ZValues => _polynumbers.Length > 1 ? _polynumbers[2] : _polynumbers.Last();

	public PolyNumber RValues => _polynumbers[0];
	public PolyNumber GValues => _polynumbers.Length > 0 ? _polynumbers[1] : _polynumbers.Last();
	public PolyNumber BValues => _polynumbers.Length > 1 ? _polynumbers[2] : _polynumbers.Last();
	public PolyNumber AValues => _polynumbers.Length > 2 ? _polynumbers[3] : _polynumbers.Last();

	public PolyNumber PolyNumberAt(int index) => _polynumbers.Length > index ? _polynumbers[index] : _polynumbers.Last();

	public int Count => _polynumbers[0].Count;
	public int Dimensions => _polynumbers.Length;

	public PolyNumberSet(params PolyNumber[] values)
	{
        // todo: allow empty polysets for init objects
		if (values.Length < 1 || !values.All(p => p.Count == values[0].Count))
		{
			throw new ArgumentException("Values count must be greater than one and have the same count.");
		}
		_polynumbers = (PolyNumber[])values.Clone();
    }

    public void AddValue(params double[] values)
    {
        if (values.Length == Dimensions)
        {
            for (int i = 0; i < Dimensions; i++)
            {
                _polynumbers[i].AddValue(values[i]);
            }
        }
        else
        {
            throw new ArgumentException("Values count must match underlying Polynumber array.");
        }
    }
    public void AddValues(double[] xValues, double[] yValues) // todo: use a 'copyInto' type method to ensure dimensionality.
    {
        for (int i = 0; i < xValues.Length; i++)
        {
            _polynumbers[0].AddValue(xValues[i]);
            _polynumbers[1].AddValue(yValues[i]);
        }
    }
    public void RemoveLastValue()
    {
        if (Count > 0)
        {
            for (int i = 0; i < Dimensions; i++)
            {
                _polynumbers[i].RemoveLastPosition();
            }
        }
        else
        {
            throw new ArgumentException("PolyNumber must have values in order to remove them.");
        }
    }
    public double[][] GetIndexedValues()
    {
        var len = Count;
        var result = new double[len][];
        for (int i = 0; i < len; i++)
        {
            result[i] = new double[Dimensions];
            for (int j = 0; j < Dimensions; j++)
            {
                result[i][j] = _polynumbers[j].ValueAtPosition(i);
            }
        }
        return result;
    }
    public bool HasNonZeroContent()
    {
        var result = false;
        var len = Count;
        for (int i = 0; i < len; i++)
        {
            for (int j = 0; j < Dimensions; j++)
            {
                if(_polynumbers[j].ValueAtPosition(i) != 0)
                {
                    result = true;
                    break;
                }
            }
        }
        return result;
    }
    public void Reset()
	{
		for (int i = 0; i < _polynumbers.Length; i++)
		{
			_polynumbers[i].Reset();
		}
	}
}