using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace TheFriend.SaveThings;

public static class SolaceCustom
{
    private static string SolaceSaveDataPath => Application.persistentDataPath + Path.DirectorySeparatorChar + Plugin.MOD_ID;

    static SolaceCustom()
    {
        Directory.CreateDirectory(SolaceSaveDataPath);
    }

    public static bool SaveStorySpecific<T>(string key, T value, StoryGameSession storySession)
    {
        //todo handle malnourished
        try
        {
            var savePath = GetStorySavePath(storySession);
            var rawValue = JsonConvert.SerializeObject(value);

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
        var saveSlot = RWCustom.Custom.rainWorld.options.saveSlot.ToString();
        var savePath = Path.Combine(SolaceSaveDataPath, saveSlot);
        Directory.CreateDirectory(savePath);
        var name = storySession.saveStateNumber;
        return Path.Combine(savePath, name + ".json");
    }

}