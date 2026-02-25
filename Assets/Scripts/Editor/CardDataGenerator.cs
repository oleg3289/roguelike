#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Roguelike.Cards;
using Roguelike.Combat.Effects;

namespace Roguelike.Editor
{
    public static class CardDataGenerator
    {
        [MenuItem("Tools/Generate Demo Cards")]
        public static void GenerateDemoCards()
        {
            // Ensure folder exists
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            if (!AssetDatabase.IsValidFolder("Assets/Resources/Data"))
                AssetDatabase.CreateFolder("Assets/Resources", "Data");
            if (!AssetDatabase.IsValidFolder("Assets/Resources/Data/Cards"))
                AssetDatabase.CreateFolder("Assets/Resources/Data", "Cards");
            
            // Check if cards already exist
            var existingStrike = AssetDatabase.LoadAssetAtPath<CardData>("Assets/Resources/Data/Cards/Strike.asset");
            var existingDefend = AssetDatabase.LoadAssetAtPath<CardData>("Assets/Resources/Data/Cards/Defend.asset");
            
            if (existingStrike != null && existingDefend != null)
            {
                Debug.Log("Demo cards already exist!");
                return;
            }
            
            // Create Strike card
            var strike = ScriptableObject.CreateInstance<CardData>();
            strike.name = "Strike";
            var so = new SerializedObject(strike);
            so.FindProperty("cardName").stringValue = "Strike";
            so.FindProperty("cardType").enumValueIndex = (int)CardType.Attack;
            so.FindProperty("targetType").enumValueIndex = (int)TargetType.SingleEnemy;
            so.FindProperty("cost").intValue = 1;
            so.FindProperty("description").stringValue = "Deal 5 damage";
            so.FindProperty("rarity").enumValueIndex = (int)CardRarity.Common;
            
            // Create damage effect for Strike
            var effectsProp = so.FindProperty("effects");
            effectsProp.arraySize = 1;
            effectsProp.GetArrayElementAtIndex(0).FindPropertyRelative("effectType").enumValueIndex = (int)EffectType.Damage;
            effectsProp.GetArrayElementAtIndex(0).FindPropertyRelative("value").intValue = 5;
            
            so.ApplyModifiedProperties();
            AssetDatabase.CreateAsset(strike, "Assets/Resources/Data/Cards/Strike.asset");
            
            // Create Defend card
            var defend = ScriptableObject.CreateInstance<CardData>();
            defend.name = "Defend";
            so = new SerializedObject(defend);
            so.FindProperty("cardName").stringValue = "Defend";
            so.FindProperty("cardType").enumValueIndex = (int)CardType.Skill;
            so.FindProperty("targetType").enumValueIndex = (int)TargetType.Self;
            so.FindProperty("cost").intValue = 1;
            so.FindProperty("description").stringValue = "Gain 6 Block";
            so.FindProperty("rarity").enumValueIndex = (int)CardRarity.Common;
            
            // Create block effect for Defend
            effectsProp = so.FindProperty("effects");
            effectsProp.arraySize = 1;
            effectsProp.GetArrayElementAtIndex(0).FindPropertyRelative("effectType").enumValueIndex = (int)EffectType.Block;
            effectsProp.GetArrayElementAtIndex(0).FindPropertyRelative("value").intValue = 6;
            
            so.ApplyModifiedProperties();
            AssetDatabase.CreateAsset(defend, "Assets/Resources/Data/Cards/Defend.asset");
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log("Demo cards generated: Strike and Defend!");
        }
    }
}
#endif