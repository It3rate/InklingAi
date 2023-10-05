using System;
using System.Collections.Generic;
using System.Linq;
using Nums.Utils;

namespace Nums.Primitives
{
	// this constrains the significant figures of a measurement, unit/not is the minimum tick size, and range is the max possible.
    // Feels like this should just be the unit chosen - that works from min tick size, but how to specify a max value? On the trait I guess?
    // the trait then needs to 'know' how measurable it is, and the units are calibrated to that, which seems overspecified
    // (eg 'length' knows a nanometer is min and a light year max, cms and inches calibrate to this. Hmm, no).
    // So each 'situation' has sig-fig/precision metadata. Working in metal units vs working in wood units. A metal length trait and a wood length trait, convertible.
    // This is what domains are. BasisFocal size(s), min tick size, max ranges, confidence/probability data at limits of measure (gaussian precision etc).
    // E.g. changing the domain 'tolerance' could change neat writing into messy.

    // Min size is tick size. BasisFocal is start/end point (only one focal allowed for a unit). MinMaxFocal is bounds in ticks. todo: add conversion methods etc.
    public class Domain : IMathElement
    {
	    public  MathElementKind Kind => MathElementKind.Domain;
        private static int domainCounter = 1 + (int)MathElementKind.Domain;
        public int Id { get; internal set; }
        public int CreationIndex => Id - (int)Kind - 1;

	    //public Brain Brain => Trait.Brain;
	    public Trait Trait { get; protected set; }
        public IFocal BasisFocal { get; protected set; }
        public IFocal MinMaxFocal { get; protected set; }
        public Number BasisNumber { get; protected set; }
        public Number MinMaxNumber { get; protected set; }

        public readonly Dictionary<int, Number> NumberStore = new Dictionary<int, Number>();
        public readonly Dictionary<int, NumberSet> NumberSetStore = new Dictionary<int, NumberSet>();

        public bool BasisIsReciprocal { get; set; }// True when ticks are larger than the unit basis

        //public int BasisNumberId => BasisNumber.Id;
        //public int BasisFocalId => BasisNumber.BasisFocal.Id;
        //public int MinMaxFocalId => MinMaxNumber.FocalId;
        //public int MinMaxNumberId => MinMaxNumber.Id;

        public NumRange MinMaxRange => BasisFocal.RangeAsBasis(MinMaxFocal);
        public double TickToBasisRatio => BasisIsReciprocal ? BasisFocal.NonZeroLength : 1.0 / BasisFocal.NonZeroLength;

        public bool IsUnitPerspective => BasisFocal.IsUnitPerspective;
        public bool IsUnotPerspective => BasisFocal.IsUnotPerspective;

        public Domain(Trait trait, IFocal basisFocal, IFocal minMaxFocal = default)
        {
	        Id = domainCounter++;
	        Trait = trait;
	        BasisFocal = basisFocal;
	        MinMaxFocal = minMaxFocal;
            BasisNumber = CreateNumber(basisFocal);
            MinMaxNumber = minMaxFocal == default ? CreateNumber(Focal.MinMaxFocal) : CreateNumber(minMaxFocal);
            Trait.DomainStore.Add(Id, this);
        }

        public int[] NumberIds() => NumberStore.Values.Select(num => num.Id).ToArray();
        public Number GetNumber(int numberId)
        {
	        NumberStore.TryGetValue(numberId, out var result);
	        return result;
        }
        public Number CreateNumber(IFocal focal, bool addToStore = true)
        {
	        return AddNumber(new Number(focal), addToStore);
        }
        public Number CreateNumber(NumRange value, bool addToStore = true)
        {
	        var focal = CreateFocalFromRange(value);
	        return AddNumber(new Number(focal), addToStore);
        }

