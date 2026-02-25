using UnityEngine;
using Roguelike.Cards;

namespace Roguelike.UI
{
    /// <summary>
    /// Sets up the combat scene with all necessary UI elements.
    /// Attach this to a GameObject in the scene.
    /// </summary>
    public class CombatSceneSetup : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CardData strikeCard;
        [SerializeField] private CardData defendCard;
        
        private void Awake()
        {
            // Ensure we have card data
            if (strikeCard == null || defendCard == null)
            {
                Debug.LogError("Card data not assigned! Make sure Strike and Defend cards exist.");
            }
        }
    }
}
