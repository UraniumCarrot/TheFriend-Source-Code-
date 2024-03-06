using UnityEngine;
using RWCustom;
using MoreSlugcats;
using System;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using System.Linq;
using Expedition;
using TheFriend.Expedition;
using ObjType = AbstractPhysicalObject.AbstractObjectType;
using TheFriend.Objects.LittleCrackerObject;
using TheFriend.Objects.BoomMineObject;
using TheFriend.Objects.SolaceScarfObject;
using TheFriend.SlugcatThings;
using TheFriend.WorldChanges;
using TheFriend.WorldChanges.WorldStates.General;

namespace TheFriend.PoacherThings;

public class RecipeType : ExtEnum<RecipeType>
{
    public RecipeType(string name, bool register = false) : base(name,register)
    {
    }
    public static readonly RecipeType None = new(nameof(None), true);
    public static readonly RecipeType Wash = new(nameof(Wash), true);

    public static readonly RecipeType MakeMine = new(nameof(MakeMine), true);
    public static readonly RecipeType MakeSpear = new(nameof(MakeSpear), true);
    public static readonly RecipeType MakeSpearEle = new(nameof(MakeSpearEle), true);
    public static readonly RecipeType MakeSpearFire = new(nameof(MakeSpearFire), true);
    public static readonly RecipeType MakeSpearExpl = new(nameof(MakeSpearExpl), true);
    public static readonly RecipeType MakeLantern = new(nameof(MakeLantern), true);
    public static readonly RecipeType MakeBomb = new(nameof(MakeBomb), true);
    public static readonly RecipeType MakeLittleCracker = new(nameof(MakeLittleCracker), true);
    
    public static readonly RecipeType ArmMine = new(nameof(ArmMine), true);
    public static readonly RecipeType ArmSpearEle = new(nameof(ArmSpearEle), true);

    public static List<RecipeType> MakeSpecialSpear = [ArmSpearEle, MakeSpearEle, MakeSpearExpl, MakeSpearFire];
}

public class DragonCrafts
{
    #region Crafting dictionary
    static Dictionary<(ObjType, ObjType), RecipeType> craftingResults = new();
    static void AddRecipe(ObjType inputOne, ObjType inputTwo, RecipeType output)
    {
        craftingResults.Add((inputOne, inputTwo), output);
    }
    static RecipeType GetResult(ObjType inputOne, ObjType inputTwo)
    {
        if (inputOne == null || inputTwo == null) return RecipeType.None;
        if (inputOne == ObjType.WaterNut || inputTwo == ObjType.WaterNut) return RecipeType.None;
        if (inputOne == LittleCrackerFisob.LittleCracker && inputTwo == LittleCrackerFisob.LittleCracker) return RecipeType.None;
        if (craftingResults.TryGetValue((inputOne, inputTwo), out var output) || craftingResults.TryGetValue((inputTwo, inputOne), out output))
            return output;
        return RecipeType.None;
    }
    #endregion
    public static void InitRecipes()
    {
        AddRecipe(LittleCrackerFisob.LittleCracker, ObjType.Rock, RecipeType.MakeMine);
        AddRecipe(ObjType.Lantern, ObjType.Rock, RecipeType.MakeMine);
        AddRecipe(ObjType.FlareBomb, ObjType.Rock, RecipeType.MakeMine);
        AddRecipe(ObjType.PuffBall, ObjType.Rock, RecipeType.MakeMine);
        AddRecipe(ObjType.Mushroom, ObjType.Rock, RecipeType.MakeMine);
        AddRecipe(ObjType.JellyFish, ObjType.Rock, RecipeType.MakeMine);

        AddRecipe(LittleCrackerFisob.LittleCracker, BoomMineFisob.BoomMine, RecipeType.ArmMine);
        AddRecipe(ObjType.Lantern, BoomMineFisob.BoomMine, RecipeType.ArmMine);
        AddRecipe(ObjType.JellyFish, BoomMineFisob.BoomMine, RecipeType.ArmMine);
        AddRecipe(ObjType.FlareBomb, BoomMineFisob.BoomMine, RecipeType.ArmMine);
        AddRecipe(ObjType.Creature, BoomMineFisob.BoomMine, RecipeType.ArmMine);
        AddRecipe(ObjType.PuffBall, BoomMineFisob.BoomMine, RecipeType.ArmMine);
        AddRecipe(ObjType.Mushroom, BoomMineFisob.BoomMine, RecipeType.ArmMine);

        AddRecipe(ObjType.Rock, ObjType.Rock, RecipeType.MakeSpear);
        AddRecipe(ObjType.Rock, ObjType.Spear, RecipeType.MakeSpearEle);
        AddRecipe(LittleCrackerFisob.LittleCracker, ObjType.Spear, RecipeType.MakeSpearFire);

        AddRecipe(ObjType.FlareBomb, ObjType.Spear, RecipeType.ArmSpearEle);
        AddRecipe(ObjType.JellyFish, ObjType.Spear, RecipeType.ArmSpearEle);
        AddRecipe(ObjType.Creature, ObjType.Spear, RecipeType.ArmSpearEle);

        AddRecipe(ObjType.FirecrackerPlant, ObjType.Spear, RecipeType.MakeLittleCracker);
        AddRecipe(ObjType.FirecrackerPlant, ObjType.Rock, RecipeType.MakeBomb);
        AddRecipe(LittleCrackerFisob.LittleCracker, ObjType.DangleFruit, RecipeType.MakeLantern);
        
        AddRecipe(ObjType.WaterNut, ObjType.ScavengerBomb, RecipeType.Wash);
        AddRecipe(ObjType.WaterNut, ObjType.Spear, RecipeType.Wash);
    }
    
