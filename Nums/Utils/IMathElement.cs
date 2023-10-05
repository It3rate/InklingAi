namespace Nums.Utils
{
	public interface IMathElement
    {
	    MathElementKind Kind { get; }
	    int Id { get; }
	    int CreationIndex { get; }
    }

    public enum MathElementKind
    {
	    None = 0,
        Network    = 0x01000000,
        Workspace  = 0x02000000,
        Formula    = 0x03000000,
        Definition = 0x04000000,
        Trait      = 0x05000000,
	    Domain     = 0x06000000,
	    Relation   = 0x07000000,
        Number     = 0x08000000,
	    Transform  = 0x09000000,
	    NumberSet  = 0x0A000000,
	    Selection  = 0x0B000000, // probably not needed
        Focal      = 0x0C000000,
    }
}  
