using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Roguelike.UI;
using Roguelike.Cards;

namespace Roguelike.Tests.PlayMode.UI
{
    /// <summary>
    /// Integration tests that instantiate actual CombatUIController and verify UI structure.
    /// These tests should FAIL until bugs are fixed.
    /// </summary>
    [TestFixture]
    public class CombatUIIntegrationTests
    {
        private GameObject testCanvas;
        private CombatUIController controller;
        private GameObject createdCanvas; // The canvas CreateUI creates
        
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            // Create canvas
            testCanvas = new GameObject("TestCanvas");
            var canvas = testCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            testCanvas.AddComponent<CanvasScaler>();
            testCanvas.AddComponent<GraphicRaycaster>();
            
            // Create EventSystem
            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            
            // Create controller ON testCanvas (but CreateUI creates its own Canvas)
            controller = testCanvas.AddComponent<CombatUIController>();
            
            // Use reflection to call CreateUI directly (to avoid needing card data)
            var createUIMethod = typeof(CombatUIController).GetMethod("CreateUI", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            createUIMethod.Invoke(controller, null);
            
            // Find the Canvas that CreateUI created (it's named "Canvas")
            createdCanvas = GameObject.Find("Canvas");
            
            // Wait a frame for layout to update
            yield return null;
        }
        
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (createdCanvas != null)
                Object.DestroyImmediate(createdCanvas);
            if (testCanvas != null)
                Object.DestroyImmediate(testCanvas);
            
            var eventSystem = UnityEngine.Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem != null)
                Object.DestroyImmediate(eventSystem.gameObject);
            
