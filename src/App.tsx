import React from 'react';
import { CardsRow } from './components/CardsRow';

export function App() {
  return (
    <div className="relative min-h-screen bg-slate-950 text-slate-100 flex items-center justify-center">
      <main className="w-full flex items-center justify-center">
        <CardsRow />
      </main>
    </div>
  );
}

export default App;
