import { Operation } from '../../core/calculator/operations';

export interface HistoryItem {
  id: number;
  operation: Operation;
  operandA: number;
  operandB: number;
  result: number;
  createdAtUtc: string;
}
