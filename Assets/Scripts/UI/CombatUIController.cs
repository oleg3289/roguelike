using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using Roguelike.Cards;
using Roguelike.Combat;
using Roguelike.Combat.Entities;
using Roguelike.Combat.AI;
using Roguelike.Combat.Effects;
using Roguelike.Combat.TurnSystem;

namespace Roguelike.UI
{
    public class CombatUIController : MonoBehaviour
    {
        [Header("Card Data")]
        [SerializeField] private CardData strikeCard;
        [SerializeField] private CardData defendCard;
        
        private Font font;
        private Text playerHealthText;
        private Text playerEnergyText;
        private Text playerBlockText;
        private Text enemyHealthText;
        private Text enemyIntentText;
        private Text turnText;
        private Transform handContainer;
        private Button endTurnButton;
        private Text combatLogText;
        
        private Player player;
        private Enemy enemy;
        private CombatContext combatContext;
        private List<CardInstance> hand = new();
        
        private void Awake()
        {
            font = Font.CreateDynamicFontFromOSFont("Arial", 16);
            
            // Load card data from Resources if not assigned in Inspector
            if (strikeCard == null)
                strikeCard = Resources.Load<CardData>("Data/Cards/Strike");
            if (defendCard == null)
                defendCard = Resources.Load<CardData>("Data/Cards/Defend");
        }
        
        private void Start()
        {
            if (strikeCard == null || defendCard == null)
            {
                Debug.LogError("Could not load cards! Make sure Resources/Data/Cards/Strike and Defend assets exist.");
                return;
            }
            
            CreateUI();
            StartCombat();
        }
        
