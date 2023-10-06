using Nums.Primitives;

namespace NumbersTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Nums.RefPrimitives;

    public class WorkspaceTests : IDisposable
    {
	    private Brain _brain;
	    private Workspace _workspace;
	    private Trait _trait;
        private Focal _unitFocal;
	    private Focal _maxMin;
	    private Domain _domain;

	    public WorkspaceTests()
        {
            _brain = Brain.ActiveBrain;
            _workspace = new Workspace(_brain);

		    _trait = Trait.CreateIn(_brain, "workspace tests");
            _unitFocal = Focal.CreateByValues(-4, 6);
		    _maxMin = Focal.CreateByValues(-54, 46);
		    _domain = new Domain(_trait, _unitFocal, _maxMin);
        }

        public void Dispose()
        {
            _brain.ClearAll();
        }

        [Fact]
	    public void CoreWorkspaceTests()
	    {
            _workspace.AddDomains(true, _domain);
            Assert.Equal(_workspace.ActiveElementCount, 3); // domain, unit and range

            var f5 = Focal.CreateByValues(20, 90);
            var n5 = _domain.CreateNumber(f5);
            _workspace.AddElements(n5);
            Assert.Equal(4, _workspace.ActiveElementCount);
            _workspace.ClearAll();
            Assert.Equal( 0, _workspace.ActiveElementCount);
            _workspace.AddDomains(true, _domain);
            Assert.Equal(4, _workspace.ActiveElementCount);
            _workspace.RemoveDomains(true, _domain);
            Assert.Equal( 0, _workspace.ActiveElementCount);
            _workspace.AddDomains(false, _domain);
            Assert.Equal( 1, _workspace.ActiveElementCount);
            _workspace.AddDomains(true, _domain);
            Assert.Equal( 4, _workspace.ActiveElementCount);
            _workspace.RemoveTraits(true, _trait);
            Assert.Equal(0, _workspace.ActiveElementCount);
        }
    }
}
