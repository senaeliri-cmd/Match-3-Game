# MatchPets — İlerleme Notu

Son güncelleme: 7 Eylül 2026

## Çalışan durum

**Özel item sistemi çalışıyor, konum bilgisi tekilleştirildi.**

- `ClearCell(x, y)` — yok etmenin tek kapısı. Sınır ve boş hücre guard'ı içeride.
- `CreateItem(prefab, x, y)` — yaratmanın tek kapısı.
- `MoveItem(from, to)` — taşımanın tek kapısı. `FallItems` bunu çağırıyor.
- `GridToWorld(x, y)` — grid→dünya dönüşümü tek yerde.
- `IsValidCoordinate(x, y)` — sınır kontrolü tek yerde.
- `ActivateSpecial(x, y, type)` — yatay roket satırı, dikey roket sütunu, bomba 3x3.
- **Zincir patlama doğrulandı** — roket → roket → bomba, üç seviye derinlik, sonsuz
  döngü yok (*mark-before-recurse*: hücre önce `null`'a çekiliyor).
- `LoadLevelInfo(level)` — tahta boyutu veriden geliyor. `Enum.TryParse` +
  `candyDict.TryGetValue` ile seviye dosyasından özel item yüklenebiliyor.
- **`item.x`/`item.y` kaldırıldı.** Konumun tek doğruluk kaynağı `board[x, y]`.
  Girdi yolu tamamen koordinat tabanlı: `Update` basma hücresini alanda saklıyor,
  `HandleRelease(pressX, pressY, releaseX, releaseY)` koordinat alıyor.
  `transform.position`'dan koordinat türetilmiyor (animasyon eklenince bozulurdu).

## Şu an üzerinde çalışılan: `MatchFinder` (Adım 6)

Eşleşme tespitini `BoardManager`'dan ayıran yeni sınıf. `Assets/Scripts/cascade.cs`
dosyasında taslak var — **adı `MatchFinder` olmalı**, cascade ayrı bir şey.

### Verilen tasarım kararları

**Sınıf `MonoBehaviour` olmayacak, `BoardManager` almayacak.** Parametresi `Cell[,] board`.
Amaç: Unity açmadan test edilebilmesi. Tahtayı **hiç değiştirmeyecek** — sadece okur ve
bulduğunu döndürür. Temizleme ve özel item üretimi `BoardManager`'da kalır.

**Dönüş: `List<MatchGroup>`.** Ayrı bir `bool` gerekmez — boş liste "eşleşme yok"
demektir. Cascade döngüsü bunu kullanır.

**İlk eşleşmede `return` YOK.** Biriktir, taramanın sonunda bir kez döndür.
Tarama sırasında `BoardManager`'ın fonksiyonlarını çağırma — gezdiğin yapıyı
gezerken değiştirmiş olursun.

**`MatchGroup` struct'ı yazıldı** — `readonly struct`, get-only property'ler:
`Position (posX, posY)`, `Horizon (left, right)`, `Vertical (top, bottom)`.
Şekil ayrı alan olarak tutulmuyor, sayılardan hesaplanıyor.

**Tarama modeli: A — her hücreden iki yöne say.** (Merkez etrafında sayma; mevcut
`CountHorizontal`/`CountVertical` mantığı.) Struct bu modele göre tasarlandı.

### Tekrar eleme — çözülmesi gereken kısım

Model A'da yatay 3'lünün üç hücresi de aynı koşuyu raporlar. Ayrıca T şeklinde
kesişim hücresinde `left != 0` olabilir, dolayısıyla "sadece `left==0` kaydet"
kuralı L/T'yi kaçırır. Ve L/T'nin dikey kolu, kolun alt ucundan bağımsız bir dikey
koşu olarak ikinci kez raporlanır → bir bomba + bir roket üretilir (yanlış).

**Kararlaştırılan çözüm — iki geçiş:**

1. **L/T geçişi:** her hücrede iki yönü de say. İkisi de 3+ ise L/T. Kaydet ve
   **her iki kolun tüm hücrelerini** `HashSet<(int,int)>` ile "kullanıldı" işaretle.
2. **Düz koşu geçişi:** `left==0 && yatay>=3` (ve `bottom==0 && dikey>=3`) olan,
   hücreleri işaretli olmayan koşuları kaydet.

`HashSet` metodun **içinde** tanımlanacak, `Cell`'e "sayıldı" alanı eklenmeyecek —
geçici bilgi kalıcı veri yapısına yazılmaz, yoksa her çağrıda sıfırlaman gerekir.

### Yazma sırası

1. Sadece **yatay** koşular → listeye ekle → `Debug.Log` ile doğrula ← **buradasın**
2. Dikey koşuları ekle
3. L/T + `HashSet` elemesi

Üçünü birden yazma; her adımda çalıştığını gör.

### Taslaktaki bilinen hatalar (`cascade.cs`)

- `using BoardManager;` — `using` namespace içindir, sınıf adı değil
- `: MonoBehaviour` kaldırılacak, parametre `Cell[,]` olacak
- Dönüş tipi geçerli C# değil → `List<MatchGroup>`
- `FindVertical`'da `nextX++/currX++` yazılmış, `nextY++/currY++` olmalı → **sonsuz döngü**
- `FindHorizontal` sağ döngüsünde `currX--`, `currX++` olmalı.
  Aslında `currX`'e hiç gerek yok — her hücreyi başlangıçtaki tipe karşılaştır.
- `j += right` — `j` y ekseni, `right` x uzunluğu; yanlış eksen
- `bottomtNum` yazım hatası, `HasMatch` tüm yollarda `return` etmiyor
- `CandyType.Ispecial` → `Items.IsSpecial`

## Sonraki adımlar

4. `BoardManager` grupları uygulasın — temizle + özel item üret (mevcut `CheckMatches`
   yerini alacak)
5. **Cascade döngüsü** — `while(true){ bul; boşsa break; uygula; FallItems; SpawnItems; }`
   Özyineleme değil döngü: doğrusal tekrar, dallanma yok.
   Sonsuz döngüye karşı tur sayacı + `LogWarning` koy.
6. Disco ball — hedef renk bilgisi takas anında doğuyor, `ActivateSpecial`'a ulaşmıyor
7. Özel item kombinasyonları (roket+roket artı, roket+bomba geniş, disco+özel)
8. Deadlock tespiti + karıştırma (`HasMatch` üzerine kurulur)
9. Oyun kuralları — hamle sayısı, hedef, kazanma/kaybetme

Sonra: animasyon, UI, ses, seviye tasarımı (okul dönemi işi).

## Kalan teknik borç

- `CountHorizontal`/`CountVertical`'da sihirli `6` — `IsValidCoordinate` kullan
- `>= 5` dalı, hem yatay hem dikey 5'li durumda sadece yatayı temizliyor
- `HandleRelease`'te `IsSpecial` aynı item için iki kez soruluyor
- Tıklama dalındaki üç satırlık tekrar → `Resolve(x, y)` metoduna
- Özel item prefab alanları (`BombPrefab` vb.) `candyDict[CandyType.bomb]` ile değiştirilebilir
- `CheckMatches`'teki `.type` / `.itemName` atamaları gereksiz (prefab zaten taşıyor)
- `Instantiate`/`Destroy` yerine object pooling
- Adım 7 — `ActivateSpecial`'ın `if` zinciri polimorfizme çevrilebilir (Open/Closed)

## Takvim

- **Eylül** — `MatchFinder` + cascade + disco (derin odak isteyen işler)
- **Ekim–Aralık** (okul, düşük tempo) — kombinasyonlar, kurallar, animasyon, UI
- **Ocak** — staj başvuruları

Yanında: algoritma/veri yapıları pratiği, CV, GitHub README + ekran görüntüleri.

## Öğrenilenler

- Değer tipi / referans tipi; Unity'de `Destroy` ertelenir, "fake null"
- Unity `==`'i ezer: yok edilmiş nesne `null`'a eşit sayılır → referans karşılaştırması
  güvenilmez, kimlik olarak koordinat kullan
- **Enum'lar sayı olarak serialize edilir** — ortadan silmek/sıra değiştirmek kaydedilmiş
  veriyi sessizce bozar (bomba prefab'ı kendini roket sandı). Sadece sona ekle.
- MonoBehaviour `new` ile yaratılamaz; struct ve düz sınıflar için kısıt yok
- Prefab dosyası kodda değişken yaratmaz — `public` alan + Inspector'da sürükleme
- `Library` türetilmiş veri, silinip yeniden üretilebilir; gerçek ayarlar `.meta`'larda
- **Değişkenin ömrü, işinin ömrü kadar olmalı** — `groupList`, `HashSet`, `FallItems`'taki
  `int a`: hepsi metod içinde. `Update`'te kareler arası yaşaması gereken şey ise alan olmalı.
- **Dallanan problem → özyineleme, doğrusal problem → döngü.** Zincir patlama dallanıyor
  (ağaç), koşu tarama ve cascade doğrusal.
- Gezdiğin veri yapısını gezerken değiştirme
- `Try` öneki `try/catch` değil, TryParse desenidir; exception kontrol akışı için kullanılmaz
- İsimlendirme: `Has`/`Is`/`Can` saf sorgular, `Try` bool+veri, PascalCase public
- **Tek konvansiyon seç:** `x`/`y` mi `row`/`column` mu — karıştırmak bu projede 5 hataya yol açtı
- Dönüşüm sınırda, tek yerde yapılır (koordinat çevirme, string→enum)
- Stack trace aşağıdan yukarı okunur; tekrar eden metod adı özyineleme derinliğidir
- Gözlemlenebilir fark üretmeyen test, test değildir
- Guard'a çevirirken koşulu ters çevir; De Morgan: `!(a && b)` = `!a || !b`
- Mekanik refactor tehlikelidir — derlenir ama yanlış olur (*silent failure*)
- Önce derleyiciye sor, derleyicinin bilemeyeceğini insana sor