        private void CreateUI()
        {
            // EventSystem
            if (FindFirstObjectByType<EventSystem>() == null)
            {
                var esGO = new GameObject("EventSystem");
                esGO.AddComponent<EventSystem>();
                esGO.AddComponent<InputSystemUIInputModule>();
            }
            
            // Canvas
            var canvasGO = new GameObject("Canvas");
            canvasGO.transform.SetParent(transform);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGO.AddComponent<GraphicRaycaster>();
            
            // Background - dark
            var bgGO = new GameObject("BG");
            bgGO.transform.SetParent(canvasGO.transform);
            var bgRect = bgGO.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.pivot = new Vector2(0.5f, 0.5f);
            bgRect.anchoredPosition = Vector2.zero;
            bgRect.sizeDelta = Vector2.zero;
            var bgImg = bgGO.AddComponent<Image>();
            bgImg.color = new Color(0.12f, 0.12f, 0.18f);
            
            // ========== ENEMY PANEL (TOP-LEFT) ==========
            // Panel container
            var enemyPanel = new GameObject("EnemyPanel");
            enemyPanel.transform.SetParent(canvasGO.transform);
            var epRect = enemyPanel.AddComponent<RectTransform>();
            epRect.anchorMin = new Vector2(0, 1);
            epRect.anchorMax = new Vector2(0, 1);
            epRect.pivot = new Vector2(0, 1);
            epRect.anchoredPosition = new Vector2(20, -20);
            epRect.sizeDelta = new Vector2(280, 120);
            var epImg = enemyPanel.AddComponent<Image>();
            epImg.color = new Color(0.18f, 0.22f, 0.28f);
            
            // Enemy name label
            CreateText(enemyPanel.transform, new Vector2(0.5f, 1), new Vector2(280, 35), "SLIME", 20, Color.white, TextAnchor.MiddleCenter, new Vector2(0, -20));
            
            // HP bar background
            var hpBgGO = new GameObject("HPBarBg");
            hpBgGO.transform.SetParent(enemyPanel.transform);
            var hpBgRect = hpBgGO.AddComponent<RectTransform>();
            hpBgRect.anchorMin = new Vector2(0.5f, 0.5f);
            hpBgRect.anchorMax = new Vector2(0.5f, 0.5f);
            hpBgRect.sizeDelta = new Vector2(250, 30);
            hpBgRect.anchoredPosition = new Vector2(0, -5);
            var hpBgImg = hpBgGO.AddComponent<Image>();
            hpBgImg.color = new Color(0.15f, 0.15f, 0.15f);
            
            // HP bar fill
            var hpFillGO = new GameObject("HPBarFill");
            hpFillGO.transform.SetParent(hpBgGO.transform);
            var hpFillRect = hpFillGO.AddComponent<RectTransform>();
            hpFillRect.anchorMin = new Vector2(0, 0.5f);
            hpFillRect.anchorMax = new Vector2(0, 0.5f);
            hpFillRect.pivot = new Vector2(0, 0.5f);
            hpFillRect.sizeDelta = new Vector2(250, 28);
            hpFillRect.anchoredPosition = Vector2.zero;
            var hpFillImg = hpFillGO.AddComponent<Image>();
            hpFillImg.color = new Color(0.75f, 0.2f, 0.2f);
            
            // HP text - centered in bar
            var hpTxtGO = new GameObject("Txt");
            hpTxtGO.transform.SetParent(hpBgGO.transform);
            var hpTxtRect = hpTxtGO.AddComponent<RectTransform>();
            hpTxtRect.anchorMin = Vector2.zero;
            hpTxtRect.anchorMax = Vector2.one;
            hpTxtRect.sizeDelta = Vector2.zero;
            hpTxtRect.anchoredPosition = Vector2.zero;
            enemyHealthText = hpTxtGO.AddComponent<Text>();
            enemyHealthText.text = "30/30";
            enemyHealthText.font = font;
            enemyHealthText.fontSize = 14;
            enemyHealthText.color = Color.white;
            enemyHealthText.alignment = TextAnchor.MiddleCenter;
            
            // Intent
            enemyIntentText = CreateText(enemyPanel.transform, new Vector2(0.5f, 0), new Vector2(280, 30), "Intent: Attack 6", 14, Color.yellow, TextAnchor.MiddleCenter, new Vector2(0, 15));
            
            // ========== TURN INDICATOR (TOP-RIGHT) ==========
            turnText = CreateText(canvasGO.transform, new Vector2(1, 1), new Vector2(150, 30), "Turn 1", 16, Color.white, TextAnchor.MiddleRight, new Vector2(-100, -25));
            
            // ========== PLAYER PANEL (BOTTOM-LEFT) ==========
            var playerPanel = new GameObject("PlayerPanel");
            playerPanel.transform.SetParent(canvasGO.transform);
            var ppRect = playerPanel.AddComponent<RectTransform>();
            ppRect.anchorMin = new Vector2(0, 0);
            ppRect.anchorMax = new Vector2(0, 0);
            ppRect.pivot = new Vector2(0, 0);
            ppRect.anchoredPosition = new Vector2(20, 20);
            ppRect.sizeDelta = new Vector2(280, 120);
            var ppImg = playerPanel.AddComponent<Image>();
            ppImg.color = new Color(0.15f, 0.25f, 0.18f);
            
            // Player name
            CreateText(playerPanel.transform, new Vector2(0.5f, 1), new Vector2(280, 30), "HERO", 20, Color.white, TextAnchor.MiddleCenter, new Vector2(0, -15));
            
            // HP
            playerHealthText = CreateText(playerPanel.transform, new Vector2(0.5f, 0.6f), new Vector2(280, 25), "HP: 80/80", 14, Color.white, TextAnchor.MiddleCenter, Vector2.zero);
            
            // Energy
            playerEnergyText = CreateText(playerPanel.transform, new Vector2(0.5f, 0.35f), new Vector2(280, 25), "Energy: 3/3", 14, new Color(0.3f, 0.8f, 0.9f), TextAnchor.MiddleCenter, Vector2.zero);
            
            // Block
            playerBlockText = CreateText(playerPanel.transform, new Vector2(0.5f, 0.1f), new Vector2(280, 25), "Block: 0", 14, Color.gray, TextAnchor.MiddleCenter, Vector2.zero);
            
            // ========== COMBAT LOG (LEFT-CENTER) ==========
            var logPanel = new GameObject("LogPanel");
            logPanel.transform.SetParent(canvasGO.transform);
            var lpRect = logPanel.AddComponent<RectTransform>();
            lpRect.anchorMin = new Vector2(0, 0.5f);
            lpRect.anchorMax = new Vector2(0, 0.5f);
            lpRect.pivot = new Vector2(0, 0.5f);
            lpRect.anchoredPosition = new Vector2(20, 0);
            lpRect.sizeDelta = new Vector2(220, 200);
            var lpImg = logPanel.AddComponent<Image>();
            lpImg.color = new Color(0, 0, 0, 0.5f);
            
            var logTxtGO = new GameObject("Txt");
            logTxtGO.transform.SetParent(logPanel.transform);
            var logTxtRect = logTxtGO.AddComponent<RectTransform>();
            logTxtRect.anchorMin = Vector2.zero;
            logTxtRect.anchorMax = Vector2.one;
            logTxtRect.offsetMin = new Vector2(10, 10);
            logTxtRect.offsetMax = new Vector2(-10, -10);
            combatLogText = logTxtGO.AddComponent<Text>();
            combatLogText.font = font;
            combatLogText.fontSize = 12;
            combatLogText.color = Color.white;
            combatLogText.alignment = TextAnchor.UpperLeft;
            
            // ========== END TURN BUTTON (RIGHT-CENTER) ==========
            var btnGO = new GameObject("EndTurnBtn");
            btnGO.transform.SetParent(canvasGO.transform);
            var btnRect = btnGO.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(1, 0.5f);
            btnRect.anchorMax = new Vector2(1, 0.5f);
            btnRect.pivot = new Vector2(1, 0.5f);
            btnRect.anchoredPosition = new Vector2(-25, 0);
            btnRect.sizeDelta = new Vector2(140, 50);
            var btnImg = btnGO.AddComponent<Image>();
            btnImg.color = new Color(0.6f, 0.25f, 0.2f);
            endTurnButton = btnGO.AddComponent<Button>();
            
            var btnTxtGO = new GameObject("Txt");
            btnTxtGO.transform.SetParent(btnGO.transform);
            var btnTxtRect = btnTxtGO.AddComponent<RectTransform>();
            btnTxtRect.anchorMin = Vector2.zero;
            btnTxtRect.anchorMax = Vector2.one;
            btnTxtRect.pivot = new Vector2(0.5f, 0.5f);
            btnTxtRect.anchoredPosition = Vector2.zero;
            btnTxtRect.sizeDelta = Vector2.zero;
            var btnTxt = btnTxtGO.AddComponent<Text>();
            btnTxt.text = "END TURN";
            btnTxt.font = font;
            btnTxt.fontSize = 14;
            btnTxt.color = Color.white;
            btnTxt.alignment = TextAnchor.MiddleCenter;
            
            // ========== HAND CONTAINER (BOTTOM-CENTER) ==========
            var handGO = new GameObject("Hand");
            handGO.transform.SetParent(canvasGO.transform);
            var handRect = handGO.AddComponent<RectTransform>();
            handRect.anchorMin = new Vector2(0.5f, 0);
            handRect.anchorMax = new Vector2(0.5f, 0);
            handRect.pivot = new Vector2(0.5f, 0);
            handRect.anchoredPosition = new Vector2(0, 20);
            handRect.sizeDelta = new Vector2(700, 180);
            var handLayout = handGO.AddComponent<HorizontalLayoutGroup>();
            handLayout.childAlignment = TextAnchor.MiddleCenter;
            handLayout.spacing = 10;
            handLayout.childControlWidth = false;
            handLayout.childControlHeight = false;
            handContainer = handGO.transform;
        }
        
