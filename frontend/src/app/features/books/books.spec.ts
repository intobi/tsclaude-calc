import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
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
  const rows = () =>
    Array.from(root.querySelectorAll('tbody tr')).map((tr) =>
      Array.from(tr.querySelectorAll('td'))
        .slice(0, 2)
        .map((td) => td.textContent!.trim()),
    );
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
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    fixture = TestBed.createComponent(Books);
    fixture.detectChanges();
    root = fixture.nativeElement;
    http = TestBed.inject(HttpTestingController);
    http.expectOne('/api/books').flush([
      { id: 1, name: 'Dune' },
      { id: 2, name: 'Emma' },
    ]);
    fixture.detectChanges();
  });

  afterEach(() => http.verify());

  it('lists the books in a table', () => {
    expect(rows()).toEqual([
      ['1', 'Dune'],
      ['2', 'Emma'],
    ]);
  });

  it('adds a book through the dialog', () => {
    click('+ Add');
    expect(dialog('book-dialog-title').open).toBe(true);

    typeName('  Ulysses ');
    submit();
    const req = http.expectOne({ method: 'POST', url: '/api/books' });
    expect(req.request.body).toEqual({ name: 'Ulysses' });
    req.flush({ id: 3, name: 'Ulysses' });
    fixture.detectChanges();

    expect(dialog('book-dialog-title').open).toBe(false);
    expect(rows().at(-1)).toEqual(['3', 'Ulysses']);
  });

  it('does not submit a blank name', () => {
    click('+ Add');
    typeName('   ');
    submit();

    http.expectNone({ method: 'POST', url: '/api/books' });
    expect(dialog('book-dialog-title').textContent).toContain('Enter a book name.');
  });

  it('edits a book with its current name prefilled', () => {
    click('Edit Dune');
    const input = dialog('book-dialog-title').querySelector('input')!;
    expect(input.value).toBe('Dune');
    expect([input.selectionStart, input.selectionEnd]).toEqual([0, 'Dune'.length]);

    typeName('Dune Messiah');
    submit();
    const req = http.expectOne({ method: 'PUT', url: '/api/books/1' });
    req.flush({ id: 1, name: 'Dune Messiah' });
    fixture.detectChanges();

    expect(rows()[0]).toEqual(['1', 'Dune Messiah']);
  });

  it('deletes a book after confirmation', () => {
    click('Delete Emma');
    expect(dialog('delete-dialog-title').open).toBe(true);
    expect(dialog('delete-dialog-title').textContent).toContain('Emma');

    const confirm = dialog('delete-dialog-title').querySelector<HTMLButtonElement>('.btn-danger')!;
    confirm.click();
    http.expectOne({ method: 'DELETE', url: '/api/books/2' }).flush(null);
    fixture.detectChanges();

    expect(rows()).toEqual([['1', 'Dune']]);
    expect(dialog('delete-dialog-title').open).toBe(false);
  });

  it('shows the server error and keeps the dialog open', () => {
    click('+ Add');
    typeName('Ulysses');
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
