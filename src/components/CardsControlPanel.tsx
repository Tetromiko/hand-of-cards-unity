import React, { useState, useEffect, useRef } from 'react';
import { HandSettings, HandLayoutMetrics } from '../types';
import { calculateMinHandWidth } from '../utils/cardsLayout';

interface CardsControlPanelProps {
  cardsCount: number;
  settings: HandSettings;
  metrics: HandLayoutMetrics;
  skinPackCount: number;
  onUpdateSetting: <K extends keyof HandSettings>(key: K, value: HandSettings[K]) => void;
  onAddCard: () => void;
  onRemoveCard: () => void;
  onResetHand: () => void;
  onUploadSkinPack: (files: FileList | File[]) => void;
  onResetSkinPack: () => void;
}

export const CardsControlPanel: React.FC<CardsControlPanelProps> = ({
  cardsCount,
  settings,
  metrics,
  skinPackCount,
  onUpdateSetting,
  onAddCard,
  onRemoveCard,
  onResetHand,
  onUploadSkinPack,
  onResetSkinPack,
}) => {
  const [isMinimized, setIsMinimized] = useState<boolean>(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const currentCardWidth = settings.cardWidth || 112;
  const currentHoverDistance = Math.max(currentCardWidth, settings.hoverDistance || currentCardWidth);
  const minHandWidth = calculateMinHandWidth(
    cardsCount,
    settings.minCardDistance,
    currentHoverDistance,
    currentCardWidth
  );
  const currentHandWidth = Math.max(minHandWidth, settings.handWidth);
  const currentCardDistance = Math.max(settings.minCardDistance, settings.cardDistance);

  // Synchronize handWidth in state if minHandWidth exceeds current setting
  useEffect(() => {
    if (settings.handWidth < minHandWidth) {
      onUpdateSetting('handWidth', minHandWidth);
    }
  }, [minHandWidth, settings.handWidth, onUpdateSetting]);

  const handleCardWidthChange = (newWidth: number) => {
    const val = Math.max(40, Math.min(300, newWidth));
    onUpdateSetting('cardWidth', val);
    if (settings.hoverDistance < val) {
      onUpdateSetting('hoverDistance', val);
    }
  };

  const handleHandWidthChange = (newWidth: number) => {
    const val = Math.max(minHandWidth, Math.min(2000, newWidth));
    onUpdateSetting('handWidth', val);
  };

  const handleCardDistanceChange = (newDist: number) => {
    const val = Math.max(settings.minCardDistance, Math.min(300, newDist));
    onUpdateSetting('cardDistance', val);
  };

  const handleMinCardDistanceChange = (newMin: number) => {
    const val = Math.max(2, Math.min(150, newMin));
    onUpdateSetting('minCardDistance', val);
    if (settings.cardDistance < val) {
      onUpdateSetting('cardDistance', val);
    }
  };

  const handleHoverDistanceChange = (newVal: number) => {
    const clamped = Math.max(currentCardWidth, Math.min(400, newVal));
    onUpdateSetting('hoverDistance', clamped);
  };

  const handleHoverLiftChange = (newLift: number) => {
    const val = Math.max(0, Math.min(120, newLift));
    onUpdateSetting('hoverLift', val);
  };

  return (
    <div
      id="cards-control-panel"
      className={`fixed top-3 left-3 z-50 bg-slate-950/95 backdrop-blur-md border border-slate-800 shadow-2xl select-none font-mono text-xs transition-all duration-200 ${
        isMinimized ? 'w-56 p-2.5' : 'w-80 p-3'
      }`}
    >
      {/* Blueprint Corner Ticks */}
      <div className="absolute top-0 left-0 w-2 h-2 border-t-2 border-l-2 border-slate-600 pointer-events-none" />
      <div className="absolute top-0 right-0 w-2 h-2 border-t-2 border-r-2 border-slate-600 pointer-events-none" />
      <div className="absolute bottom-0 left-0 w-2 h-2 border-b-2 border-l-2 border-slate-600 pointer-events-none" />
      <div className="absolute bottom-0 right-0 w-2 h-2 border-b-2 border-r-2 border-slate-600 pointer-events-none" />

      {/* Header Bar */}
      <div className="flex items-center justify-between pb-2 border-b border-slate-800/80">
        <span className="text-slate-300 font-semibold tracking-wider uppercase text-[10px]">
          Панель керування
        </span>
        <div className="flex items-center gap-1.5">
          <span className="text-[10px] text-slate-500">{cardsCount} шт.</span>
          <button
            type="button"
            title={isMinimized ? 'Розгорнути' : 'Згорнути'}
            onClick={() => setIsMinimized((prev) => !prev)}
            className="w-5 h-5 flex items-center justify-center text-slate-400 hover:text-slate-100 hover:bg-slate-800 border border-slate-800 transition-colors cursor-pointer text-[10px]"
          >
            {isMinimized ? '□' : '_'}
          </button>
        </div>
      </div>

      {/* When Minimized */}
      {isMinimized && (
        <div className="pt-2 flex items-center justify-between gap-1 text-[10px]">
          <div className="flex items-center gap-1">
            <button
              type="button"
              disabled={cardsCount <= 1}
              onClick={onRemoveCard}
              className="w-6 h-6 flex items-center justify-center bg-slate-900 hover:bg-slate-800 disabled:opacity-40 disabled:cursor-not-allowed text-slate-200 border border-slate-800 cursor-pointer"
            >
              -
            </button>
            <span className="w-8 text-center text-slate-200 font-bold">{cardsCount}</span>
            <button
              type="button"
              onClick={onAddCard}
              className="w-6 h-6 flex items-center justify-center bg-slate-900 hover:bg-slate-800 text-slate-200 border border-slate-800 cursor-pointer"
            >
              +
            </button>
          </div>
          <button
            type="button"
            onClick={onResetHand}
            className="px-2 py-1 bg-slate-900 hover:bg-slate-800 text-slate-400 hover:text-slate-200 border border-slate-800 cursor-pointer"
          >
            Скинути
          </button>
        </div>
      )}

      {/* Expanded Content */}
      {!isMinimized && (
        <div className="flex flex-col gap-2.5 pt-2">
          {/* Cards Quantity Controls & Reset */}
          <div className="flex items-center gap-1.5">
            <div className="flex items-center bg-slate-900 border border-slate-800">
              <button
                type="button"
                disabled={cardsCount <= 1}
                onClick={onRemoveCard}
                title="Прибрати карту"
                className="w-7 h-6 flex items-center justify-center bg-slate-900 hover:bg-slate-800 disabled:opacity-30 disabled:cursor-not-allowed text-slate-300 hover:text-rose-400 transition-colors cursor-pointer text-xs font-bold"
              >
                −
              </button>
              <span className="w-8 text-center text-slate-200 text-xs font-bold font-mono">
                {cardsCount}
              </span>
              <button
                type="button"
                onClick={onAddCard}
                title="Додати карту"
                className="w-7 h-6 flex items-center justify-center bg-slate-900 hover:bg-slate-800 text-slate-300 hover:text-emerald-400 transition-colors cursor-pointer text-xs font-bold"
              >
                +
              </button>
            </div>

            <button
              type="button"
              onClick={onResetHand}
              className="flex-1 h-6 px-2 bg-slate-950 hover:bg-slate-900 text-slate-500 hover:text-slate-300 border border-slate-800 hover:border-slate-700 transition-colors cursor-pointer text-[10px] uppercase tracking-wider"
            >
              Скинути руку
            </button>
          </div>

          {/* Grouped Core Layout Controls (ОСНОВА ЛЕЙАУТУ) */}
          <div className="flex flex-col gap-2 pt-0.5">
            {/* Category 1: Габарити карти та руки (CYAN BOX) */}
            <div className="flex flex-col gap-2 p-2 bg-cyan-950/20 border border-cyan-900/50">
              {/* Card Width (W) */}
              <div className="flex flex-col gap-1">
                <div className="flex justify-between items-center">
                  <span className="text-cyan-400 font-semibold text-[11px]">
                    Ширина карти (W)
                  </span>
                  <div className="flex items-center gap-0.5">
                    <input
                      type="number"
                      min={40}
                      max={300}
                      step={2}
                      value={currentCardWidth}
                      onChange={(e) => handleCardWidthChange(Number(e.target.value))}
                      className="w-12 h-5 bg-slate-900 border border-cyan-800/80 text-cyan-300 font-mono text-right text-[11px] px-1 focus:outline-none focus:border-cyan-400"
                    />
                    <span className="text-[10px] text-cyan-600">px</span>
                  </div>
                </div>
                <input
                  type="range"
                  min={60}
                  max={200}
                  step={2}
                  value={currentCardWidth}
                  onChange={(e) => handleCardWidthChange(Number(e.target.value))}
                  className="w-full h-1 bg-slate-800 rounded-none appearance-none cursor-pointer accent-cyan-400"
                />
              </div>

              <div className="border-t border-cyan-900/30" />

              {/* Hand Width */}
              <div className="flex flex-col gap-1">
                <div className="flex justify-between items-center">
                  <span className="text-cyan-400 font-semibold text-[11px]">
                    Ширина руки
                  </span>
                  <div className="flex items-center gap-0.5">
                    <input
                      type="number"
                      min={minHandWidth}
                      max={2000}
                      step={5}
                      value={currentHandWidth}
                      onChange={(e) => handleHandWidthChange(Number(e.target.value))}
                      className="w-14 h-5 bg-slate-900 border border-cyan-800/80 text-cyan-300 font-mono text-right text-[11px] px-1 focus:outline-none focus:border-cyan-400"
                    />
                    <span className="text-[10px] text-cyan-600">px</span>
                  </div>
                </div>
                <input
                  type="range"
                  min={minHandWidth}
                  max={1400}
                  step={5}
                  value={currentHandWidth}
                  onChange={(e) => handleHandWidthChange(Number(e.target.value))}
                  className="w-full h-1 bg-slate-800 rounded-none appearance-none cursor-pointer accent-cyan-400"
                />
              </div>
            </div>

            {/* Category 2: Крок розміщення та стиснення (SLATE BOX) */}
            <div className="flex flex-col gap-2 p-2 bg-slate-900/40 border border-slate-800">
              {/* Card Distance (Step) */}
              <div className="flex flex-col gap-1">
                <div className="flex justify-between items-center">
                  <span className="text-slate-300 font-semibold text-[11px]">
                    Відстань (step)
                  </span>
                  <div className="flex items-center gap-0.5">
                    <input
                      type="number"
                      min={settings.minCardDistance}
                      max={300}
                      step={2}
                      value={currentCardDistance}
                      onChange={(e) => handleCardDistanceChange(Number(e.target.value))}
                      className="w-12 h-5 bg-slate-900 border border-slate-700 text-slate-200 font-mono text-right text-[11px] px-1 focus:outline-none focus:border-slate-400"
                    />
                    <span className="text-[10px] text-slate-500">px</span>
                  </div>
                </div>
                <input
                  type="range"
                  min={settings.minCardDistance}
                  max={220}
                  step={2}
                  value={currentCardDistance}
                  onChange={(e) => handleCardDistanceChange(Number(e.target.value))}
                  className="w-full h-1 bg-slate-800 rounded-none appearance-none cursor-pointer accent-slate-400"
                />
              </div>

              <div className="border-t border-slate-800/80" />

              {/* Min Card Distance */}
              <div className="flex flex-col gap-1">
                <div className="flex justify-between items-center">
                  <span className="text-slate-300 font-semibold text-[11px]">
                    Мін. відстань
                  </span>
                  <div className="flex items-center gap-0.5">
                    <input
                      type="number"
                      min={2}
                      max={150}
                      step={2}
                      value={settings.minCardDistance}
                      onChange={(e) => handleMinCardDistanceChange(Number(e.target.value))}
                      className="w-12 h-5 bg-slate-900 border border-slate-700 text-slate-200 font-mono text-right text-[11px] px-1 focus:outline-none focus:border-slate-400"
                    />
                    <span className="text-[10px] text-slate-500">px</span>
                  </div>
                </div>
                <input
                  type="range"
                  min={4}
                  max={100}
                  step={2}
                  value={settings.minCardDistance}
                  onChange={(e) => handleMinCardDistanceChange(Number(e.target.value))}
                  className="w-full h-1 bg-slate-800 rounded-none appearance-none cursor-pointer accent-slate-400"
                />
              </div>
            </div>

            {/* Category 3: Ховер та компенсація (PINK BOX) */}
            <div className="flex flex-col gap-2 p-2 bg-pink-950/20 border border-pink-900/50">
              {/* Hover Distance (H) */}
              <div className="flex flex-col gap-1">
                <div className="flex justify-between items-center">
                  <span className="text-pink-400 font-semibold text-[11px]">
                    Відстань ховеру (H)
                  </span>
                  <div className="flex items-center gap-0.5">
                    <input
                      type="number"
                      min={currentCardWidth}
                      max={400}
                      step={2}
                      value={currentHoverDistance}
                      onChange={(e) => handleHoverDistanceChange(Number(e.target.value))}
                      className="w-12 h-5 bg-slate-900 border border-pink-800/80 text-pink-300 font-mono text-right text-[11px] px-1 focus:outline-none focus:border-pink-400"
                    />
                    <span className="text-[10px] text-pink-600">px</span>
                  </div>
                </div>
                <input
                  type="range"
                  min={currentCardWidth}
                  max={280}
                  step={2}
                  value={currentHoverDistance}
                  onChange={(e) => handleHoverDistanceChange(Number(e.target.value))}
                  className="w-full h-1 bg-slate-800 rounded-none appearance-none cursor-pointer accent-pink-400"
                />
              </div>

              <div className="border-t border-pink-900/30" />

              {/* Hover Lift */}
              <div className="flex flex-col gap-1">
                <div className="flex justify-between items-center">
                  <span className="text-pink-400 font-semibold text-[11px]">
                    Підйом (Lift)
                  </span>
                  <div className="flex items-center gap-0.5">
                    <input
                      type="number"
                      min={0}
                      max={120}
                      step={2}
                      value={settings.hoverLift ?? 28}
                      onChange={(e) => handleHoverLiftChange(Number(e.target.value))}
                      className="w-12 h-5 bg-slate-900 border border-pink-800/80 text-pink-300 font-mono text-right text-[11px] px-1 focus:outline-none focus:border-pink-400"
                    />
                    <span className="text-[10px] text-pink-600">px</span>
                  </div>
                </div>
                <input
                  type="range"
                  min={0}
                  max={80}
                  step={2}
                  value={settings.hoverLift ?? 28}
                  onChange={(e) => handleHoverLiftChange(Number(e.target.value))}
                  className="w-full h-1 bg-slate-800 rounded-none appearance-none cursor-pointer accent-pink-400"
                />
              </div>
            </div>
          </div>

          {/* Візуальні додатки та скіни (EXTENSIONS & SKINS) */}
          <div className="flex flex-col gap-1.5 pt-1 border-t border-slate-800/80">
            <span className="text-slate-400 text-[10px] font-semibold uppercase tracking-wider">
              Візуалізація та скіни
            </span>

            {/* Layout Details Toggle Switch */}
            <div className="p-1.5 bg-slate-900/60 border border-slate-800 flex items-center justify-between">
              <span className="text-slate-300 font-semibold text-[11px]">
                Деталі розмітки (Layout)
              </span>
              <button
                type="button"
                onClick={() => onUpdateSetting('showLayoutDetails', !settings.showLayoutDetails)}
                className={`px-2 py-0.5 text-[10px] font-mono font-semibold border transition-colors cursor-pointer ${
                  settings.showLayoutDetails
                    ? 'bg-cyan-950/80 text-cyan-300 border-cyan-700/80 shadow-[0_0_8px_rgba(6,182,212,0.25)]'
                    : 'bg-slate-950 text-slate-500 border-slate-800 hover:text-slate-400'
                }`}
              >
                {settings.showLayoutDetails ? '[ УВІМКНЕНО ]' : '[ ВИМКНЕНО ]'}
              </button>
            </div>

            {/* Pixel Art Rendering Toggle Switch */}
            <div className="p-1.5 bg-slate-900/60 border border-slate-800 flex items-center justify-between">
              <span className="text-slate-300 font-semibold text-[11px]">
                Піксель-арт (Crisp Pixels)
              </span>
              <button
                type="button"
                onClick={() => onUpdateSetting('pixelArt', !settings.pixelArt)}
                className={`px-2 py-0.5 text-[10px] font-mono font-semibold border transition-colors cursor-pointer ${
                  settings.pixelArt
                    ? 'bg-emerald-950/80 text-emerald-300 border-emerald-700/80 shadow-[0_0_8px_rgba(16,185,129,0.25)]'
                    : 'bg-slate-950 text-slate-500 border-slate-800 hover:text-slate-400'
                }`}
              >
                {settings.pixelArt ? '[ УВІМКНЕНО ]' : '[ ВИМКНЕНО ]'}
              </button>
            </div>

            {/* Skin Pack Control Box */}
            <div className="p-2 bg-indigo-950/20 border border-indigo-900/50 flex flex-col gap-1.5">
              <div className="flex items-center justify-between">
                <span className="text-indigo-300 font-semibold text-[11px]">
                  Скін-пак (Skin Pack)
                </span>
                <span className="text-[10px] text-indigo-400/90 font-mono">
                  {skinPackCount > 0 ? `${skinPackCount} скінів` : 'Базовий'}
                </span>
              </div>

              {/* Hidden File Input for Skin Pack */}
              <input
                ref={fileInputRef}
                type="file"
                multiple
                accept="image/png, image/jpeg, image/jpg, image/webp, image/svg+xml, image/gif"
                onChange={(e) => {
                  if (e.dataTransfer?.files || e.target.files) {
                    const files = e.target.files;
                    if (files && files.length > 0) {
                      onUploadSkinPack(files);
                      e.target.value = '';
                    }
                  }
                }}
                className="hidden"
              />

              <div className="flex items-center gap-1.5">
                <button
                  type="button"
                  onClick={() => fileInputRef.current?.click()}
                  title="Завантажити набір зображень для карт (PNG, JPG, WebP, SVG)"
                  className="flex-1 h-6 flex items-center justify-center gap-1 bg-indigo-900/40 hover:bg-indigo-900/70 text-indigo-200 hover:text-white border border-indigo-700/60 hover:border-indigo-500 transition-colors cursor-pointer text-[10px]"
                >
                  <span>⤒</span>
                  <span>Завантажити скіни</span>
                </button>

                {skinPackCount > 0 && (
                  <button
                    type="button"
                    onClick={onResetSkinPack}
                    title="Скинути до стандартних карт"
                    className="px-2 h-6 bg-slate-900 hover:bg-slate-800 text-rose-300 hover:text-rose-200 border border-rose-900/60 hover:border-rose-700 transition-colors cursor-pointer text-[10px]"
                  >
                    Скинути скін
                  </button>
                )}
              </div>
            </div>
          </div>

          {/* Mini-Telemetry Status Row */}
          <div className="p-1.5 bg-slate-950 border border-slate-800/90 flex items-center justify-between text-[10px] text-slate-400 font-mono">
            <div className="flex items-center gap-1">
              <span className="text-slate-600">Стан:</span>
              <span
                className={`font-semibold ${
                  metrics.isOverflowing ? 'text-amber-400' : 'text-emerald-400'
                }`}
              >
                {metrics.isOverflowing ? 'Overflow' : 'Normal'}
              </span>
            </div>
            <div className="flex items-center gap-2 text-slate-400">
              <span>
                step: <span className="text-slate-200 font-bold">{metrics.effectiveDistance}px</span>
              </span>
              {metrics.hasActiveCard && (
                <span>
                  h-step:{' '}
                  <span className="text-pink-400 font-bold">
                    {metrics.compressedHoverDistance}px
                  </span>
                </span>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
