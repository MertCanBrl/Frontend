import { Component, input } from '@angular/core';

/**
 * Paylaşılan kart wrapper'ı (Faz 3 / Yol B).
 *
 * İçerik ng-content ile birebir projeksiyon edilir; çocuk sınıf stilleri
 * (.info-card-title, .card-top vb.) tüketici component CSS'inde kalır ve
 * projeksiyon edilen içeriğe uygulanmaya devam eder. Yalnızca KUTU stili
 * (border/padding/radius/bg/hover) buraya taşınır. Varsayılan (Emulated)
 * encapsulation: :host kutuyu hedefler, çocuk stilleri feature'da kalır.
 *
 * Kullanım:
 *   <app-ui-card>…</app-ui-card>                 (info — varsayılan)
 *   <app-ui-card variant="form">…</app-ui-card>  (gold kenarlık)
 *   <app-ui-card variant="course">…</app-ui-card>(xl radius, hover-lift)
 */
@Component({
  selector: 'app-ui-card',
  standalone: true,
  template: '<ng-content></ng-content>',
  styleUrl: './ui-card.css',
  host: {
    '[class.ui-card--info]': "variant() === 'info'",
    '[class.ui-card--form]': "variant() === 'form'",
    '[class.ui-card--course]': "variant() === 'course'",
  },
})
export class UiCard {
  variant = input<'info' | 'form' | 'course'>('info');
}
