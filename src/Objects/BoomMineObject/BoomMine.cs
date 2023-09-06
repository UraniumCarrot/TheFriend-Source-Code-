using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using RWCustom;
using UnityEngine;
using UnityEngine.PlayerLoop;
using MoreSlugcats;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;

namespace TheFriend.Objects.BoomMineObject;

public class BoomMine : PlayerCarryableItem, IDrawable
{
    public BoomMineAbstract Abstr { get; }
    public Creature owner;
    public BoomMine(BoomMineAbstract abstr, Vector2 pos, Vector2 vel) : base(abstr)
    {
        Abstr = abstr;
        bodyChunks = new BodyChunk[1];
        bodyChunks = new[] { new BodyChunk(this, 0, abstractPhysicalObject.Room.realizedRoom.MiddleOfTile(abstractPhysicalObject.pos.Tile), 6f, 0.07f) { goThroughFloors = false } };
        bodyChunkConnections = new BodyChunkConnection[0];
        airFriction = 0.999f;
        gravity = 0.9f;
        bounce = 0f;
        surfaceFriction = 0f;
        collisionLayer = 2;
        waterFriction = 0.98f;
        buoyancy = 0.4f;
    }
    public bool activated = false;
    public bool detectionRadiusSpawned = false;
    public int ExplodeTimer;

    #region Cosmetics
    public Color darkColor;
    public float darkness;
    public float rotation;
    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[5];
        sLeaser.sprites[0] = new FSprite("Menu_Symbol_Arrow");
        sLeaser.sprites[1] = new FSprite("Menu_Symbol_Arrow");

        sLeaser.sprites[2] = new FSprite("Circle20");
        sLeaser.sprites[3] = new FSprite("Circle20");
        sLeaser.sprites[4] = new FSprite("Circle20");

