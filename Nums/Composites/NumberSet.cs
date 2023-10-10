using Nums.Utils;

namespace Nums.Primitives
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a boolean type evaluation of two or more numbers.
    /// This type of number can have more than one segment, but segments can never overlap.
    /// </summary>
    public class NumberSet : IMathElement
    {
	    public MathElementKind Kind => MathElementKind.NumberSet;
	    public int Id { get; internal set; }
	    public int CreationIndex => Id - (int)Kind - 1;

	    public virtual Domain Domain { get; set; }

        private List<IFocal> Focals { get; } = new List<IFocal>(); // todo: Focals should have no overlap and always be sorted

        public NumberSet(Domain domain, params IFocal[] focals)
        {
	        Domain = domain;
	        Focals.AddRange(focals);
            RemoveOverlaps();
        }

        public IFocal[] GetFocals() => Focals.ToArray();
        public int Count => Focals.Count;
        public void Add(IFocal focal) { Focals.Add(focal); RemoveOverlaps(); }
        public void Remove(IFocal focal) => Focals.Remove(focal);

        public void RemoveOverlaps()
        {
            if (Focals.Count > 1)
            {
                List<IFocal> result = new List<IFocal>();
                // Sort the list by start tick position
                Focals.Sort((a, b) => a.StartTickPosition.CompareTo(b.StartTickPosition));

                long start = Focals[0].StartTickPosition;
                long end = Focals[0].EndTickPosition;
                for (int i = 1; i < Focals.Count; i++)
                {
                    // Check for overlap
                    if (Focals[i].StartTickPosition <= end)
                    {
                        // Overlap, merge the ranges
                        end = Math.Max(end, Focals[i].EndTickPosition);
                    }
                    else
                    {
                        // No overlap, add the current non-overlapping range to the result list
                        result.Add(new Focal(start, end));
                        start = Focals[i].StartTickPosition;
                        end = Focals[i].EndTickPosition;
                    }
                }
                result.Add(new Focal(start, end));

                Focals.Clear();
                Focals.AddRange(result);
            }
        }


        public void Reset(IFocal[] focals)
        {
            Focals.Clear();
            Focals.AddRange(focals);
        }
        public Number? this[int index] => index < Focals.Count ? Domain.CreateNumber(Focals[index]) : null;
        public IEnumerable<Number> Numbers()
        {
	        foreach (var focal in Focals)
	        {
		        yield return Domain.CreateNumber(focal);
	        }
        }

        public Number SumAll()
        {
            var result = Domain.CreateNumber(new Focal(0,0));
            foreach (var number in Numbers())
            {
                result.Add(number);
            }
            return result;
        }

        public override bool Equals(object? obj) => obj is NumberSet other && Equals(other);
        public bool Equals(IMathElement? other) => other is NumberSet ns && Equals(ns);
        public bool Equals(NumberSet? value) => Id.Equals(value?.Id);
        public override int GetHashCode() => Id.GetHashCode();

    }
}
