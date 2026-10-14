import { Component, ElementRef, inject, signal, viewChild } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { notBlank } from '../../core/forms/not-blank.validator';
import { apiErrorMessage } from '../../core/http/api-error';
import { APP_PATHS } from '../../core/navigation';
import { Author, authorFullName } from '../authors/author.models';
import { AuthorsApi } from '../authors/authors-api';
import { BOOK_NAME_MAX_LENGTH, Book } from './book.models';
import { BooksApi } from './books-api';

@Component({
  selector: 'app-books',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './books.html',
  host: { class: 'flex w-full flex-col items-center gap-6' },
})
export class Books {
  private readonly booksApi = inject(BooksApi);
  private readonly authorsApi = inject(AuthorsApi);
  private readonly bookDialog = viewChild.required<ElementRef<HTMLDialogElement>>('bookDialog');
  private readonly deleteDialog = viewChild.required<ElementRef<HTMLDialogElement>>('deleteDialog');
  private readonly nameInput = viewChild.required<ElementRef<HTMLInputElement>>('nameInput');

  protected readonly nameMaxLength = BOOK_NAME_MAX_LENGTH;
  protected readonly authorsPath = APP_PATHS.authors;
  protected readonly fullName = authorFullName;
  protected readonly books = signal<Book[]>([]);
  /** Options of the author dropdown. */
  protected readonly authors = signal<Author[]>([]);
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
    // null shows the "Select author" placeholder until an author is chosen.
    authorId: new FormControl<number | null>(null, Validators.required),
  });
  protected readonly name = this.form.controls.name;
  protected readonly authorId = this.form.controls.authorId;

  constructor() {
    forkJoin({ books: this.booksApi.getAll(), authors: this.authorsApi.getAll() }).subscribe({
      next: ({ books, authors }) => {
        this.books.set(books);
        this.authors.set(authors);
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
    const authorId = this.authorId.value;
    if (this.form.invalid || authorId === null || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }
    const editing = this.editing();
    const request = { name: this.name.value.trim(), authorId };
    const save$ = editing
      ? this.booksApi.update(editing.id, request)
      : this.booksApi.create(request);

    this.saving.set(true);
    this.dialogError.set('');
    save$.subscribe({
      next: (saved) => {
        // Same order as the API: the book just added or edited goes to the top.
        this.books.update((books) => [saved, ...books.filter((b) => b.id !== saved.id)]);
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
    this.form.reset({ name: book?.name ?? '', authorId: book?.author.id ?? null });
    this.bookDialog().nativeElement.showModal();
    // When editing, typing replaces the current name instead of appending to it.
    this.nameInput().nativeElement.select();
  }
}
