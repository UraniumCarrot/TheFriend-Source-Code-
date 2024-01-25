using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TheFriend;
public partial class Extensions
{
    /// <summary>
    /// Data helper type for GenerateRandomList
    /// </summary>
    /// <remarks>
    /// <para>Note: Weight is multiplied by Limit when used in the calculation, so be sure to adjust the weight accordingly or items with higher limits might appear more often than expected</para>
    /// </remarks>
    public class WeightedItem<T>
    {
        /// <summary>
        /// The element this entry represents
        /// </summary>
        public T Item;
        /// <summary>
        /// Roughly how often this item should appear vs other elements
        /// </summary>
        public float Weight;
        /// <summary>
        /// The maximum amount of this element you want to see in the output list
        /// </summary>
        public int Limit;
        /// <summary>
        /// How much this item contributes to a running total
        /// </summary>
        public float? Contribution;
    }

    /// <summary>
    /// Generates a list of random elements sourced from the input list parameter "entries"
    /// </summary>
    /// <param name="inputList">The list of weighted items to use</param>
    /// <param name="count">The number of items to generate</param>
    /// <param name="seed">An optional seed for reproducible results</param>
    /// <param name="target">A maximum target "Contribution" value that the function will attempt to reach, but never exceed.</param>
    /// <returns>A list of randomly selected items from the "entries" input list</returns>
    /// <remarks>
    /// <para>Percent chance for each item to be picked is ((Limit * Weight) / totalWeight) * 100 where totalWeight is all of the individual (Limit * Weight) in the "entries" list summed together</para>
    /// <para>When an item with more than 1 as its limit is selected, its Limit will be reduced by 1, and its chances will be reduced accordingly (with others being increased due to this)</para>
    /// <para>Example: {Limit = 3, Weight = 10} will be worth 30 weight, after one has been chosen, it will now only be worth 20 weight.</para>
    /// <para>When "lowerTarget" and "upperTarget" is used, "count" is the max amount of creatures that will be used to reach the target value</para>
    /// <para>Otherwise, the function just generates the requested count.</para>
    /// </remarks>
    public static IList<T> GenerateRandomList<T>(IList<WeightedItem<T>> inputList, int count, int? seed = null, float? target = null)
    {
        // Check for empty list
        if (count <= 0) return [];

        // Check if we can reach the count using the items in the list, otherwise clamp the generated count
        var maxPossibleCount = inputList.Sum(item => item.Limit);
        if (count > maxPossibleCount)
        {
            Plugin.LogSource.LogError($"GenerateRandomList(): given a count that is higher than is possible using the supplied list! Count: {count}, Max Possible: {maxPossibleCount}, Clamping output to {maxPossibleCount} nodes!");
            count = maxPossibleCount;
        }

        // Make a deep copy of inputList (please tell me if an easier way exists to do this)
        List<WeightedItem<T>> entries = inputList
            .Select(x => new WeightedItem<T>
            {
                Item = x.Item,
                Weight = x.Weight,
                Limit = x.Limit,
                Contribution = x.Contribution
            })
            .ToList();

        // create the initial total weight, Add together all weights, taking into consideration the limit of each item
        var totalWeight = entries.Sum(item => item.Weight * item.Limit);
        var totalValue = 0f;

        // Set the RNG seed if we want to make reproducible results
        var state = SetRngSeed(seed);

        List<T> outputList = [];

        // select each item

        for (var i = 0; i < count; i++)
        {
            // Exit if we empty the entries list early
            if (!entries.Any())
                break;

            var item = GetRandomEntry(entries, totalWeight);

            if (target != null)
            {
                // Check for null contributions, and remove them if so
                if (item.Contribution == null)
                {
                    Plugin.LogSource.LogError(
                        $"GenerateRandomList(): An item with a null \"Contribution\" value was found while using targets, removing item from future selection in this function call...");
                    totalWeight -= item.Weight * item.Limit;
                    entries.Remove(item);
                    i--;
                    continue;
                }

                // If this item would put us over the target, skip it and remove it from further selection, so that they dont get in the way of items that wouldn't
                if (totalValue + item.Contribution > target)
                {
                    totalWeight -= item.Weight * item.Limit;
                    entries.Remove(item);
                    i--;
                    continue;
                }

                totalValue += item.Contribution.Value;
            }

            item.Limit -= 1;
            totalWeight -= item.Weight;

            outputList.Add(item.Item);

            if (item.Limit == 0)
                entries.Remove(item);
        }

        // If we modified the RNG using a seed, restore it back to what it was beforehand
        if (state != null)
            Random.state = state.Value;

        return outputList;
    }

    /// <summary>
    /// Gets a random entry from the supplied list of entries
    /// </summary>
    /// <param name="entries">The list of weighted items to use</param>
    /// <param name="totalWeight">The running total weight</param>
    /// <returns>A random WeightedItem from the supplied "entries" list</returns>
private static WeightedItem<T> GetRandomEntry<T>(IEnumerable<WeightedItem<T>> entries, float totalWeight)
{
    var threshold = Random.Range(0, totalWeight);
    var acc = 0f; // accumulator
    // return the first entry that passes the threshold
    return entries.FirstOrDefault(entry => (acc += entry.Weight * entry.Limit) >= threshold);
}

    /// <summary>
    /// Sets the seed of UnityEngine.Random to the supplied value, if "seed" is not null
    /// </summary>
    /// <param name="seed">The seed to init UnityEngine.Random with</param>
    /// <returns>The state of UnityEngine.Random before setting the seed, null if the supplied seed was null</returns>
    private static Random.State? SetRngSeed(int? seed)
    {
        if (seed == null) return null;
        
        var state = Random.state;
        Random.InitState(seed.Value);

        return state;
    }
}