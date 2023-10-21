using Nums.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nums.Primitives
{
	// A contiguous sequence of numbers in the same domain, an end value is the next numbers start value.
	public class PolyNumber
	{
		public MathElementKind Kind => MathElementKind.PolyNumber;
		public int Id { get; internal set; }
		public int CreationIndex => Id - (int)Kind - 1;

		public Domain Domain { get; set; }
		public PolyFocal PolyFocal { get; set; }
		public int Count => PolyFocal.Count;

		public PolyNumber(Domain domain, PolyFocal focals)
		{
			Domain = domain;
			PolyFocal = focals;
		}
		public void AddPosition(long position)
		{
			PolyFocal.AddPosition(position);
		}
		public void AddPositions(long[] positions)
		{
			PolyFocal.AddPositions(positions);
		}
		public void RemoveLastPosition() 
		{
			PolyFocal.RemoveLastPosition();
		}
		public void AddValue(double val)
		{
			var pos = Domain.GetValueOf(val);
			PolyFocal.AddPosition(pos);
		}

		public Seg ValueAt(int index)
		{
			return Domain.GetValueOf(PolyFocal.FocalAt(index));
		}
		public double ValueAtPosition(int index)
		{
			return Domain.GetValueOf(PolyFocal.PositionAt(index)).End;
		}
		public double[] Positions()
		{
			var result = new double[PolyFocal.Length];
			for (int i = 0; i < PolyFocal.Count; i++)
			{
				result[i] = ValueAtPosition(i);
			}
			return result;
		}

		public void Reset()
		{
			PolyFocal.Clear();
		}
	}
}
