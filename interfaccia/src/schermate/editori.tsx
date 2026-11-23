import type { ComponentType, ReactNode } from 'react';
import type { Api } from '../comune/api';
import type { Candidato } from '../comune/tipi';
import { RevisioneGriglie } from './griglie/RevisioneGriglie';
import { RevisioneRebus } from './rebus/RevisioneRebus';
import { EditoreCampi } from './parole/EditoreCampi';
import { EditoreCruciverba } from './parole/EditoreCruciverba';

/** Ogni editor riceve il candidato, i servizi di salvataggio e, come figli, cronometro ed esito da mostrare in fondo. */
export interface EditorProps {
  candidato: Candidato;
  servizi: Api;
  children?: ReactNode;
}

/** Editor per Tipologia: parole (A) e griglie e rebus (C). */
export const EDITORI: Readonly<Record<string, ComponentType<EditorProps>>> = {
  'crittografia-breve': EditoreCampi,
  anagramma: EditoreCampi,
  'cruciverba-classico': EditoreCruciverba,
  sudoku: RevisioneGriglie,
  futoshiki: RevisioneGriglie,
  nonogramma: RevisioneGriglie,
  rebus: RevisioneRebus,
};
