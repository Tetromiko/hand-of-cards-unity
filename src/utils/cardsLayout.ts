import { HandSettings, HandLayoutMetrics, CardTransform, FIXED_CARD_WIDTH } from '../types';

export interface SlotHoverZone {
  slot: number;
  centerX: number;
  minX: number;
  maxX: number;
  width: number;
  cardLeft: number;
  cardCenterX: number;
}

/**
 * Calculates minimum hand width by the formula:
 * Min Hand Width = (n - 2) * minCardDistance + CardWidth + hoverDistance
 */
export function calculateMinHandWidth(
  count: number,
  minCardDistance: number,
  hoverDistance?: number,
  cardWidth: number = FIXED_CARD_WIDTH
): number {
  const W = cardWidth;
  const H = Math.max(W, hoverDistance ?? W);
  if (count <= 0) return W;
  if (count === 1) return W;
  if (count === 2) return W + H;
  return (count - 2) * minCardDistance + W + H;
}

/**
 * Helper to determine occupied width & remaining compressible slots in hover state.
 */
function getHoverOccupiedWidth(
  count: number,
  activeIndex: number,
  W: number,
  H: number
): { fixedWidth: number; remainingSlots: number } {
  if (activeIndex === count - 1) {
    return {
      fixedWidth: (W + H) / 2,
      remainingSlots: count - 1,
    };
  }
  if (activeIndex === 0) {
    return {
      fixedWidth: W + (W + H) / 2,
      remainingSlots: Math.max(1, count - 2),
    };
  }
  return {
    fixedWidth: W + H,
    remainingSlots: Math.max(1, count - 2),
  };
}

/**
 * Computes global layout metrics for the hand of cards based on settings.
 */
export function computeHandMetrics(
  count: number,
  settings: HandSettings,
  activeIndex: number | null
): HandLayoutMetrics {
  const { handWidth, cardDistance, minCardDistance } = settings;
  const W = settings.cardWidth || FIXED_CARD_WIDTH;
  const H = Math.max(W, settings.hoverDistance || W);

  const minHandWidth = calculateMinHandWidth(count, minCardDistance, H, W);
  const availableWidth = Math.max(minHandWidth, handWidth);

  // Natural required span in idle state: (n - 1) * cardDistance + W
  const naturalSpan = count > 1 ? (count - 1) * Math.max(minCardDistance, cardDistance) + W : W;
  const isOverflowing = count > 1 && naturalSpan > availableWidth;

  // Auto-compress base distance if total span exceeds availableWidth
  const effectiveDistance = isOverflowing
    ? Math.max(minCardDistance, Math.floor((availableWidth - W) / (count - 1)))
    : Math.max(minCardDistance, cardDistance);

  const hasActiveCard = activeIndex !== null && count > 1;

  const { fixedWidth, remainingSlots } =
    hasActiveCard && activeIndex !== null
      ? getHoverOccupiedWidth(count, activeIndex, W, H)
      : { fixedWidth: W, remainingSlots: Math.max(1, count - 1) };

  const idealHoverSpan = hasActiveCard
    ? remainingSlots * effectiveDistance + fixedWidth
    : naturalSpan;

  const isHoverOverflowing = hasActiveCard && idealHoverSpan > availableWidth;

  const compressedHoverDistance = (() => {
    if (!hasActiveCard || count <= 1) return effectiveDistance;
    if (!isHoverOverflowing) return effectiveDistance;
    if (count === 2) return minCardDistance;
    return Math.max(minCardDistance, Math.floor((availableWidth - fixedWidth) / remainingSlots));
  })();

  return {
    count,
    availableWidth,
    effectiveDistance,
    compressedHoverDistance,
    hoverDistance: H,
    isOverflowing,
    isHoverOverflowing,
    hasActiveCard,
    activeIndex,
  };
}

/**
 * Calculates slot width for a given index based on hover state and hand compression.
 */
