# 🃏 Cards Hand Layout Engine for Unity

[![Unity 2021.3+](https://img.shields.io/badge/Unity-2021.3%2B-black.svg?style=flat-square&logo=unity)](https://unity.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=flat-square)](LICENSE.md)

Mathematical, responsive card hand layout engine for Unity UI (uGUI) with dynamic cascade compression, hover space compensation, spring animation, and real-time drag-and-drop reordering.

---

## 🌟 Key Features

- 📐 **Adaptive Cascade Compression**: Cards automatically compress step distance ($d \to d_{min}$) as the hand fills or screen shrinks.
- 🎯 **Hover Space Compensation**: When an active card expands and shifts right, the left neighbor card receives **100% of its uncovered visible area** for mouse hover interaction without expanding the total hand width.
- 🔄 **Real-Time Drag & Drop Reordering**: Neighbor cards dynamically part ways (Ghost Slot) to preview where the dragged card will land.
- ⚡ **Zero External Dependencies**: Uses pure Unity UI and native `Mathf.SmoothDamp` spring interpolation (compatible with DOTween or custom tweens).
- 🧩 **Pure Math Core**: `CardHandLayoutEngine` is a static, pure C# class with zero `MonoBehaviour` coupling, making it 100% unit-testable.

---

## 📦 Installation Guide

### Option 1: Install via Git URL (Unity Package Manager)
1. Open your Unity project (Unity 2021.3 or newer).
2. Go to **Window** > **Package Manager**.
3. Click the **`+`** button in the top-left corner and select **"Add package from git URL..."**.
4. Paste the repository URL:
   ```text
   https://github.com/Tetromiko/hand-of-cards.git?path=/Packages/com.tetromiko.cardshandlayout
   ```
5. Click **Add**. Unity will download and import the package automatically.

### Option 2: Install via `manifest.json`
Add the following dependency to your Unity project's `Packages/manifest.json`:
```json
{
  "dependencies": {
    "com.tetromiko.cardshandlayout": "https://github.com/Tetromiko/hand-of-cards.git?path=/Packages/com.tetromiko.cardshandlayout"
  }
}
```

### Option 3: Local Disk / Embedded Package
1. Clone or copy the folder `Packages/com.tetromiko.cardshandlayout` into your Unity project's `Packages/` directory.

---

## 🚀 Quick Start in 3 Minutes

1. **Create a Canvas & Hand Container**:
   - In your Unity Hierarchy, right-click and create **UI > Canvas**.
   - Inside the Canvas, create an empty GameObject named `CardHand` with a `RectTransform`.
2. **Attach the Controller**:
   - Add the `CardHandController` component to `CardHand`.
3. **Assign Card Prefab**:
   - Create a simple Card UI Prefab with the `CardView` component attached.
   - Assign the prefab to the `Card Prefab` field on `CardHandController`.
4. **Hit Play**:
   - The hand will automatically initialize with default cards and respond to hover, clicks, and drag-and-drop reordering!

---

## 🛠️ Scripting API

```csharp
using Tetromiko.CardsHandLayout;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private CardHandController handController;

    private void Start()
    {
        // Add a card
        handController.AddCard(new CardData("c1", "A", CardSuit.Spades));

        // Listen to events
        handController.onCardClicked.AddListener((card) => {
            Debug.Log($"Clicked card: {card.CardData.title}");
        });

        handController.onCardsReordered.AddListener((fromIdx, toIdx) => {
            Debug.Log($"Card moved from {fromIdx} to {toIdx}");
        });
    }
}
```

---

## 📄 License

MIT License. Free for commercial and non-commercial games.
