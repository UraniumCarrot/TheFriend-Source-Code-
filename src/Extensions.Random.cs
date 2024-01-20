using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TheFriend;
public partial class Extensions
{
    // Data helper type for GenerateRandomList
    // Item: The element this entry represents
    // Weight: Roughly how much this item should be worth when calculating vs other elements
    // Limit: The maxiumum amount of this element you want to see in the output list
    // Weight is multiplied by Limit when used in the calculation, so be sure to adjust the weight accordingly or items with higher limits might appear more often than expected
    public class WeightedItem<T>
    {
        public T Item;
        public float Weight;
        public int Limit;
    }

    // Generates a list of random elements sourced from the input list parameter "entries"
    // Percent chance for each item to be picked is ((Limit * Weight) / totalWeight) * 100 where totalWeight is all of the individual (Limit * Weight) in the list summed together
    // When an item with more than 1 as its limit is selected, its Limit will be reduced by 1, and its chances will be reduced accordingly (with others being increased due to this)
    // Items with 1 as the Limit will simply be taken out of the equation once selected, as 0 (Limit*Weight) will remove their chance of being selected.
    // The length of the output list is set using the "count" parameter
    // An option seed parameter can be used if reproducable results are desired.
    public static IList<T> GenerateRandomList<T>(IList<WeightedItem<T>> entries, int count, int? seed = null)
    {
        // Check for empty list
        if (count <= 0)
        {
            Plugin.LogSource.LogMessage($"GenerateRandomList() asked to generate empty list? count = 0");
            return new List<T>();
        }

        // Check if we can reach the count using the items in the list, otherwise log error and clamp the generated count
        var maxPossibleCount = entries.Sum(item => item.Limit);
        if (count > maxPossibleCount)
        {
            Plugin.LogSource.LogError($"GenerateRandomList(): given a count that is higher than is possible using the supplied list! Count: {count}, Max Possible: {maxPossibleCount}, Clamping output to {maxPossibleCount} nodes!");
            count = maxPossibleCount;
        }

        // create the initial total weight, Add together all weights, taking into consideration the limit of each item
        var totalWeight = entries.Sum(item => item.Weight * item.Limit);

        // Set the RNG seed if we want to make reproducable results
        Random.State? state = null;
        if (seed != null)
        {
            state = Random.state;
            Random.InitState(seed.Value);
        }

        // make a copy of the limits so we do not modify the original
        var limits = entries
            .Select(entry => entry.Limit)
            .ToList();

        List<T> outputList = new();

        // select each item
        for (var i = 0; i < count; i++)
        {
            var threshold = Random.Range(0, totalWeight);
            var acc = 0f; // accumulator

            // Find the first item that is above the threshold, and add it to the output list
            // added items are removed from future selection by decrementing their limit & removing their weight from the total
            for (var j = 0; j < entries.Count; j++)
            {
                var entry = entries[j];
                if (acc + entry.Weight * limits[j] > threshold)
                {
                    limits[j] -= 1;
                    totalWeight -= entry.Weight;
                    outputList.Add(entry.Item);
                    break;
                }
                acc += entry.Weight * limits[j];
            }
        }

        // If we modified the RNG using a seed, restore it back to what it was beforehand
        if (state != null)
        {
            Random.state = state.Value;
        }

        return outputList;
    }
}