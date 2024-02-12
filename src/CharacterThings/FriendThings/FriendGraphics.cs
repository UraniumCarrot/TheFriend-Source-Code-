using TheFriend.SlugcatThings;
using UnityEngine;
using System.Linq;
using On.JollyCoop.JollyMenu;
using bod = Player.BodyModeIndex;
using ind = Player.AnimationIndex;


namespace TheFriend.FriendThings;

public class FriendGraphics
{
    public static void FriendDrawSprites(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, FSprite head, FSprite legs)
    {
        if (!self.RenderAsPup)
        {
            if (!head.element.name.Contains("Friend") && 
                head.element.name.StartsWith("HeadA")) 
                head.SetElementByName("Friend" + head.element.name);
            if (!legs.element.name.Contains("Friend") && 
                legs.element.name.StartsWith("LegsA")) 
                legs.SetElementByName("Friend" + legs.element.name);
        }
    }

    public static void FriendTailCtor(PlayerGraphics self)
    {
        if (self.RenderAsPup)
        {
            self.tail[0] = new TailSegment(self, 8f, 2f, null, 0.85f, 1f, 1f, true);
            self.tail[1] = new TailSegment(self, 6f, 3.5f, self.tail[0], 0.85f, 1f, 0.5f, true);
            self.tail[2] = new TailSegment(self, 4f, 3.5f, self.tail[1], 0.85f, 1f, 0.5f, true);
            self.tail[3] = new TailSegment(self, 2f, 3.5f, self.tail[2], 0.85f, 1f, 0.5f, true);
        }
        else
        {
            self.tail[0] = new TailSegment(self, 9f, 4f, null, 0.85f, 1f, 1f, true);
            self.tail[1] = new TailSegment(self, 7f, 7f, self.tail[0], 0.85f, 1f, 0.5f, true);
            self.tail[2] = new TailSegment(self, 4f, 7f, self.tail[1], 0.85f, 1f, 0.5f, true);
            self.tail[3] = new TailSegment(self, 1f, 7f, self.tail[2], 0.85f, 1f, 0.5f, true);
        }
        var bp = self.bodyParts.ToList();
        bp.RemoveAll(x => x is TailSegment);
        bp.AddRange(self.tail);
        self.bodyParts = bp.ToArray();
    }
    
    public static void FriendGraphicsUpdate(PlayerGraphics self)
    {
        if ((self.player.bodyMode == bod.Crawl || 
             self.player.standing || 
             self.player.bodyMode == bod.Stand) && 
            !self.player.GetGeneral().isRidingLizard && 
            self.player.bodyMode != bod.Default && 
            self.player.GetFriend().poleSuperJumpTimer < 20)
        {
            self.tail[self.tail.Length - 1].vel.y += (self.player.standing || self.player.bodyMode == bod.Stand) ? 0.9f : 1f;
            self.tail[self.tail.Length - 3].vel.y -= (self.player.bodyMode != bod.Crawl && Mathf.Abs(self.player.firstChunk.vel.x) > 3.2f) ? 2.5f : 0f;
            if (self.player.GetFriend().WantsUp) { self.tail[self.tail.Length - 1].vel.y += 0.2f; self.lookDirection += new Vector2(0, 1f); }
        }
        if (self.player.GetFriend().WantsUp) self.head.vel.y += 1;

        if (self.player.animation == ind.StandOnBeam && self.player.GetFriend().poleSuperJumpTimer >= 20)
        {
            if (self.player.input[0].y > 0 ||
                self.player.GetFriend().upwardpolejump ||
                self.player.GetFriend().WantsUp ||
                self.player.GetFriend().YesIAmLookingUpStopThinkingOtherwise)
            {
                self.lookDirection += new Vector2(0, 0.5f); 
                self.tail[self.tail.Length - 1].vel.y += 0.4f;
                self.head.vel.y += 1;
            }
            self.tail[self.tail.Length - 1].vel.y += 1f;
            self.tail[self.tail.Length - 1].vel.x += self.player.flipDirection * -0.8f;
        }
    }
}