        private Text CreateText(Transform parent, Vector2 anchor, Vector2 size, string text, int fontSize, Color color, TextAnchor alignment, Vector2 offset)
        {
            var go = new GameObject("Txt");
            go.transform.SetParent(parent);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.sizeDelta = size;
            rect.anchoredPosition = offset;
            var txt = go.AddComponent<Text>();
            txt.text = text;
            txt.font = font;
            txt.fontSize = fontSize;
            txt.color = color;
            txt.alignment = alignment;
            return txt;
        }
        
        private void StartCombat()
        {
            player = new Player(80, 3);
            enemy = new Enemy("Slime", 30, new SimpleEnemyAI(attackDamage: 6, blockAmount: 4, attackChance: 0.7f));
            
            combatContext = new CombatContext(player);
            combatContext.AddEnemy(enemy);
            combatContext.StartCombat();
            
            enemy.DetermineNextAction(player, combatContext.TurnManager.TurnNumber);
            
            for (int i = 0; i < 5; i++)
            {
                var card = i < 3 ? strikeCard : defendCard;
                if (card != null)
                    player.AddToDeck(new CardInstance(card));
            }
            
            DrawCards(5);
            
            if (endTurnButton != null)
                endTurnButton.onClick.AddListener(OnEndTurnClicked);
            
            UpdateUI();
            Log("Combat started! Click cards to play.");
        }
        
