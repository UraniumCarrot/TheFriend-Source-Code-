using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RWCustom;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;

namespace TheFriend.Objects.LittleCrackerObject;

public class LittleCracker : Rock
{
    public Color explodeColor = new Color(1f, 0.4f, 0.3f);
    public Color redColor = Color.Lerp(new Color(1f, 0.4f, 0.3f), new Color(1f, 0f, 0f), 0.5f);
    public LittleCrackerAbstract Abstr { get; }
    public LittleCracker(LittleCrackerAbstract abstr, Vector2 pos, Vector2 vel) : base(abstr, abstr.world)
    {
        Abstr = abstr;
        bodyChunks = new BodyChunk[1];
        bodyChunks = new[] { new BodyChunk(this, 0, abstractPhysicalObject.Room.realizedRoom.MiddleOfTile(abstractPhysicalObject.pos.Tile), 3.3f, 0.07f) { goThroughFloors = false } };
        bodyChunkConnections = new BodyChunkConnection[0];
        airFriction = 0.999f;
        gravity = 0.9f;
        bounce = 0.4f;
        surfaceFriction = 0.4f;
        collisionLayer = 2;
        waterFriction = 0.98f;
        buoyancy = 0.4f;
    }
    public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu)
    {
        if (result.obj == null)
        {
            return false;
        }
        vibrate = 20;
        ChangeMode(Mode.Free);
        if (result.obj is Creature)
        {
            (result.obj as Creature).Violence(firstChunk, firstChunk.vel * firstChunk.mass, result.chunk, result.onAppendagePos, Creature.DamageType.Explosion, 0.8f, 85f);
        }
        else if (result.chunk != null)
        {
            result.chunk.vel += firstChunk.vel * firstChunk.mass / result.chunk.mass;
        }
        else if (result.onAppendagePos != null)
        {
            (result.obj as IHaveAppendages).ApplyForceOnAppendage(result.onAppendagePos, firstChunk.vel * firstChunk.mass);
        }
        Explode(thrownBy);
        return true;
    }
    public bool wasThrown = false;
    public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
    {
        base.TerrainImpact(chunk, direction, speed, firstContact);
        if ((floorBounceFrames <= 0 || direction.x != 0 && room.GetTile(firstChunk.pos).Terrain != Room.Tile.TerrainType.Slope) && wasThrown == true)
        {
            Explode(thrownBy);
        }
    }
    public override void Thrown(Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc, bool eu)
    {
        base.Thrown(thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc, eu);
        room?.PlaySound(SoundID.Slugcat_Throw_Rock, firstChunk);
        wasThrown = true;
    }
    public void Explode(Creature thrownBy)
    {
        Explosion obj = new Explosion(room, this, firstChunk.pos, 6, 30f, 5f, 0.1f, 20f, 0.2f, thrownBy, 1f, 0f, 1f);
        room.AddObject(obj);
        Explosion.ExplosionLight obj2 = new Explosion.ExplosionLight(firstChunk.pos, 150f, 0.9f, 8, explodeColor);
        room.AddObject(obj2);
        room.AddObject(new Explosion.FlashingSmoke(firstChunk.pos, Custom.RNV() * 3f * Random.value, 1f, new Color(1f, 1f, 1f), explodeColor, 11));
        room.PlaySound(SoundID.Firecracker_Bang, firstChunk.pos);
        room.InGameNoise(new Noise.InGameNoise(firstChunk.pos, 8000f, this, 1f));
        room.AddObject(new FirecrackerPlant.ScareObject(firstChunk.pos));
        room.AddObject(new Spark(firstChunk.pos, Custom.RNV() * Mathf.Lerp(5f, 11f, Random.value), color: explodeColor, null, 7, 17));
        Destroy();
    }
    public override void HitByExplosion(float hitFac, Explosion explosion, int hitChunk)
    {
        base.HitByExplosion(hitFac, explosion, hitChunk);
        if (Random.value < hitFac)
        {
            Explode(null);
        }
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        Color color = palette.blackColor;
        sLeaser.sprites[0].color = color;
        sLeaser.sprites[1].color = redColor;
    }
    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[2];
        sLeaser.sprites[0] = new FSprite("Circle20");
        sLeaser.sprites[1] = new FSprite("ScavengerHandA");
        AddToContainer(sLeaser, rCam, null);
    }
    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        float num = Custom.VecToDeg(Vector3.Slerp(lastRotation, rotation, timeStacker));
        float scale = 0.36f;
        sLeaser.sprites[0].rotation = num;
        sLeaser.sprites[1].rotation = num;
        Vector2 vector = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);
        sLeaser.sprites[0].x = vector.x - camPos.x;
        sLeaser.sprites[0].y = vector.y - camPos.y;
        sLeaser.sprites[1].x = vector.x - camPos.x;
        sLeaser.sprites[1].y = vector.y - camPos.y;
        sLeaser.sprites[0].scale = scale;
        sLeaser.sprites[1].scale = scale - 0.1f;

        if (slatedForDeletetion || room != rCam.room)
        {
            sLeaser.CleanSpritesAndRemove();
        }
    }
    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
    {
        if (newContainer == null) newContainer = rCam.ReturnFContainer("Items");
        for (int i = 0; i < sLeaser.sprites.Length; i++) sLeaser.sprites[i].RemoveFromContainer();
        newContainer.AddChild(sLeaser.sprites[0]);
        newContainer.AddChild(sLeaser.sprites[1]);
    }
}
