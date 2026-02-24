namespace Roguelike.Combat.Effects
{
    /// <summary>
    /// Heals a target.
    /// </summary>
    public class HealEffect : IEffect
    {
        private int value;
        
        public EffectType EffectType => EffectType.Heal;
        public int Value 
        { 
            get => value; 
            set => this.value = value; 
       }
        
        public HealEffect(int heal)
        {
            value = heal;
        }
        
        public void Execute(ICombatContext context, ITargetable target)
        {
            target.Heal(value);
        }
    }
}
