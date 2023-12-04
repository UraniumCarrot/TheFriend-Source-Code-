using System.Runtime.CompilerServices;
using UnityEngine;

namespace TheFriend.Objects.DelugePearlObject;

public static class PearlCWT
{
    public class DelugePearl
    {
        public int ownerInt;
        public Player owner;
        public bool isAttached;
        public bool hasBeenRead;
        public Color color;
        public DelugePearl(DataPearl.AbstractDataPearl pearl)
        {
            
        }
    }
    public static readonly ConditionalWeakTable<DataPearl.AbstractDataPearl, DelugePearl> CWT = new();
    public static DelugePearl DelugePearlData(this DataPearl.AbstractDataPearl pearl) => CWT.GetValue(pearl, _ => new(pearl));
}