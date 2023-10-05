using System.Collections.Generic;
using Nums.Operations;
using Nums.Primitives;

namespace Nums.Composites
{
    public class Definition
    {
        public int Id { get; }
        public Brain Brain { get; }
        public List<Number> Numbers; // numbers belong to traits and domains
        public List<Transform> Relations;

        public Definition(Brain brain)
        {
            Brain = brain;
            Id = Brain.NextDefinitionId();
        }
    }
}
