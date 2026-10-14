import { Component, ElementRef, inject, signal, viewChild } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { DatePipe } from '@angular/common';
import {
  AbstractControl,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { notBlank } from '../../core/forms/not-blank.validator';
import { apiErrorMessage } from '../../core/http/api-error';
import {
  AUTHOR_NAME_MAX_LENGTH,
  Author,
  GENDERS,
  Gender,
  authorFullName,
  compareAuthors,
  todayIsoDate,
} from './author.models';
import { AuthorsApi } from './authors-api';

const nameControl = () =>
  new FormControl('', {
    nonNullable: true,
    validators: [Validators.required, Validators.maxLength(AUTHOR_NAME_MAX_LENGTH), notBlank],
  });

/** Rejects dates after today (ISO strings compare in date order). */
const notInFuture = (control: AbstractControl<string>): ValidationErrors | null =>
  control.value && control.value > todayIsoDate() ? { future: true } : null;

@Component({
  selector: 'app-authors',
  imports: [ReactiveFormsModule, DatePipe],
  templateUrl: './authors.html',
  host: { class: 'flex w-full flex-col items-center gap-6' },
})
export class Authors {
  private readonly authorsApi = inject(AuthorsApi);
  private readonly authorDialog = viewChild.required<ElementRef<HTMLDialogElement>>('authorDialog');
  private readonly deleteDialog = viewChild.required<ElementRef<HTMLDialogElement>>('deleteDialog');
  private readonly firstNameInput =
    viewChild.required<ElementRef<HTMLInputElement>>('firstNameInput');

  protected readonly nameMaxLength = AUTHOR_NAME_MAX_LENGTH;
  protected readonly genders = GENDERS;
  /** Latest selectable date of birth. */
  protected readonly today = todayIsoDate();
  protected readonly fullName = authorFullName;
  protected readonly authors = signal<Author[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal('');
  /** The author being edited in the dialog; `null` while adding a new one. */
  protected readonly editing = signal<Author | null>(null);
  protected readonly deleting = signal<Author | null>(null);
  protected readonly dialogError = signal('');
  protected readonly saving = signal(false);
  protected readonly form = new FormGroup({
    firstName: nameControl(),
    lastName: nameControl(),
    // '' shows the "Select gender" placeholder until a value is chosen.
    gender: new FormControl<Gender | ''>('', {
      nonNullable: true,
      validators: Validators.required,
    }),
    dateOfBirth: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, notInFuture],
    }),
  });
  protected readonly firstName = this.form.controls.firstName;
  protected readonly lastName = this.form.controls.lastName;
  protected readonly gender = this.form.controls.gender;
  protected readonly dateOfBirth = this.form.controls.dateOfBirth;

  constructor() {
    this.authorsApi.getAll().subscribe({
      next: (authors) => {
        this.authors.set(authors);
        this.loading.set(false);
      },
      error: (e: HttpErrorResponse) => {
        this.error.set(apiErrorMessage(e, 'Could not load authors'));
        this.loading.set(false);
      },
    });
  }

  protected genderLabel(gender: Gender): string {
    return GENDERS.find((g) => g.value === gender)?.label ?? gender;
  }

  protected openAdd() {
    this.openAuthorDialog(null);
  }

  protected openEdit(author: Author) {
    this.openAuthorDialog(author);
  }

  protected save() {
    const gender = this.gender.value;
    if (this.form.invalid || !gender || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }
    const editing = this.editing();
    const request = {
      firstName: this.firstName.value.trim(),
      lastName: this.lastName.value.trim(),
      gender,
      dateOfBirth: this.dateOfBirth.value,
    };
    const save$ = editing
      ? this.authorsApi.update(editing.id, request)
      : this.authorsApi.create(request);

    this.saving.set(true);
    this.dialogError.set('');
    save$.subscribe({
      next: (saved) => {
        this.authors.update((authors) =>
          [...authors.filter((a) => a.id !== saved.id), saved].sort(compareAuthors),
        );
        this.saving.set(false);
        this.authorDialog().nativeElement.close();
      },
      error: (e: HttpErrorResponse) => {
        this.saving.set(false);
        this.dialogError.set(apiErrorMessage(e, 'Could not save the author'));
      },
    });
  }

  protected confirmDelete(author: Author) {
    this.deleting.set(author);
    this.dialogError.set('');
    this.deleteDialog().nativeElement.showModal();
  }

  protected delete() {
    const author = this.deleting();
    if (!author || this.saving()) return;

    this.saving.set(true);
    this.dialogError.set('');
    this.authorsApi.delete(author.id).subscribe({
      next: () => {
        this.authors.update((authors) => authors.filter((a) => a.id !== author.id));
        this.saving.set(false);
        this.deleteDialog().nativeElement.close();
      },
      error: (e: HttpErrorResponse) => {
        // 409 when the author still has books; the API explains what to do.
        this.saving.set(false);
        this.dialogError.set(apiErrorMessage(e, 'Could not delete the author'));
      },
    });
  }

  protected closeDialog(dialog: HTMLDialogElement) {
    dialog.close();
  }

  private openAuthorDialog(author: Author | null) {
    this.editing.set(author);
    this.dialogError.set('');
    this.form.reset({
      firstName: author?.firstName ?? '',
      lastName: author?.lastName ?? '',
      gender: author?.gender ?? '',
      dateOfBirth: author?.dateOfBirth ?? '',
    });
    this.authorDialog().nativeElement.showModal();
    // When editing, typing replaces the current name instead of appending to it.
    this.firstNameInput().nativeElement.select();
  }
}
