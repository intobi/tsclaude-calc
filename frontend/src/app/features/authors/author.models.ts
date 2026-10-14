/** Longest allowed first or last name; matches the backend validation and database columns. */
export const AUTHOR_NAME_MAX_LENGTH = 100;

export type Gender = 'Male' | 'Female';

/** Options of the gender dropdown. */
export const GENDERS: readonly { value: Gender; label: string }[] = [
  { value: 'Male', label: 'Male' },
  { value: 'Female', label: 'Female' },
];

export interface Author {
  id: number;
  firstName: string;
  lastName: string;
  gender: Gender;
  /** ISO date, e.g. "1775-12-16". */
  dateOfBirth: string;
}

export type AuthorRequest = Omit<Author, 'id'>;

/** Today's local date as an ISO date ("yyyy-MM-dd"), the format of `<input type="date">`. */
export function todayIsoDate(now = new Date()): string {
  const pad = (n: number) => String(n).padStart(2, '0');
  return `${now.getFullYear()}-${pad(now.getMonth() + 1)}-${pad(now.getDate())}`;
}

export const authorFullName = (author: Pick<Author, 'firstName' | 'lastName'>) =>
  `${author.firstName} ${author.lastName}`;

/** Same order as the API: by last name, then first name, then id. */
export function compareAuthors(a: Author, b: Author): number {
  const byText = (x: string, y: string) => (x < y ? -1 : x > y ? 1 : 0);
  return byText(a.lastName, b.lastName) || byText(a.firstName, b.firstName) || a.id - b.id;
}
