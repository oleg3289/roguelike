using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.IO;
using System.Reflection;
using Roguelike.UI;

namespace Roguelike.Tests.PlayMode.UI
{
    /// <summary>
    /// Visual comparison test against reference design.
    /// Run in Unity Editor (requires graphics context).
    /// 
    /// Workflow:
    /// 1. Reference image provided by user (Figma export)
    /// 2. Test captures current UI
    /// 3. Test fails if UI doesn't match reference
    /// 4. Fix UI until test passes
    /// </summary>
    [TestFixture]
    public class DesignComparisonTests
    {
        private const string ReferencePath = "Assets/Tests/ReferenceScreenshots";
        private const float SimilarityThreshold = 0.90f; // 90% similarity required
        
        private GameObject testCanvas;
        private CombatUIController controller;
        private GameObject createdCanvas;
        
        [SetUp]
        public void CheckGraphicsContext()
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            {
                Assert.Ignore("Design comparison requires graphics. Run in Unity Editor.");
            }
        }
        
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            testCanvas = new GameObject("TestCanvas");
            var canvas = testCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            var scaler = testCanvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            testCanvas.AddComponent<GraphicRaycaster>();
            
            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            
            controller = testCanvas.AddComponent<CombatUIController>();
            
            var createUIMethod = typeof(CombatUIController).GetMethod("CreateUI", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            createUIMethod.Invoke(controller, null);
            
            createdCanvas = GameObject.Find("Canvas");
            
            yield return null;
        }
        
        [TearDown]
        public void TearDown()
        {
            if (createdCanvas != null) Object.DestroyImmediate(createdCanvas);
            if (testCanvas != null) Object.DestroyImmediate(testCanvas);
            var eventSystem = Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem != null) Object.DestroyImmediate(eventSystem.gameObject);
        }
        
        [UnityTest]
        public IEnumerator CombatUI_InitialState_ShouldMatchReference()
        {
            yield return new WaitForEndOfFrame();
            
            // Capture current UI
            var captured = CaptureScreenshot();
            
            // Load reference
            string referenceFile = Path.Combine(ReferencePath, "CombatUI_Initial_Reference.png");
            
            if (!File.Exists(referenceFile))
            {
                Assert.Fail($"Reference image not found: {referenceFile}");
            }
            
            var reference = LoadPngAsTexture(referenceFile);
            
            // Resize captured to match reference dimensions for fair comparison
            var resizedCaptured = ResizeTexture(captured, reference.width, reference.height);
            
            // Calculate similarity
            float similarity = CalculateSimilarity(resizedCaptured, reference);
            
            // Always save captured for comparison
            Directory.CreateDirectory(ReferencePath);
            SaveTextureAsPng(captured, Path.Combine(ReferencePath, "CombatUI_Captured.png"));
            
            if (similarity < SimilarityThreshold)
            {
                // Save diff
                var diff = CreateDiffTexture(resizedCaptured, reference);
                SaveTextureAsPng(diff, Path.Combine(ReferencePath, "CombatUI_Diff.png"));
                SaveTextureAsPng(resizedCaptured, Path.Combine(ReferencePath, "CombatUI_Resized.png"));
                
                Debug.LogWarning($"[DesignComparison] Similarity: {similarity:P1}");
                Debug.LogWarning($"[DesignComparison] Captured saved to: CombatUI_Captured.png");
                Debug.LogWarning($"[DesignComparison] Diff saved to: CombatUI_Diff.png");
            }
            
            Assert.GreaterOrEqual(similarity, SimilarityThreshold, 
                $"UI similarity {similarity:P1} below {SimilarityThreshold:P0}. " +
                $"Check CombatUI_Captured.png and CombatUI_Diff.png");
        }
        
        private Texture2D CaptureScreenshot()
        {
            int width = Screen.width;
            int height = Screen.height;
            
            var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture.Apply();
            
            return texture;
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
        
        private void SaveTextureAsPng(Texture2D texture, string path)
        {
            var pngData = texture.EncodeToPNG();
            File.WriteAllBytes(path, pngData);
        }
        
        private Texture2D LoadPngAsTexture(string path)
        {
            var data = File.ReadAllBytes(path);
            var texture = new Texture2D(2, 2);
            texture.LoadImage(data);
            return texture;
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
            
            // Max possible difference per pixel is 3 (r+g+b)
            float maxDiff = pixelsA.Length * 3f;
            float similarity = 1f - (totalDiff / maxDiff);
            
            return similarity;
        }
        
        private Texture2D CreateDiffTexture(Texture2D captured, Texture2D reference)
        {
            int width = captured.width;
            int height = captured.height;
            
            var diff = new Texture2D(width, height, TextureFormat.RGB24, false);
            
            var pixelsCap = captured.GetPixels();
            var pixelsRef = reference.GetPixels();
            
            for (int i = 0; i < pixelsCap.Length; i++)
            {
                float r = Mathf.Abs(pixelsCap[i].r - pixelsRef[i].r);
                float g = Mathf.Abs(pixelsCap[i].g - pixelsRef[i].g);
                float b = Mathf.Abs(pixelsCap[i].b - pixelsRef[i].b);
                
                // Highlight differences in red
                float intensity = (r + g + b) / 3f;
                diff.SetPixel(i % width, i / width, new Color(intensity * 2, 1 - intensity, 1 - intensity));
            }
            
            diff.Apply();
            return diff;
        }
    }
}