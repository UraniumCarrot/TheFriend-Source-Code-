using UnityEngine;
using RWCustom;
using System;
using Random = UnityEngine.Random;

namespace TheFriend.Objects.FreedLizardBubbleObject;

public class NonLizardBubble : CosmeticSprite
{
    public Color color;
    public Color secondColor;
    
    public BodyChunk chunkParent;
    public Vector2 origPos;
    public Vector2 origLastPos;
    public BubbleMode bubbleMode;
    public float distanceLimit;
    public string spriteReplacer;

    public enum BubbleMode
    {
        Default,
        BubblesOnly,
        LinesOnly
    }
    
    public float life;
    private float lastLife;
    public int lifeTime;
    private float hollowNess;
    public bool hollow;
    public Vector2 originPoint;
    public bool stuckToOrigin;
    public bool freeFloating;
    public Vector2 liberatedOrigin;
    public Vector2 liberatedOriginVel;
    public Vector2 lastLiberatedOrigin;
    public float lifeTimeWhenFree;
    public float stickiness;
    public float Stickiness => 
        Mathf.Max(stickiness, Mathf.InverseLerp(4f, 14f, chunkParent.vel.magnitude));
    public float random;

    public NonLizardBubble(Vector2 origPos, Vector2 origLastPos, BodyChunk chunkParent, float intensity, float stickiness, float extraSpeed)
    {
        this.stickiness = stickiness;
        this.chunkParent = chunkParent;
        this.origPos = origPos;
        this.origLastPos = origLastPos;

        bubbleMode = BubbleMode.Default;
        spriteReplacer = "LizardBubble";
        distanceLimit = 3.5f;
        
        pos = chunkParent.pos + Custom.DegToVec(Random.value * 360f) * 5f * Random.value;
        originPoint = Custom.RotateAroundOrigo(pos - chunkParent.pos, Random.value);
        lastPos = pos;
        life = 1f;
        lifeTime = 10 + Random.Range(0, Random.Range(0, Random.Range(0, Random.Range(0, 200))));
        vel = chunkParent.vel + Custom.DegToVec(Random.value * 360f) * (Random.value * Random.value * 12f * Mathf.Pow(intensity, 1.5f) + extraSpeed);
        hollowNess = Mathf.Lerp(0.5f, 1.5f, (bubbleMode == BubbleMode.BubblesOnly) ? Random.value + 0.2f : Random.value);
        if (Stickiness == 0f) freeFloating = true;
        else stuckToOrigin = true;
        liberatedOrigin = pos;
        lastLiberatedOrigin = pos;
        random = Random.value;
    }

    public override void Update(bool eu)
    {
        vel = Vector3.Slerp(vel, Custom.DegToVec(random * 360f), 0.2f);
        vel += Custom.DirVec(origPos, pos) * Mathf.Lerp(-0.5f, distanceLimit - 0.5f, random);
        
        lastLife = life;
        life -= 1f / lifeTime;
        if (life <= 0f)
            Destroy();
        
        if (freeFloating)
            hollow = true;
        
        if (room.GetTile(pos).Terrain == Room.Tile.TerrainType.Solid)
            lifeTime = Math.Min(1, lifeTime - 5);
        
        if ((!ModManager.MSC) ? (pos.y < room.FloatWaterLevel(pos.x)) : room.PointSubmerged(pos))
        {
            vel *= 0.9f;
            vel.y += 4f;
        }
        if (stuckToOrigin)
        {
            Vector2 origin = origPos + Custom.RotateAroundOrigo(originPoint, random);
            liberatedOriginVel = origin - liberatedOrigin;
            liberatedOrigin = origin;
            lastLiberatedOrigin = origin;
            if (life < 0.5f || Random.value < 1f / (10f + Stickiness * 80f) || !Custom.DistLess(pos, origin, 10f + 90f * Stickiness))
                stuckToOrigin = false;
        }
        else if (!freeFloating)
        {
            lastLiberatedOrigin = liberatedOrigin;
            liberatedOriginVel = Vector2.Lerp(liberatedOriginVel, Custom.DirVec(liberatedOrigin, pos) * Mathf.Lerp(Vector2.Distance(liberatedOrigin, pos), 10f, 0.5f), 0.7f);
            liberatedOrigin += liberatedOriginVel;
            if (Custom.DistLess(liberatedOrigin, pos, 5f))
            {
                vel = Vector2.Lerp(vel, liberatedOriginVel, 0.3f);
                lifeTimeWhenFree = life;
                freeFloating = true;
            }
        }
        base.Update(eu);
    }


    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        float scale = 0.625f * Mathf.Lerp(Mathf.Lerp(Mathf.Sin((float)Math.PI * lastLife), lastLife, 0.5f), Mathf.Lerp(Mathf.Sin((float)Math.PI * life), life, 0.5f), timeStacker);
        sLeaser.sprites[0].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
        sLeaser.sprites[0].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y;
        sLeaser.sprites[0].color = Color.Lerp(color, secondColor, 1f - Mathf.Clamp(Mathf.Lerp(lastLife, life, timeStacker) * 2f, 0f, 1f));
        int bubble = 0;
        if (hollow)
            bubble = Custom.IntClamp((int)(Mathf.Pow(Mathf.InverseLerp(lifeTimeWhenFree, 0f, life), hollowNess) * 7f), 1, 7);
        
        sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName(spriteReplacer + bubble);

        if (((stuckToOrigin || !freeFloating) && bubbleMode != BubbleMode.BubblesOnly) || bubbleMode == BubbleMode.LinesOnly)
        {
            Vector2 origin;
            Vector2 originPos = Vector2.Lerp(origPos, origLastPos, timeStacker);
            if (stuckToOrigin)
                origin = originPos + Custom.RotateAroundOrigo(originPoint, random);
            else origin = Vector2.Lerp(lastLiberatedOrigin, liberatedOrigin, timeStacker);
            
            float longScale = Vector2.Distance(Vector2.Lerp(lastPos, pos, timeStacker), origin) / 16f;
            sLeaser.sprites[0].scaleX = Mathf.Min(scale, scale / Mathf.Lerp(longScale, 1f, 0.35f));
            sLeaser.sprites[0].scaleY = Mathf.Max(scale, longScale - 0.3125f);
            sLeaser.sprites[0].rotation =
                Custom.AimFromOneVectorToAnother(Vector2.Lerp(lastPos, pos, timeStacker), origin);
            sLeaser.sprites[0].anchorY = Mathf.InverseLerp(1.25f, 0.3125f, longScale) * 0.5f;
        }
        else
        {
            sLeaser.sprites[0].scaleX = scale;
            sLeaser.sprites[0].scaleY = scale;
            sLeaser.sprites[0].rotation = 0f;
            sLeaser.sprites[0].anchorY = 0.5f;
        }

        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }
    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[1];
        sLeaser.sprites[0] = new FSprite(spriteReplacer + "0");
        AddToContainer(sLeaser, rCam, null);
    }
    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        Color empty = new Color();
        if (color == empty) color = palette.blackColor;
        if (secondColor == empty) secondColor = palette.blackColor;
    }    
    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
    {
        if (newContainer == null) newContainer = rCam.ReturnFContainer("Background");
        FSprite[] sprites = sLeaser.sprites;
        foreach (FSprite fSprite in sprites)
        {
            fSprite.RemoveFromContainer();
            newContainer.AddChild(fSprite);
        }
    }
}