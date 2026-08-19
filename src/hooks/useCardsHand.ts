import React, { useState, useRef, useCallback } from 'react';
import { PanInfo } from 'motion/react';
import { Card, HandSettings, DEFAULT_SETTINGS, FIXED_CARD_WIDTH } from '../types';
import { DEFAULT_CARDS, createRandomCard } from '../data/cardsData';
import {
  computeHandMetrics,
  findClosestSlotIndex,
  computeCardTransform,
  getSlotXPos,
  getSlotHoverZones,
} from '../utils/cardsLayout';

export function useCardsHand() {
  const [cards, setCards] = useState<Card[]>(DEFAULT_CARDS);
  const [settings, setSettings] = useState<HandSettings>(DEFAULT_SETTINGS);
  const [skinPack, setSkinPack] = useState<string[]>([]);

  // Interaction State
  const [hoveredIndex, setHoveredIndex] = useState<number | null>(null);
  const [isDragging, setIsDragging] = useState<boolean>(false);
  const [draggingCardId, setDraggingCardId] = useState<string | null>(null);
  const [draggedIndex, setDraggedIndex] = useState<number | null>(null);
  const [dragTargetIndex, setDragTargetIndex] = useState<number | null>(null);

  const draggedIndexRef = useRef<number | null>(null);
  const dragTargetIndexRef = useRef<number | null>(null);

  const [panOffset, setPanOffset] = useState<{ x: number; y: number }>({ x: 0, y: 0 });
  const [initialDragX, setInitialDragX] = useState<number>(0);

  const containerRef = useRef<HTMLDivElement>(null);

  const activeIndex = isDragging ? dragTargetIndex : hoveredIndex;
  const metrics = computeHandMetrics(cards.length, settings, activeIndex);
  const hoverZones = getSlotHoverZones(metrics, settings);

  // Hand actions
  const addCard = useCallback(() => {
    setCards((prev) => {
      const newCard = createRandomCard();
      if (skinPack.length > 0) {
        newCard.imageUrl = skinPack[prev.length % skinPack.length];
      }
      return [...prev, newCard];
    });
  }, [skinPack]);

  const uploadSkinPack = useCallback((files: FileList | File[]) => {
    const fileArray = Array.from(files).filter((file) => file.type.startsWith('image/'));
    if (fileArray.length === 0) return;

    const urls = fileArray.map((file) => URL.createObjectURL(file));
    setSkinPack(urls);

    // Apply skin pack to current cards
    setCards((prev) =>
      prev.map((card, idx) => ({
        ...card,
        imageUrl: urls[idx % urls.length],
      }))
    );
  }, []);

  const resetSkinPack = useCallback(() => {
    setSkinPack([]);
    setCards((prev) =>
      prev.map((card) => {
        const { imageUrl, ...rest } = card;
        return rest;
      })
    );
  }, []);

  const removeLastCard = useCallback(() => {
    setCards((prev) => (prev.length > 1 ? prev.slice(0, -1) : prev));
  }, []);

  const removeCard = useCallback((id: string) => {
    setCards((prev) => prev.filter((c) => c.id !== id));
  }, []);

  const resetHand = useCallback(() => {
    setCards(DEFAULT_CARDS.map((c, i) => ({
      ...c,
      imageUrl: skinPack.length > 0 ? skinPack[i % skinPack.length] : undefined,
    })));
    setSettings(DEFAULT_SETTINGS);
    setHoveredIndex(null);
    setDraggingCardId(null);
    setDraggedIndex(null);
    setDragTargetIndex(null);
    setIsDragging(false);
  }, [skinPack]);

  const updateSetting = useCallback(<K extends keyof HandSettings>(key: K, value: HandSettings[K]) => {
    setSettings((prev) => ({ ...prev, [key]: value }));
  }, []);

  // Hover detection relative to the hand anchor point (center of the container)
  const handleMouseMove = useCallback((e: React.MouseEvent<HTMLDivElement>) => {
    if (!containerRef.current || cards.length === 0 || isDragging) return;
    const rect = containerRef.current.getBoundingClientRect();
    const mouseX = e.clientX - (rect.left + rect.width / 2);
    const mouseY = e.clientY - (rect.top + rect.height / 2);

    if (Math.abs(mouseY) > 130 || hoverZones.length === 0) {
      if (hoveredIndex !== null) setHoveredIndex(null);
      return;
    }

    const minBound = hoverZones[0].minX;
    const maxBound = hoverZones[hoverZones.length - 1].maxX;

    if (mouseX < minBound || mouseX > maxBound) {
      if (hoveredIndex !== null) setHoveredIndex(null);
      return;
    }

    const matchedZone = hoverZones.find((z) => mouseX >= z.minX && mouseX <= z.maxX);
    const targetSlot = matchedZone !== undefined ? matchedZone.slot : null;

    if (hoveredIndex !== targetSlot) {
      setHoveredIndex(targetSlot);
    }
  }, [cards.length, isDragging, hoveredIndex, hoverZones]);

  const handleMouseLeave = useCallback(() => {
    if (!isDragging) {
      setHoveredIndex(null);
    }
  }, [isDragging]);

  // Pan / Drag reorder handlers
  const handlePanStart = useCallback((cardId: string, index: number) => {
    const startX = getSlotXPos(index, metrics, settings);
    setIsDragging(true);
    setDraggingCardId(cardId);
    setDraggedIndex(index);
    setDragTargetIndex(index);
    draggedIndexRef.current = index;
    dragTargetIndexRef.current = index;
    setPanOffset({ x: 0, y: 0 });
    setInitialDragX(startX);
    setHoveredIndex(null);
  }, [metrics, settings]);

  const handlePan = useCallback((_: unknown, info: PanInfo) => {
    if (!containerRef.current || cards.length <= 1) return;

    setPanOffset({ x: info.offset.x, y: info.offset.y });

    const handRect = containerRef.current.getBoundingClientRect();
    const mouseX = info.point.x - (handRect.left + handRect.width / 2);

    const closestSlot = findClosestSlotIndex(mouseX, metrics, settings);

    dragTargetIndexRef.current = closestSlot;
    setDragTargetIndex((prev) => (prev !== closestSlot ? closestSlot : prev));
  }, [cards.length, metrics, settings]);

  const handlePanEnd = useCallback(() => {
    const targetIdx = dragTargetIndexRef.current;
    const sourceIdx = draggedIndexRef.current;

    if (
      sourceIdx !== null &&
      targetIdx !== null &&
      sourceIdx !== targetIdx
    ) {
      setCards((prev) => {
        const updated = [...prev];
        const [movedCard] = updated.splice(sourceIdx, 1);
        updated.splice(targetIdx, 0, movedCard);
        return updated;
      });
    }

    setHoveredIndex(targetIdx);
    setIsDragging(false);
    setDraggingCardId(null);
    setDraggedIndex(null);
    setDragTargetIndex(null);
    draggedIndexRef.current = null;
    dragTargetIndexRef.current = null;
    setPanOffset({ x: 0, y: 0 });
  }, []);

  const getCardTransform = useCallback(
    (index: number, cardId: string) => {
      return computeCardTransform(index, cardId, metrics, settings, {
        hoveredIndex,
        isDragging,
        draggingCardId,
        draggedIndex,
        dragTargetIndex,
        initialDragX,
        panOffset,
      });
    },
    [
      metrics,
      settings,
      hoveredIndex,
      isDragging,
      draggingCardId,
      draggedIndex,
      dragTargetIndex,
      initialDragX,
      panOffset,
    ]
  );

  return {
    cards,
    settings,
    metrics,
    hoverZones,
    containerRef,
    isDragging,
    draggingCardId,
    draggedIndex,
    dragTargetIndex,
    hoveredIndex,
    skinPack,
    addCard,
    uploadSkinPack,
    resetSkinPack,
    removeCard,
    removeLastCard,
    resetHand,
    updateSetting,
    handleMouseMove,
    handleMouseLeave,
    handlePanStart,
    handlePan,
    handlePanEnd,
    getCardTransform,
  };
}