    #region Firespear changes
    public static void Player_ThrownSpear(Player self, Spear spear)
    {
        if (QuickWorldData.SolaceCampaign && self.room.world.region.name != "HR" && spear.bugSpear)
            spear.spearDamageBonus *= 0.4f;
    }
    public static void Weapon_NewRoom(On.Weapon.orig_NewRoom orig, Weapon self, Room newRoom)
    {
        orig(self, newRoom);
        if (self is Spear spear && spear.bugSpear)
            spear.room.AddObject(new SpearHeatSource(spear, spear.firstChunk.pos));
    }
    class SpearHeatSource : UpdatableAndDeletable, IProvideWarmth
    {
        public Spear spear;
        public Vector2 Position() { return pos; }
        public Room loadedRoom => room;
        public Vector2 pos;
        public float warmth => RainWorldGame.DefaultHeatSourceWarmth * 0.5f;
        public float range => 40f;
        public SpearHeatSource(Spear spear, Vector2 pos)
        {
            this.spear = spear;
            this.pos = pos;
        }
        public override void Update(bool eu)
        {
            base.Update(eu);
            pos = spear.firstChunk.pos;
            if (!spear.bugSpear || 
                spear.room == null || 
                spear != spear.abstractPhysicalObject.realizedObject || 
                spear.room != loadedRoom || 
                loadedRoom != room.abstractRoom.realizedRoom) base.Destroy();
        }
    }
    #endregion

    #region Crafting
    public static bool Player_GraspsCanBeCrafted(On.Player.orig_GraspsCanBeCrafted orig, Player self)
    {
        if (self.grasps.Any(x => x?.grabbed is SolaceScarf) && self.input[0].y > 0 && self.FreeHand() == -1) 
            return SolaceScarfDyes.SolaceScarfCanDyeCheck(self);
        return orig(self) || AbleToPoacherCraft(self);
    }
    public static bool AbleToPoacherCraft(Player self)
    {
        if (self.input[0].y > 0 && self.FreeHand() == -1)
        {
            var name = self.slugcatStats.name;
            switch (name)
            {
                case var _ when name == MoreSlugcatsEnums.SlugcatStatsName.Gourmand: return false;
                case var _ when name == MoreSlugcatsEnums.SlugcatStatsName.Artificer: return false;
                case var _ when name == Plugin.DragonName: return true;
                default:
                    if (self.isNPC) return false;
                    if (ExpeditionGame.activeUnlocks.Contains(ExpeditionPerks.poacher)) return true;
                    return false;
            }}
        return false;
    }
    public static void Player_SpitUpCraftedObject(On.Player.orig_SpitUpCraftedObject orig, Player self)
    {
        if (self.grasps.Any(x => x?.grabbed is SolaceScarf) && SolaceScarfDyes.SolaceScarfCanDyeCheck(self))
        { SolaceScarfDyes.SolaceScarfDye(self); return; }
        if (AbleToPoacherCraft(self)) InitPoacherCraft(self);
        else orig(self);
    }
    public static void CraftFail(Player self)
    {
        self.room.PlaySound(SoundID.Big_Needle_Worm_Impale_Terrain, self.firstChunk, loop: false, 1f, 1f);
        self.room.AddObject(new Spark(self.bodyChunks[0].pos, Custom.RNV() * 60f * Random.value, Color.white, null, 20, 50));
    }
    public static void TearFirecracker(Player self, int lumps, List<PhysicalObject> ingredients)
    {
        FirecrackerPlant plant = ingredients.Find(x => x is FirecrackerPlant) as FirecrackerPlant;
        Spear spear = ingredients.Find(x => x is Spear) as Spear;

        if (spear == null || plant == null || spear.abstractSpear == null || self.room == null)
        {
            Debug.Log("Solace: TearFirecracker failed, something was null");
            CraftFail(self);
            return;
        }
        
        for (int i = plant.lumps.Length - 1; i >= 0; i--)
        {
            if (plant.lumpsPopped[i]) continue;

            self.room!.PlaySound(SoundID.Seed_Cob_Open, self.firstChunk, loop: false, 1f, 1f);
            AbstractPhysicalObject result = new LittleCrackerAbstract(self.room.world, self.abstractCreature.pos, plant.abstractPhysicalObject.ID);
            self.room!.abstractRoom.AddEntity(result);
            result.RealizeInRoom();
            plant.lumpsPopped[i] = true;
            if (i == 0)
            {
                if (self.room?.world.game.IsStorySession == true)
                    (self.room.game.session as StoryGameSession)?.RemovePersistentTracker(plant.abstractPhysicalObject);
                self.ReleaseGrasp(ingredients.IndexOf(plant));
                plant.RemoveFromRoom();
                self.room!.abstractRoom.RemoveEntity(plant.abstractPhysicalObject);
                self.SlugcatGrab(result.realizedObject, self.FreeHand());
                Debug.Log("Solace: Final popper torn off, plant destroyed");
            }
            break;
        }
    }

