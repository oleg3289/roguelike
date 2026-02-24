namespace Roguelike.Relics
{
    /// <summary>
    /// Runtime instance of a relic.
    /// </summary>
    public class RelicInstance
    {
        private readonly RelicData data;
        
        public RelicData Data => data;
        public string Name => data.RelicName;
        public string Description => data.Description;
        public RelicRarity Rarity => data.Rarity;
        
        public RelicInstance(RelicData relicData)
        {
            data = relicData;
        }
    }
}
