using UnityEngine;
using Roguelike.Combat.Effects;

namespace Roguelike.Cards
{
    /// <summary>
    /// ScriptableObject definition for a card template.
    /// Instances of cards are created from this data.
    /// </summary>
    [CreateAssetMenu(fileName = "NewCard", menuName = "Roguelike/Card Data")]
    public class CardData : ScriptableObject
    {
        [Header("Basic Info")]
        [SerializeField] private string cardName;
        [SerializeField] [TextArea(2, 4)] private string description;
        [SerializeField] [TextArea(1, 2)] private string flavorText;

        [Header("Card Properties")]
        [SerializeField] private int cost = 1;
        [SerializeField] private CardType cardType;
        [SerializeField] private TargetType targetType;
        [SerializeField] private CardRarity rarity;

        [Header("Effects")]
        [SerializeField] private EffectData[] effects;

        public string CardName => cardName;
        public string Description => description;
        public string FlavorText => flavorText;
        public int Cost => cost;
        public CardType CardType => cardType;
        public TargetType TargetType => targetType;
        public CardRarity Rarity => rarity;
        public EffectData[] Effects => effects;
    }
}
