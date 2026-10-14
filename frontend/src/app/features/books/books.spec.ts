import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { Books } from './books';

// jsdom may not implement modal dialogs; a minimal stand-in keeps the open/close state.
if (!HTMLDialogElement.prototype.showModal) {
  HTMLDialogElement.prototype.showModal = function (this: HTMLDialogElement) {
    this.open = true;
  };
  HTMLDialogElement.prototype.close = function (this: HTMLDialogElement) {
    this.open = false;
  };
}

describe('Books', () => {
  let fixture: ComponentFixture<Books>;
  let root: HTMLElement;
  let http: HttpTestingController;

  const button = (name: string) =>
    Array.from(root.querySelectorAll<HTMLButtonElement>('button')).find(
      (b) => (b.getAttribute('aria-label') ?? b.textContent!.trim()) === name,
    )!;
  const click = (name: string) => {
    button(name).click();
    fixture.detectChanges();
  };
  const dialog = (titleId: string) =>
    root.querySelector<HTMLDialogElement>(`dialog[aria-labelledby="${titleId}"]`)!;
  const herbert = { id: 1, firstName: 'Frank', lastName: 'Herbert', gender: 'Male' };
  const austen = { id: 2, firstName: 'Jane', lastName: 'Austen', gender: 'Female' };

  const rows = () =>
    Array.from(root.querySelectorAll('tbody tr')).map((tr) =>
      Array.from(tr.querySelectorAll('td'))
        .slice(0, 3)
        .map((td) => td.textContent!.trim()),
    );
  const authorSelect = () => dialog('book-dialog-title').querySelector('select')!;
  const chooseAuthor = (name: string) => {
    const select = authorSelect();
    select.selectedIndex = Array.from(select.options).findIndex((o) => o.text.trim() === name);
    select.dispatchEvent(new Event('change'));
    fixture.detectChanges();
  };
  const typeName = (value: string) => {
    const input = dialog('book-dialog-title').querySelector('input')!;
    input.value = value;
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();
  };
  const submit = () => {
    dialog('book-dialog-title').querySelector('form')!.dispatchEvent(new Event('submit'));
    fixture.detectChanges();
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Books],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();
    fixture = TestBed.createComponent(Books);
    fixture.detectChanges();
    root = fixture.nativeElement;
    http = TestBed.inject(HttpTestingController);
    http.expectOne('/api/books').flush([
      { id: 1, name: 'Dune', author: herbert },
      { id: 2, name: 'Emma', author: austen },
    ]);
    http.expectOne('/api/authors').flush([austen, herbert]);
    fixture.detectChanges();
  });

  afterEach(() => http.verify());

  it('lists the books with their authors in a table', () => {
    expect(rows()).toEqual([
      ['1', 'Dune', 'Frank Herbert'],
      ['2', 'Emma', 'Jane Austen'],
    ]);
  });

  it('adds a book through the dialog', () => {
    click('+ Add');
    expect(dialog('book-dialog-title').open).toBe(true);

    expect(Array.from(authorSelect().options).map((o) => o.text.trim())).toEqual([
      'Select author',
      'Jane Austen',
      'Frank Herbert',
    ]);
    typeName('  Dune Messiah ');
    chooseAuthor('Frank Herbert');
    submit();
    const req = http.expectOne({ method: 'POST', url: '/api/books' });
    expect(req.request.body).toEqual({ name: 'Dune Messiah', authorId: 1 });
    req.flush({ id: 3, name: 'Dune Messiah', author: herbert });
    fixture.detectChanges();

    expect(dialog('book-dialog-title').open).toBe(false);
    expect(rows()).toEqual([
      ['3', 'Dune Messiah', 'Frank Herbert'],
      ['1', 'Dune', 'Frank Herbert'],
      ['2', 'Emma', 'Jane Austen'],
    ]);
  });

  it('does not submit a book without an author', () => {
    click('+ Add');
    typeName('Dune Messiah');
    submit();

    http.expectNone({ method: 'POST', url: '/api/books' });
    expect(dialog('book-dialog-title').textContent).toContain('Select an author.');
  });

  it('does not submit a blank name', () => {
    click('+ Add');
    typeName('   ');
    submit();

    http.expectNone({ method: 'POST', url: '/api/books' });
    expect(dialog('book-dialog-title').textContent).toContain('Enter a book name.');
  });

  it('edits a book with its current name and author prefilled and moves it to the top', () => {
    click('Edit Emma');
    const input = dialog('book-dialog-title').querySelector('input')!;
    expect(input.value).toBe('Emma');
    expect([input.selectionStart, input.selectionEnd]).toEqual([0, 'Emma'.length]);
    expect(authorSelect().selectedOptions[0].text.trim()).toBe('Jane Austen');

    typeName('Emma (illustrated)');
    chooseAuthor('Frank Herbert');
    submit();
    const req = http.expectOne({ method: 'PUT', url: '/api/books/2' });
    expect(req.request.body).toEqual({ name: 'Emma (illustrated)', authorId: 1 });
    req.flush({ id: 2, name: 'Emma (illustrated)', author: herbert });
    fixture.detectChanges();

    expect(rows()).toEqual([
      ['2', 'Emma (illustrated)', 'Frank Herbert'],
      ['1', 'Dune', 'Frank Herbert'],
    ]);
  });

  it('deletes a book after confirmation', () => {
    click('Delete Emma');
    expect(dialog('delete-dialog-title').open).toBe(true);
    expect(dialog('delete-dialog-title').textContent).toContain('Emma');

    const confirm = dialog('delete-dialog-title').querySelector<HTMLButtonElement>('.btn-danger')!;
    confirm.click();
    http.expectOne({ method: 'DELETE', url: '/api/books/2' }).flush(null);
    fixture.detectChanges();

    expect(rows()).toEqual([['1', 'Dune', 'Frank Herbert']]);
    expect(dialog('delete-dialog-title').open).toBe(false);
  });

  it('shows the server error and keeps the dialog open', () => {
    click('+ Add');
    typeName('Ulysses');
    chooseAuthor('Jane Austen');
    submit();
    http
      .expectOne({ method: 'POST', url: '/api/books' })
      .flush(
        { errors: { Name: ['The Name field is required.'] } },
        { status: 400, statusText: 'Bad Request' },
      );
    fixture.detectChanges();

    expect(dialog('book-dialog-title').open).toBe(true);
    expect(dialog('book-dialog-title').textContent).toContain('The Name field is required.');
  });
});
