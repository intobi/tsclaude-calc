import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Authors } from './authors';

// jsdom may not implement modal dialogs; a minimal stand-in keeps the open/close state.
if (!HTMLDialogElement.prototype.showModal) {
  HTMLDialogElement.prototype.showModal = function (this: HTMLDialogElement) {
    this.open = true;
  };
  HTMLDialogElement.prototype.close = function (this: HTMLDialogElement) {
    this.open = false;
  };
}

describe('Authors', () => {
  let fixture: ComponentFixture<Authors>;
  let root: HTMLElement;
  let http: HttpTestingController;

  const austen = { id: 2, firstName: 'Jane', lastName: 'Austen', gender: 'Female' };
  const herbert = { id: 1, firstName: 'Frank', lastName: 'Herbert', gender: 'Male' };

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
  const authorDialog = () => dialog('author-dialog-title');
  const rows = () =>
    Array.from(root.querySelectorAll('tbody tr')).map((tr) =>
      Array.from(tr.querySelectorAll('td'))
        .slice(0, 4)
        .map((td) => td.textContent!.trim()),
    );
  const field = (label: string) =>
    Array.from(authorDialog().querySelectorAll('label'))
      .find((l) => l.textContent!.trim().startsWith(label))!
      .querySelector<HTMLInputElement | HTMLSelectElement>('input, select')!;
  const type = (label: string, value: string) => {
    const input = field(label);
    input.value = value;
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();
  };
  const chooseGender = (label: string) => {
    const select = field('Gender') as HTMLSelectElement;
    select.selectedIndex = Array.from(select.options).findIndex((o) => o.text.trim() === label);
    select.dispatchEvent(new Event('change'));
    fixture.detectChanges();
  };
  const submit = () => {
    authorDialog().querySelector('form')!.dispatchEvent(new Event('submit'));
    fixture.detectChanges();
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Authors],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    fixture = TestBed.createComponent(Authors);
    fixture.detectChanges();
    root = fixture.nativeElement;
    http = TestBed.inject(HttpTestingController);
    http.expectOne('/api/authors').flush([austen, herbert]);
    fixture.detectChanges();
  });

  afterEach(() => http.verify());

  it('lists the authors in a table', () => {
    expect(rows()).toEqual([
      ['2', 'Jane', 'Austen', 'Female'],
      ['1', 'Frank', 'Herbert', 'Male'],
    ]);
  });

  it('adds an author through the dialog and keeps the list sorted by last name', () => {
    click('+ Add');
    expect(authorDialog().open).toBe(true);
    expect(
      Array.from((field('Gender') as HTMLSelectElement).options).map((o) => o.text.trim()),
    ).toEqual(['Select gender', 'Male', 'Female']);

    type('First name', ' Leo ');
    type('Last name', ' Tolstoy ');
    chooseGender('Male');
    submit();
    const req = http.expectOne({ method: 'POST', url: '/api/authors' });
    expect(req.request.body).toEqual({ firstName: 'Leo', lastName: 'Tolstoy', gender: 'Male' });
    req.flush({ id: 3, firstName: 'Leo', lastName: 'Tolstoy', gender: 'Male' });
    fixture.detectChanges();

    expect(authorDialog().open).toBe(false);
    expect(rows().map((r) => r[2])).toEqual(['Austen', 'Herbert', 'Tolstoy']);
  });

  it('requires both names and a gender', () => {
    click('+ Add');
    type('First name', '   ');
    submit();

    http.expectNone({ method: 'POST', url: '/api/authors' });
    const text = authorDialog().textContent;
    expect(text).toContain('Enter a first name.');
    expect(text).toContain('Enter a last name.');
    expect(text).toContain('Select a gender.');
  });

  it('edits an author with the current values prefilled', () => {
    click('Edit Frank Herbert');
    expect(field('First name').value).toBe('Frank');
    expect(field('Last name').value).toBe('Herbert');
    expect((field('Gender') as HTMLSelectElement).selectedOptions[0].text.trim()).toBe('Male');

    type('First name', 'Brian');
    submit();
    const req = http.expectOne({ method: 'PUT', url: '/api/authors/1' });
    expect(req.request.body).toEqual({ firstName: 'Brian', lastName: 'Herbert', gender: 'Male' });
    req.flush({ ...herbert, firstName: 'Brian' });
    fixture.detectChanges();

    expect(rows()).toEqual([
      ['2', 'Jane', 'Austen', 'Female'],
      ['1', 'Brian', 'Herbert', 'Male'],
    ]);
  });

  it('deletes an author after confirmation', () => {
    click('Delete Jane Austen');
    const confirmDialog = dialog('delete-author-dialog-title');
    expect(confirmDialog.open).toBe(true);
    expect(confirmDialog.textContent).toContain('Jane Austen');

    confirmDialog.querySelector<HTMLButtonElement>('.btn-danger')!.click();
    http.expectOne({ method: 'DELETE', url: '/api/authors/2' }).flush(null);
    fixture.detectChanges();

    expect(rows()).toEqual([['1', 'Frank', 'Herbert', 'Male']]);
    expect(confirmDialog.open).toBe(false);
  });

  it('explains why an author with books cannot be deleted', () => {
    click('Delete Frank Herbert');
    const confirmDialog = dialog('delete-author-dialog-title');
    confirmDialog.querySelector<HTMLButtonElement>('.btn-danger')!.click();
    http
      .expectOne({ method: 'DELETE', url: '/api/authors/1' })
      .flush(
        { error: 'This author has books. Delete or reassign them first.' },
        { status: 409, statusText: 'Conflict' },
      );
    fixture.detectChanges();

    expect(confirmDialog.open).toBe(true);
    expect(confirmDialog.textContent).toContain('This author has books.');
    expect(rows()).toHaveLength(2);
  });
});