export function computeSlotWidth(
  slot: number,
  count: number,
  step: number,
  hoverDistance: number,
  activeIndex: number | null,
  cardWidth: number = FIXED_CARD_WIDTH
): number {
  const W = cardWidth;
  const H = Math.max(W, hoverDistance);

  if (count <= 1) return activeIndex === 0 ? H : W;

  const isHovered = activeIndex !== null && slot === activeIndex;
  const isEdge = slot === 0 || slot === count - 1;

  if (isHovered) {
    return isEdge ? (W + H) / 2 : H;
  }

  if (step >= W) {
    return isEdge ? (W + step) / 2 : step;
  }

  // step < W (compressed hand in idle or unhovered slot)
  return slot === count - 1 ? W : step;
}

/**
 * Determines card's visual alignment within its slot zone.
 */
export function computeCardPositionInSlot(
  slot: number,
  count: number,
  minX: number,
  maxX: number,
  step: number,
  isHovered: boolean,
  cardWidth: number = FIXED_CARD_WIDTH
): { cardLeft: number; cardCenterX: number } {
  const W = cardWidth;
  const centerX = (minX + maxX) / 2;

  if (count === 1) {
    return { cardLeft: centerX - W / 2, cardCenterX: centerX };
  }

  let cardLeft: number;

  if (slot === 0) {
    // First card always docks flush to the left boundary
    cardLeft = minX;
  } else if (slot === count - 1) {
    // Last card always docks flush to the right boundary
    cardLeft = maxX - W;
  } else if (isHovered || step >= W) {
    // Internal cards center inside their slot when hovered or when hand is expanded
    cardLeft = centerX - W / 2;
  } else {
    // Internal unhovered cards cascade from left edge when hand is compressed (step < W)
    cardLeft = minX;
  }

  return {
    cardLeft,
    cardCenterX: cardLeft + W / 2,
  };
}

/**
 * Computes exact positions and hover zones for all card slots.
 */
export function getSlotHoverZones(
  metrics: HandLayoutMetrics,
  settings: HandSettings
): SlotHoverZone[] {
  const { count, activeIndex } = metrics;
  const W = settings.cardWidth || FIXED_CARD_WIDTH;
  const H = Math.max(W, settings.hoverDistance || W);
  if (count === 0) return [];

  const hasActive = activeIndex !== null && count > 1;
  const step = hasActive ? metrics.compressedHoverDistance : metrics.effectiveDistance;

  // 1. Calculate slot widths
  const slotWidths: number[] = Array.from({ length: count }, (_, s) =>
    computeSlotWidth(s, count, step, H, activeIndex, W)
  );

  // 2. Compute total span and start from symmetric center
  const totalSpan = slotWidths.reduce((sum, w) => sum + w, 0);
  let currentLeft = -totalSpan / 2;

  // 3. Compute slot zones and card alignments
  const rawZones: SlotHoverZone[] = slotWidths.map((width, s) => {
    const minX = currentLeft;
    const maxX = minX + width;
    const centerX = (minX + maxX) / 2;
    const isHovered = hasActive && s === activeIndex;

    const { cardLeft, cardCenterX } = computeCardPositionInSlot(
      s,
      count,
      minX,
      maxX,
      step,
      isHovered,
      W
    );

    currentLeft = maxX;

    return {
      slot: s,
      centerX,
      minX,
      maxX,
      width,
      cardLeft,
      cardCenterX,
    };
  });

  // 4. Компенсація: уся непокрита зона карти (activeIndex - 1) стає її слотом/відстанню.
  // Якщо розширення активної карти таке, що вона частково або повністю відкриває карту зліва,
  // межа слота зміщується рівно до початку активної карти (або до правого краю попередньої карти,
  // якщо активна карта зміщена ще далі вправо і зовсім її не перекриває).
  if (hasActive && activeIndex !== null && activeIndex > 0) {
    const prevIdx = activeIndex - 1;
    const prevZone = rawZones[prevIdx];
    const activeZone = rawZones[activeIndex];

    // Фізичний правий край карти (activeIndex - 1)
    const prevCardRight = prevZone.cardLeft + W;

    // Фізичний лівий край активної карти (activeIndex)
    const activeCardLeft = activeZone.cardLeft;

    // Непокрита видима права межа карти (activeIndex - 1):
    // Якщо активна карта перекриває її — до початку активної карти (activeCardLeft);
    // Якщо активна карта взагалі не перекриває її — до повного правого краю (prevCardRight).
    const uncoveredRightX = Math.min(prevCardRight, activeCardLeft);

    // Якщо непокрита зона більша за базову межу, розширюємо слот карти (activeIndex - 1)
    if (uncoveredRightX > prevZone.maxX && uncoveredRightX < activeZone.maxX - 10) {
      const newSplitX = uncoveredRightX;

      prevZone.maxX = newSplitX;
      prevZone.width = prevZone.maxX - prevZone.minX;
      prevZone.centerX = (prevZone.minX + prevZone.maxX) / 2;

      activeZone.minX = newSplitX;
      activeZone.width = activeZone.maxX - activeZone.minX;
      activeZone.centerX = (activeZone.minX + activeZone.maxX) / 2;
    }
  }

  return rawZones;
}

