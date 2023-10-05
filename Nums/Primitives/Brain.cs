using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Nums.CoreConcepts;
using Nums.Utils;

namespace Nums.Primitives
{
	// Brain -> Traits, transforms

    public class Brain
    {
	    public static List<Brain> Brains { get; } = new List<Brain>();
	    public static Brain ActiveBrain => BrainA;
	    public static Brain BrainA = new Brain();
	    public static Brain BrainB = new Brain();
	    public Brain()
	    {
            Knowledge = new Knowledge(this);
		    Brains.Add(this);
	    }

        public Knowledge Knowledge { get; }
	    public List<Workspace> Workspaces { get; } = new List<Workspace>();

        public readonly Dictionary<int, Network> NetworkStore = new Dictionary<int, Network>(); 
        public readonly Dictionary<int, Formula> FormulaStore = new Dictionary<int, Formula>();
        public readonly Dictionary<int, Definition> DefinitionStore = new Dictionary<int, Definition>();
        public readonly Dictionary<int, Trait> TraitStore = new Dictionary<int, Trait>();
	    public readonly Dictionary<int, Transform> TransformStore = new Dictionary<int, Transform>();

        private int _networkCounter = 1 + (int)MathElementKind.Network;
	    private int _formulaCounter = 1 + (int)MathElementKind.Formula;
	    private int _definitionCounter = 1 + (int)MathElementKind.Definition;
        private int _traitCounter = 1 + (int)MathElementKind.Trait;
	    private int _transformCounter = 1 + (int)MathElementKind.Transform;
        public int NextNetworkId() => _networkCounter++;
        public int NextFormulaId() => _formulaCounter++;
        public int NextDefinitionId() => _definitionCounter++;
        public int NextTraitId() => _traitCounter++;
	    public int NextTransformId() => _transformCounter++;

	    public Trait AddTrait(Trait trait)
	    {
		    trait.Brain = this;
		    trait.Id = NextTraitId();
		    TraitStore.Add(trait.Id, trait);
		    return trait;
	    }
	    public bool RemoveTrait(Trait trait)
	    {
		    trait.Brain = null;
		    return TraitStore.Remove(trait.Id);
	    }
        public Transform AddTransform(Selection selection, Number repeats, TransformKind kind)
	    {
		    var result = new Transform(selection, repeats, kind);
		    TransformStore.Add(result.Id, result);
		    return result;
	    }
	    public IEnumerable<Transform> Transforms()
	    {
		    foreach (var transform in TransformStore.Values)
		    {
			    yield return transform;
		    }
	    }

        public void ClearAll()
	    {
		    foreach (var workspace in Workspaces)
            {
                workspace.ClearAll();
            }
            Workspaces.Clear();


            NetworkStore.Clear();
            FormulaStore.Clear();
            TraitStore.Clear();
            TransformStore.Clear();
        }

        public Trait TraitAt(int index)
        {
	        var id = index + (int)MathElementKind.Trait;
	        TraitStore.TryGetValue(id, out var result);
	        return result;
        }
    }
}
