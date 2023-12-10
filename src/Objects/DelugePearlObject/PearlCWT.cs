using System.Runtime.CompilerServices;
using TheFriend.SlugcatThings;
using UnityEngine;

namespace TheFriend.Objects.DelugePearlObject;

public static class PearlCWT
{
    public class DelugePearl
    {
        public const float BaseButtConnectionAssymetry = 0.9f;
        public const float BaseTailConnectionAssymetry = BaseButtConnectionAssymetry * 0.5f;
        public const float BasePearlToButtDist = 40f;

        public int ownerInt;
        public Player owner;
        public bool isAttached;
        public bool hasBeenRead;
        public Color color;
        public BodyChunkBodyPartConnection tailConnection;
        public PhysicalObject.BodyChunkConnection buttConnection;
        public DelugePearl(DataPearl.AbstractDataPearl pearl)
        {
            
        }
    }
    public static readonly ConditionalWeakTable<DataPearl.AbstractDataPearl, DelugePearl> CWT = new();
    public static DelugePearl DelugePearlData(this DataPearl.AbstractDataPearl pearl) => CWT.GetValue(pearl, _ => new(pearl));
}