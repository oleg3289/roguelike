using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using System.IO;

/// <summary>
/// Automated screenshot capture for visual comparison.
/// Run via: Unity.exe -executeMethod CaptureScreenshotAutomated.CaptureAndQuit -projectPath .
/// </summary>
public class CaptureScreenshotAutomated
{
    private const string ReferencePath = "Assets/Tests/ReferenceScreenshots";
    private const int Width = 1920;
    private const int Height = 1080;
    
    public static void CaptureAndQuit()
    {
        Debug.Log("[CaptureScreenshot] Starting automated capture...");
        
        // Ensure directory exists
        Directory.CreateDirectory(ReferencePath);
        
        // Set resolution
        Screen.SetResolution(Width, Height, false);
        
        // We need to create the scene with UI
        var go = new GameObject("CaptureSetup");
        go.AddComponent<CaptureCoroutine>();
        
        // Don't quit immediately - let the coroutine run
    }
}

public class CaptureCoroutine : MonoBehaviour
{
    private const string ReferencePath = "Assets/Tests/ReferenceScreenshots";
    
    private void Start()
    {
        StartCoroutine(CaptureRoutine());
    }
    
    private System.Collections.IEnumerator CaptureRoutine()
    {
        Debug.Log("[CaptureCoroutine] Setting up UI...");
        
        // Set resolution
        Screen.SetResolution(1920, 1080, false);
        
        // Create camera
        var camGO = new GameObject("CaptureCamera");
        var cam = camGO.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.15f, 0.27f, 0.33f);
        cam.orthographic = true;
        cam.orthographicSize = 5;
        
        // Create EventSystem
        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        
        // Create UI
        var uiGO = new GameObject("UI");
        var controller = uiGO.AddComponent<Roguelike.UI.CombatUIController>();
        
        // Get CreateUI method via reflection
        var createUIMethod = typeof(Roguelike.UI.CombatUIController).GetMethod("CreateUI", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        createUIMethod.Invoke(controller, null);
        
        // Wait for UI to render
        yield return null;
        yield return null;
        yield return null;
        
        Debug.Log("[CaptureCoroutine] Capturing screenshot...");
        
        // Capture
        var texture = ScreenCapture.CaptureScreenshotAsTexture(1);
        
        // Save
        var path = Path.Combine(ReferencePath, "CombatUI_Captured.png");
        File.WriteAllBytes(path, texture.EncodeToPNG());
        
        Debug.Log($"[CaptureCoroutine] Saved to: {path}");
        
        // Load reference and compare
        var refPath = Path.Combine(ReferencePath, "CombatUI_Initial_Reference.png");
        if (File.Exists(refPath))
        {
            var refTex = new Texture2D(2, 2);
            refTex.LoadImage(File.ReadAllBytes(refPath));
            
            // Resize reference to match captured
            var resizedRef = ResizeTexture(refTex, texture.width, texture.height);
            var similarity = CalculateSimilarity(texture, resizedRef);
            
            Debug.Log($"[CaptureCoroutine] Similarity: {similarity:P1}");
            
            if (similarity < 0.90f)
            {
                Debug.LogError($"[CaptureCoroutine] SIMILARITY BELOW 90%: {similarity:P1}");
            }
            else
            {
                Debug.Log($"[CaptureCoroutine] SIMILARITY OK: {similarity:P1}");
            }
        }
        
        // Quit
        #if UNITY_EDITOR
        EditorApplication.Exit(similarity >= 0.90f ? 0 : 1);
        #endif
        Application.Quit(similarity >= 0.90f ? 0 : 1);
    }
    
    private Texture2D ResizeTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        var result = new Texture2D(targetWidth, targetHeight, TextureFormat.RGB24, false);
        
        for (int y = 0; y < targetHeight; y++)
        {
            for (int x = 0; x < targetWidth; x++)
            {
                float u = (float)x / targetWidth;
                float v = (float)y / targetHeight;
                
                int srcX = Mathf.RoundToInt(u * source.width);
                int srcY = Mathf.RoundToInt(v * source.height);
                
                result.SetPixel(x, y, source.GetPixel(srcX, srcY));
            }
        }
        
        result.Apply();
        return result;
    }
    
    private float CalculateSimilarity(Texture2D a, Texture2D b)
    {
        var pixelsA = a.GetPixels();
        var pixelsB = b.GetPixels();
        
        if (pixelsA.Length != pixelsB.Length)
            return 0f;
        
        float totalDiff = 0f;
        
        for (int i = 0; i < pixelsA.Length; i++)
        {
            totalDiff += Mathf.Abs(pixelsA[i].r - pixelsB[i].r);
            totalDiff += Mathf.Abs(pixelsA[i].g - pixelsB[i].g);
            totalDiff += Mathf.Abs(pixelsA[i].b - pixelsB[i].b);
        }
        
        float maxDiff = pixelsA.Length * 3f;
        return 1f - (totalDiff / maxDiff);
    }
    
    private static float similarity = 0f;
}