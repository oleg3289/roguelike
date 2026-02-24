namespace Roguelike.Combat.Effects
{
    /// <summary>
    /// Gives block to a target.
    /// </summary>
    public class BlockEffect : IEffect
    {
        private int value;
        
        public EffectType EffectType => EffectType.Block;
        public int Value 
        { 
            get => value; 
            set => this.value = value; 
        }
        
        public BlockEffect(int block)
        {
            value = block;
        }
        
        public void Execute(ICombatContext context, ITargetable target)
        {
            target.AddBlock(value);
        }
    }
}