        public Number CreateNumber(long start, long end, bool addToStore = true)
        {
	        var focal = Focal.CreateByValues(start, end);
	        return AddNumber(new Number(focal), addToStore);
        }
        public Number AddNumber(Number number, bool addToStore = true)
        {
	        number.Domain = this;
	        number.Id = number.Id == 0 ? NextNumberId() : number.Id;
	        if (addToStore)
	        {
		        NumberStore[number.Id] = number;
	        }
	        return number;
        }
        public bool RemoveNumber(Number number)
        {
	        number.Domain = null;
	        return NumberStore.Remove(number.Id);
        }


        private int _numberSetCounter = 1 + (int)MathElementKind.NumberSet;
        public int NextNumberSetId() => _numberSetCounter++ + Id;
        public NumberSet AddNumberSet(NumberSet numberSet, bool addToStore = true)
        {
	        numberSet.Domain = this;
	        numberSet.Id = numberSet.Id == 0 ? NextNumberSetId() : numberSet.Id;
	        if (addToStore)
	        {
                NumberSetStore[numberSet.Id] = numberSet;
	        }
	        return numberSet;
        }
        public bool RemoveNumberSet(NumberSet numberSet)
        {
	        numberSet.Domain = null;
	        return NumberSetStore.Remove(numberSet.Id);
        }

        public Number Zero() => CreateNumber(BasisFocal.StartTickPosition, BasisFocal.StartTickPosition);
        public Number One() => CreateNumber(BasisFocal.StartTickPosition, BasisFocal.EndTickPosition);

        public NumRange GetValueOf(IFocal focal) => focal.GetRangeWithBasis(BasisFocal, BasisIsReciprocal);
        public void SetValueOf(IFocal focal, NumRange range) => focal.SetWithRangeAndBasis(range, BasisFocal, BasisIsReciprocal);
        public NumRange GetValueOf(Number num) => GetValueOf(num.Focal);
        public void SetValueOf(Number num, NumRange range) => SetValueOf(num.Focal,range);

        public NumRange ClampToInnerBasis(NumRange range) => range.ClampInner();
        public NumRange ClampToInnerTick(NumRange range) => (range / TickToBasisRatio).ClampInner() * TickToBasisRatio;
        public NumRange RoundToNearestBasis(NumRange range) => range.Round();
        public NumRange RoundToNearestTick(NumRange range) => (range / TickToBasisRatio).Round() * TickToBasisRatio;

        public long RoundToNearestTick(long value) => (long)(Math.Round(value / TickToBasisRatio) * TickToBasisRatio);
        public void RoundToNearestTick(IFocal focal)
        {
	        focal.StartTickPosition = RoundToNearestTick(focal.StartTickPosition);
	        focal.EndTickPosition = RoundToNearestTick(focal.EndTickPosition);
        }

        public IFocal CreateFocalFromRange(NumRange range)
        {
	        var result = Focal.CreateByValues(0, 1);
	        result.SetWithRangeAndBasis(range, BasisFocal, BasisIsReciprocal);
	        return result;
        }

        public void ClearAll()
        {
	        NumberStore.Clear();
        }

        private int _numberCounter = 1 + (int)MathElementKind.Number;
        public int NextNumberId() => _numberCounter++ + Id;

        public IEnumerable<Number> Numbers()
        {
	        foreach (var number in NumberStore.Values)
	        {
		        yield return number;
	        }
        }
        public void SaveNumberValues(Dictionary<int, NumRange> dict, params int[] ignoreIds)
        {
            dict.Clear();
	        foreach (var num in Numbers())
	        {
		        if (!ignoreIds.Contains(num.Id))
		        {
			        dict.Add(num.Id, num.Value);
                }
	        }
        }
        public void RestoreNumberValues(Dictionary<int, NumRange> values, params int[] ignoreIds)
        {
	        foreach (var kvp in values)
	        {
		        if (!ignoreIds.Contains(kvp.Key))
		        {
			        NumberStore[kvp.Key].Value = kvp.Value;
                }
	        }
        }
    }
}
