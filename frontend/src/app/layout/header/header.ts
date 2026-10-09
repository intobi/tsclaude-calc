import { Component, ElementRef, inject, signal } from '@angular/core';
import { NgTemplateOutlet } from '@angular/common';
import { RouterLink } from '@angular/router';

interface NavLink {
  label: string;
  href: string;
}

interface NavItem {
  label: string;
  href?: string;
  children?: NavLink[];
}

const SITE = 'https://intobi.com';

/** Burger icon lines; when the menu is open they morph into a cross. */
const BURGER_LINES = [
  'group-aria-expanded:translate-y-1.75 group-aria-expanded:rotate-45',
  'group-aria-expanded:opacity-0',
  'group-aria-expanded:-translate-y-1.75 group-aria-expanded:-rotate-45',
];

const NAV_ITEMS: NavItem[] = [
  {
    label: 'Company',
    children: [
      { label: 'About Us', href: `${SITE}/about-us/` },
      { label: 'Careers', href: `${SITE}/careers/` },
    ],
  },
  {
    label: 'Services',
    children: [
      { label: 'All services', href: `${SITE}/services/` },
      { label: 'Mobile development', href: `${SITE}/services/mobile-app-development-company/` },
      { label: 'Web development', href: `${SITE}/services/web-development-services/` },
      { label: 'UI/UX design', href: `${SITE}/services/ux-ui-design-and-consulting-services/` },
      { label: 'MVP development', href: `${SITE}/services/mvp-development-company/` },
      { label: 'SaaS development', href: `${SITE}/services/saas-development-company/` },
      {
        label: 'Custom software development',
        href: `${SITE}/services/custom-software-development-services/`,
      },
      {
        label: 'Custom CRM development',
        href: `${SITE}/services/custom-crm-development-services/`,
      },
      { label: 'ERP development', href: `${SITE}/services/erp-development-company/` },
      {
        label: 'Supply chain software',
        href: `${SITE}/services/supply-chain-software-development/`,
      },
    ],
  },
  {
    label: 'Industries',
    children: [
      { label: 'Construction', href: `${SITE}/services/construction-development-services/` },
      { label: 'Manufacturing', href: `${SITE}/services/manufacturing-it-services/` },
      { label: 'Logistics', href: `${SITE}/services/logistics-software-development-company/` },
      { label: 'Education', href: `${SITE}/services/education-app-development-services/` },
      {
        label: 'Real Estate',
        href: `${SITE}/services/real-estate-software-development-company/`,
      },
      { label: 'Healthcare', href: `${SITE}/services/healthcare-software-development-services/` },
      { label: 'Entertainment', href: `${SITE}/services/entertainment-software-development/` },
      { label: 'Telecom', href: `${SITE}/services/telecom-software-development/` },
    ],
  },
  { label: 'Portfolio', href: `${SITE}/cases/` },
  { label: 'Tech Stack', href: `${SITE}/technology/` },
  { label: 'Blog', href: `${SITE}/blog/` },
];

@Component({
  selector: 'app-header',
  imports: [NgTemplateOutlet, RouterLink],
  templateUrl: './header.html',
  host: {
    class: 'block w-full p-4',
    '(document:click)': 'onDocumentClick($event)',
    '(document:keydown.escape)': 'closeAll()',
  },
})
export class Header {
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  protected readonly items = NAV_ITEMS;
  protected readonly burgerLines = BURGER_LINES;
  protected readonly contactUrl = `${SITE}/contact/`;
  protected readonly openMenu = signal<string | null>(null);
  protected readonly mobileOpen = signal(false);

  protected menuId(item: NavItem): string {
    return `submenu-${item.label.toLowerCase().replace(/\s+/g, '-')}`;
  }

  protected toggleMenu(label: string) {
    this.openMenu.update((open) => (open === label ? null : label));
  }

  protected toggleMobile() {
    this.mobileOpen.update((open) => !open);
    this.openMenu.set(null);
  }

  protected closeAll() {
    this.openMenu.set(null);
    this.mobileOpen.set(false);
  }

  protected onDocumentClick(event: MouseEvent) {
    if (!this.host.nativeElement.contains(event.target as Node)) {
      this.closeAll();
    }
  }
}
