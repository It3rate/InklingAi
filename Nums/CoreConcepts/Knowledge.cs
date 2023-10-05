using Nums.CoreConcepts.Time;
using Nums.Primitives;

namespace Nums.CoreConcepts
{
	public class Knowledge
    {
        public static Knowledge Instance { get; private set; } // todo: integrate knowledge at the brain level.
	    public Brain Brain { get; }

	    public Knowledge(Brain brain)
	    {
		    Brain = brain;
		    Instance = this;
            Initialize();
	    }

	    private void Initialize()
	    {
            // these typed domains will be loaded from a file or something.
		    TimeTrait = TimeTrait.CreateIn(this);
		    MillisecondTimeDomain = MillisecondTimeDomain.MinMax;
        }

	    public TimeTrait TimeTrait { get; private set; }
	    public MillisecondTimeDomain MillisecondTimeDomain { get; private set; }

    }
}
