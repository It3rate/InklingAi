
namespace NumbersTests;

using Nums.Primitives;
using System;

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
        _unitFocal = new Focal(-4, 6);
		_maxMin = new Focal(-54, 46);
		_domain = new Domain(_trait, "testDomain", _unitFocal, _maxMin);
    }

    public void Dispose()
    {
        _brain.ClearAll();
    }

    [Fact]
	    public void CoreWorkspaceTests()
	    {
        _workspace.AddDomains(true, _domain);
        Assert.Equal(_workspace.ElementCount, 1); // domain

        var f5 = new Focal(20, 90);
        var n5 = _domain.CreateNumber(f5);
        _workspace.AddElements(n5);
        Assert.Equal(2, _workspace.ElementCount);
        _workspace.ClearAll();
        Assert.Equal( 0, _workspace.ElementCount);
        _workspace.AddDomains(true, _domain);
        Assert.Equal(1, _workspace.ElementCount);
        _workspace.RemoveDomains(true, _domain);
        Assert.Equal( 0, _workspace.ElementCount);
        _workspace.AddDomains(false, _domain);
        Assert.Equal( 1, _workspace.ElementCount);
        _workspace.AddDomains(true, _domain);
        Assert.Equal( 2, _workspace.ElementCount);
        _workspace.RemoveTraits(true, _trait);
        Assert.Equal(1, _workspace.ElementCount);
    }
}
