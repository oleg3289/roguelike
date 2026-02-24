using System.Collections.Generic;
using Roguelike.Cards;
using Roguelike.Combat.Status;

namespace Roguelike.Combat.Entities
{
    /// <summary>
    /// Player entity in combat.
    /// </summary>
    public class Player : IEntity
    {
        private readonly int maxHealth;
        private int currentHealth;
        private int block;
        private readonly StatusManager statusManager = new();
        private readonly List<CardInstance> deck = new();
        private readonly List<CardInstance> hand = new();
        private readonly List<CardInstance> discardPile = new();
        
        private int maxEnergy = 3;
        private int currentEnergy;
        
        public string Name => "Player";
        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public int Block => block;
        public bool IsDead => currentHealth <= 0;
        public StatusManager StatusManager => statusManager;
        
        public int CurrentEnergy => currentEnergy;
        public int MaxEnergy => maxEnergy;
        public IReadOnlyList<CardInstance> Hand => hand;
        public IReadOnlyList<CardInstance> Deck => deck;
        public IReadOnlyList<CardInstance> DiscardPile => discardPile;
        
        public Player(int health, int energy = 3)
        {
            maxHealth = health;
            currentHealth = health;
            maxEnergy = energy;
            currentEnergy = energy;
            block = 0;
        }
        
        public void TakeDamage(int amount)
        {
            amount = statusManager.ModifyIncomingDamage(amount);
            
            if (block >= amount)
            {
                block -= amount;
                return;
            }
            
            amount -= block;
            block = 0;
            currentHealth -= amount;
            
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                Die();
            }
        }
        
        public void Heal(int amount)
        {
            currentHealth += amount;
            if (currentHealth > maxHealth)
                currentHealth = maxHealth;
        }
        
        public void AddBlock(int amount)
        {
            block += amount;
        }
        
        public void ResetBlock()
        {
            block = 0;
        }
        
        public void Die()
        {
            // Player death - will be handled by combat context
        }
        
        public void ResetEnergy()
        {
            currentEnergy = maxEnergy;
        }
        
        public bool TrySpendEnergy(int cost)
        {
            if (currentEnergy >= cost)
            {
                currentEnergy -= cost;
                return true;
            }
            return false;
        }
        
        public void AddToDeck(CardInstance card) => deck.Add(card);
        public void AddToHand(CardInstance card) => hand.Add(card);
        public void Discard(CardInstance card)
        {
            hand.Remove(card);
            discardPile.Add(card);
        }
    }
}
