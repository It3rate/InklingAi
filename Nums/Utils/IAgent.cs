using System.Collections.Generic;
using Nums.CoreConcepts;
using Nums.CoreConcepts.Time;
using Nums.Primitives;

namespace Nums.Utils
{
	public interface IAgent
    {
	    Brain Brain { get; }
	    Knowledge Knowledge { get; }
	    Workspace Workspace { get; }

	    void Update(MillisecondNumber currentTime, MillisecondNumber deltaTime);

        void ClearAll();
    }
}
