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
            float runspeed = ((self.Template.breedParameters) as LizardBreedParams).baseSpeed/3;
            float swimspeed = ((self.Template.breedParameters) as LizardBreedParams).swimSpeed;

            var rider = self.GetLiz().rider;
            var input = rider.GetPoacher().UnchangedInputForLizRide;
            Vector2 inputvec = new Vector2(input[0].x, input[0].y);
            var breedparams = self.Template.breedParameters as LizardBreedParams;

            /*if ((self.Submersion < 0.5f || 
                self.Template.type == CreatureTemplate.Type.Salamander || 
                self.Template.type == MoreSlugcatsEnums.CreatureTemplateType.EelLizard) &&
                self.animation == Lizard.Animation.Standard)
            for (int i = 0; i < self.bodyChunks.Length; i++)
            {
                self.animation = Lizard.Animation.Standard;
                aquatic = true;
            }
            if (self.room.GetTile(self.firstChunk.pos).Terrain != Room.Tile.TerrainType.Air)
            {
                self.bodyChunks[0].vel.x += rider.GetPoacher().UnchangedInputForLizRide[0].x * breedparams.baseSpeed;
                self.bodyChunks[0].vel.y += (rider.GetPoacher().UnchangedInputForLizRide[0].y*breedparams.baseSpeed) / 2 * ((self.GetLiz().aquatic || self.Submersion < 0.5f) ?  1 : 0);
            }
            if (rider.GetPoacher().UnchangedInputForLizRide[0].jmp && 
                self.animation == Lizard.Animation.Standard)
            {
                if (self.Template.type == CreatureTemplate.Type.CyanLizard)
                {
                    self.EnterAnimation(Lizard.Animation.PrepareToJump,true);
                }
                else
                {
                    self.EnterAnimation(Lizard.Animation.PrepareToLounge,true);
                    self.loungeDir = rider.GetPoacher().UnchangedInputForLizRide[0].analogueDir.normalized; 
                }
            }*/
            if (input[0].AnyDirectionalInput)
            {
                // modified rideable lizards code, thanks noir <3
                if ((self.Submersion > 0.5f && self.GetLiz().aquatic) || self.room.GetTile(self.mainBodyChunk.pos).WaterSurface)
                {
                    self.mainBodyChunk.vel.y += Custom.DirVec(self.mainBodyChunk.pos, self.mainBodyChunk.pos + (inputvec * 20)).y * Mathf.Lerp(1.4f, 1.0f, 0.1f) * swimspeed; //Surprisingly, thi s works well enough.
                    self.mainBodyChunk.vel.x += Custom.DirVec(self.mainBodyChunk.pos, self.mainBodyChunk.pos + (inputvec * 20)).x * Mathf.Lerp(1.4f, 1.0f, 0.1f) * swimspeed; //Horizontal boost for some slower lizzies
                }
                self.AI.behavior = LizardAI.Behavior.FollowFriend;
                self.mainBodyChunk.vel.y += Custom.DirVec(self.mainBodyChunk.pos, self.mainBodyChunk.pos + (inputvec * 20)).y * Mathf.Lerp(1.4f, 1.0f, 0.1f) * self.LegsGripping; //Surprisingly, this works well enough.
                self.mainBodyChunk.vel.x += Custom.DirVec(self.mainBodyChunk.pos, self.mainBodyChunk.pos + (inputvec * 20)).x * Mathf.Lerp(1.4f, 1.0f, 0.1f) * runspeed * self.LegsGripping; //Horizontal boost for some slower lizzies
                self.bodyChunks[1].vel -= Custom.DirVec(self.mainBodyChunk.pos, self.mainBodyChunk.pos + (inputvec * 20)) * 0.1f * self.LegsGripping;
            }

            //if (self.animation == Lizard.Animation.PrepareToJump)
                //self.jumpModule.jumpToPoint = self.firstChunk.pos + new Vector2(input[0].x,input[0].y);
        }
    }
}