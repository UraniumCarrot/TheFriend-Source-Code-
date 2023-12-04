using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;

namespace TheFriend;

public static class DragonRideCWT
{
    public class DragonRide
    {
        public int hybridHead;
        public Vector2 seat0;
        public Vector2? lastOutsideTerrainPos;
        public bool boolseat0;
        public bool aquatic;
        //public bool IsBeingRidden;
        public bool IsRideable;
        public Player rider;

        public DragonRide()
        {
            //this.IsBeingRidden = false;
            this.aquatic = false;
            this.IsRideable = false;
            this.seat0 = Vector2.zero;
            this.lastOutsideTerrainPos = null;
        }
    }
    public static readonly ConditionalWeakTable<Lizard, DragonRide> CWT = new();
    public static DragonRide GetLiz(this Lizard lizard) => CWT.GetValue(lizard, _ => new());
}
