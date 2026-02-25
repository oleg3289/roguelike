using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

/// <summary>
/// Automated screenshot capture for visual comparison.
/// Uses Play Mode to properly render and capture UI.
/// </summary>
[InitializeOnLoad]
public class CaptureScreenshotAutomated
{
    private const string ReferencePath = "Assets/Tests/ReferenceScreenshots";
    private static int step = 0;
    private static float startTime;
    
    static CaptureScreenshotAutomated()
    {
        // Check if we should auto-run
        if (System.Environment.GetCommandLineArgs().Contains("-captureUI"))
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }
    }
    
    public static void CaptureAndQuit()
    {
        Debug.Log("[Capture] Starting capture sequence...");
        Directory.CreateDirectory(ReferencePath);
        
        // We need to enter play mode to capture screenshots
        EditorApplication.EnterPlaymode();
    }
    
    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            // Wait a frame then capture
            EditorApplication.delayCall += () =>
            {
                CaptureAfterPlaymode();
            };
        }
    }
    
    private static void CaptureAfterPlaymode()
    {
        Debug.Log("[Capture] In play mode, capturing...");
        
        try
        {
            // Find the UI in scene or create it
            var controller = Object.FindFirstObjectByType<Roguelike.UI.CombatUIController>();
            if (controller == null)
            {
                Debug.Log("[Capture] Creating new UI...");
                var uiGO = new GameObject("UI");
                controller = uiGO.AddComponent<Roguelike.UI.CombatUIController>();
                
                // Wait for Awake
                System.Threading.Thread.Sleep(500);
                
                var createUIMethod = typeof(Roguelike.UI.CombatUIController).GetMethod("CreateUI", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                createUIMethod?.Invoke(controller, null);
                
                var startCombatMethod = typeof(Roguelike.UI.CombatUIController).GetMethod("StartCombat", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                startCombatMethod?.Invoke(controller, null);
            }
            
            // Wait for render
            System.Threading.Thread.Sleep(500);
            
            // Capture
            var texture = ScreenCapture.CaptureScreenshotAsTexture(1);
            var path = Path.Combine(ReferencePath, "CombatUI_Captured.png");
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Debug.Log($"[Capture] Saved: {path}");
            
            // Compare
            var refPath = Path.Combine(ReferencePath, "CombatUI_Initial_Reference.png");
            float similarity = 0f;
            
            if (File.Exists(refPath))
            {
                var refTex = new Texture2D(2, 2);
                refTex.LoadImage(File.ReadAllBytes(refPath));
                var resizedRef = ResizeTexture(refTex, texture.width, texture.height);
                similarity = CalculateSimilarity(texture, resizedRef);
                Debug.Log($"[Capture] SIMILARITY: {similarity:P1}");
            }
            
            EditorApplication.ExitPlaymode();
            EditorApplication.delayCall += () =>
            {
                EditorApplication.Exit(similarity >= 0.70f ? 0 : 1);
            };
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Capture] Error: {e}");
            EditorApplication.ExitPlaymode();
            EditorApplication.Exit(1);
        }
    }
    
    private static Texture2D ResizeTexture(Texture2D source, int targetWidth, int targetHeight)
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
    
    private static float CalculateSimilarity(Texture2D a, Texture2D b)
    {
        var pixelsA = a.GetPixels();
        var pixelsB = b.GetPixels();
        
        if (pixelsA.Length != pixelsB.Length) return 0f;
        
        float totalDiff = 0f;
        int sigDiff = 0;
        
        for (int i = 0; i < pixelsA.Length; i++)
        {
            float diff = Mathf.Abs(pixelsA[i].r - pixelsB[i].r) +
                        Mathf.Abs(pixelsA[i].g - pixelsB[i].g) +
                        Mathf.Abs(pixelsA[i].b - pixelsB[i].b);
            totalDiff += diff;
            if (diff > 0.4f) sigDiff++;
        }
        
        float similarity = 1f - (totalDiff / (pixelsA.Length * 3f));
        Debug.Log($"[Capture] Pixel similarity: {similarity:P1}, significant diffs: {(float)sigDiff/pixelsA.Length:P1}");
        return similarity;
    }
}