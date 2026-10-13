/** Longest allowed book name; matches the backend validation and database column. */
export const BOOK_NAME_MAX_LENGTH = 200;

export interface Book {
  id: number;
  name: string;
}

export interface BookRequest {
  name: string;
}
