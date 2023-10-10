using Nums.Primitives;

namespace NumbersTests.CoreTests;


public class NumberSetTests : IDisposable
{
    private Brain _brain;
    private Trait _trait;
    private Focal _unitFocal;
    private Focal _maxMin;
    private Domain _domain;

    public NumberSetTests()
    {
        _brain = Brain.ActiveBrain;
        _trait = Trait.CreateIn(_brain, "number tests");
        _unitFocal = new Focal( 0, 10);
        _maxMin = new Focal(-1000, 1000);
        _domain = new Domain(_trait, _unitFocal, _maxMin);
    }

    public void Dispose()
    {
        _brain.ClearAll();
    }

    [Fact]
    public void NoOverlapTest()
    {
        var result = new NumberSet(_domain, new Focal(10, 20), new Focal(30, 40));

        result.RemoveOverlaps();
        Assert.Equal(2, result.Count);
        Assert.Equal(result.GetFocals(),
            new List<IFocal>
            {
                new Focal(10, 20),
                new Focal(30, 40)
            });
    }

    [Fact]
    public void OverlapTest()
    {
        var result = new NumberSet(_domain, new Focal[]
        {
                new Focal(10, 20),
                new Focal(15, 25),
                new Focal(30, 40)
        });
        result.RemoveOverlaps();
        Assert.Equal(2, result.Count);
        Assert.Equal(new List<IFocal>
            {
                new Focal(10, 25),
                new Focal(30, 40)
            }, result.GetFocals());
    }

    [Fact]
    public void MultipleOverlapTest()
    {
        var result = new NumberSet(_domain, new Focal[]
        {
                new Focal(10, 20),
                new Focal(15, 25),
                new Focal(30, 40),
                new Focal(35, 45)
            });
        result.RemoveOverlaps();
        Assert.Equal(2, result.Count);
        Assert.Equal(new List<IFocal> { new Focal(10, 25), new Focal(30, 45) }, result.GetFocals());
    }

    [Fact]
    public void AllOverlapTest()
    {
        var result = new NumberSet(_domain, new Focal[]
            {
                new Focal(10, 20),
                new Focal(15, 25),
                new Focal(20, 30)
            });
        result.RemoveOverlaps();
        Assert.Equal(1, result.Count);
        Assert.Equal(new List<IFocal> { new Focal(10, 30) }, result.GetFocals());
    }
}
