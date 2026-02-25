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
    /// Visual regression tests that compare UI screenshots against reference images.
    /// Run once to generate references, then tests will compare against them.
    /// 
    /// NOTE: These tests require a graphics context and will skip in batch mode (-nographics).
    /// Run from Unity Test Runner in Editor for screenshot capture/comparison.
    /// </summary>
    [TestFixture]
    public class VisualRegressionTests
    {
        private const string ReferencePath = "Assets/Tests/ReferenceScreenshots";
        private const float SimilarityThreshold = 0.95f; // 95% similarity required
        
        private GameObject testCanvas;
        private CombatUIController controller;
        private GameObject createdCanvas;
        
        [SetUp]
        public void CheckGraphicsContext()
        {
            // Skip in batch mode - screenshots require graphics
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            {
                Assert.Ignore("Visual regression tests require graphics context. Run in Unity Editor.");
            }
        }
        
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            // Create canvas with proper resolution for screenshots
            testCanvas = new GameObject("TestCanvas");
            var canvas = testCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            var scaler = testCanvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            testCanvas.AddComponent<GraphicRaycaster>();
            
            // Create EventSystem
            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            
            // Create controller
            controller = testCanvas.AddComponent<CombatUIController>();
            
            // Call CreateUI directly
            var createUIMethod = typeof(CombatUIController).GetMethod("CreateUI", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            createUIMethod.Invoke(controller, null);
            
            createdCanvas = GameObject.Find("Canvas");
            
            // Wait a frame for layout
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
        public IEnumerator CombatUI_ShouldMatchReferenceScreenshot()
        {
            // Wait for UI to render
            yield return new WaitForEndOfFrame();
            
            // Capture screenshot
            var captured = CaptureScreenshot();
            
            // Ensure reference directory exists
            if (!Directory.Exists(ReferencePath))
            {
                Directory.CreateDirectory(ReferencePath);
            }
            
            string referenceFile = Path.Combine(ReferencePath, "CombatUI_Reference.png");
            
            // If reference doesn't exist, create it (first run)
            if (!File.Exists(referenceFile))
            {
                SaveTextureAsPng(captured, referenceFile);
                Assert.Inconclusive($"Reference image created at {referenceFile}. Run tests again to compare.");
            }
            
            // Load reference
            var reference = LoadPngAsTexture(referenceFile);
            
            // Compare
            float similarity = CalculateSimilarity(captured, reference);
            
            // Save diff if failed
            if (similarity < SimilarityThreshold)
            {
                string diffPath = Path.Combine(ReferencePath, "CombatUI_Diff.png");
                var diff = CreateDiffTexture(captured, reference);
                SaveTextureAsPng(diff, diffPath);
                
                string capturedPath = Path.Combine(ReferencePath, "CombatUI_Captured.png");
                SaveTextureAsPng(captured, capturedPath);
            }
            
            Assert.GreaterOrEqual(similarity, SimilarityThreshold, 
                $"UI screenshot similarity {similarity:P1} is below threshold {SimilarityThreshold:P0}. " +
                $"Check CombatUI_Diff.png for differences.");
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
        
        private void SaveTextureAsPng(Texture2D texture, string path)
        {
            var pngData = texture.EncodeToPNG();
            File.WriteAllBytes(path, pngData);
        }
        
        private Texture2D LoadPngAsTexture(string path)
        {
            var data = File.ReadAllBytes(path);
            var texture = new Texture2D(2, 2); // Size doesn't matter, LoadImage will resize
            texture.LoadImage(data);
            return texture;
        }
        
        private float CalculateSimilarity(Texture2D a, Texture2D b)
        {
            // Resize if needed
            if (a.width != b.width || a.height != b.height)
            {
                // Different sizes = 0% similarity
                return 0f;
            }
            
            var pixelsA = a.GetPixels();
            var pixelsB = b.GetPixels();
            
            int matching = 0;
            int total = pixelsA.Length;
            
            for (int i = 0; i < total; i++)
            {
                if (ColorsMatch(pixelsA[i], pixelsB[i]))
                {
                    matching++;
                }
            }
            
            return (float)matching / total;
        }
        
        private bool ColorsMatch(Color a, Color b, float tolerance = 0.01f)
        {
            return Mathf.Abs(a.r - b.r) <= tolerance &&
                   Mathf.Abs(a.g - b.g) <= tolerance &&
                   Mathf.Abs(a.b - b.b) <= tolerance;
        }
        
        private Texture2D CreateDiffTexture(Texture2D captured, Texture2D reference)
        {
            int width = Mathf.Max(captured.width, reference.width);
            int height = Mathf.Max(captured.height, reference.height);
            
            var diff = new Texture2D(width, height, TextureFormat.RGB24, false);
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool hasCaptured = x < captured.width && y < captured.height;
                    bool hasReference = x < reference.width && y < reference.height;
                    
                    if (hasCaptured && hasReference)
                    {
                        var c = captured.GetPixel(x, y);
                        var r = reference.GetPixel(x, y);
                        
                        if (ColorsMatch(c, r))
                        {
                            // Matching = white with slight tint
                            diff.SetPixel(x, y, new Color(0.9f, 0.9f, 0.9f));
                        }
                        else
                        {
                            // Different = red
                            diff.SetPixel(x, y, Color.red);
                        }
                    }
                    else
                    {
                        // Missing in one = yellow
                        diff.SetPixel(x, y, Color.yellow);
                    }
                }
            }
            
            diff.Apply();
            return diff;
        }
    }
}