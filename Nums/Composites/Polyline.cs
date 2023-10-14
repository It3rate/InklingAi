namespace Nums.Composites
{
	using Nums.Primitives;
	using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class PolyLine
	{
		public PolyNumber XValues { get; set; }
		public PolyNumber YValues { get; set; }

		public PolyLine(PolyNumber xValues, PolyNumber yValues)
		{
			XValues = xValues;
			YValues = yValues;
		}
	}
}
