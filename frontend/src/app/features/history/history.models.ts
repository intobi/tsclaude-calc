export type HistoryOperation = 'add' | 'subtract' | 'multiply' | 'divide';

export interface HistoryItem {
  id: number;
  operation: HistoryOperation;
  operandA: number;
  operandB: number;
  result: number;
  createdAtUtc: string;
}
