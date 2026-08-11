# Royal Kingdom — İlerleme Notu

Son güncelleme: 4 Ağustos 2026

## Tamamlananlar

**Adım 1 — `ClearCell(x, y)`**
Tekrar eden `Destroy + item = null` tek metoda toplandı. Sınır kontrolü ve boş
hücre guard'ı metodun içinde — çağıran hiçbir yer kırpma yapmıyor.

**Adım 2 — `CandyType` bölündü**
`rocketHorizontal` / `rocketVertical`. İki ayrı prefab, Inspector'dan bağlı.
Yön, eşleşmenin oluşum yönüne göre belirleniyor.

**Adım 3 — `ActivateSpecial(x, y, type)`**
Yatay roket satırı, dikey roket sütunu, bomba 3x3'ü temizliyor.
Disco yorum satırında — hangi renkle takas edildiği bilgisi eksik.

**Adım 4 — Zincir patlama** ✅ *Console log'uyla doğrulandı*
```
ActivateSpecial: horizontalRocket @ (5,1)
SpecialItem found
ActivateSpecial: horizontalRocket @ (4,1)
```
İkinci roket aynı satırı süpürdü ama hücreler `null` olduğu için guard'dan döndü —
sonsuz döngü yok. *Mark-before-recurse* çalışıyor.

**Adım 5 — Tetikleme**
Tıklama ve takas ile özel item patlıyor. Geçersiz takas `HasMatch` ile geri alınıyor.
Ölü nesne referansı yerine `board` üzerinden kontrol yapılıyor.

## Yarın: test araçları

Rastgele tahtada senaryo beklemek yerine debug aracı yaz:

- Hızlı yol: `Update`'te tuş kısayolu — B'ye basınca `selectedItem`'ın hücresine bomba
- Temiz yol: `[ContextMenu("Bomba Koy")]` — Inspector'dan sağ tıkla çalıştır
- En iyi yol: `LevelInfo`'ya özel item isimleri ekle (`rocketH`, `bomb`...), test
  senaryolarını seviye dosyası olarak yaz

Sonuncusu `LoadLevel` refactor'üyle aynı yere dokunuyor, birlikte yapılmalı.

**Görsel test notu:** iki *yatay* roketin zinciri ekranda görünmez — ikisi de aynı
satırı süpürür. Görmek için **yatay + dikey** (artı şekli) veya **roket + bomba**
kombinasyonu kur. Gözlemlenebilir fark üretmeyen test, test değildir.

## Sonraki adımlar

- **Adım 6** — Eşleşme mantığını `BoardManager`'dan ayır. `CountHorizontal`,
  `CountVertical` zaten saf fonksiyonlar; `FindMatches` bir `MatchResult` dönmeli
  (hücre listesi + şekil bilgisi, çünkü hangi özel item'ın üretileceği şekle bağlı).
  Kazanç: Unity açmadan unit test.
- **Adım 7** — `ActivateSpecial`'ın `if` zincirini polimorfizme çevir (Open/Closed).
- **`LoadLevel(LevelInfo)`** — kurulumu `Start`'tan ayır. Koordinat dönüşümü
  (dikey çevirme) **sadece burada** yaşasın, başka hiçbir yerde `-1-y` geçmesin.

## Biriken teknik borç

- `LevelInfo.rows[x]` aslında **sütun** veriyor → `rows[y].Split(',')[x]` olmalı.
  Tahta kare olduğu için şu an patlamıyor; 8x9'a geçince `IndexOutOfRange` verir.
  Mevcut asset verisinin transpoze edilmesi gerekecek.
- Cascade yok — düşme ve doldurma sonrası yeni eşleşmeler kontrol edilmiyor.
- `>= 5` dalı, hem yatay hem dikey 5'li olan durumda sadece yatayı temizliyor.
- Disco: takas edilen rengin bilgisi `ActivateSpecial`'a ulaşmıyor.
- İki özel item takası: ayrı ele alınmalı (roket+roket artı, roket+bomba dev patlama).
- `Instantiate`/`Destroy` yerine object pooling.
- Tıklama dalındaki üç satırlık tekrar → `Resolve(Items item)` metoduna çıkarılabilir.
- Girinti bozuklukları — VS Code'da Shift+Option+F.

## Bu projede öğrenilenler

- Değer tipi / referans tipi; Unity'de `Destroy` ertelenir, "fake null"
- Unity `==` operatörünü ezer: yok edilmiş nesne `null`'a eşit sayılır →
  referans karşılaştırması güvenilmez, koordinat üzerinden kimlik kullan
- MonoBehaviour `new` ile yaratılamaz
- Prefab dosyası kodda değişken yaratmaz — `public` alan + Inspector'da sürükleme
- Guard clause, erken çıkış, tek çıkış noktası
- Compiler-driven refactoring: enum değerini silip derleyiciye çağıranları buldurma
- Mekanik refactor tehlikelidir — `Destroy(board[start,y]...)` → `ClearCell(x,y)`
  hatası derlendi ama yanlıştı (*silent failure*)
- `Try` öneki `try/catch` değil, TryParse desenidir; exception kontrol akışı için
  kullanılmaz (pahalı + `Update` içinde kalan kodu atlatır)
- İsimlendirme: `Has`/`Is`/`Can` saf sorgular, `Try` bool+veri, PascalCase public
- Off-by-one: kapsayıcı/dışlayıcı sınırı karıştırma
- Erken optimizasyon yapma — 36 hücrelik O(n²) tarama sorun değil
- Önce derleyiciye sor, derleyicinin bilemeyeceğini insana sor
