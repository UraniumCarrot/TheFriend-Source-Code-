using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheFriend;
public partial class Extensions
{
    // Basic tests for GenerateRandomList()
    public static void TestGenerateRandomList()
    {
        // Basic test with ints
        var intTest = new List<WeightedItem<int>>
        {
            new() { Item = 1, Weight = 100, Limit = 1 },
            new() { Item = 2, Weight = 50, Limit = 2 },
            new() { Item = 3, Weight = 25, Limit = 4 }
        };
        // 0 & 9 should print an error, all  should generate a random sequence adhering to the limits though
        for (var i = 0; i < 9; i++)
        {
            Plugin.LogSource.LogDebug($"Testing generation of {i} random ints from the list, using seed \"0x1234\"");
            LogList(GenerateRandomList(intTest, i, 0x1234));
        }

        // should be relatively random outputs
        for (var i = 1; i < 8; i++)
        {
            Plugin.LogSource.LogDebug($"Testing generation of 7 random ints from the list:");
            LogList(GenerateRandomList(intTest, 7));
        }

        // test with creatures
        var creatureTest = new List<WeightedItem<CreatureTemplate.Type>>
        {
            new() { Item = CreatureTemplate.Type.BlueLizard, Weight = 20, Limit = 5 },
            new() { Item = CreatureTemplate.Type.PinkLizard, Weight = 20, Limit = 5 },
            new() { Item = CreatureTemplate.Type.Salamander, Weight = 20, Limit = 5 },
            new() { Item = CreatureTemplate.Type.CyanLizard, Weight = 10, Limit = 3 },
            new() { Item = CreatureTemplate.Type.RedLizard, Weight = 5, Limit = 1 },
            new() { Item = CreatureTemplate.Type.Vulture, Weight = 5, Limit = 1 }
        };

        for (var i = 1; i < 8; i++)
        {
            Plugin.LogSource.LogDebug($"Testing generation of 10 random creatures:");
            LogList(GenerateRandomList(creatureTest, 10));
        }
    }

    // Helper function to print lists to the logger
    private static void LogList<T>(IList<T> list)
    {
        var resultString = new StringBuilder();

        resultString.Append("[");

        foreach (var item in list)
            resultString.Append($"{item}, ");

        if (list.Any())
            resultString.Remove(resultString.Length - 2, 2);

        resultString.Append("]");
        Plugin.LogSource.LogDebug(resultString);
    }
}