    public static void InitPoacherCraft(Player self)
    {
        List<PhysicalObject> ingredients = 
        [self.grasps[0].grabbed, 
            self.grasps[1].grabbed];
        var search = GetResult(ingredients[0].abstractPhysicalObject.type, ingredients[1].abstractPhysicalObject.type);

        if (search == RecipeType.None)
        {
            Debug.Log("Solace: Dragoncraft failure! These items have no recipe for Poacher! Working as intended. Generic.");
            CraftFail(self);
        }
        else if (RecipeType.MakeSpecialSpear.Contains(search))
            MakeSpearItem(self, ingredients, search);
        else if (search == RecipeType.ArmMine)
            MakeMineItem(self, ingredients);
        else if (search == RecipeType.Wash)
            WashItem(self, ingredients);
        else MakeUtilityItem(self, ingredients, search);
    }

    public static void MakeUtilityItem(Player self, List<PhysicalObject> ingredients, RecipeType recipe)
    {
        AbstractPhysicalObject result = null;
        CosmeticSprite effect = null; // Cosmetic visual
        SoundID sound = null; // Cosmetic sound
        float volume = 1f; // Volume of cosmetic sound
        float pitch = 1f; // pitch of cosmetic sound

        if (recipe == RecipeType.MakeBomb)
        {
            result = new AbstractPhysicalObject(self.room.world, ObjType.ScavengerBomb, null, self.abstractCreature.pos, self.room.game.GetNewID());
            effect = new Spark(self.bodyChunks[0].pos, Custom.RNV() * Mathf.Lerp(5f, 11f, Random.value), new Color(1f, 0.4f, 0.3f), null, 7, 17);
            sound = SoundID.Firecracker_Burn;
            volume = 0.2f;
        }
        else if (recipe == RecipeType.MakeLantern)
        {
            result = new AbstractPhysicalObject(self.room.world, ObjType.Lantern, null, self.abstractCreature.pos, self.room.game.GetNewID());
            effect = new Spark(self.bodyChunks[0].pos, Custom.RNV() * Mathf.Lerp(5f, 11f, Random.value), new Color(1f, 0.4f, 0.3f), null, 7, 17);
            sound = SoundID.Snail_Warning_Click;
        }
        else if (recipe == RecipeType.MakeMine)
        {
            result = new BoomMineAbstract(self.room.world, self.abstractCreature.pos, self.room.game.GetNewID());
            effect = new Spark(self.bodyChunks[0].pos, Custom.RNV() * 60f * Random.value, Color.white, null, 20, 50);
            sound = SoundID.Gate_Clamp_Lock;
            volume = 0.5f;
            pitch = 3f + Random.value;
        }
        else if (recipe == RecipeType.MakeSpear)
        {
            result = new AbstractSpear(self.room.world, null, self.abstractCreature.pos, self.room.game.GetNewID(), false);
            effect = new Spark(self.bodyChunks[0].pos, Custom.RNV() * 60f * Random.value, Color.white, null, 20, 50);
            sound = SoundID.Spear_Bounce_Off_Creauture_Shell;
        }
        else if (recipe == RecipeType.MakeLittleCracker)
        {
            self.GetGeneral().isMakingPoppers = true;
            var plant = ingredients.Find(x => x is FirecrackerPlant) as FirecrackerPlant;
            if (plant != null)
            {
                int lumps = plant.lumpsPopped.Count(x => !x);
                TearFirecracker(self, lumps, ingredients);
                return;
            }
        }
        self.GetGeneral().isMakingPoppers = false;
        
        MakeCosmeticEffects(self, sound, effect, volume, pitch);
        MakeCraftedItem(self, result, ingredients);
    }
    public static void MakeSpearItem(Player self, List<PhysicalObject> ingredients, RecipeType recipe)
    {
        AbstractPhysicalObject result = null;
        CosmeticSprite effect = null; // Cosmetic visual
        SoundID sound = null; // Cosmetic sound
        float volume = 1f; // Volume of cosmetic sound
        float pitch = 1f; // pitch of cosmetic sound
        var spear = ingredients.Find(x => x is Spear) as Spear;

        if (spear?.abstractSpear == null)
        {
            Debug.Log("Solace: Spearcraft failed, no abstract");
            CraftFail(self);
            return;
        }
        bool isANormalSpear = spear is not ExplosiveSpear && spear is not ElectricSpear && !spear.bugSpear;
        if (!isANormalSpear && recipe != RecipeType.ArmSpearEle)
        {
            Debug.Log("Solace: Spearcraft failed, this spear can't be crafted with");
            CraftFail(self);
            return;
        }

        if (recipe == RecipeType.MakeSpearFire)
        {
            result = new AbstractSpear(self.room.world, null, self.abstractCreature.pos, self.room.game.GetNewID(), false, 0.5f);
            effect = new Spark(self.bodyChunks[0].pos, Custom.RNV() * Mathf.Lerp(5f, 11f, Random.value), new Color(1f, 0.4f, 0.3f), null, 7, 17);
            sound = SoundID.Firecracker_Burn;
            volume = 0.2f;
        }
        else if (recipe == RecipeType.MakeSpearEle)
        {
            result = new AbstractSpear(self.room.world, null, self.abstractCreature.pos, self.room.game.GetNewID(), false)
                { electric = true, electricCharge = 0 };
            effect = new Spark(self.bodyChunks[0].pos, Custom.RNV() * 60f * Random.value, Color.white, null, 20, 50);
            sound = SoundID.Spear_Bounce_Off_Creauture_Shell;
        }
        else if (recipe == RecipeType.MakeSpearExpl)
        {
            result = new AbstractSpear(self.room.world, null, self.abstractCreature.pos, self.room.game.GetNewID(), true);
            sound = SoundID.Vulture_Wing_Woosh_LOOP;
        }
        else if (recipe == RecipeType.ArmSpearEle)
        {
            if (spear is not ElectricSpear || spear.abstractSpear.electricCharge >= 3)
            {
                Debug.Log("Solace: Failed to charge electric spear, invalid spear");
                CraftFail(self);
                return;
            }

            switch (ingredients.Find(x => x is not Spear))
            {
                case JellyFish: spear.abstractSpear.electricCharge += 3; break;
                case Centipede: spear.abstractSpear.electricCharge += 3; break;
                case FlareBomb a: spear.abstractSpear.electricCharge += 1; 
                    a.StartBurn();
                    break;
                default:
                    Debug.Log("Solace: Failed to charge electric spear, invalid charger");
                    CraftFail(self);
                    return;
            }
            ingredients.Remove(ingredients.Find(x => x is not Spear));
            sound = SoundID.Zapper_Zap;
            effect = new Explosion.ExplosionLight(self.bodyChunks[0].pos, 200f, 1f, 4, new Color(0.8f, 0.8f, 1f));
            if (ingredients.Contains(spear)) ingredients.Remove(spear);
        }
        
        MakeCosmeticEffects(self, sound, effect, volume, pitch);
        MakeCraftedItem(self, result, ingredients, -1, self.grasps.IndexOf(self.grasps.First(x => x.grabbed == spear)));
    }
    public static void MakeMineItem(Player self, List<PhysicalObject> ingredients)
    {
        CosmeticSprite effect = new Spark(self.bodyChunks[0].pos, Custom.RNV() * 60f * Random.value, Color.white, null, 20, 50);
        var mine = ingredients.Find(x => x is BoomMine) as BoomMine;
        var notMine = ingredients.Find(x => x is not BoomMine);
        float pitch = 3f + Random.value;
        int red = 1;
        int green = 2;
        int blue = 3;
        int upgrade = notMine switch
        {
            Lantern or LittleCracker => red,
            PuffBall or Mushroom => green,
            JellyFish or FlareBomb or Centipede => blue,
            _ => 0
        };
        if (upgrade == 0 || mine == null || mine.Abstr == null)
        {
            Debug.Log("Solace: Item cannot be used as mine ammo");
            CraftFail(self);
            return;
        }
             if (mine.Abstr.slot1 == 0) mine.Abstr.slot1 = upgrade;
        else if (mine.Abstr.slot2 == 0) mine.Abstr.slot2 = upgrade;
        else if (mine.Abstr.slot3 == 0) mine.Abstr.slot3 = upgrade;
        else { Debug.Log("Solace: Mine cannot take more ammo"); CraftFail(self); return; }
        
        if (ingredients.Contains(mine)) ingredients.Remove(mine);
        switch (notMine)
        {
            case JellyFish: if (ingredients.Contains(notMine)) ingredients.Remove(notMine); break;
            case Centipede: if (ingredients.Contains(notMine)) ingredients.Remove(notMine); break;
        }
        MakeCosmeticEffects(self, SoundID.Gate_Clamp_Lock, effect, 0.5f, pitch);
        MakeCraftedItem(self, null, ingredients, self.grasps.IndexOf(self.grasps.First(x => x.grabbed == notMine)));
    }

