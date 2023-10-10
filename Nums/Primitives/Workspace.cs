using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Nums.CoreConcepts.Time;
using Nums.Utils;

namespace Nums.Primitives
{
    public class Workspace : IMathElement, IEquatable<Workspace>
    {
	    private static int _idCounter = 0;
	    public MathElementKind Kind => MathElementKind.Transform;
	    public int Id { get; }
	    public int CreationIndex => Id - (int)Kind;

        public Brain Brain { get; }

        private List<IMathElement> Elements { get; } = new();

        public bool IsActive { get; set; } = true;

        public Workspace(Brain brain)
        {
	        Brain = brain;
	        Id = _idCounter++;
            Brain.Workspaces.Add(this);
        }

        public void Update(MillisecondNumber currentTime, MillisecondNumber deltaTime)
        {
	        //var num = lastDomain?.Numbers().ElementAt(2);
	        //if (num != null)
	        //{
		       // num.Focal.EndTickPosition = num.Focal.EndTickPosition + test;
         //       if(Math.Abs(num.Focal.EndTickPosition) > 70){test = -test;}
	        //}
        }

        public int ElementCount => Elements.Count;
        public bool IsElementActive(IMathElement element) => Elements.Contains(element);

        public void AddElements(params IMathElement[] elements)
        {
	        foreach (var element in elements)
	        {
		        Elements.Add(element);
	        }
        }
        public void RemoveElements(params IMathElement[] elements)
        {
	        foreach (var element in elements)
	        {
		        Elements.Remove(element);
	        }
        }

        public void AddTraits(bool includeChildren, params Trait[] traits)
        {
	        foreach (var trait in traits)
	        {
		        AddElements(trait);
		        if (includeChildren)
		        {
			        AddDomains(includeChildren,trait.DomainStore.Values.ToArray());
		        }
	        }
        }
        public void RemoveTraits(bool includeChildren, params Trait[] traits)
        {
	        foreach (var trait in traits)
	        {
		        RemoveElements(trait);
		        if (includeChildren)
		        {
			        RemoveDomains(includeChildren, trait.DomainStore.Values.ToArray());
		        }
	        }
        }

        private Domain lastDomain;
        public void AddDomains(bool includeChildren, params Domain[] domains)
        {
	        foreach (var domain in domains)
	        {
		        lastDomain = domain;
		        AddElements(domain);
	        }
        }
        public void RemoveDomains(bool includeChildren, params Domain[] domains)
        {
	        foreach (var domain in domains)
	        {
		        RemoveElements(domain);
	        }
        }

        public IEnumerable<Domain> ActiveSiblingDomains(Domain domain)
        {
	        var ds = domain.Trait.DomainStore;
	        var domains = ds.Values.Where(key => Elements.Contains(key));
	        foreach (var d in domains)
	        {
		        if (d != domain)
		        {
			        yield return d;
		        }
	        }
        }

        public void ClearAll()
        {
            Elements.Clear();
        }

        public override bool Equals(object? obj) => obj is Workspace other && Equals(other);
        public bool Equals(IMathElement? other) => other is Workspace wksp && Equals(wksp);
        public bool Equals(Workspace? value) => Id.Equals(value?.Id);
        public override int GetHashCode() => Id.GetHashCode();

    }
}