        AddToContainer(sLeaser, rCam, null);
    }
    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        Color color = palette.blackColor;
        darkColor = color;
        darkness = palette.darkness;

        sLeaser.sprites[0].color = color;
        sLeaser.sprites[1].color = Color.Lerp(Color.Lerp(color, Color.white, 0.5f), color, darkness);
    }
    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        Color red = Color.Lerp(new Color(1f, 0.4f, 0.3f), new Color(1f, 0f, 0f), 0.5f);
        Color green = new Color(0.7f,1f,0.3f);
        Color blue = new Color(0.3f,0.7f,1f);
        Color grey = Color.Lerp(darkColor, Color.white, 0.5f);
        Color upgradeColor1;
        Color upgradeColor2;
        Color upgradeColor3;

        switch (Abstr.slot1)
        {
            default: upgradeColor1 = grey; break;
            case 0: upgradeColor1 = grey; break;
            case 1: upgradeColor1 = red; break;
            case 2: upgradeColor1 = green; break;
            case 3: upgradeColor1 = blue; break;
        }
        switch (Abstr.slot2)
        {
            default: upgradeColor2 = grey; break;
            case 0: upgradeColor2 = grey; break;
            case 1: upgradeColor2 = red; break;
            case 2: upgradeColor2 = green; break;
            case 3: upgradeColor2 = blue; break;
        }
        switch (Abstr.slot3)
        {
            default: upgradeColor3 = grey; break;
            case 0: upgradeColor3 = grey; break;
            case 1: upgradeColor3 = red; break;
            case 2: upgradeColor3 = green; break;
            case 3: upgradeColor3 = blue; break;
        }

        sLeaser.sprites[3].color = Color.Lerp(upgradeColor1, darkColor, darkness);
        sLeaser.sprites[2].color = Color.Lerp(upgradeColor2, darkColor, darkness);
        sLeaser.sprites[4].color = Color.Lerp(upgradeColor3, darkColor, darkness);

        if (blink > 0)
        {
            if (blink > 1 && Random.value < 0.5f)
            {
                sLeaser.sprites[0].color = blinkColor;
            }
            else sLeaser.sprites[0].color = darkColor;
        }


        Vector2 pos = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);
        var posX = pos.x - camPos.x;
        var posY = pos.y - camPos.y;
        float scale = 0.15f;
        for (int h = 0; h < 2; h++)
        {
            sLeaser.sprites[h].rotation = rotation;
            sLeaser.sprites[h].x = posX;
        }
        if (rotation == 45f) sLeaser.sprites[1].x = posX + 2.4f;
        if (rotation == - 45f) sLeaser.sprites[1].x = posX - 2.4f;

        sLeaser.sprites[0].scale = 1.6f;
        sLeaser.sprites[1].scale = 0.4f;
        sLeaser.sprites[0].y = posY;
        sLeaser.sprites[1].y = posY + 3.77f;

        for (int i = 2; i < 5; i++)
        {
            sLeaser.sprites[i].y = posY - 3f;
            sLeaser.sprites[i].scale = scale;
        }
        sLeaser.sprites[2].x = posX; // Middle
        if (rotation == 45f) sLeaser.sprites[2].x = posX - 1.5f;
        if (rotation == -45f) sLeaser.sprites[2].x = posX + 1.5f;
        if (rotation == 45f || rotation == -45f) sLeaser.sprites[2].y = posY - 1.5f - 0.5f;

        sLeaser.sprites[3].x = posX - 5f; // Left
        if (rotation == 45f) sLeaser.sprites[3].x = posX + 2f;
        if (rotation == -45f) sLeaser.sprites[3].x = posX - 2f;
        if (rotation == 45f || rotation == -45f) sLeaser.sprites[3].y = posY - 6f - 0.5f;

        sLeaser.sprites[4].x = posX + 5f; // Right
        if (rotation == 45f) sLeaser.sprites[4].x = posX - 6f;
        if (rotation == -45f) sLeaser.sprites[4].x = posX + 6f;
        if (rotation == 45f || rotation == -45f) sLeaser.sprites[4].y = posY + 3f - 0.7f;

        if (slatedForDeletetion || room != rCam.room)
        {
            sLeaser.CleanSpritesAndRemove();
        }
    }

    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
    {
        if (newContainer == null) newContainer = rCam.ReturnFContainer("Items");
        for (int i = 0; i < sLeaser.sprites.Length; i++) sLeaser.sprites[i].RemoveFromContainer();
        newContainer.AddChild(sLeaser.sprites[0]);
        newContainer.AddChild(sLeaser.sprites[1]);
        newContainer.AddChild(sLeaser.sprites[2]);
        newContainer.AddChild(sLeaser.sprites[3]);
        newContainer.AddChild(sLeaser.sprites[4]);
    }

    public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
    {
        base.TerrainImpact(chunk, direction, speed, firstContact);
        if (speed > 0.5f && firstContact)
        {
            room.PlaySound(SoundID.Vulture_Mask_Terrain_Impact, base.firstChunk, loop: false, Custom.LerpMap(speed, 4f, 9f, 0.2f, 1f) * 2, 1f);
            room.AddObject(new Spark(firstChunk.pos, Custom.RNV() * 10f * Random.value, color: new Color(1f, 1f, 1f), null, 20, 50));
        }
    }

    #endregion

    #region Active

    public override void Update(bool eu)
    {
        base.Update(eu);
        if (this.grabbedBy.Count != 0) ExplodeTimer = 20;
        else if (ExplodeTimer > 0)
        {
            ExplodeTimer--;
            return;
        }

        try
        {
            if (room != null && this.grabbedBy.Count == 0)
            {
                if (room?.IdentifySlope(base.firstChunk.pos) == Room.SlopeDirection.DownRight) rotation = 45f;
                else if (room?.IdentifySlope(base.firstChunk.pos) == Room.SlopeDirection.UpLeft) rotation = -45f;
                else rotation = 45f;
                if (room?.GetTile(base.firstChunk.pos).Terrain == Room.Tile.TerrainType.Floor || room?.GetTile(base.firstChunk.pos).Terrain == Room.Tile.TerrainType.Air) rotation = 0f;
                if (room?.GetTile(base.firstChunk.pos).Terrain == Room.Tile.TerrainType.Slope) base.firstChunk.vel = Vector2.zero;

                for (int i = 0; i < room?.abstractRoom?.creatures?.Count(); i++)
                {
                    var crit = room?.abstractRoom?.creatures[i]?.realizedCreature;
                    if (crit != null && 
                        (crit != owner || 
                         (this.room.game.IsStorySession && 
                          crit is not Player)) && 
                        crit is not Fly && 
                        crit is not JetFish && 
                        crit is not GarbageWorm && 
                        Custom.DistLess(
                            (crit is Lizard) ? crit.bodyChunks[1].pos : crit.mainBodyChunk.pos, 
                            this.firstChunk.pos + Vector2.up * 3f, 
                            50f) && 
                        Abstr.slot1 != 0 && 
                        !(crit.dead))
                    {
                        if (!(room.abstractRoom.shelter))
                        {
                            Explosion(crit);
                            return;
                        }
                    }
                }
            }
            if (this.grabbedBy.Count > 0 && this.grabbedBy[0].grabber != owner) owner = this.grabbedBy[0].grabber;
        }
        catch(Exception e) { Debug.Log("Solace: Harmless Exception occured related to BoomMine"); Debug.LogException(e); }
    }
    public void Explosion(Creature victim)
    {
        Debug.Log("Solace: BOOM, BABY! Mine exploded!");
        Effects(victim);
        Abstr.slot1 = 0;
        Abstr.slot2 = 0;
        Abstr.slot3 = 0;
    }
    public void Effects(Creature victim)
    {
        Color sporeColor = Color.Lerp(base.color, new Color(0.02f, 0.1f, 0.08f), 0.85f);
        Color explodeColor = new Color(1f, 0.4f, 0.3f);

        int redStrength = 0;
        int greenStrength = 0;
        int blueStrength = 0;
        if (Abstr.slot1 == 1) redStrength += 1;
        if (Abstr.slot2 == 1) redStrength += 1;
        if (Abstr.slot3 == 1) redStrength += 1;

        if (Abstr.slot1 == 2) greenStrength += 1;
        if (Abstr.slot2 == 2) greenStrength += 1;
        if (Abstr.slot3 == 2) greenStrength += 1;

        if (Abstr.slot1 == 3) blueStrength += 1;
        if (Abstr.slot2 == 3) blueStrength += 1;
        if (Abstr.slot3 == 3) blueStrength += 1;

        if (redStrength != 0)
        {
            Debug.Log("Solace: Red damage! Strength " + redStrength);
            room.AddObject(new SootMark(room, firstChunk.pos, 80f, bigSprite: true));
            room.AddObject(new Explosion(room, this, firstChunk.pos, 7, 80f * redStrength, 2f * redStrength, 1.5f * redStrength, 60 * redStrength, 0.25f, owner, 0.7f, 160f, 1f));
            room.AddObject(new Explosion.ExplosionLight(firstChunk.pos, 80f * redStrength, 1f, 7, explodeColor));
            room.AddObject(new Explosion.ExplosionLight(firstChunk.pos, 60f * redStrength, 1f, 3, new Color(1f, 1f, 1f)));
            room.AddObject(new ExplosionSpikes(room, firstChunk.pos, 14, 30f, 9f, 7f, 170f, explodeColor));
            room.AddObject(new ShockWave(firstChunk.pos, 110f * redStrength, 0.045f, 5));

            room.ScreenMovement(firstChunk.pos, default, 0.3f * redStrength);
            room.PlaySound(SoundID.Bomb_Explode, firstChunk.pos, redStrength, 1);
            room.InGameNoise(new Noise.InGameNoise(firstChunk.pos, 2000f * redStrength, this, 1f));
        }
        if (greenStrength != 0)
        {
            Debug.Log("Solace: Green damage! Strength " + greenStrength);
            InsectCoordinator smallInsects = null;
            for (int i = 0; i < room.updateList.Count; i++)
            {
                if (room.updateList[i] is InsectCoordinator)
                {
                    smallInsects = room.updateList[i] as InsectCoordinator;
                    break;
                }
            }
            for (int j = 0; j < 70 * (greenStrength*greenStrength); j++)
            {
                room.AddObject(new SporeCloud(firstChunk.pos, Custom.RNV() * Random.value * 10f, sporeColor, greenStrength, owner.abstractCreature, j % 20, smallInsects));
            }
            room.AddObject(new SporePuffVisionObscurer(firstChunk.pos));
            room.PlaySound(SoundID.Puffball_Eplode, firstChunk.pos, greenStrength, 1);
        }
        if (blueStrength != 0)
        {
            int stun = Mathf.RoundToInt((150 * (blueStrength*blueStrength)));
            var bolt = new LightningBolt(victim.firstChunk.pos, victim.firstChunk.pos + (50 * Vector2.up * blueStrength), 1, 1f, 0.5f, 1f, 0.61f, true);
            Debug.Log("Solace: Blue damage! Strength " + blueStrength);
            room.AddObject(bolt);
            room.AddObject(new Explosion.ExplosionLight(victim.firstChunk.pos + (50 * Vector2.up * blueStrength), 200f, 1f, 4, new Color(0f, 0.3f, 1f)));
            Vector2 vector = Custom.DegToVec(360f * Random.value);
            room.AddObject(new MouseSpark(victim.firstChunk.pos + (50 * Vector2.up * blueStrength), victim.firstChunk.vel + vector * 36f * Random.value, 20f, new Color(0f, 0.3f, 1f)));
            
            bolt.type = 0;
            room.PlaySound(SoundID.Jelly_Fish_Tentacle_Stun, victim.firstChunk, loop: false, 1f, 1f);
            if (victim is not Centipede && victim is not DaddyLongLegs)
            {
                victim.Stun(stun);
                room.AddObject(new CreatureSpasmer(victim, allowDead: false, stun));
                room.PlaySound(SoundID.Death_Lightning_Spark_Spontaneous, victim.firstChunk, loop: false, 1f, 1f);
                room.PlaySound(SoundID.Zapper_Zap, victim.firstChunk, loop: false, 1f, 1f);
                Debug.Log("Solace: Creature stunned! Length: " + stun);
            }
        }
    }
    #endregion
}