    public static void WashItem(Player self, List<PhysicalObject> ingredients)
    {
        AbstractPhysicalObject result = null;
        CosmeticSprite effect = new WaterDrip(
            self.firstChunk.pos,
            Custom.RNV() * Mathf.Lerp(5f, 11f, Random.value),
            waterColor: true);
        
        if (ingredients.Any(x => x is ScavengerBomb))
            result = new AbstractPhysicalObject(self.room.world, ObjType.Rock,null, self.abstractCreature.pos, self.room.game.GetNewID());
        if (ingredients.Any(x => x is Spear spr && spr.bugSpear))
            result = new AbstractSpear(self.room.world, null, self.abstractCreature.pos, self.room.game.GetNewID(), false);
        
        if (!ingredients.Any(x => x is SwollenWaterNut) || result == null)
        {
            Debug.Log("Solace: Make sure your bubblefruit is popped. If it is, it cannot wash this item. ");
            CraftFail(self);
            return;
        }
        
        MakeCosmeticEffects(self, SoundID.Swollen_Water_Nut_Terrain_Impact, null);
        ToolMethods.Repeat(()=> MakeCosmeticEffects(self, null, effect), 4);
        MakeCraftedItem(self, result, ingredients, -1, self.grasps.IndexOf(self.grasps.First(x => x.grabbed is not SwollenWaterNut)));
    }
    
