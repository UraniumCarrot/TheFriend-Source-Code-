using MoreSlugcats;
using TheFriend.SlugcatThings;
using UnityEngine;
using RWCustom;

namespace TheFriend.Creatures.LizardThings;

public class LizardRideControl
{
    public static void Apply()
    {
        On.Lizard.Update += LizardOnUpdate;
    }

    public static void LizardOnUpdate(On.Lizard.orig_Update orig, Lizard self, bool eu)
    {
        orig(self, eu);
        if (self.GetLiz().rider != null)
        {
            self.AI.behavior = LizardAI.Behavior.FollowFriend;
            var breedparams = self.Template.breedParameters as LizardBreedParams;
            float runspeed = breedparams.baseSpeed/3;
            float swimspeed = breedparams.swimSpeed;

            var rider = self.GetLiz().rider;
            var input = rider.GetGeneral().UnchangedInputForLizRide;
            Vector2 inputvec = new Vector2(input[0].x, input[0].y);
            DragonRiding.DragonRideCommands(self,rider);

            // General movement
            if (input[0].AnyDirectionalInput)
            {
                // modified rideable lizards code, thanks noir <3
                if ((self.Submersion > 0.5f && self.GetLiz().aquatic) || self.room.GetTile(self.mainBodyChunk.pos).WaterSurface)
                {
                    self.mainBodyChunk.vel.y += Custom.DirVec(self.mainBodyChunk.pos, self.mainBodyChunk.pos + (inputvec * 20)).y * Mathf.Lerp(1.4f, 1.0f, 0.1f) * swimspeed; //Surprisingly, thi s works well enough.
                    self.mainBodyChunk.vel.x += Custom.DirVec(self.mainBodyChunk.pos, self.mainBodyChunk.pos + (inputvec * 20)).x * Mathf.Lerp(1.4f, 1.0f, 0.1f) * swimspeed; //Horizontal boost for some slower lizzies
                }
                self.mainBodyChunk.vel.y += Custom.DirVec(self.mainBodyChunk.pos, self.mainBodyChunk.pos + (inputvec * 20)).y * Mathf.Lerp(1.4f, 1.0f, 0.1f) * self.LegsGripping; //Surprisingly, this works well enough.
                self.mainBodyChunk.vel.x += Custom.DirVec(self.mainBodyChunk.pos, self.mainBodyChunk.pos + (inputvec * 20)).x * Mathf.Lerp(1.4f, 1.0f, 0.1f) * runspeed * self.LegsGripping; //Horizontal boost for some slower lizzies
                self.bodyChunks[1].vel -= Custom.DirVec(self.mainBodyChunk.pos, self.mainBodyChunk.pos + (inputvec * 20)) * 0.1f * self.LegsGripping;
            }
            
            if (input[0].jmp)
            {
                if (self.Template.type == MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard && self.tongue.Ready)
                {
                    self.EnterAnimation(Lizard.Animation.ShootTongue, forceAnimationChange: false);
                }
                else if (self.Template.type != CreatureTemplate.Type.CyanLizard)
                {
                    self.EnterAnimation(Lizard.Animation.PrepareToLounge, forceAnimationChange: false);
                }
                else
                {
                    self.loungeDir = input[0].analogueDir;
                    self.EnterAnimation(Lizard.Animation.PrepareToLounge, forceAnimationChange: false);
                    self.loungeDir = input[0].analogueDir;
                }
            }
        }
    }
}