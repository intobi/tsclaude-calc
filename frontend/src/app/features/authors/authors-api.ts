import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Author, AuthorRequest } from './author.models';

const AUTHORS_URL = '/api/authors';

@Injectable({ providedIn: 'root' })
export class AuthorsApi {
  private readonly http = inject(HttpClient);

  getAll(): Observable<Author[]> {
    return this.http.get<Author[]>(AUTHORS_URL);
  }

  create(request: AuthorRequest): Observable<Author> {
    return this.http.post<Author>(AUTHORS_URL, request);
  }

  update(id: number, request: AuthorRequest): Observable<Author> {
    return this.http.put<Author>(`${AUTHORS_URL}/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${AUTHORS_URL}/${id}`);
  }
}
