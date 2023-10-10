using Nums.CoreConcepts.Time;
using Nums.Primitives;

namespace Nums.CoreConcepts.Counter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class CounterTrait : Trait
    { 
        public static CounterTrait CreateIn(Knowledge knowledge) => (CounterTrait)knowledge.Brain.AddTrait(new CounterTrait());

        public CounterTrait() : base("Counter") { }
    }
}
