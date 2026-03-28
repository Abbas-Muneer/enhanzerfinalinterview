import { Routes } from '@angular/router';

export const appRoutes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'purchase'
  },
  {
    path: 'purchase',
    loadComponent: () =>
      import('./modules/purchase/pages/purchase-bill/purchase-bill.component').then((module) => module.PurchaseBillComponent)
  },
  {
    path: '**',
    redirectTo: 'purchase'
  }
];
