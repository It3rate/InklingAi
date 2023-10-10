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

        public string Name { get; } = "";
        public Trait Trait { get; protected set; }
        public IFocal BasisFocal { get; protected set; }
        public IFocal MinMaxFocal { get; protected set; }
        public ulong TickSize { get; protected set; } = 1L; // min of 1, tick sizes larger than units are estimates to the nearest ticksize

        public Seg MinMaxNum => MinMaxFocal.GetSegWithBasis(BasisFocal);
        public bool IsApproximate => BasisFocal.Length < TickSize;
        public double TickToBasisRatio => TickSize / BasisFocal.Length;

        public bool IsUnitPerspective => BasisFocal.IsUnitPerspective;
        public bool IsUnotPerspective => BasisFocal.IsUnotPerspective;

        public Domain(Trait trait, string name, IFocal basisFocal, IFocal? minMaxFocal = default)
        {
            Id = domainCounter++;
            Name = name;
	        Trait = trait;

	        BasisFocal = basisFocal;
	        MinMaxFocal = minMaxFocal ?? Focal.MinMaxFocal.Clone();
            Trait.DomainStore.Add(Name, this);
        }

        public Number CreateNumber(IFocal focal)
        {
	        return new Number(this, focal);
        }
        public Number CreateNumber(Seg value)
        {
	        var focal = CreateFocalFromRange(value);
	        return new Number(this, focal);
        }

        public Number CreateNumber(long start, long end)
        {
	        var focal = new Focal(start, end);
	        return new Number(this, focal);
        }

        public Number Zero() => CreateNumber(BasisFocal.StartTickPosition, BasisFocal.StartTickPosition);
        public Number One() => CreateNumber(BasisFocal.StartTickPosition, BasisFocal.EndTickPosition);

        public Seg GetValueOf(IFocal focal) => focal.GetSegWithBasis(BasisFocal);
        public void SetValueOf(IFocal focal, Seg range) => focal.SetTicksWithSegBasis(range, BasisFocal);
        public Seg GetValueOf(Number num) => GetValueOf(num.Focal);
        public void SetValueOf(Number num, Seg range) => SetValueOf(num.Focal,range);

        public Seg ClampToInnerBasis(Seg range) => range.ClampInner();
        public Seg ClampToInnerTick(Seg range) => (range / TickToBasisRatio).ClampInner() * TickToBasisRatio;
        public Seg RoundToNearestBasis(Seg range) => range.Round();
        public Seg RoundToNearestTick(Seg range) => (range / TickToBasisRatio).Round() * TickToBasisRatio;

        public long RoundToNearestTick(long value) => (long)(Math.Round(value / TickToBasisRatio, MidpointRounding.AwayFromZero) * TickToBasisRatio);
        public void RoundToNearestTick(IFocal focal)
        {
	        focal.StartTickPosition = RoundToNearestTick(focal.StartTickPosition);
	        focal.EndTickPosition = RoundToNearestTick(focal.EndTickPosition);
        }

        public IFocal CreateFocalFromRange(Seg range)
        {
	        var result = new Focal(0, 1);
	        result.SetTicksWithSegBasis(range, BasisFocal);
	        return result;
        }

        public override bool Equals(object? obj) => obj is Domain other && Equals(other);
        public bool Equals(IMathElement? other) => other is Domain && Equals(other);
        public bool Equals(Domain? value) => Id.Equals(value?.Id);
        public override int GetHashCode() => Id.GetHashCode();

        //public bool Equals(Domain? value) =>
        //    Name.Equals(value?.Name) && 
        //    Trait.Equals(value?.Trait) && 
        //    BasisFocal.Equals(value?.BasisFocal) && 
        //    MinMaxFocal.Equals(value?.MinMaxFocal) &&
        //    TickSize.Equals(value?.TickSize);


    }
}
