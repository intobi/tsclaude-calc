import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Book, BookRequest } from './book.models';

const BOOKS_URL = '/api/books';

@Injectable({ providedIn: 'root' })
export class BooksApi {
  private readonly http = inject(HttpClient);

  getAll(): Observable<Book[]> {
    return this.http.get<Book[]>(BOOKS_URL);
  }

  create(request: BookRequest): Observable<Book> {
    return this.http.post<Book>(BOOKS_URL, request);
  }

  update(id: number, request: BookRequest): Observable<Book> {
    return this.http.put<Book>(`${BOOKS_URL}/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${BOOKS_URL}/${id}`);
  }
}
