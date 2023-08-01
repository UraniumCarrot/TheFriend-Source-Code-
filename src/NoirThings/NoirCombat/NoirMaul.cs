using RWCustom;
using UnityEngine;

namespace TheFriend.NoirThings;

public partial class NoirCatto
{
    private static void NoirMaul(Player self, bool eu)
    {
        var whichGrasp = 0;
        if (ModManager.MMF && (self.grasps[0]?.grabbed is not Creature) && self.grasps[1]?.grabbed is Creature)
        {
            whichGrasp = 1;
        }

        if (self.grasps[whichGrasp] != null && self.maulTimer % 20 == 0 && self.maulTimer % 40 != 0)
        {
            self.room.PlaySound(SoundID.Slugcat_Eat_Meat_B, self.mainBodyChunk);
            self.room.PlaySound(SoundID.Drop_Bug_Grab_Creature, self.mainBodyChunk, false, 1f, 0.76f);
            if (RainWorld.ShowLogs) Debug.Log("Mauled target");

            if (self.grasps[whichGrasp].grabbed is Creature crit && !crit.dead)
            {
                for (var num12 = Random.Range(8, 14); num12 >= 0; num12--)
                {
                    self.room.AddObject(new WaterDrip(Vector2.Lerp(self.grasps[whichGrasp].grabbedChunk.pos, self.mainBodyChunk.pos, Random.value) + self.grasps[whichGrasp].grabbedChunk.rad * Custom.RNV() * Random.value, Custom.RNV() * 6f * Random.value + Custom.DirVec(self.grasps[whichGrasp].grabbed.firstChunk.pos, (self.mainBodyChunk.pos + (self.graphicsModule as PlayerGraphics).head.pos) / 2f) * 7f * Random.value + Custom.DegToVec(Mathf.Lerp(-90f, 90f, Random.value)) * Random.value * self.EffectiveRoomGravity * 7f, false));
                }

                crit.SetKillTag(self.abstractCreature);
                float dmgToDeal = 1f;
                var critHealth = ((HealthState)crit.State).health;

                var dmg = critHealth * crit.Template.baseDamageResistance * 0.5f;
                if (dmg > 1f)
                {
                    dmgToDeal = dmg;
                }

                //Debug.Log(dmgToDeal);
                crit.Violence(self.bodyChunks[0], new Vector2?(new Vector2(0f, 0f)), self.grasps[whichGrasp].grabbedChunk, null, Creature.DamageType.Bite, dmgToDeal, 15f);
                crit.Stun(5);

                //Debug.Log($"Health: {((HealthState)crit.State).health}");

                if (critHealth <= 0f)
                {
                    if (RainWorld.ShowLogs) Debug.Log("Creature health below zero, releasing...");
                    self.TossObject(whichGrasp, eu);
                    self.ReleaseGrasp(whichGrasp);
                }

            }
        }
    }
}