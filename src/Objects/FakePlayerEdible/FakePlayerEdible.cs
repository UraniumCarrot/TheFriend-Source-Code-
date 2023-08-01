using UnityEngine;

namespace TheFriend.Objects.FakePlayerEdible;

public abstract class FakePlayerEdible : PlayerCarryableItem, IPlayerEdible
{
    #region basic properties
    public float mass { get; set; }
    public float radius { get; set; }
    public float customGravity { get; set; }
    public float customBuoyancy { get; set; }
    public int customCollisionLayer { get; set; }
    #endregion
    
    #region item properties
    public UpdatableAndDeletable parent { get; set; } // Item that this stores
    public Color[] colors { get; set; }

    public Player.ObjectGrabability grabability { get; set; }
    #endregion
    
    #region food properties
    public bool Edible { get; set; } // Whether object can be eaten
    public bool Swallowable { get; set; } // Whether object can be stored in stomach
    public int BitesLeft { get; set; } // How many bites are left of object
    public int FoodPoints { get; set; } // How many food points object is worth
    public bool QuarterPoints; // Whether object gives full or quarter points
    public bool SwallowUnlessNotFull { get; set; } // Requires a full stomach to be stored, otherwise will be eaten
    public bool EdibleEvenWhenFull { get; set; } // Allows object to be eaten even if full
    #endregion
    
    #region cosmetic properties
    public SoundID eatNoise { get; set; } // Cosmetic noise played when a bite is taken
    public SoundID eatNoiseLast { get; set; } // Cosmetic noise played when the last bite is taken
    public CosmeticSprite effect { get; set; } // Cosmetic sprite used when a bite is taken
    #endregion
    
    public bool AutomaticPickUp => false; // Batfly-esque autograb, just keep it disabled.

    public FakePlayerEdible(AbstractPhysicalObject abstr, UpdatableAndDeletable parent, Vector2 pos) : base(abstr)
    {
        this.parent = parent;
        grabability = parent.GetPos().grab;
        pos = parent.GetPos().pos;

        bodyChunks = new BodyChunk[1];
        bodyChunks[0] = new BodyChunk(this, 0, pos, radius, mass);
        bodyChunkConnections = new BodyChunkConnection[0];
        gravity = customGravity;
        buoyancy = customBuoyancy;
        collisionLayer = customCollisionLayer;
    }

    public virtual void BitByPlayer(Creature.Grasp grasp, bool eu)
    {
        BitesLeft--;
        if (effect != null) room.AddObject(effect);
        if (eatNoise != null)
        {
            if (eatNoiseLast == null) eatNoiseLast = eatNoise;
            room.PlaySound(BitesLeft < 1 ? eatNoiseLast : eatNoise, firstChunk.pos);
        }
        if (BitesLeft < 1)
        {
            (grasp.grabber as Player)?.ObjectEaten(this);
            grasp.Release();
            Destroy();
        }
    }

    public virtual void ThrowByPlayer()
    {
    }
}