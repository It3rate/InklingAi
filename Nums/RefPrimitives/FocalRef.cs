
using System;
using Nums.Primitives;
using Nums.Utils;

namespace Nums.RefPrimitives
{
    public class FocalRef : FocalBase // todo: Eventually need to consider what the benefits are of having virtual positions like this. Maybe none?
    {
        private static int _positionCounter = 1;
        public Trait MyTrait { get; }
        public int StartId { get; set; } // ref to start point position
        public int EndId { get; set; } // ref to end point position
        public override long StartTickPosition
        {
            get => MyTrait.PositionStore[StartId];
            set => MyTrait.PositionStore[StartId] = value;
        }
        public override long EndTickPosition
        {
            get => MyTrait.PositionStore[EndId];
            set => MyTrait.PositionStore[EndId] = value;
        }

        // FocalRefs are stored by ID in their trait. The positions are also indexed in the trait's PositionStore.
        private FocalRef(Trait trait, int startId, int endId)
        {
            MyTrait = trait;
            StartId = startId;
            EndId = endId;
            trait.FocalStore.Add(Id, this);
        }
        public static FocalRef CreateByIds(Trait trait, int startId, int endId)
        {
            var result = new FocalRef(trait, startId, endId);
            return result;
        }
        public static FocalRef CreateByValues(Trait trait, long startPosition, long endPosition)
        {
            trait.PositionStore.Add(_positionCounter++, startPosition);
            trait.PositionStore.Add(_positionCounter++, endPosition);
            var result = new FocalRef(trait, _positionCounter - 2, _positionCounter - 1);
            return result;
        }

        public override IFocal Clone()
        {
            return CreateByValues(MyTrait, StartTickPosition, EndTickPosition);
        }
    }
}