        private void DrawCards(int count)
        {
            var deck = new List<CardInstance>(player.Deck);
            for (int i = 0; i < count && deck.Count > 0; i++)
            {
                int idx = Random.Range(0, deck.Count);
                player.AddToHand(deck[idx]);
                hand.Add(deck[idx]);
                deck.RemoveAt(idx);
            }
            RenderHand();
        }
        
        private void RenderHand()
        {
            foreach (Transform child in handContainer)
                Destroy(child.gameObject);
            
            for (int i = 0; i < hand.Count; i++)
                CreateCardUI(hand[i], i);
        }
        
        private void CreateCardUI(CardInstance card, int index)
        {
            var go = new GameObject("Card");
            go.transform.SetParent(handContainer);
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(120, 160);
            
            var img = go.AddComponent<Image>();
            img.color = card.Data.CardType == CardType.Attack ? new Color(0.7f, 0.25f, 0.2f) : new Color(0.25f, 0.45f, 0.65f);
            
            var btn = go.AddComponent<Button>();
            btn.interactable = player.CurrentEnergy >= card.Data.Cost;
            
            int idx = index;
            btn.onClick.AddListener(() => OnCardClicked(idx));
            
            // Name (top center)
            var nameGO = new GameObject("Name");
            nameGO.transform.SetParent(go.transform);
            var nameRect = nameGO.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.5f, 1f);
            nameRect.anchorMax = new Vector2(0.5f, 1f);
            nameRect.pivot = new Vector2(0.5f, 1f);
            nameRect.anchoredPosition = new Vector2(0, -10);
            nameRect.sizeDelta = new Vector2(110, 30);
            var nameTxt = nameGO.AddComponent<Text>();
            nameTxt.text = card.Data.CardName;
            nameTxt.font = font;
            nameTxt.fontSize = 16;
            nameTxt.color = Color.white;
            nameTxt.alignment = TextAnchor.MiddleCenter;
            nameTxt.fontStyle = FontStyle.Bold;
            
            // Description (center)
            var descGO = new GameObject("Desc");
            descGO.transform.SetParent(go.transform);
            var descRect = descGO.AddComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0.5f, 0.5f);
            descRect.anchorMax = new Vector2(0.5f, 0.5f);
            descRect.sizeDelta = new Vector2(110, 60);
            descRect.anchoredPosition = Vector2.zero;
            var descTxt = descGO.AddComponent<Text>();
            descTxt.text = card.Data.Description;
            descTxt.font = font;
            descTxt.fontSize = 12;
            descTxt.color = Color.white;
            descTxt.alignment = TextAnchor.MiddleCenter;
            
