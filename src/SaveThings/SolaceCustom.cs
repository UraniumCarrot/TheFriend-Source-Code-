using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace TheFriend.SaveThings;

public static class SolaceCustom
{
    public static void Apply()
    {
        On.PlayerProgression.ClearOutSaveStateFromMemory += PlayerProgressionOnClearOutSaveStateFromMemory;
        On.PlayerProgression.WipeSaveState += PlayerProgressionOnWipeSaveState;
    }

    private static string SolaceSaveDataPath => Application.persistentDataPath + Path.DirectorySeparatorChar + Plugin.MOD_ID;
    private static readonly Dictionary<string, string> MalnourishedSaveData;

    static SolaceCustom()
    {
        MalnourishedSaveData = new Dictionary<string, string>();
        Directory.CreateDirectory(SolaceSaveDataPath);
    }

    public static bool SaveStorySpecific<T>(string key, T value, StoryGameSession storySession)
    {
        try
        {
            var rawValue = JsonConvert.SerializeObject(value);
            if (storySession.saveState.malnourished)
            {
                var name = storySession.saveStateNumber;
                Plugin.LogSource.LogInfo($"{name} starved! Saving {key} to memory...");
                MalnourishedSaveData[key + name] = rawValue;
                return true;
            }

            var savePath = GetStorySavePath(storySession);

            if (File.Exists(savePath))
            {
                var rawData = File.ReadAllText(savePath);
                var jsonData = JsonConvert.DeserializeObject<Dictionary<string, string>>(rawData);
                jsonData[key] = rawValue;
                rawData = JsonConvert.SerializeObject(jsonData, Formatting.Indented);
                File.WriteAllText(savePath, rawData);
            }
            else
            {
                var jsonData = new Dictionary<string, string>();
                jsonData[key] = rawValue;
                var rawData = JsonConvert.SerializeObject(jsonData, Formatting.Indented);
                File.WriteAllText(savePath, rawData);
            }
        }
        catch (Exception ex)
        {
            Plugin.LogSource.LogError($"ERROR WHILE SAVING: {key}");
            Plugin.LogSource.LogError(ex);
            return false;
        }
        return true;
    }

    public static bool LoadStorySpecific<T>(string key, out T value, StoryGameSession storySession)
    {
        value = default;
        try
        {
            if (storySession.saveState.malnourished)
            {
                var name = storySession.saveStateNumber;
                Plugin.LogSource.LogInfo($"{name} starved! Trying to load {key} from memory...");
                if (MalnourishedSaveData.TryGetValue(key + name, out var rawValue))
                {
                    value = JsonConvert.DeserializeObject<T>(rawValue);
                    MalnourishedSaveData.Remove(key + name);
                    return true;
                }
                Plugin.LogSource.LogInfo("Failed to load from memory, attempting to load from disk instead");
            }

            var savePath = GetStorySavePath(storySession);

            if (File.Exists(savePath))
            {
                var rawData = File.ReadAllText(savePath);
                var jsonData = JsonConvert.DeserializeObject<Dictionary<string, string>>(rawData);
                if (jsonData.TryGetValue(key, out var rawValue))
                {
                    value = JsonConvert.DeserializeObject<T>(rawValue);
                    return true;
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            Plugin.LogSource.LogError($"ERROR WHILE LOADING: {key}");
            Plugin.LogSource.LogError(ex);
            return false;
        }
    }

    private static string GetStorySavePath(StoryGameSession storySession)
    {
        return GetStorySavePath(storySession.saveStateNumber);
    }
    private static string GetStorySavePath(SlugcatStats.Name name)
    {
        var saveSlot = RWCustom.Custom.rainWorld.options.saveSlot.ToString();
        var savePath = Path.Combine(SolaceSaveDataPath, saveSlot);
        Directory.CreateDirectory(savePath);
        return Path.Combine(savePath, name + ".json");
    }

    //-----

    private static void PlayerProgressionOnClearOutSaveStateFromMemory(On.PlayerProgression.orig_ClearOutSaveStateFromMemory orig, PlayerProgression self)
    {
        orig(self);
        MalnourishedSaveData.Clear();
    }

    private static void PlayerProgressionOnWipeSaveState(On.PlayerProgression.orig_WipeSaveState orig, PlayerProgression self, SlugcatStats.Name savestatenumber)
    {
        orig(self, savestatenumber);
        var savePath = GetStorySavePath(savestatenumber);
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
        }
    }
}