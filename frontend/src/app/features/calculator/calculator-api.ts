import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { Operation } from '../../core/calculator/operations';

interface CalculationResponse {
  result: number;
}

@Injectable({ providedIn: 'root' })
export class CalculatorApi {
  private readonly http = inject(HttpClient);

  /** Calculates on the server, which also records the calculation in the user's history. */
  calculate(a: number, b: number, operation: Operation): Observable<number> {
    return this.http
      .post<CalculationResponse>('/api/calculate', { a, b, operation })
      .pipe(map(({ result }) => result));
  }
}
