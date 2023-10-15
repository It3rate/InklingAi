using Nums.Composites;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InkPad.Adapters
{
	public class SkiaPolyLineAdapter
	{
		private PolyNumberSet _polyline;

		public SkiaPolyLineAdapter(PolyNumberSet polyline)
		{
			_polyline = polyline;
		}

		public SKPoint MakePoint(double[] values) => new SKPoint((float)values[0], (float)values[1]);
		public SKPoint[] ToPoints()
		{
			var result = new SKPoint[_polyline.Count];
			var pts = _polyline.GetIndexedValues();

			for (int i = 0; i < pts.Length; i++)
			{
				result[i] = MakePoint(pts[i]);
			}
			return result;
		}

		public SKPath ToQuadCurves(bool closePath = true)
		{
			var result = new SKPath();
			var pts = _polyline.GetIndexedValues();

			result.MoveTo(MakePoint(pts[0]));
			for (int i = 1; i < pts.Length; i++)
			{
				result.QuadTo(MakePoint(pts[i]), MakePoint(pts[i + 1]));
			}

			if(closePath == true)
			{
				result.Close();
			}
			return result;
		}

		public SKPath ToPath(bool closePath = true)
		{
			var result = new SKPath();
			result.AddPoly(ToPoints(), closePath);
			return result;
		}

		public SKPath PointsToOptimizedCurve(int optLevel)
		{
			throw new NotImplementedException();
		}
	}
}