            yield return null;
        }
        
        [Test]
        public void EndTurnButton_Text_ShouldFillParentButton()
        {
            // Find EndTurnButton in the Canvas that CreateUI created
            Assert.IsNotNull(createdCanvas, "Canvas should be created by CreateUI");
            var endTurnBtn = createdCanvas.transform.Find("EndTurnBtn");
            Assert.IsNotNull(endTurnBtn, "EndTurnBtn should exist");
            
            // Find text child
            var textChild = endTurnBtn.Find("Txt");
            Assert.IsNotNull(textChild, "Text should be child of EndTurnBtn");
            
            var textRect = textChild.GetComponent<RectTransform>();
            
            // Verify anchors fill parent
            Assert.AreEqual(Vector2.zero, textRect.anchorMin, 
                "Text anchorMin should be (0,0) to fill parent");
            Assert.AreEqual(Vector2.one, textRect.anchorMax, 
                "Text anchorMax should be (1,1) to fill parent");
            Assert.AreEqual(Vector2.zero, textRect.sizeDelta, 
                "Text sizeDelta should be zero when anchored to fill parent");
        }
        
        [Test]
        public void EndTurnButton_TextShouldBeContainedInButton()
        {
            Assert.IsNotNull(createdCanvas, "Canvas should be created by CreateUI");
            var endTurnBtn = createdCanvas.transform.Find("EndTurnBtn");
            Assert.IsNotNull(endTurnBtn, "EndTurnBtn should exist");
            
            var buttonRect = endTurnBtn.GetComponent<RectTransform>();
            var textChild = endTurnBtn.Find("Txt");
            
            if (textChild == null)
            {
                Assert.Fail("Text child not found in EndTurnBtn");
                return;
            }
            
            var textRect = textChild.GetComponent<RectTransform>();
            
            // Get world corners
            Vector3[] buttonCorners = new Vector3[4];
            Vector3[] textCorners = new Vector3[4];
            
            buttonRect.GetWorldCorners(buttonCorners);
            textRect.GetWorldCorners(textCorners);
            
            // Button bounds
            float btnMinX = buttonCorners[0].x;
            float btnMaxX = buttonCorners[2].x;
            float btnMinY = buttonCorners[0].y;
            float btnMaxY = buttonCorners[2].y;
            
            // Check all text corners are within button
            for (int i = 0; i < 4; i++)
            {
                Assert.GreaterOrEqual(textCorners[i].x, btnMinX - 1f, 
                    $"Text corner {i} x should be >= button min x");
                Assert.LessOrEqual(textCorners[i].x, btnMaxX + 1f, 
                    $"Text corner {i} x should be <= button max x");
                Assert.GreaterOrEqual(textCorners[i].y, btnMinY - 1f, 
                    $"Text corner {i} y should be >= button min y");
                Assert.LessOrEqual(textCorners[i].y, btnMaxY + 1f, 
                    $"Text corner {i} y should be <= button max y");
            }
        }
        
        [Test]
        public void EnemyPanel_HpTextShouldFillHpBar()
        {
            Assert.IsNotNull(createdCanvas, "Canvas should be created by CreateUI");
            var enemyPanel = createdCanvas.transform.Find("EnemyPanel");
            Assert.IsNotNull(enemyPanel, "EnemyPanel should exist");
            
            var hpBarBg = enemyPanel.Find("HPBarBg");
            Assert.IsNotNull(hpBarBg, "HPBarBg should exist");
            
            var hpText = hpBarBg.Find("Txt");
            Assert.IsNotNull(hpText, "HP text should exist in HPBarBg");
            
            var textRect = hpText.GetComponent<RectTransform>();
            
            // Verify anchors fill parent
            Assert.AreEqual(Vector2.zero, textRect.anchorMin, 
                "HP text anchorMin should be (0,0) to fill parent");
            Assert.AreEqual(Vector2.one, textRect.anchorMax, 
                "HP text anchorMax should be (1,1) to fill parent");
            Assert.AreEqual(Vector2.zero, textRect.sizeDelta, 
                "HP text sizeDelta should be zero when anchored to fill parent");
        }
        
        [Test]
        public void LogPanel_TextShouldHaveProperOffsets()
        {
            Assert.IsNotNull(createdCanvas, "Canvas should be created by CreateUI");
            var logPanel = createdCanvas.transform.Find("LogPanel");
            Assert.IsNotNull(logPanel, "LogPanel should exist");
            
            var logText = logPanel.Find("Txt");
            Assert.IsNotNull(logText, "Log text should exist in LogPanel");
            
            var textRect = logText.GetComponent<RectTransform>();
            
            // Log text should fill parent but with padding
            Assert.AreEqual(Vector2.zero, textRect.anchorMin, 
                "Log text anchorMin should be (0,0)");
            Assert.AreEqual(Vector2.one, textRect.anchorMax, 
                "Log text anchorMax should be (1,1)");
            
            // Check offsets (padding)
            Assert.AreEqual(10f, textRect.offsetMin.x, 0.1f, "Left padding should be 10");
            Assert.AreEqual(10f, textRect.offsetMin.y, 0.1f, "Bottom padding should be 10");
            Assert.AreEqual(-10f, textRect.offsetMax.x, 0.1f, "Right padding should be 10");
            Assert.AreEqual(-10f, textRect.offsetMax.y, 0.1f, "Top padding should be 10");
        }
        
        [Test]
        public void AllTextElements_ShouldBeContainedInTheirParents()
        {
            Assert.IsNotNull(createdCanvas, "Canvas should be created by CreateUI");
            // Find all text elements
            var allText = createdCanvas.GetComponentsInChildren<Text>();
            List<GameObject> failedElements = new List<GameObject>();
            
            foreach (var text in allText)
            {
                var textRect = text.rectTransform;
                var parentRect = textRect.parent as RectTransform;
                
                if (parentRect == null) continue;
                
                // Skip texts that are direct children of Canvas - they're positioned at screen edges
                // and intentionally may extend to screen boundaries
                if (parentRect.GetComponent<Canvas>() != null) continue;
                
                Vector3[] textCorners = new Vector3[4];
                Vector3[] parentCorners = new Vector3[4];
                
                textRect.GetWorldCorners(textCorners);
                parentRect.GetWorldCorners(parentCorners);
                
                float tolerance = 0.5f; // Small tolerance for floating point
                
                for (int i = 0; i < 4; i++)
                {
                    bool withinX = textCorners[i].x >= parentCorners[0].x - tolerance && 
                                   textCorners[i].x <= parentCorners[2].x + tolerance;
                    bool withinY = textCorners[i].y >= parentCorners[0].y - tolerance && 
                                   textCorners[i].y <= parentCorners[2].y + tolerance;
                    
                    if (!withinX || !withinY)
                    {
                        failedElements.Add(text.gameObject);
                        break;
                    }
                }
            }
            
            if (failedElements.Count > 0)
            {
                List<string> details = new List<string>();
                foreach (var go in failedElements)
                {
                    details.Add($"{go.name} (parent: {go.transform.parent?.name ?? "none"})");
                }
                Assert.Fail($"Text elements not contained in parents: {string.Join(", ", details)}");
            }
        }
    }
}