using UnityEngine;
using Roguelike.Cards;

namespace Roguelike.UI
{
    /// <summary>
    /// Bootstrapper for the combat scene. Auto-creates the Combat object with UI controller.
    /// Just add this to any GameObject in the scene and press Play.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class CombatSceneSetup : MonoBehaviour
    {
        [Header("Optional: Assign cards here to override auto-load")]
        [SerializeField] private CardData strikeCard;
        [SerializeField] private CardData defendCard;
        
        private void Awake()
        {
            // Ensure CombatUIController exists in scene
            var existingController = FindFirstObjectByType<CombatUIController>();
            if (existingController == null)
            {
                Debug.Log("[CombatSceneSetup] Creating Combat object...");
                var combatGO = new GameObject("Combat");
                existingController = combatGO.AddComponent<CombatUIController>();
            }
            
            // Only assign if we have cards AND the controller's fields are empty
            // (CombatUIController auto-loads from Resources in its Awake)
            if (strikeCard != null && defendCard != null)
            {
                var controllerType = typeof(CombatUIController);
                var strikeField = controllerType.GetField("strikeCard", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var defendField = controllerType.GetField("defendCard", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                strikeField?.SetValue(existingController, strikeCard);
                defendField?.SetValue(existingController, defendCard);
            }
        }
        
#if UNITY_EDITOR
        [UnityEditor.MenuItem("GameObject/Combat/Create Combat Setup", false, 10)]
        private static void CreateCombatSetup()
        {
            var go = new GameObject("[Combat Setup]");
            go.AddComponent<CombatSceneSetup>();
            UnityEditor.Selection.activeGameObject = go;
            Debug.Log("[CombatSceneSetup] Created setup object. Press Play to see combat UI.");
        }
        
        [UnityEditor.MenuItem("GameObject/Combat/Create Combat Object", false, 11)]
        private static void CreateCombatObject()
        {
            var go = new GameObject("Combat");
            var controller = go.AddComponent<CombatUIController>();
            UnityEditor.Selection.activeGameObject = go;
            
            // Auto-assign cards
            var strike = UnityEditor.AssetDatabase.LoadAssetAtPath<CardData>("Assets/Resources/Data/Cards/Strike.asset");
            var defend = UnityEditor.AssetDatabase.LoadAssetAtPath<CardData>("Assets/Resources/Data/Cards/Defend.asset");
            
            if (strike != null && defend != null)
            {
                var so = new UnityEditor.SerializedObject(controller);
                so.FindProperty("strikeCard").objectReferenceValue = strike;
                so.FindProperty("defendCard").objectReferenceValue = defend;
                so.ApplyModifiedProperties();
                Debug.Log($"[Combat] Created with cards: {strike.name}, {defend.name}");
            }
        }
#endif
    }
}