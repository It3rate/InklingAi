namespace Nums.Composites;

using Nums.Primitives;
using System;
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
		if (values.Length < 1 || !values.All(p => p.Count == values[0].Count))
		{
			throw new ArgumentException("Values count must be greater than one and have the same count.");
		}
		_polynumbers = (PolyNumber[])values.Clone();
	}

	public void AddValues(params double[] values)
	{
		if(values.Length == _polynumbers.Length)
		{
			for (int i = 0; i < values.Length; i++)
			{
				_polynumbers[i].AddValue(values[i]);
			}
		}
		else
		{
			throw new ArgumentException("Values count must match underlying Polynumber array.");
		}
	}
	public double[][] GetIndexedValues()
	{
		var result = new double[Count][];
		for (int i = 0; i < Count; i++)
		{
			result[i] = new double[Dimensions];
			for (int j = 0; j < Dimensions; j++)
			{
				result[i][j] = _polynumbers[j].ValueAtPosition(i);
			}
		}
		return result;
	}
}