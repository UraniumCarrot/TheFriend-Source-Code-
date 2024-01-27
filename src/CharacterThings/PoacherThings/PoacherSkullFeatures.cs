using HUD;
using TheFriend.Objects.LittleCrackerObject;
using UnityEngine;
using bod = Player.BodyModeIndex;
using ind = Player.AnimationIndex;

namespace TheFriend.PoacherThings;

public class PoacherSkullFeatures
{
    public static void Apply()
    {
        On.Player.SpearStick += Player_SpearStick;
        On.Creature.LoseAllGrasps += Creature_LoseAllGrasps;
        On.Player.Grabbed += Player_Grabbed;
        On.Creature.Violence += CreatureOnViolence;
        On.HUD.TextPrompt.UpdateGameOverString += TextPrompt_UpdateGameOverString;
    }

    public static void TextPrompt_UpdateGameOverString(On.HUD.TextPrompt.orig_UpdateGameOverString orig, TextPrompt self, Options.ControlSetup.Preset controllerType)
    { // Custom gameover text
        orig(self, controllerType);
        var pl = self?.hud?.owner as Creature;
        if (pl != null && pl.room != null && pl.room.game.StoryCharacter == Plugin.DragonName && !pl.room.game.IsArenaSession && !pl.dead)
        {
            Debug.Log("Solace: TextPrompt.UpdateGameOverString hook is trying to run!");
            self.gameOverString += ", or find a way to survive";
        }
    }
    public static void Creature_LoseAllGrasps(On.Creature.orig_LoseAllGrasps orig, Creature self)
    {
        if (self is Player player && player.slugcatStats.name == Plugin.DragonName && self.State.alive && (!self.Stunned || (self as Player).dangerGraspTime > 0)) return;
        orig(self);
    }
    public static bool Player_SpearStick(On.Player.orig_SpearStick orig, Player self, Weapon source, float dmg, BodyChunk chunk, PhysicalObject.Appendage.Pos appPos, Vector2 direction)
    {
        var original = orig(self, source, dmg, chunk, appPos, direction);
        if (self.slugcatStats.name == Plugin.DragonName)
        {
            if (self.dead) return original;
            if (self.bodyMode == Player.BodyModeIndex.Stunned) return original;
            
            int stun = (source is ExplosiveSpear) ? 100 : (self.standing) ? 25 : 50;
            float reduction = (self.bodyMode == Player.BodyModeIndex.Crawl || self.animation == Player.AnimationIndex.BellySlide) ? 3 : 
                (self.standing) ? 2 : 0;
            
            return PoacherReflect(
                self, 
                source, 
                (int)Mathf.Sign(self.firstChunk.vel.x), 
                (int)Mathf.Sign(direction.x), 
                stun, 
                reduction) 
                   
                   || original;
        }
        else return original;
    }

    public static void Player_Grabbed(On.Player.orig_Grabbed orig, Player self, Creature.Grasp grasp)
    { // Poacher skull flicker from grabs
        orig(self, grasp);
        if (!self.TryGetPoacher(out _)) return;
        if (grasp.grabber is not Player && grasp.grabber is not DaddyLongLegs)
            PoacherGraphics.PoacherFlicker(self);
    }
    public static void CreatureOnViolence(On.Creature.orig_Violence orig, Creature self, BodyChunk source, Vector2? directionandmomentum, BodyChunk hitchunk, PhysicalObject.Appendage.Pos hitappendage, Creature.DamageType type, float damage, float stunbonus)
    { // Poacher skull flicker from rocks
        orig(self, source, directionandmomentum, hitchunk, hitappendage, type, damage, stunbonus);
        if (self is Player pl && pl.TryGetPoacher(out _))
            if (source?.owner is Rock && source?.owner is not LittleCracker)
            {
                if (pl.bodyMode == bod.Crawl) { self.firstChunk.vel.y += 10; pl.animation = ind.Flip; }
                PoacherGraphics.PoacherFlicker(pl);
            }
    }
    
    
    public static bool PoacherReflect(Player self, Weapon source, int playerDir, int weaponDir, int stun = 0, float knockbackReduction = 1)
    {
        bool Hurt = true;
        Vector2 knockback = source.firstChunk.vel * source.firstChunk.mass / self.firstChunk.mass / knockbackReduction;
        if ((playerDir == weaponDir && Mathf.Abs(self.firstChunk.vel.x) > 2f) ||
            (self.bodyMode == Player.BodyModeIndex.Crawl || self.animation == Player.AnimationIndex.BellySlide))
        {
            bool stopSound = source is Spear;
            if (stun > 0) self.Stun(stun);
            self.firstChunk.vel += knockback;
            PoacherGraphics.PoacherFlicker(self, stopSound);
            Hurt = false;
        }
        return Hurt;
    }
}

