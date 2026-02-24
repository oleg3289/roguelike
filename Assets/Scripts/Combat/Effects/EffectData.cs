using UnityEngine;

namespace Roguelike.Combat.Effects
{
    /// <summary>
    /// Serializable data for defining effects in CardData.
    /// </summary>
    [System.Serializable]
    public class EffectData
    {
        [SerializeField] private EffectType effectType;
        [SerializeField] private int value;
        [SerializeField] private string statusType; // Optional, for ApplyStatus

        public EffectType EffectType => effectType;
        public int Value => value;
        public string StatusType => statusType;

        public EffectData(EffectType type, int val, string status = null)
        {
            effectType = type;
            value = val;
            statusType = status;
        }
    }
}
