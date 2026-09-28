# MatchPets — İlerleme Notu

Son güncelleme: 23 Eylül 2026

## Çalışan durum

**Cascade çalışıyor.** Oyun artık gerçek bir match-3 döngüsüne sahip.

### Tek kapılar
- `ClearCell(x, y)` — yok etme. Sınır ve boş hücre guard'ı içeride.
- `CreateItem(prefab, x, y)` — yaratma.
- `MoveItem(from, to)` — taşıma.
- `GridToWorld(x, y)` / `IsValidCoordinate(x, y)` — dönüşüm ve sınır, tek yerde.

### Konum
`item.x` / `item.y` kaldırıldı. Tek doğruluk kaynağı `board[x, y]`. Girdi yolu
koordinat tabanlı: `Update` basma hücresini alanda saklıyor, `HandleRelease`
dört koordinat alıyor. `transform.position`'dan koordinat türetilmiyor.

### `MatchFinder` (ayrı sınıf, `MonoBehaviour` değil, `Cell[,]` alıyor)
İki geçiş: önce yatay koşular (`left == 0` olan hücre grubu yaratır, koşunun
tüm hücreleri sözlüğe ve `group.Cells`'e girer), sonra dikey (`bottom == 0`).
Dikey koşu, hücrelerinden biri sözlükte bulunursa mevcut gruba **merge** olur —
L/T böyle tespit ediliyor. Kesişimde `Position` da o hücreye kaydırılıyor.

Döndürdüğü: `Dictionary<(int,int), MatchGroup>`. Anahtarlar = temizlenecek
hücreler, `Values.Distinct()` = gruplar.

`MatchGroup` bir **class** (referans tipi — merge sırasında aynı nesne paylaşılıyor).
Alanları: `Position`, `Horizon (left,right)`, `Vertical (top,bottom)`,
`Cells` (**HashSet**, tekrar olmasın diye).

### `ApplyMatches`
Her grup için: hücreleri `ClearCell` ile temizle, sonra şekle bakıp `Position`'a
özel item üret. `switch` ifadesi, öncelik: disco (5+) → bomba (L/T) → dikey roket
→ yatay roket → yok (3'lü). Eski 100 satırlık `CheckMatches` silindi.

### Cascade döngüsü (`HandleRelease` içinde)
```
guard'lar → SwapItems → geçerlilik kontrolü (geçersizse geri al)
→ basılan özelse ClearCell → bırakılan özelse ClearCell
→ FallItems/SpawnItems → while(matches.Count > 0){ Apply, Fall, Spawn, Find }
```
50 tur sigortası var (`LogWarning` + `break`).

### Test seviyeleri
`CascadeTest1` (yatay 3'lü + 4'lü → 7 hücre, 2 grup),
`CascadeTest2` (L şekli → 5 hücre, 1 grup). İkisi de doğru sonuç veriyor.

## Yarın: özel item'ın doğduğu hücre

**Sorun:** oyuncu sürükleyip 4'lü yaptığında roket koşunun **sol ucunda** doğuyor,
oyuncunun bıraktığı hücrede değil. Ticari match-3'lerde beklenen davranış ikincisi.

**Kararlaştırılan çözüm:** `ApplyMatches`'e ikinci parametre:

```csharp
void ApplyMatches(Dictionary<(int,int), MatchGroup> foundMatches,
                  (int, int)? preferredCell = null)
```

Grup içinde: `preferredCell` doluysa **ve** `g.Cells` onu içeriyorsa oraya doğsun,
yoksa `g.Position`. (`Cells` HashSet olduğu için `Contains` O(1).)

`HandleRelease`'te `preferred = (releaseX, releaseY)` ile başla, **ilk turdan sonra
`null`'a çek** — cascade turlarında oyuncu hücresi yok, `Position` kullanılmalı.

`(int, int)?` = nullable tuple. `.HasValue` ile dolu mu diye sorulur, `.Value` ile
içindekine ulaşılır. `(-1,-1)` sentinel yerine `null` tercih edildi: "değer yok" ile
"geçersiz koordinat" farklı şeyler.

## Sonraki adımlar

1. **Disco ball** — hedef renk takas anında doğuyor, `ActivateSpecial`'a ulaşmıyor.
   `TypesOnBoard`'a null guard gerekiyor.
2. **Özel item kombinasyonları** — roket+roket artı, roket+bomba geniş, disco+özel.
3. **Deadlock tespiti + karıştırma** — `HasMatch` üzerine kurulur. Mülakat malzemesi.
4. **Oyun kuralları** — hamle sayısı, hedef, kazanma/kaybetme.

Sonra: animasyon, UI, ses, seviye tasarımı.

## Kalan teknik borç

- Cascade döngüsünde `FallItems/SpawnItems` iki yerde — `do-while` ile tek yere iner
- `CountHorizontal`/`CountVertical` (BoardManager'daki eskiler) hâlâ sihirli `6` kullanıyor
- Dikey bir koşu iki farklı yatay grubu keserse ikisi de L/T sayılır → iki bomba (nadir)
- `MatchFinder` her `HandleRelease`'te yeniden `new`leniyor — alan olabilir
- Adım 7 — `ActivateSpecial`'ın `if` zinciri polimorfizme çevrilebilir (Open/Closed)
- `Instantiate`/`Destroy` yerine object pooling

## Takvim

- **Eylül–Ekim** — disco, kombinasyonlar, deadlock, kurallar
- **Ekim–Aralık** (okul, düşük tempo) — animasyon, UI, ses, seviye tasarımı
- **Ocak** — staj başvuruları

Yanında: algoritma/veri yapıları pratiği, CV, GitHub README + ekran görüntüleri.

## Öğrenilenler

- Değer/referans tipi; Unity'de `Destroy` ertelenir ("fake null"), `==` ezilmiştir
- **Enum'lar sayı olarak serialize edilir** — ortadan silmek kaydedilmiş veriyi bozar
- MonoBehaviour `new` ile yaratılamaz; struct ve düz sınıflarda kısıt yok
- **Tip, veriye dair bir iddiadır** — sıra varsa `List`, benzersizlik varsa `HashSet`,
  eşleme varsa `Dictionary`
- **class vs struct:** merge sırasında aynı nesnenin paylaşılması gerekiyordu → class
- **Değişkenin ömrü, işinin ömrü kadar olmalı** (metod içi vs alan)
- **Dallanan problem → özyineleme, doğrusal problem → döngü**
- **Bir guard sadece kontrol ettiği koordinatı korur** — gezinen fonksiyonda kontrol
  döngünün koşulunda olmalı
- Gezdiğin veri yapısını gezerken değiştirme
- **Önce ölç, sonra uygula** — sayma ile kaydetme ayrı adımlar
- Bilgiyi elindeyken sakla, sonradan yeniden üretme (kesişim hücresi, `Cells`)
- **Tanım tipli, çağrı tipsiz** — `CreateItem(Items prefab, int x)` vs `CreateItem(p, x)`
- `foreach (var (x, y) in ...)` = deconstruction; `foreach ((int x,int y) cell in ...)`
  tek bir `cell` değişkeni tanımlar
- `using` direktifleri **dosya başına** geçerlidir
- `Try` öneki `try/catch` değil, TryParse desenidir
- İsimlendirme: fiil + nesne, somut fiil (`Apply`, `Clear`, `Find` — `Process` değil)
- Tek konvansiyon seç: `x`/`y` mi `row`/`column` mu — karıştırmak 5+ hataya yol açtı
- Dönüşüm sınırda, tek yerde (koordinat çevirme, string→enum)
- Geçen bir test, kodun doğru olduğunu değil **o senaryoda** doğru olduğunu gösterir
- Önce derleyiciye sor, derleyicinin bilemeyeceğini insana sor
