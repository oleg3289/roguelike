using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
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
        
        private GameObject createdCanvas;
        private Camera testCamera;
        private CombatUIController controller;
        
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
            // Create a camera for rendering (batch mode doesn't have one by default)
            var cameraGO = new GameObject("TestCamera");
            testCamera = cameraGO.AddComponent<Camera>();
            testCamera.clearFlags = CameraClearFlags.SolidColor;
            testCamera.backgroundColor = new Color(0.15f, 0.27f, 0.33f); // Match UI background
            testCamera.orthographic = true;
            testCamera.orthographicSize = 5;
            testCamera.depth = -1;
            
            // EventSystem
            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            
            // Create controller which will create its own Canvas
            var controllerGO = new GameObject("CombatUIController");
            controller = controllerGO.AddComponent<CombatUIController>();
            
            // Invoke CreateUI via reflection
            var createUIMethod = typeof(CombatUIController).GetMethod("CreateUI", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            createUIMethod.Invoke(controller, null);
            
            // Find the created Canvas
            createdCanvas = GameObject.Find("Canvas");
            
            // Wait for UI to render (WaitForEndOfFrame doesn't work in batch mode)
            yield return null;
            yield return null; // Extra frame for safety
        }
        
        [TearDown]
        public void TearDown()
        {
            if (createdCanvas != null) Object.DestroyImmediate(createdCanvas);
            if (testCamera != null) Object.DestroyImmediate(testCamera.gameObject);
            var controllerGO = GameObject.Find("CombatUIController");
            if (controllerGO != null) Object.DestroyImmediate(controllerGO);
            var eventSystem = Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem != null) Object.DestroyImmediate(eventSystem.gameObject);
        }
        
        [UnityTest]
        public IEnumerator CombatUI_InitialState_ShouldMatchReference()
        {
            // Skip in batch mode - screenshot capture requires Editor
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null ||
                Screen.width <= 0)
            {
                Assert.Ignore("Design comparison requires Unity Editor (not batch mode). Run in Editor Test Runner.");
            }
            
            // Extra frame for UI to settle
            yield return null;
            
            // Capture current UI
            var captured = CaptureScreenshot();
            
            // Load reference
            string referenceFile = Path.Combine(ReferencePath, "CombatUI_Initial_Reference.png");
            
            if (!File.Exists(referenceFile))
            {
                Assert.Fail($"Reference image not found: {referenceFile}");
            }
            
            var reference = LoadPngAsTexture(referenceFile);
            
            // Downscale reference to match captured size (batch mode uses small resolution)
            var resizedReference = ResizeTexture(reference, captured.width, captured.height);
            
            // Calculate similarity
            float similarity = CalculateSimilarity(captured, resizedReference);
            
            // Always save captured for comparison
            Directory.CreateDirectory(ReferencePath);
            SaveTextureAsPng(captured, Path.Combine(ReferencePath, "CombatUI_Captured.png"));
            SaveTextureAsPng(resizedReference, Path.Combine(ReferencePath, "CombatUI_Reference_Resized.png"));
            
            if (similarity < SimilarityThreshold)
            {
                // Save diff
                var diff = CreateDiffTexture(captured, resizedReference);
                SaveTextureAsPng(diff, Path.Combine(ReferencePath, "CombatUI_Diff.png"));
                
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
            // Must be called at end of frame - requires Editor mode
            // In batch mode, this will fail
            var texture = ScreenCapture.CaptureScreenshotAsTexture(1);
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