            // Cost (bottom right)
            var costBg = new GameObject("Cost");
            costBg.transform.SetParent(go.transform);
            var costRect = costBg.AddComponent<RectTransform>();
            costRect.anchorMin = new Vector2(1f, 0f);
            costRect.anchorMax = new Vector2(1f, 0f);
            costRect.pivot = new Vector2(1f, 0f);
            costRect.anchoredPosition = new Vector2(-8, 8);
            costRect.sizeDelta = new Vector2(30, 30);
            var costImg = costBg.AddComponent<Image>();
            costImg.color = new Color(0.9f, 0.75f, 0.2f);
            var costTxtGO = new GameObject("Txt");
            costTxtGO.transform.SetParent(costBg.transform);
            var costTxtRect = costTxtGO.AddComponent<RectTransform>();
            costTxtRect.anchorMin = Vector2.zero;
            costTxtRect.anchorMax = Vector2.one;
            costTxtRect.pivot = new Vector2(0.5f, 0.5f);
            costTxtRect.anchoredPosition = Vector2.zero;
            costTxtRect.sizeDelta = Vector2.zero;
            var costTxt = costTxtGO.AddComponent<Text>();
            costTxt.text = card.Data.Cost.ToString();
            costTxt.font = font;
            costTxt.fontSize = 18;
            costTxt.color = Color.black;
            costTxt.alignment = TextAnchor.MiddleCenter;
        }
        
        private void OnCardClicked(int index)
        {
            if (index < 0 || index >= hand.Count) return;
            
            var card = hand[index];
            if (!player.TrySpendEnergy(card.Data.Cost))
            {
                Log("Not enough energy!");
                return;
            }
            
            ExecuteCard(card);
            hand.RemoveAt(index);
            player.Discard(card);
            RenderHand();
            UpdateUI();
            
            if (enemy.IsDead)
            {
                Log("VICTORY!");
                endTurnButton.interactable = false;
            }
        }
        
        private void ExecuteCard(CardInstance card)
        {
            foreach (var effect in card.Data.Effects)
            {
                if (effect.EffectType == EffectType.Damage)
                {
                    int dmg = player.StatusManager.ModifyOutgoingDamage(effect.Value);
                    Log($"You dealt {dmg} damage!");
                    enemy.TakeDamage(dmg);
                }
                else if (effect.EffectType == EffectType.Block)
                {
                    int blk = player.StatusManager.ModifyBlock(effect.Value);
                    Log($"You gained {blk} block!");
                    player.AddBlock(blk);
                }
            }
        }
        
        private void OnEndTurnClicked()
        {
            Log("End turn");
            
            // End player turn phase
            combatContext.TurnManager.EndPlayerTurn();
            combatContext.TurnManager.StartEnemyTurn();
            
            player.ResetBlock();
            enemy.DetermineNextAction(player, combatContext.TurnManager.TurnNumber);
            ExecuteEnemyTurn();
            
            if (player.IsDead)
            {
                Log("DEFEAT!");
                endTurnButton.interactable = false;
                combatContext.TurnManager.EndCombat();
                UpdateUI();
                return;
            }
            
            // End enemy turn - this increments turn number!
            combatContext.TurnManager.EndEnemyTurn();
            
            player.ResetEnergy();
            DrawCards(Mathf.Min(2, player.Deck.Count));
            UpdateUI();
            RenderHand();
        }
        
        private void ExecuteEnemyTurn()
        {
            if (enemy.NextAction != null)
                Log($"{enemy.Name}: {enemy.NextAction.Description}");
            enemy.ExecuteAction(player);
            enemy.StatusManager.ProcessTurnEnd(enemy, combatContext);
            enemy.ResetBlock();
            UpdateUI();
        }
        
        private void UpdateUI()
        {
            playerHealthText.text = $"HP: {player.CurrentHealth}/{player.MaxHealth}";
            playerEnergyText.text = $"Energy: {player.CurrentEnergy}/{player.MaxEnergy}";
            playerBlockText.text = $"Block: {player.Block}";
            enemyHealthText.text = $"{enemy.CurrentHealth}/{enemy.MaxHealth}";
            enemyIntentText.text = $"Intent: {(enemy.NextAction != null ? enemy.NextAction.Description : "???")}";
            turnText.text = $"Turn {combatContext.TurnManager.TurnNumber}";
        }
        
        private void Log(string msg)
        {
            if (combatLogText != null)
                combatLogText.text += msg + "\n";
            Debug.Log(msg);
        }
    }
}