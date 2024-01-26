using System;
using System.Collections.Generic;
using System.Linq;
using RWCustom;
using TheFriend.SlugcatThings;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TheFriend.Objects.SolaceScarfObject;

public class SolaceScarf : PlayerCarryableItem, IDrawable
{
    public List<FSprite> spriteList = new List<FSprite>();
    public float maxDarkness;
    public Color highlightColor;
    public LightSource light;
    
    public Vector2[,] rag = new Vector2[6,6];
    public TriangleMesh ragMesh;
    public Vector2 ragAttachPos;
    // rag[i,0] - gravity direction
    // rag[i,1] - ? something fucked up
    // rag[i,2] - movement, multiplying it makes the scarf wag!
    // rag[i,3] - width?
    // rag[i,4] - also width?
    // rag[i,5] - ALSO width?
    // multiplying the width of the entire triangle mesh makes it change the side the rag is on
    
    public AbstractCreature wearer;
    public GenericObjectStick stick;
    public float rotation;
    private float conRad = 7f;
    private SharedPhysics.TerrainCollisionData scratchTerrainCollisionData = new SharedPhysics.TerrainCollisionData();
    
    public SolaceScarfAbstract Abstr { get; }
    public SolaceScarf(SolaceScarfAbstract abstr, Vector2 pos, Vector2 vel) : base(abstr)
    {
        Abstr = abstr;
        bodyChunks = new BodyChunk[1];
        bodyChunks = new[] { new BodyChunk(this, 0, abstractPhysicalObject.Room.realizedRoom.MiddleOfTile(abstractPhysicalObject.pos.Tile), 6f, 0.07f) { goThroughFloors = false } };
        bodyChunkConnections = new BodyChunkConnection[0];
        airFriction = 0.999f;
        gravity = 0.9f;
        bounce = 0.5f;
        surfaceFriction = 0.4f;
        collisionLayer = 2;
        waterFriction = 0.98f;
        buoyancy = 1.1f;
        color = Abstr.baseCol;
        highlightColor = Abstr.highCol;
    }

