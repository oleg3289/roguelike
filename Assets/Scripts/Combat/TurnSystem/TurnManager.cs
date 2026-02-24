using System;
using System.Collections.Generic;

namespace Roguelike.Combat.TurnSystem
{
    /// <summary>
    /// Manages turn order and state transitions in combat.
    /// </summary>
    public class TurnManager
    {
        private TurnState currentState;
        private int turnNumber;
        
        public event Action<TurnState> OnStateChanged;
        public event Action<int> OnTurnStarted;
        
        public TurnState CurrentState => currentState;
        public int TurnNumber => turnNumber;
        
        public TurnManager()
        {
            currentState = TurnState.PlayerTurn;
            turnNumber = 1;
        }
        
        public void StartCombat()
        {
            currentState = TurnState.PlayerTurn;
            turnNumber = 1;
            OnStateChanged?.Invoke(currentState);
            OnTurnStarted?.Invoke(turnNumber);
        }
        
        public void EndPlayerTurn()
        {
            currentState = TurnState.TurnEnd;
            OnStateChanged?.Invoke(currentState);
        }
        
        public void StartEnemyTurn()
        {
            currentState = TurnState.EnemyTurn;
            OnStateChanged?.Invoke(currentState);
        }
        
        public void EndEnemyTurn()
        {
            turnNumber++;
            currentState = TurnState.PlayerTurn;
            OnStateChanged?.Invoke(currentState);
            OnTurnStarted?.Invoke(turnNumber);
        }
        
        public void EndCombat()
        {
            currentState = TurnState.CombatEnd;
            OnStateChanged?.Invoke(currentState);
        }
    }
}
