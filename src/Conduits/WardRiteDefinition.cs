using System.Collections.Generic;

namespace VikingWards.Conduits
{
    public sealed class OfferingCost
    {
        public string ItemPrefab { get; }
        public int Amount { get; }

        public OfferingCost(string itemPrefab, int amount)
        {
            ItemPrefab = itemPrefab;
            Amount = amount;
        }
    }

    public sealed class WardRiteDefinition
    {
        public string Id { get; }
        public DeityType Deity { get; }
        public int Tier { get; }
        public string Name { get; }
        public string RuneGlyph { get; }
        public string Description { get; }
        public IReadOnlyList<OfferingCost> Costs { get; }

        public WardRiteDefinition(
            string id,
            DeityType deity,
            int tier,
            string name,
            string runeGlyph,
            string description,
            params OfferingCost[] costs)
        {
            Id = id;
            Deity = deity;
            Tier = tier;
            Name = name;
            RuneGlyph = runeGlyph;
            Description = description;
            Costs = costs;
        }
    }
}