    public static void MakeCraftedItem(Player self, AbstractPhysicalObject result, List<PhysicalObject> ingredients, int releaseOverride = -1, int grabOverride = -1)
    {
        if (ingredients.Any() && !ingredients.All(x => x == null))
            foreach (PhysicalObject obj in ingredients) // Remove materials
            {
                if (self.room.world.game.IsStorySession)
                    (self.room.game.session as StoryGameSession)?.RemovePersistentTracker(obj.abstractPhysicalObject);
                obj.RemoveFromRoom();
                self.room.abstractRoom.RemoveEntity(obj.abstractPhysicalObject);
                self.ReleaseGrasp((releaseOverride != -1) ? releaseOverride : ingredients.IndexOf(obj));
            }
        if (result != null) // Realize result
        {
            self.room.abstractRoom.AddEntity(result);
            result.RealizeInRoom();
            if (self.FreeHand() != -1)
            { self.SlugcatGrab(result.realizedObject, (grabOverride != -1) ? grabOverride : self.FreeHand()); }
        }
    }

    public static void MakeCosmeticEffects(Player self, SoundID id, UpdatableAndDeletable effect, float volume = 1f, float pitch = 1f)
    {
        if (id != null)
            self.room.PlaySound(id, self.firstChunk, loop: false, volume, pitch);
        if (effect != null)
            self.room.AddObject(effect);
    }
    
    public static void PoacherQuickCraft(Player self)
    {
        if (self.craftingObject && self.GetGeneral().isMakingPoppers && 
            self.grasps.Count(i => i.grabbed is FirecrackerPlant) == 1 && 
            self.swallowAndRegurgitateCounter < 70) 
            self.swallowAndRegurgitateCounter = 70;
    }
    #endregion
}
