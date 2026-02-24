using System.Collections.Generic;
using Roguelike.Combat.Entities;
using Roguelike.Combat.Effects;

namespace Roguelike.Combat.Status
{
    /// <summary>
    /// Manages status effects on an entity.
    /// </summary>
    public class StatusManager
    {
        private readonly Dictionary<StatusType, IStatusEffect> effects = new();
        
        public IReadOnlyDictionary<StatusType, IStatusEffect> Effects => effects;
        
        public void ApplyStatus(StatusType type, int stacks)
        {
            if (effects.TryGetValue(type, out var existing))
            {
                existing.ApplyStacks(stacks);
            }
            else
            {
                effects[type] = new StatusEffect(type, stacks);
            }
        }
        
        public void RemoveStatus(StatusType type, int stacks = 0)
        {
            if (stacks == 0)
            {
                effects.Remove(type);
            }
            else if (effects.TryGetValue(type, out var existing))
            {
                existing.RemoveStacks(stacks);
                if (existing.Stacks <= 0)
                {
                    effects.Remove(type);
                }
            }
        }
        
        public int GetStatusStacks(StatusType type)
        {
            return effects.TryGetValue(type, out var effect) ? effect.Stacks : 0;
        }
        
        public bool HasStatus(StatusType type)
        {
            return effects.ContainsKey(type);
        }
        
        public void ProcessTurnStart(IEntity owner, ICombatContext context)
        {
            foreach (var effect in effects.Values)
            {
                effect.OnTurnStart(owner, context);
            }
        }
        
        public void ProcessTurnEnd(IEntity owner, ICombatContext context)
        {
            var toRemove = new List<StatusType>();
            
            foreach (var kvp in effects)
            {
                kvp.Value.OnTurnEnd(owner, context);
                if (kvp.Value.Stacks <= 0)
                {
                    toRemove.Add(kvp.Key);
                }
            }
            
            foreach (var type in toRemove)
            {
                effects.Remove(type);
            }
        }
        
        public int ModifyOutgoingDamage(int baseDamage)
        {
            int damage = baseDamage;
            
            if (HasStatus(StatusType.Strength))
            {
                damage += GetStatusStacks(StatusType.Strength);
            }
            
            if (HasStatus(StatusType.Weak))
            {
                damage = damage * 3 / 4; // 25% reduction
            }
            
            return damage;
        }
        
        public int ModifyIncomingDamage(int baseDamage)
        {
            int damage = baseDamage;
            
            if (HasStatus(StatusType.Vulnerable))
            {
                damage = damage * 3 / 2; // 50% increase
            }
            
            return damage;
        }
        
        public int ModifyBlock(int baseBlock)
        {
            int block = baseBlock;
            
            if (HasStatus(StatusType.Dexterity))
            {
                block += GetStatusStacks(StatusType.Dexterity);
            }
            
            if (HasStatus(StatusType.Frail))
            {
                block = block * 3 / 4; // 25% reduction
            }
            
            return block;
        }
    }
}
