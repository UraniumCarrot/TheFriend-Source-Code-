using TheFriend.SlugcatThings;
using UnityEngine;
using bod = Player.BodyModeIndex;
using ind = Player.AnimationIndex;


namespace TheFriend.FriendThings;

public class FriendGraphics
{
    public static void Apply()
    {
        
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