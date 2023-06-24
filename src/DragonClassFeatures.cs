using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using HUD;
using JollyCoop;
using Menu;
using RWCustom;
using UnityEngine;

namespace TheFriend;

public class DragonClassFeatures
{
    public static void Apply()
    {
        On.Player.SpearStick += Player_SpearStick;
        On.Creature.LoseAllGrasps += Creature_LoseAllGrasps;
        On.HUD.TextPrompt.UpdateGameOverString += TextPrompt_UpdateGameOverString;
    }

    public static void TextPrompt_UpdateGameOverString(On.HUD.TextPrompt.orig_UpdateGameOverString orig, TextPrompt self, global::Options.ControlSetup.Preset controllerType)
    {
        Debug.Log("Solace: TextPrompt.UpdateGameOverString trying to run orig...");
        orig(self, controllerType);
        var pl = self?.hud?.owner as Creature;
        Debug.Log("Solace: TextPrompt.UpdateGameOverString orig ran!");
        if ((self?.hud?.owner as Player)?.room?.game?.StoryCharacter == Plugin.DragonName)
        {
            Debug.Log("Solace: TextPrompt.UpdateGameOverString hook is trying to run!");
            self.gameOverString += ", or find a way to survive";
        }
    }
    public static void Creature_LoseAllGrasps(On.Creature.orig_LoseAllGrasps orig, Creature self)
    {
        if (self is Player player && player.slugcatStats.name == Plugin.DragonName && self.State.alive && (!self.Stunned || (self as Player).dangerGraspTime > 0)) return;
        else orig(self);
    }
    public static bool Player_SpearStick(On.Player.orig_SpearStick orig, Player self, Weapon source, float dmg, BodyChunk chunk, PhysicalObject.Appendage.Pos appPos, Vector2 direction)
    {
        if (self.slugcatStats.name == Plugin.DragonName) 
        {
            if (self.dead) return true;
            if (self.bodyMode == Player.BodyModeIndex.Stunned) return true;
            if (self.standing)
            {
                if ((Mathf.Abs(self.firstChunk.vel.x) > 2f && Mathf.Abs(direction.x) > 2f) && Mathf.Sign(direction.x) == Mathf.Sign(self.firstChunk.vel.x)) 
                {
                    if (source is ExplosiveSpear) self.Stun(100);
                    else self.Stun(25);
                    self.firstChunk.vel += (source.firstChunk.vel * source.firstChunk.mass / self.firstChunk.mass) / 2;
                    self.GetPoacher().flicker = Custom.IntClamp(200 / 3, 3, 15);
                    return false; 
                }
                else { self.firstChunk.vel += source.firstChunk.vel * source.firstChunk.mass / self.firstChunk.mass; return true; }
            }
            else
            {
                if (self.bodyMode == Player.BodyModeIndex.Crawl || self.animation == Player.AnimationIndex.BellySlide)
                {
                    if (source is ExplosiveSpear) self.Stun(50);
                    self.firstChunk.vel += (source.firstChunk.vel * source.firstChunk.mass / self.firstChunk.mass) / 3;
                    self.GetPoacher().flicker = Custom.IntClamp(200 / 3, 3, 15);
                    return false;
                }
                else
                {
                    if (source is ExplosiveSpear) self.Stun(100);
                    else self.Stun(50);
                    self.firstChunk.vel += source.firstChunk.vel * source.firstChunk.mass / self.firstChunk.mass;
                    self.GetPoacher().flicker = Custom.IntClamp(200 / 3, 3, 15);
                    return false;
                }
            }
        }
        else return orig(self, source, dmg, chunk, appPos, direction);
    }
}
