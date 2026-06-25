import { HttpErrorResponse } from '@angular/common/http';

/**
 * Bir HTTP hatasından kullanıcıya gösterilebilecek en anlamlı mesajı çıkarır.
 *
 * Backend'in döndürdüğü farklı gövde biçimlerini destekler:
 *  - Düz metin gövdesi:            "Bu ders kodu zaten kullanımda."
 *  - { message } nesnesi:          { message: "..." }
 *  - ValidationProblemDetails:     { errors: { Credit: ["Kredi 1 ile 10..."] }, title: "..." }
 *
 * Hiçbiri bulunamazsa `fallback` döner.
 */
export function extractErrorMessage(err: unknown, fallback: string): string {
  if (!(err instanceof HttpErrorResponse)) return fallback;

  // Sunucuya hiç ulaşılamadı (CORS / ağ / kapalı API)
  if (err.status === 0) return 'Sunucuya ulaşılamadı. Bağlantınızı kontrol edip tekrar deneyin.';

  const body = err.error;

  // Düz metin gövdesi
  if (typeof body === 'string' && body.trim()) return body.trim();

  if (body && typeof body === 'object') {
    // { message: "..." } biçimi
    const message = (body as { message?: unknown }).message;
    if (typeof message === 'string' && message.trim()) return message.trim();

    // ASP.NET ValidationProblemDetails: { errors: { Field: ["mesaj", ...] } }
    const errors = (body as { errors?: Record<string, string[]> }).errors;
    if (errors && typeof errors === 'object') {
      const messages = Object.values(errors).flat().filter((m) => typeof m === 'string' && m.trim());
      if (messages.length) return messages.join(' ');
    }

    // ProblemDetails başlığı
    const title = (body as { title?: unknown }).title;
    if (typeof title === 'string' && title.trim()) return title.trim();
  }

  return fallback;
}

/**
 * `responseType: 'blob'` ile yapılan isteklerde (örn. PDF indirme) hata gövdesi de
 * Blob olarak gelir; bu yüzden önce metne çevrilip JSON'a ayrıştırılır, ardından
 * standart {@link extractErrorMessage} mantığına devredilir.
 */
export async function extractBlobErrorMessage(err: unknown, fallback: string): Promise<string> {
  if (err instanceof HttpErrorResponse && err.error instanceof Blob) {
    try {
      const text = await err.error.text();
      const parsed = text ? JSON.parse(text) : null;
      const rebuilt = new HttpErrorResponse({
        error: parsed,
        status: err.status,
        statusText: err.statusText,
        url: err.url ?? undefined,
      });
      return extractErrorMessage(rebuilt, fallback);
    } catch {
      // Gövde JSON değilse (boş yanıt, HTML hata sayfası vb.) genel mesaja düş
      return extractErrorMessage(err, fallback);
    }
  }

  return extractErrorMessage(err, fallback);
}
