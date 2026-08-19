import React, { useState, useCallback } from 'react';
import { useCardsHand } from '../hooks/useCardsHand';
import { CardsControlPanel } from './CardsControlPanel';
import { CardsBoxLayout } from './CardsBoxLayout';

export const CardsRow: React.FC = () => {
  const {
    cards,
    settings,
    metrics,
    hoverZones,
    skinPack,
    containerRef,
    addCard,
    uploadSkinPack,
    resetSkinPack,
    removeLastCard,
    resetHand,
    updateSetting,
    handleMouseMove,
    handleMouseLeave,
    handlePanStart,
    handlePan,
    handlePanEnd,
    getCardTransform,
  } = useCardsHand();

  const [isDragOverCanvas, setIsDragOverCanvas] = useState<boolean>(false);

  const handleDragOver = useCallback((e: React.DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    e.stopPropagation();
    if (e.dataTransfer.types.includes('Files')) {
      setIsDragOverCanvas(true);
    }
  }, []);

  const handleDragLeave = useCallback((e: React.DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragOverCanvas(false);
  }, []);

  const handleDrop = useCallback(
    (e: React.DragEvent<HTMLDivElement>) => {
      e.preventDefault();
      e.stopPropagation();
      setIsDragOverCanvas(false);
      if (e.dataTransfer.files && e.dataTransfer.files.length > 0) {
        uploadSkinPack(e.dataTransfer.files);
      }
    },
    [uploadSkinPack]
  );

  return (
    <>
      <CardsControlPanel
        cardsCount={cards.length}
        settings={settings}
        metrics={metrics}
        skinPackCount={skinPack.length}
        onUpdateSetting={updateSetting}
        onAddCard={addCard}
        onRemoveCard={removeLastCard}
        onResetHand={resetHand}
        onUploadSkinPack={uploadSkinPack}
        onResetSkinPack={resetSkinPack}
      />

      <div
        ref={containerRef}
        onMouseMove={handleMouseMove}
        onMouseLeave={handleMouseLeave}
        onDragOver={handleDragOver}
        onDragLeave={handleDragLeave}
        onDrop={handleDrop}
        className={`relative flex items-center justify-center w-full h-96 select-none transition-colors ${
          isDragOverCanvas ? 'bg-cyan-950/20 ring-2 ring-cyan-500/50' : ''
        }`}
      >
        {isDragOverCanvas && (
          <div className="absolute inset-4 z-40 flex flex-col items-center justify-center bg-slate-950/80 border-2 border-dashed border-cyan-400 font-mono text-cyan-300 pointer-events-none">
            <span className="text-2xl mb-1">⤒</span>
            <span className="text-sm font-bold">Відпустіть файли зображень для створення скін-паку</span>
            <span className="text-xs text-slate-400 mt-0.5">PNG, JPG, WebP, SVG</span>
          </div>
        )}

        <CardsBoxLayout
          cards={cards}
          hoverZones={hoverZones}
          cardWidth={settings.cardWidth}
          minCardDistance={settings.minCardDistance}
          hoverDistance={settings.hoverDistance}
          handWidth={settings.handWidth}
          showLayoutDetails={settings.showLayoutDetails}
          pixelArt={settings.pixelArt}
          getCardTransform={getCardTransform}
          onPanStart={handlePanStart}
          onPan={handlePan}
          onPanEnd={handlePanEnd}
        />
      </div>
    </>
  );
};
