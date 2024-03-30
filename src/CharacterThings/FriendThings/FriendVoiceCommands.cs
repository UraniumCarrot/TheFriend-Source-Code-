using System.Collections.Generic;
using System.Linq;
using MoreSlugcats;
using RWCustom;
using TheFriend.Creatures.LizardThings.DragonRideThings;
using TheFriend.FriendThings;
using TheFriend.Objects;
using TheFriend.Objects.FreedLizardBubbleObject;
using UnityEngine;

namespace TheFriend.CharacterThings.FriendThings;

public class FriendVoiceCommands
{
    public static void FriendCommandingBark(Player self, FriendCWT.Friend data, int taps)
    {
        if (data.VoiceCooldown == 80)
        {
            switch (taps)
            {
                case 2: FriendFollowMe(self); break;
                case 3: FriendLeaveMe(self); break;
                case >= 4 and < 100: FriendRunAway(self); break;
                case 100: FriendPainHelpMe(self); break;
            }
        }
        FriendTalk(self.graphicsModule as PlayerGraphics);
    }
    
    public static void FriendFollowMe(Player self)
    {
        Debug.Log("Solace: Friend command Follow");
        self.GetFriend().voice.FriendMakeSound(FriendVoice.FriendEmotion.Follow);
        self.room.AddObject(new EmotionParticle(
            self.graphicsModule.HypothermiaColorBlend(self.ShortCutColor()), 
            self.firstChunk.pos,
            FriendGetEmotionDirection(self),
            "FriendA"));
        foreach (Lizard liz in FriendFindReactors(self, 1000, 800))
            if (liz.TryGetLiz(out var data))
            {
                var relation = liz.abstractCreature.state.socialMemory.GetOrInitiateRelationship(self.abstractCreature.ID);
                if (relation.like > 0.5f && data.TemporaryFriendshipTimer == 0) return;
                data.TemporaryFriendshipTimer = 1000;
                relation.InfluenceLike(1000);
                relation.InfluenceTempLike(1000);
                
                if (MMF.cfgExtraLizardSounds.Value) liz.voice.MakeSound(MMFEnums.LizardVoiceEmotion.Love);
            }
    }
    public static void FriendLeaveMe(Player self)
    {
        Debug.Log("Solace: Friend command Leave");
        self.GetFriend().voice.FriendMakeSound(FriendVoice.FriendEmotion.Stop);
        self.room.AddObject(new EmotionParticle(
            self.graphicsModule.HypothermiaColorBlend(self.ShortCutColor()), 
            self.firstChunk.pos,
            FriendGetEmotionDirection(self),
            "TravellerA"));
        foreach (Lizard liz in FriendFindReactors(self, 1000, 800))
            if (liz.TryGetLiz(out var data))
                if (liz.AI.friendTracker.friendRel != null && data.TemporaryFriendshipTimer > 0)
                {
                    liz.AI.friendTracker.friendRel.like = 0;
                    liz.AI.friendTracker.friendRel.tempLike = 0;
                    liz.AI.friendTracker.friend = null;
                    liz.AI.friendTracker.friendRel = null;
                    Debug.Log("Solace: Artificially tamed lizard has stopped following");
                    liz.voice.MakeSound(LizardVoice.Emotion.Boredom);
                }
    }
    public static void FriendRunAway(Player self)
    {
        Debug.Log("Solace: Friend command Run!");
        self.GetFriend().voice.FriendMakeSound(FriendVoice.FriendEmotion.Fear);
        self.room.AddObject(new EmotionParticle(
            self.graphicsModule.HypothermiaColorBlend(self.ShortCutColor()),
            self.firstChunk.pos,
            FriendGetEmotionDirection(self),
            "HunterA"));
        List<AbstractCreature> exclusionList = new List<AbstractCreature>();
        foreach (Lizard liz in FriendFindReactors(self, 1000, 800))
            if (liz.TryGetLiz(out var data))
                if (liz.AI.friendTracker.friendRel != null)
                    if (data.TemporaryFriendshipTimer > 0)
                    {
                        liz.AI.friendTracker.friendRel.like = 0;
                        liz.AI.friendTracker.friendRel.tempLike = 0;
                        liz.AI.friendTracker.friend = null;
                        liz.AI.friendTracker.friendRel = null;
                        Debug.Log("Solace: Artificially tamed lizard has stopped following");
                        liz.voice.MakeSound(LizardVoice.Emotion.Fear);
                    }
                    else exclusionList.Add(liz.abstractCreature);

        if (FriendFindReactors(self, 1000, 800).Count() > 0)
        {
            if (!self.room.updateList.Exists(x => x is SpecificScareObject obj && obj.species == CreatureTemplate.Type.LizardTemplate)) 
                self.room.AddObject(
                    new SpecificScareObject(
                        self.firstChunk.pos, 
                        CreatureTemplate.Type.LizardTemplate, 
                        800, 
                        200) 
                        { exclusionList = exclusionList });
            else
            {
                var obj = self.room.updateList.Find(x =>
                    x is SpecificScareObject obj && obj.species == CreatureTemplate.Type.LizardTemplate);
                var scare = obj as SpecificScareObject;
                scare.exclusionList = exclusionList;
                scare.pos = self.firstChunk.pos;
                scare.ResetLifetime();
            } 
        }
    }

