using UnityEngine;

namespace Roguelike.Relics
{
    /// <summary>
    /// ScriptableObject definition for a relic.
    /// </summary>
    [CreateAssetMenu(fileName = "NewRelic", menuName = "Roguelike/Relic")]
    public class RelicData : ScriptableObject
    {
        [SerializeField] private string relicName;
        [SerializeField] private string description;
        [SerializeField] private RelicRarity rarity;
        [SerializeField] private Sprite icon;
        
        public string RelicName => relicName;
        public string Description => description;
        public RelicRarity Rarity => rarity;
        public Sprite Icon => icon;
    }
}
