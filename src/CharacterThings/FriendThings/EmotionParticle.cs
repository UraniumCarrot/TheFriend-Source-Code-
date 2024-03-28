using UnityEngine;

namespace TheFriend.CharacterThings.FriendThings;

public class EmotionParticle : CosmeticSprite
{
    public readonly int maxLifetime;
    public int lifetime;
    public Color color;
    public Color secondColor;
    public LightSource light;
    public Vector2 direction;
    public string spriteName;

    public EmotionParticle(Color color, Vector2 position, float degrees, string spriteName, int lifetime = 60)
    {
        this.color = color;
        this.spriteName = spriteName;
        this.lifetime = lifetime;
        direction = RWCustom.Custom.DegToVec(degrees).normalized * 3;

        pos = position;
        lastPos = position;
        maxLifetime = lifetime;
    }
    public EmotionParticle(Color color, Vector2 position, Vector2 direction, string spriteName, int lifetime = 60)
    {
        this.color = color;
        this.spriteName = spriteName;
        this.lifetime = lifetime;
        this.direction = direction.normalized * 3;

        pos = position;
        lastPos = position;
        lifetime = 80;
        maxLifetime = lifetime;
    }

    public override void Update(bool eu)
    {
        if (lifetime > 0) lifetime--;
        else Destroy();
        base.Update(eu);
        if (light == null) GenerateLightSource();
        else LightUpdate();
        
        vel = Vector2.Lerp(Vector2.zero, direction, (lifetime - (maxLifetime/3.5f)) / maxLifetime);
    }

    public override void Destroy()
    {
        base.Destroy();
        if (light != null)
            light.Destroy();
        light = null;
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker,
        Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        sLeaser.sprites[0].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
        sLeaser.sprites[0].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y;
        sLeaser.sprites[0].scale = Mathf.Lerp(0, 0.8f, (float)lifetime / 20);
        sLeaser.sprites[0].color = Color.Lerp(secondColor, color, (float)lifetime/20);
    }
    
    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[1];
        sLeaser.sprites[0] = new FSprite(spriteName);
        sLeaser.sprites[0].x = pos.x;
        sLeaser.sprites[0].y = pos.y;
        AddToContainer(sLeaser, rCam, null);
    }
    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        Color empty = new Color();
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

    public void GenerateLightSource()
    {
        light = new LightSource(pos, false, color, this)
        { affectedByPaletteDarkness = 0.5f, flat = true };
        room.AddObject(light);
    }

    public void LightUpdate()
    {
        light.color = color;
        light.setAlpha = Mathf.Lerp(0f, 0.8f, (float)lifetime / 20);
        light.setRad = Mathf.Lerp(0f, 18f, (float)lifetime / 20);
        light.setPos = pos;
    }
}