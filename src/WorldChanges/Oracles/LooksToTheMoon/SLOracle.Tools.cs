using UnityEngine;
using RWCustom;

namespace TheFriend.WorldChanges.Oracles.LooksToTheMoon;

public partial class SLOracle
{
    public static void SLStopFlight(SLOracleBehavior self)
    {
        self.forceFlightMode = false;
    }
    public static void SLStopGravity(SLOracleBehavior self)
    {
        var gravity = self.oracle.room.world.rainCycle.brokenAntiGrav;
        if (gravity.on)
        {
            gravity.progress = 0;
            self.oracle.room.PlaySound(SoundID.Broken_Anti_Gravity_Switch_Off, 0f, self.oracle.room.game.cameras[0].room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.BrokenZeroG), 1f);
        }
        gravity.on = false;
        gravity.counter = 600;
        gravity.from = 1;
        gravity.to = 0;
    }
}