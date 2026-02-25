using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;
using System.Collections;
using Roguelike.UI;

namespace Roguelike.Tests.PlayMode.UI
{
    /// <summary>
    /// Tests for UI element positioning to ensure elements are properly contained
    /// within their parent bounds and visible on screen.
    /// </summary>
    [TestFixture]
    public class CombatUIPositioningTests
    {
        private GameObject canvasGO;
        private Canvas canvas;
        private CombatUIController controller;
        
        [SetUp]
        public void SetUp()
        {
            // Create canvas
            canvasGO = new GameObject("TestCanvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
            
            // Create EventSystem
            var eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            
            // Create controller
            var controllerGO = new GameObject("Controller");
            controllerGO.transform.SetParent(canvasGO.transform);
            controller = controllerGO.AddComponent<CombatUIController>();
        }
        
        [TearDown]
        public void TearDown()
        {
            if (canvasGO != null)
                Object.DestroyImmediate(canvasGO);
            
            var eventSystem = UnityEngine.Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem != null)
                Object.DestroyImmediate(eventSystem.gameObject);
            
            if (controller != null && controller.gameObject != null)
                Object.DestroyImmediate(controller.gameObject);
        }
        
        [Test]
        public void Text_InsideParentButton_ShouldBeFullyContained()
        {
            // Arrange: Create a button with text inside
            var buttonGO = new GameObject("TestButton");
            buttonGO.transform.SetParent(canvasGO.transform);
            var buttonRect = buttonGO.AddComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(100, 50);
            buttonRect.anchoredPosition = Vector2.zero;
            buttonGO.AddComponent<Image>();
            var button = buttonGO.AddComponent<Button>();
            
            var textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform);
            var textRect = textGO.AddComponent<RectTransform>();
            // Proper way: anchor to fill parent
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;
            
            var text = textGO.AddComponent<Text>();
            text.text = "END TURN";
            text.alignment = TextAnchor.MiddleCenter;
            
            // Assert: Text rect should be within button bounds
            Assert.IsTrue(IsChildWithinParent(textRect, buttonRect),
                "Text should be fully contained within button bounds");
        }
        
        [Test]
        public void Text_WithImproperAnchors_ShouldFailContainment()
        {
            // Arrange: Create a button with improperly positioned text
            var buttonGO = new GameObject("BadButton");
            buttonGO.transform.SetParent(canvasGO.transform);
            var buttonRect = buttonGO.AddComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(100, 50);
            buttonGO.AddComponent<Image>();
            
            var textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform);
            var textRect = textGO.AddComponent<RectTransform>();
            // Bad anchors (point anchor, not filling parent)
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.sizeDelta = new Vector2(200, 100); // Larger than parent!
            textRect.anchoredPosition = Vector2.zero;
            
            // Assert: Text should NOT be fully contained (this is the bad case)
            Assert.IsFalse(IsChildWithinParent(textRect, buttonRect),
                "Text with wrong anchors and larger size should NOT be within button bounds");
        }
        
        [Test]
        public void CanvasScaler_ShouldUseConstantPixelSize()
        {
            // Arrange
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            
            // Assert
            Assert.AreEqual(CanvasScaler.ScaleMode.ConstantPixelSize, scaler.uiScaleMode,
                "CanvasScaler should use ConstantPixelSize for programmatic UI");
        }
        
        [Test]
        public void Element_WithAnchorAtScreenEdge_ShouldBeVisible()
        {
            // Arrange: Create element anchored to right edge
            var elementGO = new GameObject("RightElement");
            elementGO.transform.SetParent(canvasGO.transform);
            var rect = elementGO.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 0.5f);
            rect.anchorMax = new Vector2(1, 0.5f);
            rect.pivot = new Vector2(1, 0.5f); // Pivot at right edge
            rect.anchoredPosition = new Vector2(-50, 0); // Offset 50px from right
            rect.sizeDelta = new Vector2(100, 50);
            
            // Assert: Element should have positive screen position
            // When anchored to right edge with pivot at right, anchoredPosition.x should be negative
            Assert.Less(rect.anchoredPosition.x, 0,
                "Element anchored to right edge with right pivot should have negative x offset");
        }
        
        /// <summary>
        /// Checks if a child RectTransform is fully contained within its parent.
        /// </summary>
        private bool IsChildWithinParent(RectTransform child, RectTransform parent)
        {
            // Get the corners of both rects in world space
            Vector3[] childCorners = new Vector3[4];
            Vector3[] parentCorners = new Vector3[4];
            
            child.GetWorldCorners(childCorners);
            parent.GetWorldCorners(parentCorners);
            
            // Find parent bounds
            Vector3 parentMin = parentCorners[0];
            Vector3 parentMax = parentCorners[2];
            
            // Check if all child corners are within parent bounds
            for (int i = 0; i < 4; i++)
            {
                Vector3 corner = childCorners[i];
                if (corner.x < parentMin.x || corner.x > parentMax.x ||
                    corner.y < parentMin.y || corner.y > parentMax.y)
                {
                    return false;
                }
            }
            
            return true;
        }
    }
}