/**
 * Returns the exact horizontal center position (X) for a specific card slot index.
 */
export function getSlotXPos(
  slot: number,
  metrics: HandLayoutMetrics,
  settings: HandSettings
): number {
  const zones = getSlotHoverZones(metrics, settings);
  if (slot >= 0 && slot < zones.length) {
    return zones[slot].cardCenterX;
  }
  return 0;
}

/**
 * Determines which slot `i` a card should occupy depending on whether a reorder drag is active.
 */
export function getCardTargetSlot(
  cardIndex: number,
  isDragging: boolean,
  draggedIndex: number | null,
  dragTargetIndex: number | null
): number {
  if (!isDragging || draggedIndex === null || dragTargetIndex === null) {
    return cardIndex;
  }

  if (draggedIndex < dragTargetIndex) {
    if (cardIndex > draggedIndex && cardIndex <= dragTargetIndex) {
      return cardIndex - 1;
    }
  } else if (draggedIndex > dragTargetIndex) {
    if (cardIndex >= dragTargetIndex && cardIndex < draggedIndex) {
      return cardIndex + 1;
    }
  }

  return cardIndex;
}

/**
 * Finds the slot index matching the given mouse X coordinate.
 */
export function findClosestSlotIndex(
  mouseX: number,
  metrics: HandLayoutMetrics,
  settings: HandSettings
): number {
  const zones = getSlotHoverZones(metrics, settings);
  if (zones.length === 0) return 0;

  for (const zone of zones) {
    if (mouseX >= zone.minX && mouseX <= zone.maxX) {
      return zone.slot;
    }
  }

  if (mouseX < zones[0].minX) return 0;
  return zones[zones.length - 1].slot;
}

/**
 * Computes final visual transform (X, Y, Scale, Z-Index) for a card at index `i`.
 */
export function computeCardTransform(
  cardIndex: number,
  cardId: string,
  metrics: HandLayoutMetrics,
  settings: HandSettings,
  interactionState: {
    hoveredIndex: number | null;
    isDragging: boolean;
    draggingCardId: string | null;
    draggedIndex: number | null;
    dragTargetIndex: number | null;
    initialDragX: number;
    panOffset: { x: number; y: number };
  }
): CardTransform {
  const { hoveredIndex, isDragging, draggingCardId, draggedIndex, dragTargetIndex, initialDragX, panOffset } = interactionState;
  const isThisCardDragged = isDragging && draggingCardId === cardId;
  const isHovered = !isDragging && hoveredIndex === cardIndex;
  const hoverLift = settings.hoverLift ?? 28;

  const slot = getCardTargetSlot(cardIndex, isDragging, draggedIndex, dragTargetIndex);
  let targetX = getSlotXPos(slot, metrics, settings);
  let targetY = isHovered ? -hoverLift : 0;

  if (isThisCardDragged) {
    targetX = initialDragX + panOffset.x;
    targetY = -hoverLift + panOffset.y;
  }

  const zIndex = isThisCardDragged ? 1000 : cardIndex + 1;
  const scale = isThisCardDragged ? 1.05 : 1;
  const state = isThisCardDragged ? 'dragged' : isHovered ? 'hovered' : 'idle';

  return {
    x: targetX,
    y: targetY,
    scale,
    zIndex,
    state,
  };
}
