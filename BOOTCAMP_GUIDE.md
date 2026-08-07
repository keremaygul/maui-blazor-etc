# 📘 YENİ İŞ İÇİN TEKNOLOJİ VE GELİŞTİRME REHBERİ (BOOTCAMP GUIDE)

> **Kapsam**: ASP.NET C# Arka Planı İle Blazor (MAUI), Business Central, AL Language, Protobuf, Power Apps ve Monaco Editor Teknolojilerine Hızlı Geçiş ve Öğrenme Rehberi.

---

## 📑 İÇİNDEKİLER

1. [ASP.NET C# Geliştiricisinin Zihniyet Değişimi](#1-aspnet-c-geliştiricisinin-zihniyet-değişimi)
2. [Bölüm 1: Blazor & .NET MAUI Hybrid (Mobil & Web)](#bölüm-1-blazor--net-maui-hybrid-mobil--web)
3. [Bölüm 2: AL Programming Language (Business Central Dili)](#bölüm-2-al-programming-language-business-central-dili)
4. [Bölüm 3: Business Central & ERP İş Akışları (Document Flow)](#bölüm-3-business-central--erp-iş-akışları-document-flow)
5. [Bölüm 4: Protobuf & gRPC (Yüksek Performanslı Veri İletişimi)](#bölüm-4-protobuf--grpc-yüksek-performanslı-veri-iletişimi)
6. [Bölüm 5: Power Apps & Power Platform (Low-Code & Otomasyon)](#bölüm-5-power-apps--power-platform-low-code--otomasyon)
7. [Bölüm 6: Monaco Editor Entegrasyonu (Web Kod Editörü)](#bölüm-6-monaco-editor-entegrasyonu-web-kod-editörü)
8. [🚀 İlk İş Günü İçin Altın Kurallar & Hile Kağıdı (Cheat Sheet)](#-ilk-iş-günü-için-altın-kurallar--hile-kağıdı-cheat-sheet)

---

## 1. ASP.NET C# Geliştiricisinin Zihniyet Değişimi

Kendi C# ve ASP.NET MVC/Web API tecrüben harika bir temel oluşturuyor. Ancak bu yeni ekosistemde şu 3 temel zihniyet farkına dikkat etmelisin:

1. **ERP Sistemlerinde Core Koda Dokunulmaz (Extension Modeli)**:
   - Klasik C# projesinde bir sınıfa yeni property eklemek için sınıfı açıp yazarsın.
   - Business Central'da (AL) orijinal tabloya dokunamazsın! `tableextension` yazarak var olan tabloya "dışarıdan" yeni alanlar eklersin.
2. **Belge Odaklı (Document-Centric) Veri Yapısı**:
   - Web API'lerde tekil CRUD yapmaya alışkınızdır.
   - ERP'de her iş süreci bir **Document Header (Başlık)** ve **Document Lines (Satırlar)** çiftinden oluşur (Örn: Sipariş, Sevkiyat, Fatura).
3. **Blazor Hybrid (MAUI + Web)**:
   - JavaScript/React/Angular yazmak yerine C# ve Razor bileşenleri ile native mobil ve masaüstü uygulaması geliştirirsin. Web UI bir WebView içinde çalışır ama tüm C# kodun cihazın native katmanına doğrudan erişir.

---

## Bölüm 1: Blazor & .NET MAUI Hybrid (Mobil & Web)

### 📌 Nedir?
MAUI (Multi-platform App UI), C# ile iOS, Android, Windows ve Mac uygulamaları yapmanı sağlar. **Blazor Hybrid** ise MAUI penceresinin içine bir `BlazorWebView` yerleştirerek HTML/CSS UI'ını Blazor bileşenleriyle yazmanı sağlar.

### 💡 Temel Mantık ve Kavramlar

#### 1. Razor Component (`.razor`)
Bir `.razor` dosyası HTML + C# kodunun birleşimidir.
```razor
@page "/products"
@inject StockService StockService

<h3>@title</h3>

<button class="btn btn-primary" @onclick="AddProduct">Ekle</button>

@code {
    private string title = "Ürün Listesi";

    private void AddProduct() {
        // C# Mantığı
    }
}
```

#### 2. Dependency Injection (DI) & Lifetime
- `MauiProgram.cs` içerisinde servisler kaydedilir:
  ```csharp
  builder.Services.AddSingleton<StockService>();
  ```
- ⚠️ **DİKKAT**: MAUI tek bir süreçte (process) çalıştığı için web'deki gibi her HTTP isteğinde yeni bir scope oluşmaz. Servisleri genelde **Singleton** kaydetmek mantıklıdır.

#### 3. Data Binding (Çift Yönlü Veri Bağlama)
- `@bind="model.Name"`: Girdideki değişimi değişkene yansıtır.
- `@bind:after="OnFilter"`: Veri bağlandıktan **sonra** çalışacak metodu belirtir (NET 7+).

#### 4. Componentler Arası İletişim
- `[Parameter]`: Parent component'ten Child component'e veri aktarır.
- `EventCallback`: Child'dan Parent'a olay bildirir.
- `Service (DI)`: Uygulama genelinde ortak durum (state) yönetimi.

### ⚠️ Blazor & MAUI'de Dikkat Edilmesi Gerekenler
1. **UI Thread & Async İşlemler**: Async bir işlem sonrasında UI otomatik güncellenmezse `StateHasChanged()` metodunu çağır.
2. **Android SDK & Target Frameworks**: `dotnet build` yaparken Android SDK bilgisayarında yoksa hata alabilirsin. Sadece Windows için derlemek için `-f net10.0-windows10.0.19041.0` parametresini kullan.

---

## Bölüm 2: AL Programming Language (Business Central Dili)

### 📌 Nedir?
Microsoft Dynamics 365 Business Central (eski adıyla Navision / NAV) özelleştirmeleri yapmak için kullanılan özel programlama dilidir. VS Code ortamında geliştirilir.

### 🧱 AL'ın 5 Temel Yapı Taşı

1. **Table (Tablo)**: Veritabanı tablosu. C#'taki Entity Framework sınıflarına denk gelir.
2. **Page (Sayfa)**: Kullanıcı arayüzü. 
   - `PageType = List` → DataGrid / Tablo görünümü.
   - `PageType = Card` → Detay / Form görünümü.
3. **Codeunit (İş Mantığı)**: Servis katmanı. C#'taki Service sınıflarıdır.
4. **Enum**: Sabit değerler grubu.
5. **Extension (`tableextension`, `pageextension`)**: Orijinal objeleri değiştirmeden genişletme.

### 🔄 AL vs C# Birebir Karşılıklar Tablosu

| İşlem / Kavram | C# (EF Core & ASP.NET) | AL (Business Central) |
|---|---|---|
| Tablo Tanımı | `public class Item { public int Id {get;set;} }` | `table 50100 "Item Tracking" { fields { field(1; "Entry No."; Integer) {} } }` |
| Primary Key Bulma | `db.Items.Find(id)` | `Item.Get(EntryNo)` |
| Filtreleme (WHERE) | `.Where(x => x.Status == Active)` | `Item.SetRange(Status, Status::Active);` |
| Hızlı Toplam (SUM) | `.Sum(x => x.Quantity)` | `Item.CalcSums(Quantity);` |
| Döngü (foreach) | `foreach(var item in list)` | `if Item.FindSet() then repeat ... until Item.Next() = 0;` |
| Hata Fırlatma | `throw new Exception("Hata");` | `Error('Hata mesajı %1', Param);` |
| Kullanıcı Mesajı | `Console.WriteLine()` / `MessageBox` | `Message('Bilgi: %1', Param);` |
| Triggers | Interceptors / SaveChanges override | `OnInsert()`, `OnModify()`, `OnDelete()`, `OnValidate()` |
| Events | `event EventHandler OnBeforeSave;` | `[IntegrationEvent(false, false)]` & `[EventSubscriber(...)]` |

### ⚠️ AL Yazarken Dikkat Edilmesi Gerekenler
1. **ID Aralıkları (ID Ranges)**: Standart alan ve objeler 1-49999 arasındadır. Kendi yazdığın custom objeler için **50000 - 99999** (veya lisanslı aralık) kullanmalısın (`app.json` içinde tanımlanır).
2. **Quoted Identifiers**: AL'da değişken ve alan isimlerinde boşluk olabilir, ancak çift tırnak içine alınmalıdır: `"Item No."`, `"Location Code"`.
3. **Insert/Modify Parametresi**: `Item.Insert(true)` çağrısındaki `true` parametresi tablonun `OnInsert` trigger'ının çalışmasını sağlar. `false` yaparsan trigger çalışmaz!
4. **Var Bloğu**: AL'da değişkenler metodun başında `var` bloğunda tanımlanır. C#'taki gibi kodun ortasında inline değişken oluşturamazsın.

---

## Bölüm 3: Business Central & ERP İş Akışları (Document Flow)

### 📌 ERP Mantığı Nedir?
ERP (Enterprise Resource Planning) yazılımları bir şirketin Muhasebe, Depo, Satış, Satın Alma ve Üretim süreçlerini entegre yönetir.

### 🔄 Standart Belge Yaşam Döngüsü (Sales Order Flow)

```
[Sales Order (Sipariş)] ──(Release)──> [Released Order (Onaylı)]
                                            │
                                  ┌─────────┴─────────┐
                                  ▼                   ▼
                           (Post Shipment)      (Post Invoice)
                                  │                   │
                                  ▼                   ▼
                         [Posted Shipment]    [Posted Invoice]
                          (Stok Düştü)         (Bakiye Arttı)
```

1. **Sales Order (Sipariş Oluşturma)**:
   - Header (Müşteri, Tarih) ve Lines (Sipariş edilen ürünler, miktarlar) eklenir.
   - Durumu `Open` (Açık) haldedir, düzenlenebilir.
2. **Release (Onaylama & Kilitleme)**:
   - Sipariş kontrol edilir (Müşterinin kredi limiti aşıldı mı? Stok var mı?).
   - Durum `Released` olur. Belge artık düzenlemeye kilitlenir.
3. **Post Shipment (Sevkiyatı Kesinleştirme)**:
   - Depodan ürünler çıkar. 
   - Tablolarda **Inventory (Stok) düşer**. Geri alınamaz `Posted Sales Shipment` belgesi oluşur.
4. **Post Invoice (Faturalama)**:
   - Fatura kesilir. Müşterinin **Balance (Açık Borcu) artar**. 
   - `Posted Sales Invoice` oluşur ve sipariş arşivlenir/kapatılır.

### ⚠️ ERP Geliştirmede Dikkat Edilmesi Gerekenler
1. **Posting İşlemleri Geri Alınamaz**: ERP'de "Post" edilmiş bir veriyi veritabanından `DELETE` edemezsin! Yanlış fatura kesildiyse **Credit Memo (İade/Düzeltme Faturası)** kesilerek ters kayıt atılır.
2. **FlowFields**: Business Central'da bir tablodaki toplam tutar veya mevcut stok veritabanında saklanmaz. `FlowField` denilen dinamik hesaplama formülleriyle SQL `SUM()` gibi anlık hesaplanır.

---

## Bölüm 4: Protobuf & gRPC (Yüksek Performanslı Veri İletişimi)

### 📌 Nedir?
**Protocol Buffers (Protobuf)**, Google tarafından geliştirilen ikili (binary) veri serileştirme formatıdır. **gRPC** ise Protobuf kullanarak mikroservisler ve uygulamalar arası haberleşmeyi sağlayan protokoldür.

### 📊 Benchmark Sonuçlarımız (JSON vs Protobuf)
Yaptığımız testlerde 100 ürünlük listede şu sonuçları elde ettik:
- **Veri Boyutu**: JSON `21.4 KB` iken Protobuf `7.9 KB` (**2.7 kat daha küçük!**).
- **Hız**: Protobuf serileştirmede **1.8x**, seriden çıkarmada (deserialize) **3.5x daha hızlı**.

### 💡 `.proto` Şema Yapısı
```protobuf
syntax = "proto3";
package warehouse;

enum StockStatus {
    STOCK_STATUS_UNSPECIFIED = 0; // ⚠️ İlk değer 0 olmalı!
    STOCK_STATUS_IN_STOCK = 1;
}

message Product {
    int32 id = 1;         // ⚠️ Field numaraları eşsiz olmalı!
    string name = 2;
    double price = 6;
    StockStatus status = 9;
}
```

### ⚠️ Protobuf Kullanırken Dikkat Edilmesi Gerekenler
1. **Field Numaralarını Asla Değiştirme**: `int32 id = 1;` ifadesindeki `= 1` verinin değerini değil, binary paketteki etiketini gösterir. Numarayı değiştirirsen eski versiyon uygulamalar veriyi okuyamaz!
2. **Default Değerler İletilmez**: Sayı `0`, metin `""` ise Protobuf bant genişliği tasarrufu için bu veriyi ağdan göndermez, karşı taraf otomatik default kabul eder.

---

## Bölüm 5: Power Apps & Power Platform (Low-Code & Otomasyon)

### 📌 Nedir?
Microsoft Power Platform, kod yazmadan veya az kod yazarak (Low-Code) iş uygulamaları ve otomasyonlar yapmayı sağlar.

### 📱 3 Ana Power Apps Türü
1. **Canvas App (Tuval Uygulaması)**: Sürükle-bırak UI tasarımı. Mobil ve tablet ekranları için idealdir.
2. **Model-Driven App**: Dataverse veritabanı tablosundan otomatik form ve listeler üretir (Business Central mantığı gibidir).
3. **Power Pages (Portal)**: Şirket dışı müşterilerin/tedarikçilerin kullandığı web portalları.

### 🧪 Power Fx Formül Dili vs LINQ

| İhtiyaç | Power Fx | C# LINQ |
|---|---|---|
| Filtreleme | `Filter(Products, Qty > 10)` | `products.Where(x => x.Qty > 10)` |
| Tek Kayıt Bul | `LookUp(Products, ID = 5)` | `products.Single(x => x.Id == 5)` |
| Güncelleme | `Patch(Products, record, { Qty: 20 })` | `record.Qty = 20; db.SaveChanges();` |
| Silme | `Remove(Products, record)` | `db.Remove(record);` |

### ⚠️ Power Apps'te Dikkat Edilmesi Gerekenler
1. **Delegation Limit (2000 Satır Sınırı)**: Power Apps büyük verilerle çalışırken sorguyu veritabanına devretmeye çalışır. Desteklenmeyen bir fonksiyon kullanırsan uygulama sadece ilk 2000 satırı çeker ve hatalı sonuç verebilir!

---

## Bölüm 6: Monaco Editor Entegrasyonu (Web Kod Editörü)

### 📌 Nedir?
Monaco Editor, VS Code'un arkaplanında çalışan açık kaynaklı web kod editörüdür. Web sitelerine kod düzenleyici gömmek için kullanılır.

### 💡 Nasıl Çalışır?
1. **AMD Loader (`require.js`)**: Editör modüllerini yükler.
2. **Monarch Tokenizer**: Dildeki anahtar kelimeleri regex ile tarayıp renklendirir (Syntax Highlighting).
3. **CompletionItemProvider**: Ctrl+Space basıldığında otomatik tamamlama ve kod taslakları (Snippets) sunar.

---

## 🚀 İlk İş Günü İçin Altın Kurallar & Hile Kağıdı (Cheat Sheet)

1. **AL Dili Geliştirirken**:
   - Her zaman VS Code ve **AL Language** eklentisini kullan.
   - Değişken tanımlarını metodun üstündeki `var` bloğuna yaz.
   - Orijinal tabloda değişiklik yapacaksan `tableextension` oluştur.
2. **Blazor Geliştirirken**:
   - UI güncellemeleri tetiklenmezse `StateHasChanged()` çağır.
   - MAUI projelerinde servisleri genel olarak `AddSingleton` ekle.
3. **Business Central Mantığında**:
   - `Post` edilmiş veriyi silmeye çalışma. İade/Düzeltme (Credit Memo) mantığı kurgula.
   - Belge yapısında Header ve Line ilişkilerini doğru kur.
4. **Git Kuralları**:
   - Projede şirket ve müşteri özel isimlerini kesinlikle kullanma.
   - Anlamlı commit mesajları yaz (`feat(01-blazor): add barcode scanner`).

---

*Bu rehber ve yaptığımız 6 örnek proje reponda kayıtlıdır:*
🔗 **GitHub Remote**: `https://github.com/keremaygul/maui-blazor-etc.git`
