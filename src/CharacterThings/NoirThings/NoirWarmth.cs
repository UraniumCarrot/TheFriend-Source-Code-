using MoreSlugcats;
using UnityEngine;

namespace TheFriend.CharacterThings.NoirThings;

public partial class NoirCatto
{
    public class CatFur : UpdatableAndDeletable, IProvideWarmth
    {
        public readonly Player Owner;
        public CatFur(Player owner)
        {
            Owner = owner;
            room = owner.room;
        }

        public Vector2 Position()
        {
            return Owner.firstChunk.pos;
        }

        public Room loadedRoom => Owner.room;
        public float warmth => RainWorldGame.DefaultHeatSourceWarmth;
        public float range => 50f;

        public override void Update(bool eu)
        {
            base.Update(eu);
            if (Owner.room == null || Owner.room != room) this.Destroy();
        }
    }

    public static void RoomOnAddObject(Room self, UpdatableAndDeletable obj)
    {
        if (obj is Player pl && pl.SlugCatClass == Plugin.NoirName)
        {
            self.AddObject(new CatFur(pl)); // Kitty adapts to the cold
        }
    }
}