# Video → Metin + Anahtar Kare MCP Projesi — Yol Haritası

**Hedef:** Instagram ve genel videoların sesini metne dönüştüren, belirgin sahne değişimlerindeki kareleri görsel olarak dosyalayan bir MCP sunucusu yapmak. Böylece Claude gibi yapay zekâlar bir videoyu "izleyebilir": transkript + anahtar kareler = videonun özü.

**İyi haber:** Bu iş için MCP fazlasıyla uygun, ayrı bir app'e gerek yok. MCP zaten tam olarak bunun için var: Claude'a olmayan bir yeteneği araç olarak eklemek.

---

## Mimari Özet

```
[Video URL veya dosya]
        │
        ▼
  1. İndirme (yt-dlp)
        │
        ▼
  2. Ses ayrıştırma (ffmpeg)
        │
        ├──▶ 3a. Transkripsiyon (faster-whisper) ──▶ zaman damgalı .txt/.srt
        │
        └──▶ 3b. Sahne tespiti (PySceneDetect) ──▶ anahtar kareler .jpg
        │
        ▼
  4. MCP tool sonucu: transkript metni + kare dosya yolları
        │
        ▼
  Claude transkripti okur, kareleri görür → videoyu "izlemiş" olur
```

---

## Faz 0 — Hazırlık (1 gün)

- Python 3.11+ kur (bu ekosistemin en olgun araçları Python'da).
- ffmpeg'i sisteme kur (`brew install ffmpeg` / `apt install ffmpeg` / Windows'ta winget).
- Temel paketler: `pip install yt-dlp faster-whisper scenedetect[opencv] mcp`

**Önemli hukuki not:** Instagram'ın kullanım şartları otomatik indirmeyi yasaklar ve giriş gerektiren içerikte hesabın kısıtlanma riski vardır. En güvenli yol: aracı kendi indirdiğin/sana ait dosyalarla ve herkese açık içerikle kişisel kullanım için tasarlamak. Genel videolar (YouTube vb.) için de aynı hassasiyet geçerli.

---

## Faz 1 — Önce MCP'siz Çalışan Boru Hattı (2–4 gün)

MCP'ye sarmadan önce her adımı düz bir Python scripti olarak çalıştır. Hata ayıklamak böyle 10 kat kolay.

**1. İndirme** — yt-dlp Instagram, YouTube, TikTok dahil yüzlerce siteyi destekler:
```python
import yt_dlp
opts = {"outtmpl": "downloads/%(id)s.%(ext)s", "format": "mp4"}
with yt_dlp.YoutubeDL(opts) as ydl:
    ydl.download([url])
```
Instagram için giriş gerektiğinde `cookiesfrombrowser` seçeneğiyle kendi tarayıcı çerezlerini kullanabilirsin (yalnızca kendi hesabın için).

**2. Ses ayrıştırma:**
```bash
ffmpeg -i video.mp4 -vn -ar 16000 -ac 1 audio.wav
```
16 kHz mono, Whisper'ın beklediği format — dosya küçük, transkripsiyon hızlı olur.

**3. Transkripsiyon** — faster-whisper, OpenAI Whisper'ın 4 kat hızlı sürümü ve tamamen yerel çalışır (API maliyeti yok):
```python
from faster_whisper import WhisperModel
model = WhisperModel("small")  # Türkçe için "medium" daha iyi
segments, info = model.transcribe("audio.wav", language="tr")
for s in segments:
    print(f"[{s.start:.1f}s → {s.end:.1f}s] {s.text}")
```

**4. Sahne tespiti** — PySceneDetect belirgin görsel değişimleri bulup kareyi kaydeder:
```python
from scenedetect import detect, ContentDetector, save_images, open_video
scenes = detect("video.mp4", ContentDetector(threshold=27))
save_images(scenes, open_video("video.mp4"), num_images=1, output_dir="frames/")
```
`threshold` düşükse daha çok kare, yüksekse sadece büyük değişimler.

**Faz 1 çıktısı:** `python pipeline.py <url>` dediğinde `output/<video_id>/` klasöründe `transcript.txt` + `frames/` oluşuyor. Bu çalışmadan Faz 2'ye geçme.

---

## Faz 2 — MCP Sunucusuna Sarma (2–3 gün)

