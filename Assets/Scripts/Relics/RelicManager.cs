using System.Collections.Generic;
using Roguelike.Combat;
using Roguelike.Combat.Entities;
using Roguelike.Cards;

namespace Roguelike.Relics
{
    /// <summary>
    /// Manages all relics the player has collected.
    /// </summary>
    public class RelicManager
    {
        private readonly List<RelicInstance> relics = new();
        
        public IReadOnlyList<RelicInstance> Relics => relics;
        public int RelicCount => relics.Count;
        
        public void AddRelic(RelicInstance relic)
        {
            relics.Add(relic);
        }
        
        public void RemoveRelic(RelicInstance relic)
        {
            relics.Remove(relic);
        }
        
        public bool HasRelic(string relicName)
        {
            return relics.Exists(r => r.Name == relicName);
        }
        
        // Trigger methods for relic effects
        public void TriggerOnCombatStart(CombatContext context, Player player)
        {
            foreach (var relic in relics)
            {
                if (relic.Data is IRelicEffect effect)
                {
                    effect.OnCombatStart(context, player);
                }
            }
        }
        
        public void TriggerOnCombatEnd(CombatContext context, Player player)
        {
            foreach (var relic in relics)
            {
                if (relic.Data is IRelicEffect effect)
                {
                    effect.OnCombatEnd(context, player);
                }
            }
        }
        
        public void TriggerOnTurnStart(CombatContext context, Player player)
        {
            foreach (var relic in relics)
            {
                if (relic.Data is IRelicEffect effect)
                {
                    effect.OnTurnStart(context, player);
                }
            }
        }
        
        public void TriggerOnTurnEnd(CombatContext context, Player player)
        {
            foreach (var relic in relics)
            {
                if (relic.Data is IRelicEffect effect)
                {
                    effect.OnTurnEnd(context, player);
                }
            }
        }
        
        public void TriggerOnCardPlayed(CombatContext context, Player player, CardInstance card)
        {
            foreach (var relic in relics)
            {
                if (relic.Data is IRelicEffect effect)
                {
                    effect.OnCardPlayed(context, player, card);
                }
            }
        }
        
        public void TriggerOnDamageDealt(CombatContext context, Player player, int damage)
        {
            foreach (var relic in relics)
            {
                if (relic.Data is IRelicEffect effect)
                {
                    effect.OnDamageDealt(context, player, damage);
                }
            }
        }
        
        public void TriggerOnDamageTaken(CombatContext context, Player player, int damage)
        {
            foreach (var relic in relics)
            {
                if (relic.Data is IRelicEffect effect)
                {
                    effect.OnDamageTaken(context, player, damage);
                }
            }
        }
    }
}