    #region VoiceTools
    public static void FriendTalk(PlayerGraphics self, float intensity = 2.5f)
    {
        if (Random.value > 0.7f)
        {
            NonLizardBubble bubble =
                new NonLizardBubble(self.head.pos, self.head.lastPos, self.player.firstChunk, 0f, 0.2f, 0f);
            bubble.distanceLimit = 1.3f;
            bubble.color = self.HypothermiaColorBlend(self.player.ShortCutColor());
            bubble.vel = (Custom.DegToVec(Random.value * 360f) * Mathf.Lerp(4f, 6f, Random.value)) * 0.3f;
            bubble.lifeTime = (int)Mathf.Lerp(10f, 15f, Random.value);
            self.player.room.AddObject(bubble);
        }
        self.HeadShiver(intensity);
        self.player.Blink(5);
    }
    public static Vector2 FriendGetEmotionDirection(Player self)
    {
        Vector2 result;
        Vector2 look = (self.graphicsModule as PlayerGraphics).lookDirection;
        if (look != Vector2.zero)
            result = (self.bodyMode == Player.BodyModeIndex.Crawl) ? new Vector2(self.ThrowDirection, look.y).normalized : look.normalized;
        else result = new Vector2(self.ThrowDirection, 0.5f);

        return result;
    }

    public static void FriendGrabAlerter(Player self)
    {
        if (self.TryGetFriend(out var data))
        {
            data.VoiceCooldown = 80;
            FriendCommandingBark(self, data, 100);
        }
    }

    public static List<Lizard> FriendFindReactors(Player self, float noiseStrength, float reactionRadius)
    {
        
        List<Lizard> result = new List<Lizard>();
        Noise.InGameNoise noise = new Noise.InGameNoise(self.firstChunk.pos, noiseStrength, self, 1f);
        self.room.InGameNoise(noise);
        foreach (AbstractCreature crit in self.room.abstractRoom.creatures.FindAll(x => x.realizedCreature is Lizard))
        {
            var liz = crit.realizedCreature as Lizard;
            if (liz != null)
                if (Custom.DistLess(self.firstChunk.pos, liz.AveragedPosition(), reactionRadius))
                    if (!liz.dead &&
                        !liz.Stunned &&
                        liz.Deaf < 0.5f)
                        result.Add(liz);
        }
        return result;
    }
    #endregion
    
    // Atypical Friend voice lines
    
    public static void FriendWhimperWhileStunLoop(Player self, FriendCWT.Friend data)
    {
        if (data.VoiceCooldown <= 0) data.VoiceCooldown = 150;
        if (data.VoiceCooldown > 110)
        {
            data.voice.pitchOverride = Random.Range(1.5f, 1.8f);
            FriendTalk(self.graphicsModule as PlayerGraphics, 1f);
        }
        if (data.VoiceCooldown == 150)
            FriendWhimper(self,data);
    }
    public static void FriendWhimper(Player self, FriendCWT.Friend data)
    {
        self.GetFriend().voice.FriendMakeSound(FriendVoice.FriendEmotion.Whimper);
        
        foreach (Lizard liz in FriendFindReactors(self, 500, 200))
            if (liz.TryGetLiz(out var lizdata))
            {
                var relation = liz.abstractCreature.state.socialMemory.GetOrInitiateRelationship(self.abstractCreature.ID);
                if (relation.like > 0.5f && lizdata.TemporaryFriendshipTimer == 0) return;
                lizdata.TemporaryFriendshipTimer = 1000;
                relation.InfluenceLike(1000);
                relation.InfluenceTempLike(1000);
                
                liz.voice.MakeSound(LizardVoice.Emotion.BloodLust);
            }
    }
    public static void FriendPainHelpMe(Player self)
    {
        Debug.Log("Solace: Friend command Assist");
        self.GetFriend().voice.FriendMakeSound(FriendVoice.FriendEmotion.Pain);
        self.room.AddObject(new EmotionParticle(
            self.graphicsModule.HypothermiaColorBlend(self.ShortCutColor()), 
            self.firstChunk.pos,
            FriendGetEmotionDirection(self),
            "SurvivorA"));
        foreach (Lizard liz in FriendFindReactors(self, 500, 200))
            if (liz.TryGetLiz(out var data))
            {
                var relation = liz.abstractCreature.state.socialMemory.GetOrInitiateRelationship(self.abstractCreature.ID);
                if (relation.like > 0.5f && data.TemporaryFriendshipTimer == 0) return;
                data.TemporaryFriendshipTimer = 1000;
                relation.InfluenceLike(1000);
                relation.InfluenceTempLike(1000);
                
                liz.voice.MakeSound(LizardVoice.Emotion.BloodLust);
            }
    }
}