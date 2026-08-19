using System;
using UnityEngine;
using UnityEngine.UIElements;
using Tetromiko.CardsHandLayout.UIToolkit;

namespace Tetromiko.CardsHandLayout.Samples
{
    /// <summary>
    /// Demo controller for the UI Toolkit Card Hand sample.
    /// Sets up Camera, UIDocument, CardHandUIToolkitController, and CardHandUIToolkitControlPanel at runtime.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class CardHandUIToolkitDemoController : MonoBehaviour
    {
        private UIDocument uiDocument;
        private CardHandUIToolkitController handController;
        private CardHandUIToolkitControlPanel controlPanel;

        private void Awake()
        {
            SetupCamera();
            EnsureUIDocumentComponents();
        }

        private void SetupCamera()
        {
            var cam = Camera.main;
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.12f, 0.12f, 0.14f, 1f);
            }
        }

        private void EnsureUIDocumentComponents()
        {
            uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                uiDocument = gameObject.AddComponent<UIDocument>();
            }

            handController = GetComponent<CardHandUIToolkitController>();
            if (handController == null)
            {
                handController = gameObject.AddComponent<CardHandUIToolkitController>();
            }

            controlPanel = GetComponent<CardHandUIToolkitControlPanel>();
            if (controlPanel == null)
            {
                controlPanel = gameObject.AddComponent<CardHandUIToolkitControlPanel>();
            }
        }
    }
}
