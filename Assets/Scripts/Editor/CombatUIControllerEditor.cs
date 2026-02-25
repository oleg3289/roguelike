#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Roguelike.UI;
using Roguelike.Cards;

[CustomEditor(typeof(CombatUIController))]
public class CombatUIControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        var controller = (CombatUIController)target;
        
        EditorGUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "Card data is auto-loaded from Resources/Data/Cards/ at runtime.\n" +
            "Click below to assign them in edit mode for preview.",
            MessageType.Info);
        
        if (GUILayout.Button("Auto-Assign Cards", GUILayout.Height(30)))
        {
            AssignCards(controller);
        }
        
        // Show current assignment status
        EditorGUILayout.Space(5);
        var strikeProp = serializedObject.FindProperty("strikeCard");
        var defendProp = serializedObject.FindProperty("defendCard");
        
        EditorGUILayout.LabelField("Current Assignment:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"  Strike: {(strikeProp.objectReferenceValue != null ? strikeProp.objectReferenceValue.name : "None")}");
        EditorGUILayout.LabelField($"  Defend: {(defendProp.objectReferenceValue != null ? defendProp.objectReferenceValue.name : "None")}");
    }
    
    private void AssignCards(CombatUIController controller)
    {
        var strike = AssetDatabase.LoadAssetAtPath<CardData>("Assets/Resources/Data/Cards/Strike.asset");
        var defend = AssetDatabase.LoadAssetAtPath<CardData>("Assets/Resources/Data/Cards/Defend.asset");
        
        if (strike != null && defend != null)
        {
            var strikeProp = serializedObject.FindProperty("strikeCard");
            var defendProp = serializedObject.FindProperty("defendCard");
            
            strikeProp.objectReferenceValue = strike;
            defendProp.objectReferenceValue = defend;
            serializedObject.ApplyModifiedProperties();
            
            Debug.Log($"[CombatUIController] Assigned cards: {strike.name}, {defend.name}");
            EditorUtility.SetDirty(controller);
        }
        else
        {
            Debug.LogError($"[CombatUIController] Could not find cards! " +
                $"Strike: {(strike != null ? "found" : "missing")}, " +
                $"Defend: {(defend != null ? "found" : "missing")}");
        }
    }
}
#endif