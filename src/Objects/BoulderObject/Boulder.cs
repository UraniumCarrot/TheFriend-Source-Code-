using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RWCustom;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Solace.Objects.BoulderObject;

internal class Boulder : PlayerCarryableItem, IDrawable
{
    public Vector2 rotation;

    public Vector2 lastRotation;

    public Vector2? setRotation;
    public BoulderAbstract Abstr { get; }
    public Boulder(BoulderAbstract abstr, Vector2 pos, Vector2 vel) : base(abstr)
    {
        Abstr = abstr;
        bodyChunks = new BodyChunk[1];
        bodyChunks = new[] { new BodyChunk(this, 0, abstractPhysicalObject.Room.realizedRoom.MiddleOfTile(abstractPhysicalObject.pos.Tile), 20f, 2f) { goThroughFloors = false } };
        bodyChunkConnections = new BodyChunkConnection[0];
        airFriction = 0.999f;
        gravity = 1f;
        bounce = 0.2f;
        surfaceFriction = 0.4f;
        collisionLayer = 1;
        waterFriction = 0.98f;
        buoyancy = 0.4f;
    }
    public override void Update(bool eu)
    {
        base.Update(eu);
        lastRotation = rotation;
        if (setRotation.HasValue)
        {
            rotation = setRotation.Value;
            setRotation = null;
        }
        rotation = (rotation - Custom.PerpendicularVector(rotation) * (firstChunk.ContactPoint.y < 0 ? 0.15f : 0.05f) * firstChunk.vel.x).normalized * 2;
    }
    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        Color color = palette.blackColor;
        sLeaser.sprites[0].color = color;
    }
    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[1];
        sLeaser.sprites[0] = new FSprite("Futile_White");
        sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["JaggedCircle"];
        sLeaser.sprites[0].alpha = Mathf.Lerp(0.1f, 0.3f, UnityEngine.Random.value);
        AddToContainer(sLeaser, rCam, null);
    }
    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        Vector2 vector = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);
        Vector2 v = Vector3.Slerp(lastRotation, rotation, timeStacker);
        sLeaser.sprites[0].rotation = Custom.VecToDeg(v);
        sLeaser.sprites[0].x = vector.x - camPos.x;
        sLeaser.sprites[0].y = vector.y - camPos.y;
        sLeaser.sprites[0].scale = 3f;
        if (slatedForDeletetion || room != rCam.room)
        {
            sLeaser.CleanSpritesAndRemove();
        }
    }
    public override void PlaceInRoom(Room placeRoom)
    {
        base.PlaceInRoom(placeRoom);
        firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
        rotation = Custom.RNV();
        lastRotation = rotation;
    }
    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
    {
        if (newContainer == null) newContainer = rCam.ReturnFContainer("Items");
        for (int i = 0; i < sLeaser.sprites.Length; i++) sLeaser.sprites[i].RemoveFromContainer();
        newContainer.AddChild(sLeaser.sprites[0]);
    }

    public bool SlamConditions()
    {
        Vector2 vel = bodyChunks[0].vel;
        if (bodyChunks[0].vel.magnitude < vel.magnitude) vel = bodyChunks[0].vel;
        if (gravity == 0) return false;
        if (Submersion > 0) return false;
        if (vel.y >= -10f && vel.magnitude <= 25f) return false;
        else return true;
    }
    public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
    {
        base.Collide(otherObject, myChunk, otherChunk);
        bool crit = otherObject is Creature;
        if (crit == true && SlamConditions() == true);
    }
}
