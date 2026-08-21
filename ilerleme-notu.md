# Royal Kingdom — İlerleme Notu

Son güncelleme: 4 Ağustos 2026

## Çalışan durum (commit atıldı)

**Özel item sistemi tamamen çalışıyor.**

- `ClearCell(x, y)` — yok etmenin tek kapısı. Sınır kontrolü ve boş hücre guard'ı içeride,
  bu yüzden hiçbir çağıran kırpma yapmıyor.
- `CreateItem(prefab, x, y)` — yaratmanın tek kapısı. Grid→dünya koordinat dönüşümü burada.
- `ActivateSpecial(x, y, type)` — yatay roket satırı, dikey roket sütunu, bomba 3x3.
- Tetikleme: tıklama ve takas. Geçersiz takas `HasMatch` ile geri alınıyor.
- **Zincir patlama doğrulandı** — yatay roket → dikey roket → bomba, üç seviye derinlik,
  sonsuz döngü yok (*mark-before-recurse*: hücre önce `null`'a çekiliyor).
- `LoadLevelInfo(level)` — seviye yükleme `Start`'tan ayrıldı. Tahta boyutu veriden geliyor.
- `Awake`'te `Dictionary<CandyType, Items>` kuruluyor, `Enum.TryParse` + `TryGetValue` ile
  seviye dosyasından özel item yüklenebiliyor (`SpecialItemTestCase` bunun için var).
- Rastgele üretim havuzu (`prefabs`) ile tam liste (`allPrefabs`) ayrı.

## Sıradaki işler — bu sıra önemli

**1. Adım 6 — `FindMatches` (3–4 gün)**
Refactor gibi görünüyor ama cascade için zorunlu ön koşul: cascade'de başlangıç item'ı
yok, tahtanın tamamını taraman gerekiyor. Dönüş tipi sadece hücre listesi olamaz —
hangi şekil olduğu da lazım (özel item üretimi ona bağlı). **Bu projedeki en tasarım-
ağırlıklı iş; acele etme, üstüne üç şey daha kurulacak.**

**2. Cascade (2–3 gün)** — patlat → düşür → doldur → tekrar ara, eşleşme kalmayana kadar.
Mevcut koddaki gizli hataları ortaya dökecek.

**3. Disco ball (1 gün)** — zor kısmı kod değil, bilginin akışı: hedef renk takas anında
doğuyor ama `ActivateSpecial`'a ulaşmıyor.

**4. Kombinasyonlar (2–3 gün)** — roket+roket artı, roket+bomba geniş, disco+özel.

**5. Deadlock + karıştırma (2 gün)** — `HasMatch` üzerine kurulur. Mülakat malzemesi.

**6. Oyun kuralları (1–2 gün)** — hamle sayısı, hedef, kazanma/kaybetme.

`MoveItem(from, to)` refactor'ünü cascade'den **önce** yap — `item.x`, `item.y` ve
`transform.position` üçlüsü `FallItems` ve `SwapItems`'ta elle senkronize ediliyor ve
cascade `FallItems`'ı çok daha sık çağıracak.

## Takvim

- **Ağustos kalanı** — 1, 2, 3 (derin odak isteyen işler)
- **Eylül** (taşınma haftası hariç) — 4, 5, 6
- **Ekim–Aralık** (okul, düşük tempo) — animasyon, UI, ses, seviye tasarımı
- **Ocak** — başvuru

Projenin yanında: algoritma/veri yapıları pratiği, CV, GitHub README + ekran görüntüleri.

## Kalan teknik borç

- `>= 5` dalı, hem yatay hem dikey 5'li olan durumda sadece yatayı temizliyor
- Tıklama dalındaki üç satırlık tekrar → `Resolve(Items item)` metoduna çıkarılabilir
- `HandleRelease`'te `IsSpecial` aynı item için iki kez soruluyor (105 ve 110)
- `Instantiate`/`Destroy` yerine object pooling
- Adım 7 — `ActivateSpecial`'ın `if` zinciri polimorfizme çevrilebilir (Open/Closed)
- `Debug.Log`'ları temizle (`In IsSpecial` gürültü yapıyor)

## Öğrenilenler

- Değer tipi / referans tipi; Unity'de `Destroy` ertelenir, "fake null"
- Unity `==`'i ezer: yok edilmiş nesne `null`'a eşit sayılır → referans karşılaştırması
  güvenilmez, kimlik olarak koordinat kullan
- **Enum'lar sayı olarak serialize edilir** — ortadan silmek/sıra değiştirmek kaydedilmiş
  veriyi sessizce bozar. Sadece sona ekle, ya da değerleri açıkça sabitle.
- MonoBehaviour `new` ile yaratılamaz; prefab dosyası kodda değişken yaratmaz
- Guard clause, erken çıkış, tek çıkış noktası — ve guard'a çevirirken koşulu ters çevir
- Compiler-driven refactoring: enum değerini silip derleyiciye çağıranları buldurma
- Mekanik refactor tehlikelidir — derlenir ama yanlış olur (*silent failure*)
- `Try` öneki `try/catch` değil, TryParse desenidir; exception kontrol akışı için kullanılmaz
- İsimlendirme: `Has`/`Is`/`Can` saf sorgular, `Try` bool+veri, PascalCase public
- Tek konvansiyon seç: `x`/`y` mi `row`/`column` mu — karıştırmak bu projede 4 hataya yol açtı
- Dönüşüm sınırda, tek yerde yapılır (koordinat çevirme, string→enum)
- Stack trace aşağıdan yukarı okunur; tekrar eden metod adı özyineleme derinliğidir
- Gözlemlenebilir fark üretmeyen test, test değildir
- Erken optimizasyon yapma; önce derleyiciye sor, sonra insana