    #region cosmetics
    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[2];
        sLeaser.sprites[0] = new FSprite("Circle20");
        sLeaser.sprites[1] = TriangleMesh.MakeLongMesh(rag.GetLength(0), false, true);
        ragMesh = sLeaser.sprites[1] as TriangleMesh;
        //sLeaser.sprites[1].shader = rCam.game.rainWorld.Shaders["JaggedSquare"];
        spriteList = [sLeaser.sprites[0], sLeaser.sprites[1]];
        AddToContainer(sLeaser, rCam, null);
    }
    
    #region rag
    public void RagDraw(RoomCamera.SpriteLeaser sLeaser, Vector2 camPos, float timeStacker)
    {
        Vector2 vector = firstChunk.pos;
        float num = 0;
        for (int i = 0; i < rag.GetLength(0); i++)
        {
            float f = (float)i / (float)(rag.GetLength(0) - 1);
            Vector2 vector2 = Vector2.Lerp(rag[i, 1], rag[i, 0], timeStacker);
            float num2 = (2f + 2f * Mathf.Sin(Mathf.Pow(f, 2f) * (float)Math.PI)) *
                         Vector3.Slerp(rag[i, 4], rag[i, 3], timeStacker).x;
            Vector2 normalized = (vector - vector2).normalized;
            Vector2 vector3 = Custom.PerpendicularVector(normalized);
            float num3 = Vector2.Distance(vector, vector2) / 5f;
            (sLeaser.sprites[1] as TriangleMesh)!.MoveVertice(i * 4,
                vector - normalized * num3 - vector3 * (num2 + num) * 0.5f - camPos);
            (sLeaser.sprites[1] as TriangleMesh)!.MoveVertice(i * 4 + 1,
                vector - normalized * num3 + vector3 * (num2 + num) * 0.5f - camPos);
            (sLeaser.sprites[1] as TriangleMesh)!.MoveVertice(i * 4 + 2,
                vector2 + normalized * num3 - vector3 * num2 - camPos);
            (sLeaser.sprites[1] as TriangleMesh)!.MoveVertice(i * 4 + 3,
                vector2 + normalized * num3 + vector3 * num2 - camPos);
            vector = vector2;
            num = num2;
        }
    }

    public void ResetRag()
    {
        Vector2 pos = firstChunk.pos;
        for (int a = 0; a < rag.GetLength(0); a++)
        {
            rag[a, 0] = pos;
            rag[a, 1] = pos;
            rag[a, 2] *= 0f;
        }
    }
    public override void PlaceInRoom(Room placeRoom)
    {
        base.PlaceInRoom(placeRoom);
        ResetRag();
    }
    public override void NewRoom(Room newRoom)
    {
        base.NewRoom(newRoom);
        ResetRag();
    }
    #endregion
    
    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
    {
        if (newContainer == null) newContainer = rCam.ReturnFContainer("Items");
        for (int i = 0; i < sLeaser.sprites.Length; i++) sLeaser.sprites[i].RemoveFromContainer();
        newContainer.AddChild(spriteList[0]);
        newContainer = rCam.ReturnFContainer("Background");
        newContainer.AddChild(spriteList[1]);
    }
    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        maxDarkness = palette.blackColor.Lit();
        if (color == Color.black) color = new Color(1,0.5f,0);
        
        var limitedColor = color.MakeLit((color.Lit() > maxDarkness) ? color.Lit() : maxDarkness);
        var limitedHiCol = highlightColor.MakeLit((color.Lit() > maxDarkness) ? highlightColor.Lit() : maxDarkness);
        spriteList[0].color = limitedColor;
        List<Color> col = [limitedColor, limitedHiCol];
        for (int i = 0; i < ragMesh.verticeColors.Length; i++)
            (spriteList[1] as TriangleMesh)!.verticeColors[i] = Extensions.HSLMultiLerp((col), (float)(i-4) / 20);
    }
    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        RagDraw(sLeaser,camPos,timeStacker);

        if (blink > 1 && Random.value > 0.5f)
        {
            spriteList[0].color = Color.white;
            for (int i = 0; i < ragMesh.verticeColors.Length; i++)
                ragMesh.verticeColors[i] = Color.white;
        }
        else
        {
            var limitedColor = color.MakeLit((color.Lit() > maxDarkness) ? color.Lit() : maxDarkness);
            var limitedHiCol = highlightColor.MakeLit((color.Lit() > maxDarkness) ? highlightColor.Lit() : maxDarkness);
            spriteList[0].color = limitedColor;
            List<Color> col = [limitedColor, limitedHiCol];

            if (spriteList[0].color != color) spriteList[0].color = limitedColor;
                for (int i = 0; i < ragMesh.verticeColors.Length; i++)
                    (sLeaser.sprites[1] as TriangleMesh)!.verticeColors[i] = Extensions.HSLMultiLerp((col), (float)(i-4) / 20);
        }
        if (Abstr.baseCol != color!) Abstr.baseCol = color;
        if (Abstr.highCol != highlightColor!) Abstr.highCol = highlightColor;

        if (wearer != null && wearer.realizedCreature != null)
        {
            var pl = wearer.realizedCreature as Player;
            if (pl != null)
            {
                if (pl.GetGeneral().head?.container?._childNodes?.Contains(spriteList[0]) == false)
                {
                    pl.GetGeneral().head.container.AddChild(spriteList[0]);
                    spriteList[0].MoveBehindOtherNode(pl.GetGeneral().head);
                }
                if (pl.GetGeneral().head?.container?._childNodes == null) ScarfContainerReset(rCam, timeStacker, camPos);
                spriteList[0].SetPosition(pl.GetGeneral().scarfPos);
                spriteList[0].rotation = pl.GetGeneral().scarfRotation;
                spriteList[0].scaleY = 0.7f;
                spriteList[0].scaleX = 0.9f;
            }
        }
        else
        {
            ScarfContainerReset(rCam, timeStacker, camPos);

            spriteList[0].scaleX = 0.5f;
            spriteList[0].scaleY = 0.5f;
        }
        rotation = spriteList[0].rotation;
        
        if (slatedForDeletetion || room != rCam.room)
        {
            sLeaser.CleanSpritesAndRemove();
        }
    }
    #endregion

    public override void Update(bool eu)
    {
        base.Update(eu);
        if (room != null && room.PlayersInRoom.Any() &&
            room.PlayersInRoom.Exists(x => x.abstractCreature.ID.number == Abstr.wearerID))
        {
            var user = room.PlayersInRoom.Find(x => x.abstractCreature.ID.number == Abstr.wearerID).abstractCreature;
            if (!(user.realizedCreature as Player).GetGeneral().wearingAScarf)
            {
                wearer = user;
                (user.realizedCreature as Player).GetGeneral().wearingAScarf = true;
            }
        }
        
        LightnessUpdate();
        RagUpdate1();
        RagUpdate2();
        if (wearer != null)
        {
            if (wearer.realizedCreature != null)
            {
                var pl = wearer.realizedCreature as Player;
                Vector2 pos = Vector2.Lerp(pl.bodyChunks[0].pos, pl.bodyChunks[1].pos, 0.15f);
                firstChunk.pos = pos;
            }
            stick ??= new GenericObjectStick(this.abstractPhysicalObject, wearer);
            WearerUpdate(wearer.realizedCreature as Player);
        }
        else if (grabbedBy.Any())
        {
            var a = grabbedBy.Find(x => x.grabbed == this);
            var grabber = a.grabber;
            if (grabber != null) 
                if (grabber is Player player) GrabbedUpdate(player);
        }
        else grabTimer = 0;
    }
    public void RagUpdate1()
    {
        for (int i = 0; i < rag.GetLength(0); i++)
        {
            float t = (float)i / (rag.GetLength(0) - 1);
            rag[i, 1] = rag[i, 0];
            rag[i, 0] += rag[i, 2];
            //rag[i, 2] -= rotation * Mathf.InverseLerp(1f, 0f, i) * 0.8f;
            rag[i, 4] = rag[i, 3];
            rag[i, 3] = (rag[i, 3] + rag[i, 5] * Custom.LerpMap(Vector2.Distance(rag[i, 0], rag[i, 1]), 1f, 18f, 0.05f, 0.3f)).normalized;
            rag[i, 5] = (rag[i, 5] + Custom.RNV() * Random.value * Mathf.Pow(Mathf.InverseLerp(1f, 18f, Vector2.Distance(rag[i, 0], rag[i, 1])), 0.3f)).normalized;
            if (room.PointSubmerged(rag[i, 0]))
            {
                rag[i, 2] *= Custom.LerpMap(rag[i, 2].magnitude, 1f, 10f, 1f, 0.5f, Mathf.Lerp(1.4f, 0.4f, t));
                rag[i, 2].y += 0.05f;
                rag[i, 2] += Custom.RNV() * 0.1f;
                continue;
            }
            rag[i, 2] *= Custom.LerpMap(Vector2.Distance(rag[i, 0], rag[i, 1]), 1f, 6f, 0.999f, 0.7f, Mathf.Lerp(1.5f, 0.5f, t));
            rag[i, 2].y -= room.gravity * Custom.LerpMap(Vector2.Distance(rag[i, 0], rag[i, 1]), 1f, 6f, 0.6f, 0f);
            if (i % 3 == 2 || i == rag.GetLength(0) - 1)
            {
                SharedPhysics.TerrainCollisionData cd = scratchTerrainCollisionData.Set(rag[i, 0], rag[i, 1], rag[i, 2], 1f, new IntVector2(0, 0), goThroughFloors: false);
                cd = SharedPhysics.HorizontalCollision(room, cd);
                cd = SharedPhysics.VerticalCollision(room, cd);
                cd = SharedPhysics.SlopesVertically(room, cd);
                rag[i, 0] = cd.pos;
                rag[i, 2] = cd.vel;
                if (cd.contactPoint.x != 0)
                {
                    rag[i, 2].y *= 0.6f;
                }
                if (cd.contactPoint.y != 0)
                {
                    rag[i, 2].x *= 0.6f;
                }
            }
        }
    }
    public void RagUpdate2()
    {
        for (int j = 0; j < rag.GetLength(0); j++)
        {
            if (j > 0)
            {
                Vector2 normalized = (rag[j, 0] - rag[j - 1, 0]).normalized;
                float num = Vector2.Distance(rag[j, 0], rag[j - 1, 0]);
                float num2 = ((num > conRad) ? 0.5f : 0.25f);
                rag[j, 0] += normalized * (conRad - num) * num2;
                rag[j, 2] += normalized * (conRad - num) * num2;
                rag[j - 1, 0] -= normalized * (conRad - num) * num2;
                rag[j - 1, 2] -= normalized * (conRad - num) * num2;
                if (j > 1)
                {
                    normalized = (rag[j, 0] - rag[j - 2, 0]).normalized;
                    rag[j, 2] += normalized * 0.2f;
                    rag[j - 2, 2] -= normalized * 0.2f;
                }
                if (j < rag.GetLength(0) - 1)
                {
                    rag[j, 3] = Vector3.Slerp(rag[j, 3], (rag[j - 1, 3] * 2f + rag[j + 1, 3]) / 3f, 0.1f);
                    rag[j, 5] = Vector3.Slerp(rag[j, 5], (rag[j - 1, 5] * 2f + rag[j + 1, 5]) / 3f, Custom.LerpMap(Vector2.Distance(rag[j, 1], rag[j, 0]), 1f, 8f, 0.05f, 0.5f));
                }
            }
            else
            {
                rag[j, 0] = firstChunk.pos;
                rag[j, 2] *= 0f;
            }
        }
    }

    public int grabTimer;
    public void GrabbedUpdate(Player player)
    {
        if (player.input[0].pckp && 
            !player.craftingObject && 
            wearer == null && 
            !player.GetGeneral().wearingAScarf) 
            grabTimer++;
        else grabTimer = 0;
        if (grabTimer > 25)
        {
            wearer = player.abstractCreature;
            Abstr.wearerID = player.abstractCreature.ID.number;
            player.GetGeneral().wearingAScarf = true;
            player.ReleaseGrasp(player.grasps.IndexOf(player.grasps.First((x => x?.grabbed == this))));
            grabTimer = 0;
        }
    }
    public void WearerUpdate(Player player)
    {
        bool imHoldingFood = false;
        if (player.grasps != null)
        {
            imHoldingFood = player.grasps.Any(x => x?.grabbed is IPlayerEdible);
            player.grasps.FirstOrDefault(x => x?.grabbed?.firstChunk == firstChunk)?.Release();
        }
        
        player.HypothermiaGain -= 0.0005f;
        firstChunk.vel = player.firstChunk.vel;
        if (player.input[0].pckp && player.input[0].y > 0)
        {
            if (player.FreeHand() != -1 && 
                player.objectInStomach == null && 
                !player.craftingObject &&
                !(imHoldingFood && player.FoodInStomach < player.MaxFoodInStomach))
                grabTimer++;
            else grabTimer = 0;
        }
        else grabTimer = 0;
        if (grabTimer > 25)
        {
            wearer.stuckObjects.Remove(stick);
            stick.Deactivate();
            stick = null;
            Abstr.wearerID = -10;
            grabTimer = 0;
            wearer = null;
            player.GetGeneral().wearingAScarf = false;
            player.SlugcatGrab(this,player.FreeHand());
        }
    }
    public void LightnessUpdate()
    {
        if (light == null && Abstr.IGlow > 0)
        {
            light = new LightSource(firstChunk.pos, false, highlightColor, this)
            { affectedByPaletteDarkness = 0.5f };
            room.AddObject(light);
            Debug.Log("Solace: Making scarf light");
        }
        else if (light != null)
        {
            light.color = Color.Lerp(light.color,highlightColor,0.05f);
            light.setPos = spriteList[0].GetPosition();
            if (light.rad == 0) light.setRad = Abstr.IGlow * 15;
            else light.setRad = Mathf.Lerp(light.rad,Abstr.IGlow * 15,0.05f);
            light.setAlpha = 1;
            if (light.slatedForDeletetion || light.room != room || Abstr.IGlow <= 0)
            {
                Debug.Log("Solace: Destroying scarf light");
                light = null;
            }
        }
    }

    public void ScarfContainerReset(RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        spriteList[0].SetPosition(Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker)-camPos);
        if (spriteList[0].container != rCam.ReturnFContainer("Items"))
        {
            spriteList[0].RemoveFromContainer();
            rCam.ReturnFContainer("Items").AddChild(spriteList[0]);
        }
    }
}