namespace Roguelike.Cards
{
    /// <summary>
    /// Runtime instance of a card during gameplay.
    /// Holds mutable state (current cost, modifications, etc.)
    /// </summary>
    public class CardInstance
    {
        private readonly CardData data;
        private int currentCost;
        private bool isModified;
        private bool exhausted;

        public CardInstance(CardData cardData)
        {
            data = cardData;
            currentCost = cardData.Cost;
            isModified = false;
            exhausted = false;
        }

        public CardData Data => data;
        public int CurrentCost => currentCost;
        public bool IsModified => isModified;
        public bool IsExhausted => exhausted;

        public void SetCost(int newCost)
        {
            if (currentCost != newCost)
            {
                currentCost = newCost;
                isModified = true;
            }
        }

        public void Reset()
        {
            currentCost = data.Cost;
            isModified = false;
            exhausted = false;
        }

        public void Exhaust()
        {
            exhausted = true;
        }

        public CardInstance Clone()
        {
            return new CardInstance(data)
            {
                currentCost = this.currentCost,
                isModified = this.isModified,
                exhausted = this.exhausted
            };
        }
    }
}
