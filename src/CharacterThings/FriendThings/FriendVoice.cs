using System;
using MoreSlugcats;
using UnityEngine;
using RWCustom;
using TheFriend.FriendThings;
using Random = UnityEngine.Random;

namespace TheFriend.CharacterThings.FriendThings;

public class FriendVoice : CreatureVoice
{
    public class FriendEmotion : ExtEnum<FriendEmotion>
    {
        public FriendEmotion(string value, bool register = false) : base(value, register){}

        public static readonly FriendEmotion Fear = new FriendEmotion("Fear", true);
        public static readonly FriendEmotion Follow = new FriendEmotion("Follow", true);
        public static readonly FriendEmotion Stop = new FriendEmotion("Stop", true);
        public static readonly FriendEmotion Pain = new FriendEmotion("Pain", true);
        public static readonly FriendEmotion Whimper = new FriendEmotion("Whimper", true);
    }
    public class FriendArticulation
    {
        public FriendVoice voice;
        public float length;
        public float maxVolume;
        public float[,] modifier;
        public FriendArticulation(FriendVoice voice, float length, float maxVolume)
        {
            this.voice = voice;
            this.length = length;
            this.maxVolume = maxVolume;
            modifier = new float[2 + (int)(length / 20f), 2];
            for (int i = 0; i < modifier.GetLength(0); i++)
            {
                modifier[i, 0] = Random.value;
                modifier[i, 1] = Random.value;
            }
        }

        public float ReturnMod(float f, int m)
        {
            f *= modifier.GetLength(0) - 1;
            int num = Mathf.FloorToInt(f);
            f -= num;
            return Mathf.Lerp(modifier[num, m], modifier[Math.Min(num + 1, modifier.GetLength(0) - 1), m], f);
        }
    }

    public Player owner;
    public float? pitchOverride;
    public float myPitch;
    public float currentArticulationProgression;
    public int articulationIndex = -1;
    public float currentEmotionIntensity;
    public SoundID myVoiceTrigger;
    public FriendArticulation[] articulations;
    public FriendArticulation currentArt => articulations[articulationIndex];
    
    public FriendVoice(Player self) : base(self, self.firstChunk.index)
    {
        Debug.Log("Solace: Friend voice constructed");
        owner = self;
        articulations = new FriendArticulation[ExtEnum<FriendEmotion>.values.Count];
        Random.State state = Random.state;
        Random.InitState(self.abstractCreature.ID.number);
        myVoiceTrigger = FriendVoiceID();
        myPitch = Custom.ClampedRandomVariation(0.5f, 0.2f, 0.8f) * 2f;


        float randomLength = Mathf.Lerp(0.5f, 1.5f, Random.value);
        FriendArticulation friendArt = new FriendArticulation(this, 80f, 1f);
        for (int i = 0; i < articulations.Length; i++)
        {
            FriendEmotion emote = new FriendEmotion(ExtEnum<FriendEmotion>.values.GetEntry(i));
            float length = 40f;
            float volume = 0.5f;

            switch (emote.value)
            {
                case nameof(FriendEmotion.Fear):
                    length = 40f;
                    volume = 0.5f; break;
                case nameof(FriendEmotion.Stop):
                    length = 35f;
                    volume = 0.5f; break;
                case nameof(FriendEmotion.Pain):
                    length = 50f;
                    volume = 0.8f; break;
                case nameof(FriendEmotion.Follow):
                    length = 50f;
                    volume = 0.5f; break;
                case nameof(FriendEmotion.Whimper):
                    length = 50f;
                    volume = 0.2f; break;
            }
            if (emote == FriendEmotion.Follow) // Don't ask me why, but this apparently stops everything from breaking.
                articulations[i] = new FriendArticulation(this, length * randomLength, volume);
            else if (emote == FriendEmotion.Whimper)
                articulations[i] = new FriendArticulation(this, 25f, volume);
            else articulations[i] = new FriendArticulation(this, length * randomLength, volume);
            for (int a = 0; a < articulations[i].modifier.GetLength(0); a++)
            {
                float strange = (float)a / (articulations[i].modifier.GetLength(0) - 1);
                articulations[i].modifier[a, 0] = Mathf.Lerp(articulations[i].modifier[a, 0], friendArt.ReturnMod(strange, 0), 0.5f);
                articulations[i].modifier[a, 1] = Mathf.Lerp(articulations[i].modifier[a, 1], friendArt.ReturnMod(strange, 1), 0.5f);
            }
        }
        Random.state = state;
    }

    public SoundID FriendVoiceID()
    {
        string name;
        if (MMF.cfgExtraLizardSounds.Value)
            name = "Black";
        else name = "Green";
        return new SoundID("Lizard_Voice_" + name + "_A");
    }
    public SoundID FriendLoveID()
    {
        return new SoundID("Lizard_Voice_Black_A");
    }

    public void FriendMakeSound(FriendEmotion emote, float intensity = -1)
    {
        if (intensity < 0) intensity = Mathf.Lerp(0.8f, 1.2f, Random.value);
        bool iLove = MMF.cfgExtraLizardSounds.Value && emote == FriendEmotion.Follow;
        SoundID myVoice = myVoiceTrigger;
        
        if (iLove) myVoice = FriendLoveID();
        
        currentArticulationProgression = 0f;
        articulationIndex = emote.index;
        soundID = myVoice;
        currentEmotionIntensity = intensity;
    }

    public override void Update()
    {
        base.Update();
        if (articulationIndex > -1)
        {
            currentArticulationProgression += 1f / (currentArt.length * Custom.LerpMap(currentEmotionIntensity, 0f, 2f, 0.5f, 1.5f));
            if (currentArticulationProgression >= 1f)
            {
                soundID = SoundID.None;
                articulationIndex = -1;
            }
            else
            {
                Volume = currentArt.ReturnMod(currentArticulationProgression, 0) * currentArt.maxVolume * Custom.LerpMap(currentEmotionIntensity, 0f, 2f, 0.4f, 3f);
                Pitch = Mathf.Lerp(0.5f, 1.5f, currentArt.ReturnMod(currentArticulationProgression, 1)) * myPitch;
                if (pitchOverride != null ) soundA.pitch = pitchOverride.Value;
                else soundA.pitch += 1f;
            }
        }
    }
}