# 🃏 Cards Hand Layout Engine

[![TypeScript](https://img.shields.io/badge/TypeScript-5.8-blue?style=flat-square&logo=typescript)](https://www.typescriptlang.org/)
[![React](https://img.shields.io/badge/React-19.0-61dafb?style=flat-square&logo=react)](https://react.dev/)
[![Tailwind CSS](https://img.shields.io/badge/Tailwind_CSS-v4-38bdf8?style=flat-square&logo=tailwindcss)](https://tailwindcss.com/)
[![Motion](https://img.shields.io/badge/Motion-React-f43f5e?style=flat-square)](https://motion.dev/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=flat-square)](https://opensource.org/licenses/MIT)

> **Математичний рушій адаптивної руки карт** для ігор та інтерфейсів із динамічним стисненням каскаду, компенсацією простору при наведенні (Hover Space Compensation) та плавним перевпорядкуванням (Drag & Drop Reordering).

---

## 🌟 Ключові особливості (Features)

- 📐 **Математичний адаптивний каскад**:
  Автоматичне плавне стиснення кроку між картами ($d \to d_{min}$) при зміні ширини контейнера або додаванні нових карт.
- 🎯 **Точна компенсація простору при наведенні (Hover Space Compensation)**:
  Коли активна карта розширюється і відсувається вправо, карта зліва від неї **отримує 100% своєї відкритої площі** для наведення миші. Сумарна ширина руки при цьому **не змінюється**.
- 🔄 **Інтерактивне перетягування (Drag & Drop Reordering)**:
  Плавне переміщення карт із миттєвою реакцією сусідів (Ghost Slot), які розсуваються та звільняють цільовий слот у реальному часі.
- 🎨 **Модульний шар скінів та піксель-арту**:
  - Підтримка власного скін-паку (Drag & Drop завантаження PNG, WebP, SVG).
  - Підтримка чіткого рендерингу піксель-арту (`nearest-neighbor / crisp pixels`).
- 🛠️ **Інженерний режим інспекції розмітки (Layout Wireframe Details)**:
  Візуалізація точних зон наведення (Slot Zones), фактичного спану карток, меж мінімальної ширини та габаритів.
- 🕹️ **Повна готовність до портування на Unity / Godot / Unreal**:
  Математична логіка повністю відокремлена від візуалізації. Докладна специфікація логіки міститься у [`UNITY_SPECIFICATION.md`](./UNITY_SPECIFICATION.md).

---

## 📐 Як працює математика розмітки

### 1. Формула абсолютної мінімальної ширини руки ($W_{min}$)
Щоб при наведенні на будь-яку карту вміст гарантовано поміщався у межі руки при максимальному стисненні:

$$
W_{min}(n) = \begin{cases} 
W & n \le 1 \\
W + H & n = 2 \\
(n - 2) \cdot d_{min} + W + H & n \ge 3 
\end{cases}
$$

де:
- $n$ — кількість карт
- $W$ — ширина однієї карти (`cardWidth`)
- $H$ — ширина слота під активну карту (`hoverDistance`)
- $d_{min}$ — мінімально допустимий крок (`minCardDistance`)

### 2. Принцип компенсації відкритої площі
```
[ Карта 1 ]──[    Карта 2    ]──[     АКТИВНА КАРТА 3 (піднята)     ]──[ Карта 4 ]
              ▲               ▲
              └───────────────┴── Точка розділу = min(Правий край К2, Лівий край К3)
```
Карта 3 віддає свій відкритий лівий простір Карті 2. Завдяки цьому вся видима площа Карти 2 миттєво реагує на курсор без жодних мертвих зон.

---

## 🚀 Швидкий старт (Getting Started)

### Встановлення та запуск локально:

```bash
# 1. Клонувати репозиторій
git clone https://github.com/your-username/cards-hand-layout.git
cd cards-hand-layout

# 2. Встановити залежності
npm install

# 3. Запустити локальний сервер розробки
npm run dev
```

Відкрийте браузер за адресою [http://localhost:3000](http://localhost:3000).

---

## 🏗️ Структура проєкту (Architecture)

```text
├── src/
│   ├── components/
│   │   ├── CardsBoxLayout.tsx      # Візуальний рендеринг карт, маркерів та інженерних дужок
│   │   ├── CardsControlPanel.tsx    # Панель налаштувань (габарити, крок, ховер, скін-пак)
│   │   └── CardsRow.tsx             # Контейнер полотна з підтримкою Drag-and-Drop завантаження
│   ├── hooks/
│   │   └── useCardsHand.ts          # Стан інтеракцій (Hover, Drag, Reorder, Skin Pack)
│   ├── utils/
│   │   └── cardsLayout.ts           # ЧИСТИЙ МАТЕМАТИЧНИЙ РУШІЙ (геометрія, зони, компенсація)
│   ├── data/
│   │   └── cardsData.ts             # Стартові дані гральних карт
│   ├── types.ts                     # TypeScript типи та конфігурація HandSettings
│   ├── App.tsx                      # Головна точка входу
│   └── main.tsx
├── UNITY_SPECIFICATION.md           # Повний концептуальний опис для портування в Unity
├── package.json
└── README.md
```

---

## 🎮 Портування на Unity (Game Engines)

Математика рушія спроектована за принципом **чистих функцій (Pure Functions)**. Для легкого переносу на **C# (uGUI / UI Toolkit / 2D / 3D)** ознайомтеся з файлом:

👉 **[Читати UNITY_SPECIFICATION.md](./UNITY_SPECIFICATION.md)** — повний алгоритмічний опис усіх станів, крайових випадків та геометрії.

---

## 📄 Ліцензія (License)

Цей проєкт поширюється під відкритою ліцензією [MIT](./LICENSE). Ви можете вільно використовувати алгоритми та код у комерційних та некомерційних ігрових проєктах.
