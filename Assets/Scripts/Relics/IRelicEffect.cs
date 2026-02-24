using Roguelike.Combat;
using Roguelike.Combat.Entities;

namespace Roguelike.Relics
{
    /// <summary>
    /// Interface for relics that have gameplay effects.
    /// </summary>
    public interface IRelicEffect
    {
        void OnCombatStart(CombatContext context, Player player);
        void OnCombatEnd(CombatContext context, Player player);
        void OnTurnStart(CombatContext context, Player player);
        void OnTurnEnd(CombatContext context, Player player);
        void OnCardPlayed(CombatContext context, Player player, Cards.CardInstance card);
        void OnDamageDealt(CombatContext context, Player player, int damage);
        void OnDamageTaken(CombatContext context, Player player, int damage);
    }
}