Python MCP SDK'sındaki FastMCP ile boru hattını araçlara dönüştür:

```python
from mcp.server.fastmcp import FastMCP
mcp = FastMCP("video-watcher")

@mcp.tool()
def watch_video(url: str) -> str:
    """Videoyu indir, transkriptini çıkar ve anahtar kareleri kaydet."""
    path = download(url)
    audio = extract_audio(path)
    transcript = transcribe(audio)
    frames = detect_scenes(path)
    return format_result(transcript, frames)

if __name__ == "__main__":
    mcp.run()
```

Önerilen araç seti:
- `watch_video(url)` — tam boru hattı, zaman damgalı transkript döndürür
- `transcribe_file(path)` — kullanıcının elindeki yerel dosya için
- `get_frames(video_id)` — kareleri görüntü içeriği olarak döndürür (MCP, image content type destekler; Claude kareleri gerçekten görebilir)
- `list_processed()` — daha önce işlenen videoların arşivi

Sonra Claude Desktop'ın konfigürasyonuna (`claude_desktop_config.json`) sunucunu ekleyip test et. Uzun videolarda işlem dakikalar sürebilir; MCP'nin progress notification desteğiyle ilerleme bildir veya işi ikiye böl: `start_processing(url)` hemen bir iş kimliği döndürsün, `get_result(job_id)` sonucu alsın.

---

## Faz 3 — "İzleme" Kalitesini Artırma (1–2 hafta, opsiyonel)

- **Transkript + kare eşleştirme:** Her anahtar kareye o andaki transkript cümlesini iliştir. Claude'a "3. saniyede şu görünürken şu söyleniyordu" bilgisi gider — asıl sihir burada.
- **Kare içi yazı (OCR):** Instagram videolarında altyazı/metin overlay çok yaygın. `pytesseract` ile karelerdeki yazıyı da metne kat.
- **Konuşmacı ayrımı:** `pyannote.audio` ile kim ne zaman konuşmuş etiketle.
- **Özet katmanı:** İstersen sunucu içinden Anthropic API'sini çağırıp kareleri ve transkripti tek bir "video özeti"ne dönüştürebilirsin — ama genelde ham veriyi Claude'a verip yorumu sohbetteki Claude'a bırakmak daha esnek.

---

## Faz 4 — Paylaşım ve Erişim (opsiyonel)

- **Yerel kullanım:** Claude Desktop konfigürasyonu yeterli, burada durabilirsin.
- **Uzak sunucu:** claude.ai web/mobilden bağlanmak istersen sunucuyu HTTP transport ile (streamable HTTP) bir sunucuda barındırıp OAuth eklemen gerekir. Bu ciddi ek iş; önce yerel sürümü olgunlaştır.
- **Dağıtım:** Başkaları kullansın istersen GitHub'a koy, `uvx` ile tek komutla kurulabilir yap.

---

## Gerçekçi Zorluklar

1. **Instagram erişimi en kırılgan halka.** yt-dlp zaman zaman kırılır, Instagram tarafı sık değişir. Mimarini "indirme katmanı değiştirilebilir" olacak şekilde kur; dosya yoluyla çalışan mod her zaman yedek plan olsun.
2. **Whisper model boyutu / hız dengesi:** `small` hızlı ama Türkçede hata yapar, `medium`/`large-v3` doğru ama yavaş. GPU varsa large, yoksa medium ile başla.
3. **Görsellerin Claude'a taşınması:** Çok kare = çok token. Videobaşına 5–10 anahtar kareyle sınırla, gerekirse Claude'un "şu aralıktan daha fazla kare ver" diyebileceği bir araç ekle.

---

## Özet Takvim

| Faz | Süre | Çıktı |
|-----|------|-------|
| 0 | 1 gün | Ortam hazır |
| 1 | 2–4 gün | Çalışan CLI boru hattı |
| 2 | 2–3 gün | Claude Desktop'ta çalışan MCP |
| 3 | 1–2 hafta | OCR, konuşmacı ayrımı, kare-metin eşleştirme |
| 4 | opsiyonel | Uzak sunucu / dağıtım |

İlk çalışan sürüm ~1 haftalık akşam mesaisiyle gerçekçi. En kritik tavsiye: Faz 1'i atlamadan, MCP'ye bulaşmadan önce düz scripti çalıştır.
