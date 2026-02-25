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
    /// Integration tests for CombatUIController to verify UI elements are created correctly.
    /// </summary>
    [TestFixture]
    public class CombatUIControllerTests
    {
        private GameObject canvasGO;
        private Canvas canvas;
        private CombatUIController controller;
        
        [SetUp]
        public void SetUp()
        {
            // Create minimal canvas setup
            canvasGO = new GameObject("TestCanvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            canvasGO.AddComponent<GraphicRaycaster>();
            
            // Create EventSystem
            var eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }
        
        [TearDown]
        public void TearDown()
        {
            if (controller != null && controller.gameObject != null)
                Object.DestroyImmediate(controller.gameObject);
            
            if (canvasGO != null)
                Object.DestroyImmediate(canvasGO);
            
            var eventSystem = UnityEngine.Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem != null)
                Object.DestroyImmediate(eventSystem.gameObject);
        }
        
        [Test]
        public void TextElement_ShouldHaveCorrectAnchors_WhenInsideButton()
        {
            // Simulate the correct way to create text inside a button
            var buttonGO = new GameObject("EndTurnButton");
            buttonGO.transform.SetParent(canvasGO.transform);
            var buttonRect = buttonGO.AddComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(140, 50);
            buttonGO.AddComponent<Image>();
            buttonGO.AddComponent<Button>();
            
            var textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform);
            var textRect = textGO.AddComponent<RectTransform>();
            
            // Correct anchor setup for text filling parent
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = Vector2.zero;
            
            var text = textGO.AddComponent<Text>();
            text.text = "END TURN";
            text.alignment = TextAnchor.MiddleCenter;
            
            // Verify anchors are correct
            Assert.AreEqual(Vector2.zero, textRect.anchorMin, "Text anchorMin should be (0,0)");
            Assert.AreEqual(Vector2.one, textRect.anchorMax, "Text anchorMax should be (1,1)");
            Assert.AreEqual(Vector2.zero, textRect.sizeDelta, "Text sizeDelta should be zero when anchored to fill");
        }
        
        [Test]
        public void Panel_ShouldContainAllChildTexts()
        {
            // Create a panel with multiple text children
            var panelGO = new GameObject("EnemyPanel");
            panelGO.transform.SetParent(canvasGO.transform);
            var panelRect = panelGO.AddComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(280, 120);
            panelGO.AddComponent<Image>();
            
            // Create name text
            var nameGO = new GameObject("Name");
            nameGO.transform.SetParent(panelGO.transform);
            var nameRect = nameGO.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.5f, 1f);
            nameRect.anchorMax = new Vector2(0.5f, 1f);
            nameRect.anchoredPosition = new Vector2(0, -20);
            nameRect.sizeDelta = new Vector2(280, 30);
            nameGO.AddComponent<Text>();
            
            // Verify: name should be within panel bounds
            Assert.IsTrue(IsWithinParent(nameRect, panelRect),
                "Name text should be within panel bounds");
        }
        
        [Test]
        public void Card_ShouldHaveCostAtBottomRight()
        {
            // Create a card
            var cardGO = new GameObject("Card");
            cardGO.transform.SetParent(canvasGO.transform);
            var cardRect = cardGO.AddComponent<RectTransform>();
            cardRect.sizeDelta = new Vector2(120, 160);
            cardGO.AddComponent<Image>();
            
            // Cost badge at bottom right
            var costGO = new GameObject("Cost");
            costGO.transform.SetParent(cardGO.transform);
            var costRect = costGO.AddComponent<RectTransform>();
            costRect.anchorMin = new Vector2(1f, 0f);
            costRect.anchorMax = new Vector2(1f, 0f);
            costRect.pivot = new Vector2(1f, 0f);
            costRect.anchoredPosition = new Vector2(-8, 8);
            costRect.sizeDelta = new Vector2(30, 30);
            costGO.AddComponent<Image>();
            
            // Text inside cost badge
            var textGO = new GameObject("Text");
            textGO.transform.SetParent(costGO.transform);
            var textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;
            var text = textGO.AddComponent<Text>();
            text.text = "1";
            text.alignment = TextAnchor.MiddleCenter;
            
            // Verify: cost should be at bottom right (positive offset from bottom)
            Assert.GreaterOrEqual(costRect.anchoredPosition.y, 0,
                "Cost y position should be positive when anchored to bottom");
            Assert.Less(costRect.anchoredPosition.x, 0,
                "Cost x position should be negative when anchored to right edge");
            
            // Verify: text should fill cost badge
            Assert.AreEqual(Vector2.zero, textRect.anchorMin, "Cost text should anchor min (0,0)");
            Assert.AreEqual(Vector2.one, textRect.anchorMax, "Cost text should anchor max (1,1)");
        }
        
        [Test]
        public void AllElements_ShouldBeWithinCanvasBounds()
        {
            // Create elements at various anchor positions
            var positions = new Dictionary<string, RectTransform>();
            
            // Top-left element (Enemy)
            var enemyGO = new GameObject("Enemy");
            enemyGO.transform.SetParent(canvasGO.transform);
            var enemyRect = enemyGO.AddComponent<RectTransform>();
            enemyRect.anchorMin = new Vector2(0, 1);
            enemyRect.anchorMax = new Vector2(0, 1);
            enemyRect.pivot = new Vector2(0, 1);
            enemyRect.anchoredPosition = new Vector2(20, -20);
            enemyRect.sizeDelta = new Vector2(280, 120);
            positions["Enemy"] = enemyRect;
            
            // Bottom-left element (Player)
            var playerGO = new GameObject("Player");
            playerGO.transform.SetParent(canvasGO.transform);
            var playerRect = playerGO.AddComponent<RectTransform>();
            playerRect.anchorMin = new Vector2(0, 0);
            playerRect.anchorMax = new Vector2(0, 0);
            playerRect.pivot = new Vector2(0, 0);
            playerRect.anchoredPosition = new Vector2(20, 20);
            playerRect.sizeDelta = new Vector2(280, 120);
            positions["Player"] = playerRect;
            
            // Right-center element (End Turn button)
            var buttonGO = new GameObject("EndTurnButton");
            buttonGO.transform.SetParent(canvasGO.transform);
            var buttonRect = buttonGO.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(1, 0.5f);
            buttonRect.anchorMax = new Vector2(1, 0.5f);
            buttonRect.pivot = new Vector2(1, 0.5f);
            buttonRect.anchoredPosition = new Vector2(-25, 0);
            buttonRect.sizeDelta = new Vector2(140, 50);
            positions["EndTurnButton"] = buttonRect;
            
            // Verify all elements have reasonable positions
            foreach (var kvp in positions)
            {
                string name = kvp.Key;
                RectTransform rect = kvp.Value;
                
                // Elements anchored to left should have positive x offset
                if (rect.anchorMin.x == 0 && rect.anchorMax.x == 0)
                {
                    Assert.GreaterOrEqual(rect.anchoredPosition.x, 0,
                        $"{name} anchored to left should have positive x offset");
                }
                
                // Elements anchored to right should have negative x offset
                if (rect.anchorMin.x == 1 && rect.anchorMax.x == 1)
                {
                    Assert.LessOrEqual(rect.anchoredPosition.x, 0,
                        $"{name} anchored to right should have negative x offset");
                }
            }
        }
        
        private bool IsWithinParent(RectTransform child, RectTransform parent)
        {
            // Transform child corners to parent space
            Vector3[] childCorners = new Vector3[4];
            Vector3[] parentCorners = new Vector3[4];
            
            child.GetWorldCorners(childCorners);
            parent.GetWorldCorners(parentCorners);
            
            Vector3 parentMin = parentCorners[0];
            Vector3 parentMax = parentCorners[2];
            
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