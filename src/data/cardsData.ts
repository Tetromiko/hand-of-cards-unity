import { Card, Suit } from '../types';

export const DEFAULT_CARDS: Card[] = [
  { id: 'card-1', suit: '♠', rank: 'A', color: 'black' },
  { id: 'card-2', suit: '♥', rank: 'K', color: 'red' },
  { id: 'card-3', suit: '♦', rank: 'Q', color: 'red' },
  { id: 'card-4', suit: '♣', rank: 'J', color: 'black' },
  { id: 'card-5', suit: '♠', rank: '10', color: 'black' },
];

export const SUITS: Suit[] = ['♠', '♥', '♦', '♣'];
export const RANKS = ['A', 'K', 'Q', 'J', '10', '9', '8', '7', '6', '5', '4', '3', '2'];

export function createRandomCard(): Card {
  const suit = SUITS[Math.floor(Math.random() * SUITS.length)];
  const rank = RANKS[Math.floor(Math.random() * RANKS.length)];
  const color = suit === '♥' || suit === '♦' ? 'red' : 'black';
  return {
    id: `card-${Math.random().toString(36).substring(2, 9)}`,
    suit,
    rank,
    color,
  };
}
