using MoreSlugcats;
using UnityEngine;
using Expedition;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;
using TheFriend.Creatures.FamineCreatures;
using TheFriend.WorldChanges.WorldStates.General;

namespace TheFriend.WorldChanges;
public abstract class FamineWorld
{
    public static void HasFamines(RainWorldGame self)
    {
        if (QuickWorldData.SolaceCampaign && !Configs.NoFamine) FamineBool = true;
        
        if (self.rainWorld.ExpeditionMode &&
            ExpeditionGame.activeUnlocks.Contains(Expedition.ExpeditionBurdens.famine))
            FamineBurdenBool = true;
        else FamineBurdenBool = false;

        if (Configs.NoFamine) FamineBool = false;
    }

    // Helps majority of the code here tell slugcat has famines
    public static bool FamineBool; // Global bool used to tell if the world has Solace famines, has no requirements unlike above
    public static bool FamineBurdenBool;

    public static bool IsDiseased(PhysicalObject consumable) => IsDiseased(consumable.abstractPhysicalObject);
    public static bool IsDiseased(AbstractPhysicalObject consumable) // General disease bool handler
    {
        var region = consumable.world.name;
        bool value = false;
        
        var oldState = Random.state;
        try
        {
            var newState = consumable.ID.RandomSeed; //c.placedObjectIndex != -1 ? ((c.world.GetAbstractRoom(c.originRoom)?.name.GetHashCode() ?? 0) ^ c.placedObjectIndex) : c.ID.number;
            Random.InitState(newState);
            switch (consumable.type.value)
            {
                case nameof(AbstractPhysicalObject.AbstractObjectType.DangleFruit): 
                    value = FamineBurdenBool || Random.value > 0.2; break;
                case nameof(MoreSlugcatsEnums.AbstractObjectType.LillyPuck): 
                    value = FamineBurdenBool || Random.value > 0.05; break;
                case nameof(MoreSlugcatsEnums.AbstractObjectType.GooieDuck): 
                    value = FamineBurdenBool || Random.value > 0.9; break;
                case nameof(MoreSlugcatsEnums.AbstractObjectType.DandelionPeach): 
                    value = FamineBurdenBool || Random.value > 0.4; break;
                    default: return false;
            }
            if (consumable is not AbstractConsumable) value = false;
            if (!FamineBool && !FamineBurdenBool) value = false;
            if (region == "UG" && !FamineBurdenBool) value = false;
            
            return value;
        }
        finally
        {
            Random.state = oldState;
        }
    }

    // Diseased food value
    public static int SlugcatStats_NourishmentOfObjectEaten(On.SlugcatStats.orig_NourishmentOfObjectEaten orig, SlugcatStats.Name slugcatIndex, IPlayerEdible eatenobject)
    {
        var num = orig(slugcatIndex, eatenobject);
        var quarters1 = eatenobject.FoodPoints;
        if (eatenobject is GlowWeed && slugcatIndex == Plugin.DragonName) num = 4;
        if (!FamineBool && !FamineBurdenBool) return num;

        if (eatenobject is PhysicalObject obj && IsDiseased(obj))
        {
            if (slugcatIndex == SlugcatStats.Name.Red || slugcatIndex == MoreSlugcatsEnums.SlugcatStatsName.Artificer || num == 0)
            {
                num = 0;
            }
            else if (Random.value > 0.50)
            {
                if (quarters1 >= 2)
                {
                    num = quarters1 / 2;
                }
                if (quarters1 < 2)
                {
                    num = 0;
                }
            }
            else num = quarters1;
        }

        FamineCentipede.NourishmentOfCentiEaten(slugcatIndex, eatenobject, ref num);
        return num;
    }

    // Diseased DangleFruit
    public static void DangleFruit_ApplyPalette(On.DangleFruit.orig_ApplyPalette orig, DangleFruit self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) // Makes it brown
    {
        orig(self, sLeaser, rCam, palette);
        if (IsDiseased(self.AbstrConsumable))
        {
            self.color = Color.Lerp(new Color(0.2f, 0.16f, 0.1f), palette.blackColor, palette.darkness);
        }
    }
    public static void DangleFruit_DrawSprites(On.DangleFruit.orig_DrawSprites orig, DangleFruit self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) // Makes it small
    {
        if (IsDiseased(self.AbstrConsumable))
        {
            sLeaser.sprites[0].scaleX = 0.8f;
            sLeaser.sprites[1].scaleX = 0.8f;
        }
        orig(self,sLeaser,rCam,timeStacker,camPos);
    }

    // Diseased LilyPuck
    public static void LillyPuck_ApplyPalette(On.MoreSlugcats.LillyPuck.orig_ApplyPalette orig, LillyPuck self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);
        if (IsDiseased(self.AbstrConsumable))
        {
            self.color = Color.Lerp(new Color(0.3f, 0.24f, 0.18f), palette.blackColor, palette.darkness);
            sLeaser.sprites[0].color = self.color;
            for (int i = 0; i < self.flowerLeavesCount; i++)
            {
                Color a = Color.Lerp(self.color, self.flowerColor, Mathf.Clamp(self.lightFade, 0.3f - 0.3f * palette.darkness, 1f));
                sLeaser.sprites[1 + i * 2].color = Color.Lerp(a, self.color, i / (self.flowerLeavesCount / 2f));
                sLeaser.sprites[2 + i * 2].color = Color.Lerp(sLeaser.sprites[1 + i * 2].color, Color.Lerp(new Color(1f, 1f, 1f), palette.blackColor, palette.darkness), palette.darkness / 20f);
            }
        }
    }
    public static void LillyPuck_Update(On.MoreSlugcats.LillyPuck.orig_Update orig, LillyPuck self, bool eu)
    {
        orig(self,eu);
        if (IsDiseased(self.AbstrConsumable))
        {
            self.light.rad = 0f;
            self.lightFade = 0f;
            self.light.stayAlive= false;
        }
    }

    // Diseased GooieDuck
    public static void GooieDuck_ApplyPalette(On.MoreSlugcats.GooieDuck.orig_ApplyPalette orig, GooieDuck self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);
        if (IsDiseased(self.AbstrConsumable))
        {
            self.CoreColor = Color.Lerp(new Color(0.2f, 0.21f, 0.32f), palette.blackColor, palette.darkness/ 3f);
            self.HuskColor = Color.Lerp(new Color(0.2f, 0.16f, 0.1f), palette.blackColor, palette.darkness);
        }
    }

    // Diseased DandelionPeach
    public static void DandelionPeach_ctor(On.MoreSlugcats.DandelionPeach.orig_ctor orig, DandelionPeach self, AbstractPhysicalObject abstractPhysicalObject)
    {
        orig(self, abstractPhysicalObject);
        if (IsDiseased(self.AbstrConsumable))
        {
            self.airFriction = 0.999f;
            self.gravity = 0.9f;
        }
    }
    public static void DandelionPeach_ApplyPalette(On.MoreSlugcats.DandelionPeach.orig_ApplyPalette orig, DandelionPeach self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);
        if (IsDiseased(self.AbstrConsumable))
        {
            self.color = Color.Lerp(new Color(0.87f, 0.85f, 0.75f), palette.blackColor, Mathf.Pow(palette.darkness,2f));
            sLeaser.sprites[1].color = Color.Lerp(Color.Lerp(palette.fogColor, new Color(1f, 1f, 1f), 0.5f), palette.blackColor, palette.darkness);
            sLeaser.sprites[2].color = Color.Lerp(self.color, sLeaser.sprites[1].color, 0.3f);
            self.puffCount = 0;
        }
    }
}