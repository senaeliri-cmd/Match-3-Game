# Royal Kingdom — İlerleme Notu

Son güncelleme: 4 Ağustos 2026

## Bu oturumda yapılanlar

**Adım 1 — `ClearCell(x, y)` ✅**
Tekrar eden `Destroy + item = null` ikilisi tek metoda toplandı. Artık `Destroy(`
sadece `ClearCell` içinde geçiyor. Metod kendi içinde sınır kontrolü ve boş hücre
guard'ı yapıyor — bu yüzden çağıran hiçbir yerin kırpma yapmasına gerek yok.

**Adım 2 — `CandyType` bölündü ✅**
`rocket` silindi, `rocketHorizontal` / `rocketVertical` eklendi. İki ayrı prefab
oluşturuldu ve Inspector'dan bağlandı. Yön, eşleşmenin oluşum yönüne göre
belirleniyor.

**Adım 3 — `ActivateSpecial(x, y, type)` ✅**
Yatay roket satırı, dikey roket sütunu, bomba 3x3'ü temizliyor.
Disco yorum satırında — hangi renkle takas edildiği bilgisi eksik.

**Adım 5 — Tetikleme 🔄 devam ediyor**
`HandleRelease` yeniden yazılıyor. Çift tıklama fikri denendi, gerek olmadığı
görüldü — tek tıklama zaten çalışıyor.

## Yarın kaldığın yer

`HandleRelease` içinde çözülecek iki şey:

1. **Tek çıkış noktası.** Tıklama dalında erken `return` var, bu yüzden
   `FallItems` / `SpawnItems` atlanıyor ve tahtada delik kalıyor.
   Hangi yoldan gidilirse gidilsin bu ikisi sonda bir kez çalışmalı.

2. **İki özel item takas edilirse çökme.** `a` patladıktan sonra `clickedItem`
   yok edilmiş olabilir; ona dokunmadan önce hâlâ tahtada mı diye sorulmalı.
   Nesneye değil, `board` dizisine sor.

## Sonraki adımlar

- **Adım 4** — Zincir patlama testi. Yapı hazır (`ClearCell` özel item görünce
  `ActivateSpecial` çağırıyor, hücreyi önce `null`'a çektiği için sonsuz döngü
  kırılıyor — *mark-before-recurse*). Ama henüz gerçek senaryoda test edilmedi.
- **Adım 6** — Eşleşme mantığını `BoardManager`'dan ayır. `CountHorizontal`,
  `CountVertical` ve tespit kısmı Unity'ye ihtiyaç duymuyor; ayrı sınıfa çıkarsa
  unit test yazılabilir. Mülakat sorusu: "eşleşme mantığını nasıl test edersin?"
- **Adım 7** — `ActivateSpecial`'ın `if` zincirini polimorfizme çevir.
  Open/Closed Principle örneği.

## Biriken teknik borç

- `LevelInfo.rows[x]` aslında **sütun** veriyor. Seviye tasarımına geçmeden önce
  ya isim ya okuma sırası düzeltilmeli.
- Geçersiz takas geri alınmıyor — eşleşme oluşmazsa item'lar yerine dönmeli.
- Düşme ve doldurma sonrası yeni eşleşmeler kontrol edilmiyor (cascade yok).
- `>= 5` dalı hem yatay hem dikey 5'li olan durumda sadece yatayı temizliyor.
- `Instantiate` / `Destroy` yerine ileride object pooling gerekecek.
- Git commit alışkanlığı — her çalışan halde commit atılmalı.

## Bu oturumda öğrenilenler

- Değer tipi / referans tipi farkı; Unity'de `Destroy` ertelenir, "fake null"
- MonoBehaviour `new` ile yaratılamaz
- Prefab dosyası kodda değişken yaratmaz — `public` alan + Inspector'da sürükleme
- Guard clause, erken çıkış, kod derinliğini azaltma
- Compiler-driven refactoring: enum değerini silip derleyiciye çağıranları buldurma
- Off-by-one: kapsayıcı/dışlayıcı sınır karıştırmamak
- Unity Console okuma: en üstteki hatayı oku, Collapse'a dikkat et
- Erken optimizasyon yapma — 36 hücrelik O(n²) tarama sorun değil
