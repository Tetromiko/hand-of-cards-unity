using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace Tetromiko.CardsHandLayout.Samples
{
    public class CardHandDemoController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CardHandController handController;
        [SerializeField] private CardHandWebControlPanel controlPanel;
        [SerializeField] private CardHandLayoutOverlay layoutOverlay;

        private void Awake()
        {
            if (handController == null)
            {
                handController = FindObjectOfType<CardHandController>();
            }

            if (handController == null && Application.isPlaying)
            {
                BuildCompleteDemoUI();
            }
        }

        private void Start()
        {
            EnsureEventSystem();

            if (handController == null)
            {
                handController = FindObjectOfType<CardHandController>();
            }

            if (controlPanel == null)
            {
                controlPanel = FindObjectOfType<CardHandWebControlPanel>();
                if (controlPanel != null && handController != null)
                {
                    controlPanel.Initialize(handController);
                }
            }

            if (layoutOverlay == null && handController != null)
            {
                layoutOverlay = handController.GetComponentInChildren<CardHandLayoutOverlay>();
                if (layoutOverlay == null)
                {
                    var overlayObj = new GameObject("LayoutOverlay", typeof(RectTransform), typeof(CardHandLayoutOverlay));
                    overlayObj.transform.SetParent(handController.transform, false);
                    layoutOverlay = overlayObj.GetComponent<CardHandLayoutOverlay>();
                    layoutOverlay.Initialize(handController);
                }
            }
        }

        private void EnsureEventSystem()
        {
            EventSystemAdapter.EnsureAdaptiveEventSystem();
        }

        public void BuildCompleteDemoUI()
        {
            // Find or create Canvas
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                var canvasObj = new GameObject("DemoCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = canvasObj.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                var scaler = canvasObj.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
            }

            // Dark Backdrop - Slate 950 (#020617)
            var backdrop = canvas.GetComponentInChildren<Image>();
            if (backdrop == null)
            {
                var bgObj = new GameObject("DarkBackdrop", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                bgObj.transform.SetParent(canvas.transform, false);
                bgObj.transform.SetAsFirstSibling();
                var bgRt = bgObj.GetComponent<RectTransform>();
                bgRt.anchorMin = Vector2.zero;
                bgRt.anchorMax = Vector2.one;
                bgRt.sizeDelta = Vector2.zero;
                bgObj.GetComponent<Image>().color = new Color(0.012f, 0.024f, 0.055f, 1f);
            }

            // Create Hand Controller Container
            var handObj = new GameObject("CardHand", typeof(RectTransform), typeof(CardHandController));
            handObj.transform.SetParent(canvas.transform, false);

            var handRt = handObj.GetComponent<RectTransform>();
            handRt.anchorMin = new Vector2(0.5f, 0.5f);
            handRt.anchorMax = new Vector2(0.5f, 0.5f);
            handRt.pivot = new Vector2(0.5f, 0.5f);
            handRt.anchoredPosition = new Vector2(100f, -40f);
            handRt.sizeDelta = new Vector2(800f, 300f);

            handController = handObj.GetComponent<CardHandController>();
            handController.Settings.handWidth = 680f;
            handController.Settings.cardWidth = 112f;
            handController.Settings.cardHeight = 160f;
            handController.Settings.cardDistance = 56f;
            handController.Settings.minCardDistance = 24f;
            handController.Settings.hoverDistance = 112f;
            handController.Settings.hoverLift = 28f;
            handController.Settings.showLayoutDetails = true;

            // Add Layout Overlay
            var overlayObj = new GameObject("LayoutOverlay", typeof(RectTransform), typeof(CardHandLayoutOverlay));
            overlayObj.transform.SetParent(handObj.transform, false);
            layoutOverlay = overlayObj.GetComponent<CardHandLayoutOverlay>();
            layoutOverlay.Initialize(handController);

            // Add Web Blueprint Control Panel
            var panelObj = new GameObject("WebControlPanel", typeof(RectTransform), typeof(CardHandWebControlPanel));
            panelObj.transform.SetParent(canvas.transform, false);
            controlPanel = panelObj.GetComponent<CardHandWebControlPanel>();
            controlPanel.Initialize(handController);
        }
    }
}
