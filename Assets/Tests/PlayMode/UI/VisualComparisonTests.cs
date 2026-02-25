using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.IO;

namespace Roguelike.Tests.PlayMode.UI
{
    /// <summary>
    /// Visual comparison test that MUST run with graphics (not -nographics).
    /// Command: Unity.exe -runTests -projectPath . -testPlatform PlayMode -testResults results.xml
    /// (without -batchmode or with -batchmode but WITHOUT -nographics)
    /// </summary>
    [TestFixture]
    public class VisualComparisonTests
    {
        private const string ReferencePath = "Assets/Tests/ReferenceScreenshots";
        private const float SimilarityThreshold = 0.70f; // 70% - honest threshold
        
        [UnityTest]
        public IEnumerator CombatUI_VisualComparison()
        {
            // Skip if no graphics context
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            {
                Assert.Ignore("Requires graphics context. Run without -nographics flag.");
            }
            
            Debug.Log("[VisualTest] Setting up UI...");
            
            // Create camera
            var camGO = new GameObject("TestCamera");
            var cam = camGO.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.15f, 0.27f, 0.33f);
            cam.orthographic = true;
            
            // Create EventSystem
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            
            // Create UI
            var uiGO = new GameObject("UI");
            var controller = uiGO.AddComponent<Roguelike.UI.CombatUIController>();
            
            // Wait for Awake
            yield return null;
            
            // Call CreateUI and StartCombat via reflection
            var createUIMethod = typeof(Roguelike.UI.CombatUIController).GetMethod("CreateUI", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var startCombatMethod = typeof(Roguelike.UI.CombatUIController).GetMethod("StartCombat", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            createUIMethod?.Invoke(controller, null);
            startCombatMethod?.Invoke(controller, null);
            
            // Wait for rendering
            yield return null;
            yield return null;
            yield return null;
            
            // Force render
            cam.Render();
            
            Debug.Log("[VisualTest] Capturing screenshot...");
            
            // Capture - this works in Play Mode tests without batchmode
            var captured = ScreenCapture.CaptureScreenshotAsTexture(1);
            
            // Save captured
            Directory.CreateDirectory(ReferencePath);
            var capturedPath = Path.Combine(ReferencePath, "CombatUI_Captured.png");
            File.WriteAllBytes(capturedPath, captured.EncodeToPNG());
            Debug.Log($"[VisualTest] Saved: {capturedPath}");
            
            // Load reference
            var refPath = Path.Combine(ReferencePath, "CombatUI_Initial_Reference.png");
            if (!File.Exists(refPath))
            {
                Assert.Fail($"Reference not found: {refPath}");
            }
            
            var reference = new Texture2D(2, 2);
            reference.LoadImage(File.ReadAllBytes(refPath));
            
            // Resize reference to match captured
            var resizedRef = ResizeTexture(reference, captured.width, captured.height);
            
            // Calculate similarity
            float similarity = CalculateSimilarity(captured, resizedRef);
            float sigDiffPercent = CalculateSignificantDiffPercent(captured, resizedRef);
            
            Debug.Log($"[VisualTest] SIMILARITY: {similarity:P1}");
            Debug.Log($"[VisualTest] Significant differences: {sigDiffPercent:P1}");
            
            // Save diff image for debugging
            var diffPath = Path.Combine(ReferencePath, "CombatUI_Diff.png");
            CreateDiffImage(captured, resizedRef, diffPath);
            
            // Cleanup
            Object.DestroyImmediate(camGO);
            Object.DestroyImmediate(esGO);
            Object.DestroyImmediate(uiGO);
            
            Assert.GreaterOrEqual(similarity, SimilarityThreshold, 
                $"UI similarity {similarity:P1} below {SimilarityThreshold:P0}. Check CombatUI_Captured.png and CombatUI_Diff.png");
        }
        
        private Texture2D ResizeTexture(Texture2D source, int w, int h)
        {
            var result = new Texture2D(w, h, TextureFormat.RGB24, false);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int srcX = Mathf.RoundToInt((float)x / w * source.width);
                    int srcY = Mathf.RoundToInt((float)y / h * source.height);
                    result.SetPixel(x, y, source.GetPixel(srcX, srcY));
                }
            }
            result.Apply();
            return result;
        }
        
        private float CalculateSimilarity(Texture2D a, Texture2D b)
        {
            var pa = a.GetPixels();
            var pb = b.GetPixels();
            if (pa.Length != pb.Length) return 0f;
            
            float totalDiff = 0f;
            for (int i = 0; i < pa.Length; i++)
            {
                totalDiff += Mathf.Abs(pa[i].r - pb[i].r);
                totalDiff += Mathf.Abs(pa[i].g - pb[i].g);
                totalDiff += Mathf.Abs(pa[i].b - pb[i].b);
            }
            return 1f - (totalDiff / (pa.Length * 3f));
        }
        
        private float CalculateSignificantDiffPercent(Texture2D a, Texture2D b)
        {
            var pa = a.GetPixels();
            var pb = b.GetPixels();
            if (pa.Length != pb.Length) return 1f;
            
            int sigDiff = 0;
            for (int i = 0; i < pa.Length; i++)
            {
                float diff = Mathf.Abs(pa[i].r - pb[i].r) +
                            Mathf.Abs(pa[i].g - pb[i].g) +
                            Mathf.Abs(pa[i].b - pb[i].b);
                if (diff > 0.4f) sigDiff++;
            }
            return (float)sigDiff / pa.Length;
        }
        
        private void CreateDiffImage(Texture2D a, Texture2D b, string path)
        {
            var w = Mathf.Min(a.width, b.width);
            var h = Mathf.Min(a.height, b.height);
            var diff = new Texture2D(w, h, TextureFormat.RGB24, false);
            
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    var ca = a.GetPixel(x, y);
                    var cb = b.GetPixel(x, y);
                    float d = Mathf.Abs(ca.r - cb.r) + Mathf.Abs(ca.g - cb.g) + Mathf.Abs(ca.b - cb.b);
                    
                    // Highlight differences in red
                    if (d > 0.3f)
                        diff.SetPixel(x, y, new Color(1, 0, 0, 1));
                    else
                        diff.SetPixel(x, y, new Color(ca.r, ca.g, ca.b, 1));
                }
            }
            diff.Apply();
            File.WriteAllBytes(path, diff.EncodeToPNG());
        }
    }
}