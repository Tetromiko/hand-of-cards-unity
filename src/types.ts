export type Suit = '♠' | '♥' | '♦' | '♣';

export const FIXED_CARD_WIDTH = 112;

export interface Card {
  id: string;
  suit: Suit;
  rank: string;
  color: 'red' | 'black';
  imageUrl?: string;
  name?: string;
}

export interface HandSettings {
  handWidth: number;
  cardWidth: number;
  cardDistance: number;
  minCardDistance: number;
  hoverDistance: number;
  hoverLift: number; // незалежний параметр підйому при наведенні
  showLayoutDetails: boolean; // увімкнути/вимкнути інженерні деталі розмітки
  pixelArt: boolean; // підтримка піксель-арту (crisp nearest-neighbor rendering)
}

export const DEFAULT_SETTINGS: HandSettings = {
  handWidth: 680,
  cardWidth: 112,
  cardDistance: 56,
  minCardDistance: 24,
  hoverDistance: 112,
  hoverLift: 28,
  showLayoutDetails: true,
  pixelArt: true,
};

/**
 * Card interaction state
 */
export type CardInteractionState = 'idle' | 'hovered' | 'dragged';

/**
 * Calculated visual transform for a card
 */
export interface CardTransform {
  x: number;
  y: number;
  scale: number;
  zIndex: number;
  state: CardInteractionState;
}

/**
 * Calculated hand layout metrics
 */
export interface HandLayoutMetrics {
  count: number;
  availableWidth: number;
  effectiveDistance: number;
  compressedHoverDistance: number;
  hoverDistance: number;
  isOverflowing: boolean;
  isHoverOverflowing: boolean;
  hasActiveCard: boolean;
  activeIndex: number | null;
}
