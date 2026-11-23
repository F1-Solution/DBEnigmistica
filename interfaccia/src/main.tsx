import { createRoot } from 'react-dom/client';
import { App } from './App';
import './stile.css';

const radice = document.getElementById('radice');
if (!radice) throw new Error('Elemento radice mancante in index.html.');
createRoot(radice).render(<App />);
