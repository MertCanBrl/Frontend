import { Directive, input } from '@angular/core';

/**
 * Paylaşılan buton directive'i (Faz 3 / Yol B).
 *
 * Native <button> üzerine attribute olarak uygulanır; type, (click), [disabled],
 * routerLink ve form submit davranışı tamamen native korunur (forwarding yok).
 * Stiller global `ui-button.css` partial'ında, yalnızca bu directive'in emit ettiği
 * `.ui-btn*` sınıflarına bağlı yaşar — tüketici bu sınıfları elle yazmaz.
 *
 * Kullanım:
 *   <button uiBtn>Kaydet</button>                       (primary, md)
 *   <button uiBtn variant="ghost">İptal</button>
 *   <button uiBtn variant="secondary" size="sm">…</button>
 *   <button uiBtn variant="icon" tone="danger">…</button>
 *   <button uiBtn size="block" type="submit">Giriş</button>
 */
@Directive({
  selector: 'button[uiBtn]',
  standalone: true,
  host: {
    class: 'ui-btn',
    '[class.ui-btn--primary]': "variant() === 'primary'",
    '[class.ui-btn--ghost]': "variant() === 'ghost'",
    '[class.ui-btn--secondary]': "variant() === 'secondary'",
    '[class.ui-btn--icon]': "variant() === 'icon'",
    '[class.ui-btn--sm]': "size() === 'sm'",
    '[class.ui-btn--block]': "size() === 'block'",
    '[class.ui-btn--danger]': "tone() === 'danger'",
    '[class.ui-btn--accent]': "tone() === 'accent'",
  },
})
export class UiBtn {
  /** Görsel aile. icon = 30×30 ikon butonu. */
  variant = input<'primary' | 'ghost' | 'secondary' | 'icon'>('primary');
  /** Boyut. sm = kompakt, block = tam genişlik CTA (login). */
  size = input<'md' | 'sm' | 'block'>('md');
  /** Vurgu rengi — yalnızca icon variant'ında anlamlı (danger/accent hover). */
  tone = input<'default' | 'danger' | 'accent'>('default');
}
