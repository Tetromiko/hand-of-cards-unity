import React from 'react';
import { motion, AnimatePresence, PanInfo } from 'motion/react';
import { Card, CardTransform, FIXED_CARD_WIDTH } from '../types';
import { SlotHoverZone, calculateMinHandWidth } from '../utils/cardsLayout';

interface CardsBoxLayoutProps {
  cards: Card[];
  hoverZones: SlotHoverZone[];
  cardWidth: number;
  minCardDistance: number;
  hoverDistance: number;
  handWidth: number;
  showLayoutDetails: boolean;
  pixelArt: boolean;
  getCardTransform: (index: number, cardId: string) => CardTransform;
  onPanStart: (cardId: string, index: number) => void;
  onPan: (_: unknown, info: PanInfo) => void;
  onPanEnd: () => void;
}

export const CardsBoxLayout: React.FC<CardsBoxLayoutProps> = ({
  cards,
  hoverZones,
  cardWidth,
  minCardDistance,
  hoverDistance,
  handWidth,
  showLayoutDetails,
  pixelArt,
  getCardTransform,
  onPanStart,
  onPan,
  onPanEnd,
}) => {
  const count = hoverZones.length;
  const totalMinX = count > 0 ? hoverZones[0].minX : 0;
  const totalMaxX = count > 0 ? hoverZones[count - 1].maxX : 0;
  const totalWidth = totalMaxX - totalMinX;
  const totalCenterX = (totalMinX + totalMaxX) / 2;

  const cardHeight = 160;
  const minHandWidth = calculateMinHandWidth(count, minCardDistance, hoverDistance, cardWidth);
  const effectiveHandWidth = Math.max(minHandWidth, handWidth);

  return (
    <div className="relative w-full h-[400px] flex items-center justify-center select-none font-mono">
      {/* Target Marker Bounding Boxes (Чиста геометрія без заокруглень) */}
      <div className="absolute inset-0 flex items-center justify-center pointer-events-auto">
        <AnimatePresence>
          {cards.map((card, i) => {
            const transform = getCardTransform(i, card.id);
            const isDragged = transform.state === 'dragged';
            const isHovered = transform.state === 'hovered';

            const tickColor = isDragged
              ? 'border-emerald-400'
              : isHovered
              ? 'border-pink-400'
              : 'border-slate-400';

            const textColor = isDragged
              ? 'text-emerald-400'
              : isHovered
              ? 'text-pink-400'
              : 'text-slate-300';

            return (
              <motion.div
                key={card.id}
                data-card-id={card.id}
                onPanStart={() => onPanStart(card.id, i)}
                onPan={onPan}
                onPanEnd={onPanEnd}
                style={{
                  position: 'absolute',
                  width: cardWidth,
                  height: cardHeight,
                }}
                initial={false}
                animate={{
                  opacity: 1,
                  scale: transform.scale,
                  x: transform.x,
                  y: transform.y,
                  zIndex: transform.zIndex,
                }}
                exit={{ opacity: 0, scale: 0.7 }}
                transition={
                  isDragged
                    ? { duration: 0 }
                    : { type: 'spring', stiffness: 360, damping: 28 }
                }
                className="relative flex items-center justify-center cursor-grab active:cursor-grabbing touch-none select-none bg-slate-950 rounded-none overflow-hidden"
              >
                {/* Custom Card Image if uploaded with pixel art support */}
                {card.imageUrl ? (
                  <div className="absolute inset-0 overflow-hidden bg-slate-900 flex items-center justify-center">
                    <img
                      src={card.imageUrl}
                      alt={card.name || `Card ${i}`}
                      style={{
                        imageRendering: pixelArt ? 'pixelated' : 'auto',
                      }}
                      className={`w-full h-full object-cover select-none pointer-events-none ${
                        pixelArt ? '[image-rendering:pixelated]' : ''
                      }`}
                      referrerPolicy="no-referrer"
                    />
                    {/* Subtle Index Pill for uploaded card (only in layout details mode) */}
                    {showLayoutDetails && (
                      <div className="absolute top-1 left-1 px-1 py-0.5 bg-slate-950/85 border border-slate-700/80 text-[9px] font-mono font-semibold text-slate-200 pointer-events-none">
                        #{i}
                      </div>
                    )}
                  </div>
                ) : showLayoutDetails ? (
                  /* Center Index Number for wireframe layout mode */
                  <div className="absolute inset-0 flex items-center justify-center pointer-events-none">
                    <span className={`text-xl font-mono font-bold select-none ${textColor}`}>
                      {i}
                    </span>
                  </div>
                ) : (
                  /* Clean flat card representation without details */
                  <div className="absolute inset-0 p-2 flex flex-col justify-between bg-slate-900/60 pointer-events-none select-none">
                    <div className="flex justify-between items-start">
                      <span
                        className={`text-xs font-bold font-mono ${
                          card.color === 'red' ? 'text-red-400' : 'text-slate-300'
                        }`}
                      >
                        {card.rank}
                      </span>
                      <span
                        className={`text-xs ${
                          card.color === 'red' ? 'text-red-400' : 'text-slate-400'
                        }`}
                      >
                        {card.suit}
                      </span>
                    </div>
                    <div className="flex items-center justify-center">
                      <span
                        className={`text-2xl ${
                          card.color === 'red' ? 'text-red-500/70' : 'text-slate-500/70'
                        }`}
                      >
                        {card.suit}
                      </span>
                    </div>
                    <div className="flex justify-between items-end rotate-180">
                      <span
                        className={`text-xs font-bold font-mono ${
                          card.color === 'red' ? 'text-red-400' : 'text-slate-300'
                        }`}
                      >
                        {card.rank}
                      </span>
                      <span
                        className={`text-xs ${
                          card.color === 'red' ? 'text-red-400' : 'text-slate-400'
                        }`}
                      >
                        {card.suit}
                      </span>
                    </div>
                  </div>
                )}

                {/* Corner Tick Marks (only in layout details mode, strictly inside bounds) */}
                {showLayoutDetails && (
                  <>
                    <div className={`absolute top-0 left-0 w-3.5 h-3.5 border-t-2 border-l-2 ${tickColor} pointer-events-none z-10`} />
                    <div className={`absolute top-0 right-0 w-3.5 h-3.5 border-t-2 border-r-2 ${tickColor} pointer-events-none z-10`} />
                    <div className={`absolute bottom-0 left-0 w-3.5 h-3.5 border-b-2 border-l-2 ${tickColor} pointer-events-none z-10`} />
                    <div className={`absolute bottom-0 right-0 w-3.5 h-3.5 border-b-2 border-r-2 ${tickColor} pointer-events-none z-10`} />
                  </>
                )}
              </motion.div>
            );
          })}
        </AnimatePresence>
      </div>

      {/* Bottom Dimension Brackets (Only rendered when showLayoutDetails is true) */}
      {showLayoutDetails && (
        <div className="absolute inset-0 flex items-center justify-center pointer-events-none">
          {/* Row 1: Slot / Card brackets */}
          {hoverZones.map((zone) => {
            const isCard1x = zone.width >= cardWidth - 1;
            const bracketColor = isCard1x
              ? 'border-pink-500 text-pink-400'
              : 'border-cyan-400 text-cyan-300';

            return (
              <motion.div
                key={`bracket-${zone.slot}`}
                initial={false}
                animate={{
                  x: zone.centerX,
                  width: Math.max(4, zone.width),
                }}
                transition={{ type: 'spring', stiffness: 360, damping: 28 }}
                style={{
                  position: 'absolute',
                  top: 'calc(50% + 92px)',
                  height: 10,
                }}
                className="flex flex-col items-center"
              >
                <div
                  className={`w-full h-2 border-b-2 border-l-2 border-r-2 ${bracketColor}`}
                />
              </motion.div>
            );
          })}

          {/* Row 2: Actual Cards Span Bracket (Фактична ширина карток) */}
          {count > 0 && (
            <motion.div
              initial={false}
              animate={{
                x: totalCenterX,
                width: totalWidth,
              }}
              transition={{ type: 'spring', stiffness: 360, damping: 28 }}
              style={{
                position: 'absolute',
                top: 'calc(50% + 108px)',
              }}
              className="flex flex-col items-center"
            >
              <div className="w-full h-2.5 border-b-2 border-l-2 border-r-2 border-indigo-400/90" />
              <span className="mt-0.5 text-[11px] text-indigo-300 font-mono tracking-tight">
                {Math.round(totalWidth)}px
              </span>
            </motion.div>
          )}

          {/* Row 3: Configured Hand Width Bracket (Ширина руки) */}
          {count > 0 && (
            <motion.div
              initial={false}
              animate={{
                x: 0,
                width: effectiveHandWidth,
              }}
              transition={{ type: 'spring', stiffness: 360, damping: 28 }}
              style={{
                position: 'absolute',
                top: 'calc(50% + 144px)',
              }}
              className="flex flex-col items-center"
            >
              <div className="w-full h-2.5 border-b-2 border-l-2 border-r-2 border-teal-400/80" />
              <span className="mt-0.5 text-[10px] text-teal-300/90 font-mono tracking-tight whitespace-nowrap">
                hand width: {effectiveHandWidth}px
              </span>
            </motion.div>
          )}

          {/* Row 4: Minimum Hand Width Bracket (Мінімальна ширина руки) */}
          {count > 0 && (
            <motion.div
              initial={false}
              animate={{
                x: 0,
                width: minHandWidth,
              }}
              transition={{ type: 'spring', stiffness: 360, damping: 28 }}
              style={{
                position: 'absolute',
                top: 'calc(50% + 180px)',
              }}
              className="flex flex-col items-center"
            >
              <div className="w-full h-2.5 border-b-2 border-l-2 border-r-2 border-dashed border-amber-400/70" />
              <span className="mt-0.5 text-[10px] text-amber-300/90 font-mono tracking-tight whitespace-nowrap">
                min: {minHandWidth}px
              </span>
            </motion.div>
          )}
        </div>
      )}
    </div>
  );
};
