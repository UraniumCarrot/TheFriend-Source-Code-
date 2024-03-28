using RWCustom;
using TheFriend.WorldChanges.WorldStates.General;
using UnityEngine;

namespace TheFriend.WorldChanges.Oracles;

public static class OracleTools
{
    public static void OracleBlink(Oracle self)
    {
        (self.graphicsModule as OracleGraphics)!.eyesOpen = 0f;
    }
    public static void OracleMoveArms(Oracle self, Vector2 value, ToolMethods.MathMode mode)
    {
        foreach (Oracle.OracleArm.Joint joint in self.arm.joints)
            switch (mode)
            {
                case ToolMethods.MathMode.mult: joint.vel *= value; break;
                case ToolMethods.MathMode.set: joint.vel = value; break;
                case ToolMethods.MathMode.add: joint.vel += value; break;
            }
    }
    public static void OracleMoveArms(Oracle self, float value)
    {
        foreach (GenericBodyPart part in (self.graphicsModule as OracleGraphics)!.hands)
            part.vel += Custom.DirVec(part.pos, self.oracleBehavior.player.mainBodyChunk.pos) * 3f;
    }
    public static void OracleHeadDown(Oracle self)
    {
        (self.graphicsModule as OracleGraphics)!.head.vel += Custom.DegToVec(-90f);
    }
    public static void OracleGrantMarks(OracleBehavior self, int stun)
    {
        self.StunCoopPlayers(stun);
        foreach (Player player in self.PlayersInRoom)
            for (int i = 0; i < 20; i++)
                self.oracle.room.AddObject(new Spark(player.mainBodyChunk.pos, Custom.RNV() * Random.value * 40f, Color.white, null, 30, 120));
    }

    public static void OracleBehaviorOverride(On.Oracle.orig_ctor orig, Oracle self, AbstractPhysicalObject abstr, Room room)
    {
        orig(self, abstr, room);
        if (QuickWorldData.SolaceCampaign && self.oracleBehavior is SLOracleBehavior behav)
            behav.movementBehavior = SLOracleBehavior.MovementBehavior.Idle;
    }
}