﻿using LizardCosmetics;
using RWCustom;
using TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Dependencies;
using UnityEngine;
using Color = UnityEngine.Color;

namespace TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Unique;

public class FreeWhiskers : Whiskers, IFreedCosmetic
{ // Unfinished
    public LizColorMode[] colorMode => new LizColorMode[3];
    public bool IGlow;
    public float LightUp;
    public Color lightUpColor;
    public bool darkenWithHead { get; }
    public float dark { get; set; }
    public FreeWhiskers(
        LizardGraphics lGraphics, 
        int startSprite, 
        Color lightUpColor, 
        bool IGlow = false) : base(lGraphics,startSprite)
    {
        this.IGlow = IGlow;
        this.lightUpColor = lightUpColor;
    }

    public override void Update()
    {
        LightUp = (!IGlow) ? 0 : (lGraphics.lizard.bubble > 0) ? Mathf.Min(LightUp + 0.1f, 1f) : LightUp * 0.9f;

        if (IGlow)
        {
            lGraphics.lightSource.color = lightUpColor;
            lGraphics.lightSource.alpha = 0.35f * LightUp;
        }
        
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < amount; j++)
            {
                whiskers[i, j].vel += whiskerDir(i, j, 1f) * whiskerProps[j, 2];
                if (lGraphics.lizard.room.PointSubmerged(whiskers[i, j].pos))
                {
                    whiskers[i, j].vel *= 0.8f;
                }
                else
                {
                    whiskers[i, j].vel.y -= 0.6f;
                }
                whiskers[i, j].Update();
                whiskers[i, j].ConnectToPoint(AnchorPoint(i, j, 1f), whiskerProps[j, 0], push: false, 0f, lGraphics.lizard.mainBodyChunk.vel, 0f, 0f);
                if (!RWCustom.Custom.DistLess(lGraphics.head.pos, whiskers[i, j].pos, 200f))
                {
                    whiskers[i, j].pos = lGraphics.head.pos;
                }
                whiskerLightUp[j, i, 1] = whiskerLightUp[j, i, 0];
                if (whiskerLightUp[j, i, 0] < Mathf.InverseLerp(0f, 0.3f, LightUp))
                {
                    whiskerLightUp[j, i, 0] = Mathf.Lerp(whiskerLightUp[j, i, 0], Mathf.InverseLerp(0f, 0.3f, LightUp), 0.7f) + 0.05f;
                }
                else
                {
                    whiskerLightUp[j, i, 0] -= 0.025f;
                }
                whiskerLightUp[j, i, 0] += Mathf.Lerp(-1f, 1f, Random.value) * 0.03f * LightUp;
                whiskerLightUp[j, i, 0] = Mathf.Clamp(whiskerLightUp[j, i, 0], 0f, 1f);
            }
        }
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette rPalette)
    {
        this.palette = rPalette;
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < amount; j++)
            {
                for (int k = 0; k < (sLeaser.sprites[startSprite + j * 2 + i] as TriangleMesh)!.verticeColors.Length; k++)
                {
                    (sLeaser.sprites[startSprite + j * 2 + i] as TriangleMesh)!.verticeColors[k] = lGraphics.effectColor;
                }
            }
        }
    }

    public static void WhiskerDrawSprites(LizardGraphics self, FreeWhiskers whisker, RoomCamera.SpriteLeaser sLeaser, float timeStacker)
    {
        float num2 = Mathf.Lerp(self.lizard.lastJawOpen, self.lizard.JawOpen, timeStacker);
        if (self.lizard.JawReadyForBite && self.lizard.Consious)
            num2 += Random.value * 0.2f;
        num2 = Mathf.Lerp(num2, Mathf.Lerp(self.lastVoiceVisualization, self.voiceVisualization, timeStacker) + 0.2f, 
            Mathf.Lerp(self.lastVoiceVisualizationIntensity, self.voiceVisualizationIntensity, timeStacker) * 0.8f);
        num2 = Mathf.Clamp(num2, 0f, 1f);
                
        var baseColor = self.HeadColor(timeStacker);
        var lightColor = whisker.lightUpColor;
        sLeaser.sprites[self.SpriteHeadStart + 2].color = Color.Lerp( baseColor, lightColor, Mathf.Pow(whisker.LightUp, 1f - 0.95f * num2));
    }
}