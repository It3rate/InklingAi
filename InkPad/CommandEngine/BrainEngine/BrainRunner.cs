namespace InkPad.CommandEngine.BrainEngine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Nums.Primitives;
    using Nums.Utils;
    using SkiaSharp;

    public class BrainRunner : Runner
    {
        public INumAgent Agent { get; set; }
        public Brain Brain => Agent.Brain;
        public Workspace Workspace => Agent.Workspace;
        public BrainRunner(INumAgent agent) : base()
        {
            Agent = agent;
        }
        protected override void Update()
        {
            Agent.Update(CurrentMS, DeltaMS);

        }
    }
}
