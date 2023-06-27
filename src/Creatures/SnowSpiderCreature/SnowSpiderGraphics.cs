using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using IL;
using On;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;

namespace TheFriend.Creatures.SnowSpiderCreature;

public class SnowSpiderGraphics : BigSpiderGraphics
{
    public static void Apply()
    {
        On.BigSpiderGraphics.ScaleAttachPos += BigSpiderGraphics_ScaleAttachPos;
        On.BigSpider.Violence += BigSpider_Violence;
    }

    // Overrides
    public static float originalBodyThickness;
    public SnowSpiderGraphics(PhysicalObject ow) : base(ow)
    {
        originalBodyThickness = 1.2f + Random.value * 3 / 9;
        bug = ow as SnowSpider;
        tailEnd = new GenericBodyPart(this, 3f, 0.5f, 0.99f, bug.bodyChunks[1]);
        lastDarkness = -1f;
        legLength = 65f;
        mandibles = new GenericBodyPart[2];
        for (int i = 0; i < mandibles.GetLength(0); i++)
        {
            mandibles[i] = new GenericBodyPart(this, 1f, 0.5f, 0.9f, owner.bodyChunks[0]);
        }
        legs = new Limb[2, 4];
        legFlips = new float[2, 4, 2];
        legsTravelDirs = new Vector2[2, 4];
        for (int j = 0; j < legs.GetLength(0); j++)
        {
            for (int k = 0; k < legs.GetLength(1); k++)
            {
                legs[j, k] = new Limb(this, bug.mainBodyChunk, j * 4 + k, 0.1f, 0.7f, 0.99f, 12f, 0.95f);
            }
        }
        bodyParts = new BodyPart[11];
        bodyParts[0] = tailEnd;
        bodyParts[1] = mandibles[0];
        bodyParts[2] = mandibles[1];
        int num = 3;
        for (int l = 0; l < legs.GetLength(0); l++)
        {
            for (int m = 0; m < legs.GetLength(1); m++)
            {
                bodyParts[num] = legs[l, m];
                num++;
            }
        }
        totalScales = 1;
        Random.State state = Random.state;
        Random.InitState(bug.abstractCreature.ID.RandomSeed);
        scales = new Vector2[20 + Random.Range(16, Random.Range(20, 28))][,];
        scaleStuckPositions = new Vector2[scales.Length];
        scaleSpecs = new Vector2[scales.Length, 2];
        legsThickness = 1.2f;
        bodyThickness = 2f + Random.value;
        if (Random.value < 0.5f)
        {
            deadLeg = new IntVector2(Random.Range(0, 2), Random.Range(0, 4));
        }
        num = 0;
        for (int n = 0; n < scales.Length; n++)
        {
            scaleSpecs[n, 0] = new Vector2(0.5f + 0.5f * Random.value, Mathf.Lerp(3f, 15f, Random.value)); //Scale length/how many there are?
            scaleSpecs[n, 1] = Custom.RNV();
            scales[n] = new Vector2[Random.Range(2, Random.Range(3, 5)), 4];
            totalScales += scales[n].GetLength(0);
            for (int num2 = 0; num2 < scales[n].GetLength(0); num2++)
            {
                scales[n][num2, 3].x = num;
                num++;
            }
            scaleStuckPositions[n] = new Vector2(Mathf.Lerp(-0.5f, 0.5f, Random.value), Mathf.Lerp(Random.value, Mathf.InverseLerp(0f, scales.Length - 1, n), 0.5f));
        }
        Random.state = state;
        soundLoop = new ChunkDynamicSoundLoop(bug.mainBodyChunk);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        legsThickness = 1.2f;

        for (int m = 0; m < scales.Length; m++)
        {
            float num2 = Mathf.Lerp(lastMandiblesCharge, mandiblesCharge, timeStacker);
            Vector2 vector2 = Vector2.Lerp(bug.bodyChunks[1].lastPos, bug.bodyChunks[1].pos, timeStacker) + Custom.RNV() * Random.value * 3.5f * num2;
            Vector2 vector17 = Vector2.Lerp(Vector2.Lerp(tailEnd.lastPos, tailEnd.pos, timeStacker), vector2, scaleStuckPositions[m].y);
            for (int n = 0; n < scales[m].GetLength(0); n++)
            {
                Vector2 vector18 = Vector2.Lerp(scales[m][n, 1], scales[m][n, 0], timeStacker);
                float num8 = Mathf.InverseLerp(0f, scales[m].GetLength(0) - 1, n);
                sLeaser.sprites[FirstScaleSprite + (int)scales[m][n, 3].x].x = vector17.x - camPos.x;
                sLeaser.sprites[FirstScaleSprite + (int)scales[m][n, 3].x].y = vector17.y - camPos.y;
                sLeaser.sprites[FirstScaleSprite + (int)scales[m][n, 3].x].rotation = Custom.AimFromOneVectorToAnother(vector17, vector18);
                sLeaser.sprites[FirstScaleSprite + (int)scales[m][n, 3].x].scaleY = Vector2.Distance(vector18, vector17) / sLeaser.sprites[FirstScaleSprite + (int)scales[m][n, 3].x].element.sourcePixelSize.y;
                sLeaser.sprites[FirstScaleSprite + (int)scales[m][n, 3].x].scaleX = Mathf.Lerp(0.28f, Mathf.Lerp(1.5f, 0.5f, num8), bug.State.health); // SCALE WIDTH!!!
                vector17 = vector18;
            }
        }
    }
    public static Color bodycolor;
    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        base.ApplyPalette(sLeaser, rCam, palette);
        blackColor = Color.Lerp(Color.white, palette.blackColor, palette.darkness * 1.5f);
        bodycolor = blackColor;
        for (int i = 0; i < sLeaser.sprites.Length; i++) sLeaser.sprites[i].color = blackColor;
        for (int j = 0; j < 2; j++)
        {
            for (int k = 0; k < 4; k++)
            {
                (sLeaser.sprites[MandibleSprite(j, 1)] as CustomFSprite).verticeColors[k] = blackColor;
            }
        }
        for (int l = 0; l < scales.Length; l++)
        {
            for (int m = 0; m < scales[l].GetLength(0); m++)
            {
                float num2 = (Mathf.InverseLerp(0f, scales[l].GetLength(0) - 1, m) + Mathf.InverseLerp(0f, 5f, m)) / 2f;
                sLeaser.sprites[FirstScaleSprite + (int)scales[l][m, 3].x].color = Color.Lerp(blackColor, yellowCol, num2 * Mathf.Lerp(0.3f, 0.9f, scaleSpecs[l, 0].x));
            }
        }
    }
    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser, rCam);
        sLeaser.sprites[HeadSprite].scaleX = 0.8f;
        sLeaser.sprites[HeadSprite].scaleY = 1f;
        for (int i = 0; i < legs.GetLength(0); i++)
        {
            for (int j = 0; j < legs.GetLength(1); j++)
            {
                sLeaser.sprites[LegSprite(i, j, 0)] = new FSprite("CentipedeLegA"); //("CentipedeLegA");
                sLeaser.sprites[LegSprite(i, j, 1)] = new FSprite("SpiderLeg" + j + "A");
                if (j == 0)
                {
                    sLeaser.sprites[LegSprite(i, j, 2)] = new FSprite("CentipedeLegB");
                }
                else
                {
                    sLeaser.sprites[LegSprite(i, j, 2)] = new FSprite("SpiderLeg" + j + "B");
                }
            }
        }
        for (int k = 0; k < mandibles.Length; k++)
        {
            sLeaser.sprites[MandibleSprite(k, 0)] = new FSprite("SpiderLeg0A"); //("SpiderLeg0A");
            sLeaser.sprites[MandibleSprite(k, 1)] = new CustomFSprite("SpiderLeg0B"); //("SpiderLeg0B");
        }
        for (int l = 0; l < totalScales; l++)
        {
            sLeaser.sprites[FirstScaleSprite + l] = new FSprite("guardianArm"); //("LizardScaleA1");
            sLeaser.sprites[FirstScaleSprite + l].anchorY = 0f;
        }
        AddToContainer(sLeaser, rCam, null);
    }

    // Hooks
    public static Vector2 BigSpiderGraphics_ScaleAttachPos(On.BigSpiderGraphics.orig_ScaleAttachPos orig, BigSpiderGraphics self, int scl, float timeStacker)
    {
        if (self is SnowSpiderGraphics)
        {
            float num = Mathf.Lerp(self.lastFlip, self.flip, timeStacker) * 0.25f;
            float num2 = Mathf.InverseLerp(0.2f, 0f, self.scaleStuckPositions[scl].y);
            Vector2 vector = Vector2.Lerp(self.bug.bodyChunks[1].lastPos, self.bug.bodyChunks[1].pos, timeStacker) + Custom.RNV() * Random.value * 3.5f * Mathf.Lerp(self.lastMandiblesCharge, self.mandiblesCharge, timeStacker);
            vector = Vector2.Lerp(vector, Vector2.Lerp(self.bug.bodyChunks[0].lastPos, self.bug.bodyChunks[0].pos, timeStacker), Mathf.InverseLerp(0.5f, 1f, self.scaleStuckPositions[scl].y) * 0.7f);
            Vector2 vector2 = Custom.DirVec(vector, Vector2.Lerp(self.bug.bodyChunks[0].lastPos, self.bug.bodyChunks[0].pos, timeStacker));
            return Vector2.Lerp(vector, Vector2.Lerp(self.tailEnd.lastPos, self.tailEnd.pos, timeStacker), num2 * 0.6f) + Custom.PerpendicularVector(vector2) * (self.scaleStuckPositions[scl].x - 0.5f * num) * Mathf.Pow(Mathf.Clamp01(Mathf.Sin(self.scaleStuckPositions[scl].y * (float)Math.PI)), 2f) * 14f + vector2 * Mathf.Lerp(-1f, 1f, self.scaleStuckPositions[scl].y) * 14f * Mathf.Pow(1f - num2, 2f);
        }
        return orig(self, scl, timeStacker);
    }
    public static void BigSpider_Violence(On.BigSpider.orig_Violence orig, BigSpider self, BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos hitAppendage, Creature.DamageType type, float damage, float stunBonus)
    {
        orig(self, source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
        if (self.Template.type == CreatureTemplateType.SnowSpider && !self.dead)
        {
            InsectCoordinator smallInsects = null;
            for (int i = 0; i < self.room.updateList.Count; i++)
            {
                if (self.room.updateList[i] is InsectCoordinator)
                {
                    smallInsects = self.room.updateList[i] as InsectCoordinator;
                    break;
                }
            }
            for (int i = 0; i < 5; i++)
            {
                SporeCloud cloud = new SporeCloud(self.firstChunk.pos, Custom.RNV() * Random.value * 2f - new Vector2(0, 1), bodycolor, damage > 0.5 ? 0.8f : 0.45f, null, i % 20, smallInsects);
                cloud.nonToxic = true;
                self.room.AddObject(cloud);
                Debug.Log("Solace: Cloud spawned for SnowSpider");
            }
        }
    }
}
