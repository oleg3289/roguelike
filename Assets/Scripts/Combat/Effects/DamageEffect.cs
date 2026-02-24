namespace Roguelike.Combat.Effects
{
    /// <summary>
    /// Deals damage to a target.
    /// </summary>
    public class DamageEffect : IEffect
    {
        private int value;
        
        public EffectType EffectType => EffectType.Damage;
        public int Value 
        { 
            get => value; 
            set => this.value = value; 
        }
        
        public DamageEffect(int damage)
        {
            value = damage;
        }
        
        public void Execute(ICombatContext context, ITargetable target)
        {
            target.TakeDamage(value);
        }
    }
}
