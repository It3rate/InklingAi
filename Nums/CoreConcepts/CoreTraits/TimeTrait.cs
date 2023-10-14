using Nums.Primitives;

namespace Nums.CoreConcepts.Time
{
    public class TimeTrait : Trait
    {
        public static TimeTrait Instance {get;} = new TimeTrait();
        private TimeTrait() : base("Time") { }
    }
}
