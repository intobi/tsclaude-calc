import { Component, ElementRef, inject, signal, viewChild } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import {
  AbstractControl,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { BOOK_NAME_MAX_LENGTH, Book } from './book.models';
import { BooksApi } from './books-api';

/** Rejects names made only of whitespace (the backend does the same). */
const notBlank = (control: AbstractControl<string>): ValidationErrors | null =>
  control.value.trim() ? null : { blank: true };

/** The API's own message: `{ error }` for business errors, `{ errors }` for validation problems. */
function apiErrorMessage(response: HttpErrorResponse, fallback: string): string {
  const body = response.error;
  const validation = body?.errors ? Object.values<string[]>(body.errors)[0]?.[0] : undefined;
  return body?.error ?? validation ?? fallback;
}

@Component({
  selector: 'app-books',
  imports: [ReactiveFormsModule],
  templateUrl: './books.html',
  host: { class: 'flex w-full flex-col items-center gap-6' },
})
export class Books {
  private readonly booksApi = inject(BooksApi);
  private readonly bookDialog = viewChild.required<ElementRef<HTMLDialogElement>>('bookDialog');
  private readonly deleteDialog = viewChild.required<ElementRef<HTMLDialogElement>>('deleteDialog');
  private readonly nameInput = viewChild.required<ElementRef<HTMLInputElement>>('nameInput');

  protected readonly nameMaxLength = BOOK_NAME_MAX_LENGTH;
  protected readonly books = signal<Book[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal('');
  /** The book being edited in the dialog; `null` while adding a new one. */
  protected readonly editing = signal<Book | null>(null);
  protected readonly deleting = signal<Book | null>(null);
  protected readonly dialogError = signal('');
  protected readonly saving = signal(false);
  protected readonly form = new FormGroup({
    name: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(BOOK_NAME_MAX_LENGTH), notBlank],
    }),
  });
  protected readonly name = this.form.controls.name;

  constructor() {
    this.booksApi.getAll().subscribe({
      next: (books) => {
        this.books.set(books);
        this.loading.set(false);
      },
      error: (e: HttpErrorResponse) => {
        this.error.set(apiErrorMessage(e, 'Could not load books'));
        this.loading.set(false);
      },
    });
  }

  protected openAdd() {
    this.openBookDialog(null);
  }

  protected openEdit(book: Book) {
    this.openBookDialog(book);
  }

  protected save() {
    if (this.form.invalid || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }
    const editing = this.editing();
    const request = { name: this.name.value.trim() };
    const save$ = editing
      ? this.booksApi.update(editing.id, request)
      : this.booksApi.create(request);

    this.saving.set(true);
    this.dialogError.set('');
    save$.subscribe({
      next: (saved) => {
        this.books.update((books) =>
          editing ? books.map((b) => (b.id === saved.id ? saved : b)) : [...books, saved],
        );
        this.saving.set(false);
        this.bookDialog().nativeElement.close();
      },
      error: (e: HttpErrorResponse) => {
        this.saving.set(false);
        this.dialogError.set(apiErrorMessage(e, 'Could not save the book'));
      },
    });
  }

  protected confirmDelete(book: Book) {
    this.deleting.set(book);
    this.dialogError.set('');
    this.deleteDialog().nativeElement.showModal();
  }

  protected delete() {
    const book = this.deleting();
    if (!book || this.saving()) return;

    this.saving.set(true);
    this.dialogError.set('');
    this.booksApi.delete(book.id).subscribe({
      next: () => {
        this.books.update((books) => books.filter((b) => b.id !== book.id));
        this.saving.set(false);
        this.deleteDialog().nativeElement.close();
      },
      error: (e: HttpErrorResponse) => {
        this.saving.set(false);
        this.dialogError.set(apiErrorMessage(e, 'Could not delete the book'));
      },
    });
  }

  protected closeDialog(dialog: HTMLDialogElement) {
    dialog.close();
  }

  private openBookDialog(book: Book | null) {
    this.editing.set(book);
    this.dialogError.set('');
    this.form.reset({ name: book?.name ?? '' });
    this.bookDialog().nativeElement.showModal();
    // When editing, typing replaces the current name instead of appending to it.
    this.nameInput().nativeElement.select();
  }
}
