using RWCustom;
using UnityEngine;

namespace TheFriend.Creatures.LizardThings.DragonRideThings;

public class DragonRiderSeat : UpdatableAndDeletable, IDrawable
{
    public Vector2 Position() => pos;
    public Lizard host;
    
    public Vector2 pos;
    public BodyChunk startChunk;
    public BodyChunk endChunk;
    public float lerpFactor;
    
    public DragonRiderSeat(Lizard host, BodyChunk startChunk, BodyChunk endChunk, float lerpFactor)
    {
        pos = Vector2.Lerp(startChunk.pos,endChunk.pos, lerpFactor);
        this.host = host;
        this.startChunk = startChunk;
        this.endChunk = endChunk;
        this.lerpFactor = lerpFactor;
    }

    public override void Update(bool eu)
    {
        if (host == null || !host.Liz().seats.Contains(this) || host.dead)
        {
            Destroy();
            return;
        }
        pos = Vector2.Lerp(startChunk.pos,endChunk.pos, lerpFactor);
        room = host.room;
    }

    public override void Destroy()
    {
        Debug.Log("Solace: Destroying lizard seats");
        base.Destroy();
    }
    
    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        sLeaser.sprites[0].SetPosition(pos - camPos);
    }

    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[1];
        sLeaser.sprites[0] = new FSprite("Futile_White");
        sLeaser.sprites[0].color = Custom.HSL2RGB(Random.value, 1, 0.5f);
        sLeaser.sprites[0].scale = 1;
        AddToContainer(sLeaser, rCam, null);
        Debug.Log("Seat spriteinit");
    }

    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
    }

    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
    {
        if (newContainer == null)
            newContainer = rCam.ReturnFContainer("Items");
        newContainer.AddChild(sLeaser.sprites[0]);
        Debug.Log("Seat addcont");
    }
}