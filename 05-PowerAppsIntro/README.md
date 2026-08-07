# 05 - Power Apps (Power Platform) Giriş

Microsoft Power Platform ve Power Apps hakkında kapsamlı rehber.

## 🎯 Power Platform Nedir?

Microsoft Power Platform, 4 ana bileşenden oluşur:

```
┌─────────────────────────────────────────────────┐
│              POWER PLATFORM                      │
├────────────┬────────────┬──────────┬─────────────┤
│ Power Apps │ Power      │ Power BI │ Power       │
│            │ Automate   │          │ Virtual     │
│ Uygulama   │ İş akışı   │ Raporlama│ Agents      │
│ geliştirme │ otomasyonu │ & analiz │ AI chatbot  │
└────────────┴────────────┴──────────┴─────────────┘
```

## 📱 Power Apps Türleri

### 1. Canvas App (Tuval Uygulaması)
- **Ne**: Sürükle-bırak ile uygulama tasarla
- **Nerede**: Mobil, tablet, web
- **Ne zaman**: Özel UI gerektiğinde, hızlı prototip
- **Analoji**: WinForms Designer gibi ama web/mobil için

```
Canvas App ≈ Drag & Drop UI Builder
├── Screen (Sayfa)
│   ├── Gallery (DataGrid/ListView)
│   ├── Form (Veri giriş formu)
│   ├── Button, TextInput, Label...
│   └── Her element'in Properties'i var
└── Data Sources (Veri kaynakları)
    ├── SharePoint Lists
    ├── Dataverse Tables
    ├── SQL Server
    ├── Excel Online
    └── Custom Connectors (API)
```

### 2. Model-Driven App
- **Ne**: Veri modeli üzerinden otomatik UI oluşturma
- **Nerede**: Web (responsive)
- **Ne zaman**: Standart iş uygulamaları, CRM benzeri
- **Analoji**: Business Central page'leri gibi (table → otomatik form/list)

```
Model-Driven App ≈ Business Central Page mantığı
├── Dataverse Table (veri modeli)
│   → Otomatik Form ve View oluşturulur
├── Business Rules (validasyon)
├── Charts & Dashboards
└── Security Roles
```

### 3. Power Pages (eski adı: Portal)
- **Ne**: Harici kullanıcılara açık web sitesi
- **Nerede**: Web
- **Ne zaman**: Müşteri/tedarikçi portalı (FiftyPortal gibi!)

---

## 📝 Power Fx Formül Dili

Power Apps'in formül dili **Power Fx**. Excel formüllerine çok benzer.

### Temel Formüller

```
// ============== VERİ İŞLEMLERİ ==============

// Tüm kayıtları getir (SELECT * FROM)
Items                                    // Tablo referansı

// Filtreleme (WHERE)
Filter(Items, Category = "Electronics")  // LINQ: items.Where(x => x.Category == "Electronics")
Filter(Items, Quantity < MinimumStock)    // LINQ: items.Where(x => x.Quantity < x.MinimumStock)

// Arama (LIKE)
Search(Items, SearchBox.Text, "Name", "Description")

// Sıralama (ORDER BY)
SortByColumns(Items, "Name", SortOrder.Ascending)

// İlk kaydı al (FIRST / TOP 1)
First(Items)                             // LINQ: items.First()
Last(Items)                              // LINQ: items.Last()

// Lookup (tek kayıt bul)
LookUp(Items, ID = 5)                   // LINQ: items.Single(x => x.Id == 5)
LookUp(Items, Barcode = "4012345678901")

// Sayma
CountRows(Items)                         // LINQ: items.Count()
CountIf(Items, Status = "Active")        // LINQ: items.Count(x => x.Status == "Active")

// Toplam
Sum(Items, Quantity)                     // LINQ: items.Sum(x => x.Quantity)
Average(Items, Price)                    // LINQ: items.Average(x => x.Price)
Max(Items, Price)                        // LINQ: items.Max(x => x.Price)


// ============== KAYIT İŞLEMLERİ (CRUD) ==============

// Yeni kayıt ekle (INSERT)
Patch(Items, Defaults(Items), {
    Name: "New Product",
    Category: "Electronics",
    Quantity: 50,
    Price: 29.99
})

// Güncelle (UPDATE)
Patch(Items, LookUp(Items, ID = 5), {
    Quantity: 100,
    Price: 35.00
})

// Sil (DELETE)
Remove(Items, LookUp(Items, ID = 5))

// Toplu güncelleme
ForAll(
    Filter(Items, Status = "Pending"),
    Patch(Items, ThisRecord, { Status: "Active" })
)


// ============== NAVIGASYON ==============

// Başka ekrana git
Navigate(DetailScreen, ScreenTransition.Fade)

// Geri dön
Back()

// Parametre ile git
Navigate(EditScreen, ScreenTransition.None, { SelectedItem: Gallery1.Selected })


// ============== KOŞULLAR & MANTIK ==============

// If
If(Quantity < MinimumStock, "Low Stock", "OK")

// Switch
Switch(Status,
    "Active", "🟢 Active",
    "Low", "🟡 Low Stock",
    "Out", "🔴 Out of Stock",
    "❓ Unknown"
)

// Ve / Veya
If(Quantity > 0 And Status = "Active", true, false)
If(Category = "Electronics" Or Category = "Accessories", true, false)


// ============== STRING İŞLEMLERİ ==============

Concatenate("Hello ", User().FullName)   // String.Concat
"Hello " & User().FullName               // & operatörü
Text(Price, "€#,##0.00")                // ToString("C2")
Upper("hello")                          // ToUpper()
Lower("HELLO")                          // ToLower()
Len("Hello")                            // .Length
Left("Hello World", 5)                  // Substring(0, 5) → "Hello"
Mid("Hello World", 7, 5)               // Substring(6, 5) → "World"


// ============== TARİH İŞLEMLERİ ==============

Today()                                 // DateTime.Today
Now()                                   // DateTime.Now
DateAdd(Today(), 30, TimeUnit.Days)     // DateTime.Today.AddDays(30)
DateDiff(StartDate, EndDate, TimeUnit.Days)  // (EndDate - StartDate).Days
Year(Today())                           // DateTime.Today.Year
Month(Today())                          // DateTime.Today.Month
```

---

## 🔄 Power Fx vs C# Karşılaştırma

| Power Fx | C# / LINQ | Açıklama |
|----------|-----------|----------|
| `Filter(Items, Qty > 10)` | `items.Where(x => x.Qty > 10)` | Filtreleme |
| `LookUp(Items, ID = 5)` | `items.Single(x => x.Id == 5)` | Tek kayıt bul |
| `Patch(Items, record, {...})` | `dbContext.Update(record)` | Güncelle |
| `Remove(Items, record)` | `dbContext.Remove(record)` | Sil |
| `CountRows(Items)` | `items.Count()` | Sayma |
| `Sum(Items, Price)` | `items.Sum(x => x.Price)` | Toplam |
| `SortByColumns(Items, "Name")` | `items.OrderBy(x => x.Name)` | Sıralama |
| `FirstN(Items, 10)` | `items.Take(10)` | İlk N kayıt |
| `Collect(Collection, record)` | `list.Add(record)` | Koleksiyona ekle |
| `ClearCollect(Col, Items)` | `list = items.ToList()` | Koleksiyon oluştur |
| `Navigate(Screen)` | `Navigation.NavigateTo()` | Sayfa yönlendirme |
| `Set(varName, value)` | `var varName = value` | Global değişken |
| `UpdateContext({x: 1})` | `var x = 1` | Lokal değişken |

---

## 🏗 Power Automate (Flow) Temelleri

Power Automate, iş akışlarını otomatikleştirmek için kullanılır.

### Yaygın Senaryolar
1. **Yeni e-posta gelince** → SharePoint'e kaydet
2. **Form doldurulunca** → Onay süreci başlat → Email gönder
3. **Stok düşük seviyeye inince** → Teams'de bildirim gönder
4. **Her gün saat 9:00'da** → Rapor oluştur ve email ile gönder

### Temel Bileşenler
```
Trigger (Tetikleyici)
    ↓
Action (Eylem)
    ↓
Condition (Koşul)
   ├── Yes → Action A
   └── No  → Action B
    ↓
Apply to Each (Döngü)
    ↓
Response (Yanıt)
```

### Connector'lar (Bağlantılar)
- **Microsoft 365**: Outlook, Teams, SharePoint, OneDrive, Excel
- **Dynamics 365**: Business Central, Sales, Customer Service
- **Azure**: SQL, Blob Storage, Functions
- **3rd Party**: SAP, Salesforce, Google, Slack, Twitter
- **Custom**: HTTP Request, Custom Connector (kendi API'n)

---

## 📊 Dataverse (Common Data Service)

Power Platform'un veritabanı. Business Central'daki tablo yapısına benzer.

| Dataverse | Business Central (AL) | C# (EF Core) |
|-----------|----------------------|---------------|
| Table | Table | DbSet<T> |
| Column | Field | Property |
| Row | Record | Entity instance |
| View | Page (List) | IQueryable |
| Form | Page (Card) | Razor Page |
| Business Rule | Trigger | FluentValidation |
| Choice | Enum | Enum |
| Lookup | TableRelation | Navigation Property |

---

## 🔗 Power Apps + Business Central

Power Apps, Business Central ile doğrudan entegre olabilir:

1. **BC Connector**: Power Apps içinden BC API'lerine erişim
2. **Power Automate**: BC event'lerini dinleme, akış tetikleme
3. **Power BI**: BC verilerini raporlama
4. **Dataverse Virtual Tables**: BC tablolarını Dataverse'te sanal tablo olarak gösterme

### Örnek Senaryo: Depo Tarama Uygulaması
```
Power App (Canvas)
    ↓ [Barkod tara]
Power Automate Flow
    ↓ [BC API çağır]
Business Central
    ↓ [Stok güncelle]
Power BI Dashboard
    ↓ [Raporu güncelle]
Teams Notification
    ↓ [Stok uyarısı gönder]
```

---

## 🛠 Geliştirme Ortamı

### Ücretsiz Başlangıç
1. **Power Apps Developer Plan**: [powerapps.microsoft.com/developer-plan](https://powerapps.microsoft.com/en-us/developerplan/)
   - Ücretsiz Dataverse ortamı
   - Canvas & Model-driven app oluşturma
   - Power Automate akışları

2. **Microsoft 365 Developer Program**: [developer.microsoft.com/microsoft-365/dev-program](https://developer.microsoft.com/en-us/microsoft-365/dev-program)
   - 25 lisanslı E5 sandbox
   - SharePoint, Teams, Exchange dahil

---

## 💡 İpuçları

1. **Delegation**: Power Apps büyük veri setlerinde (>2000 satır) filtrelemeyi sunucuya devreder. Tüm fonksiyonlar delegation desteklemez — `Search()` desteklemez ama `Filter()` destekler.

2. **Collections**: Lokal (offline) veri tutmak için Collection kullan. `ClearCollect()` ile sunucudan veri çek, sonra lokal olarak işle.

3. **Variables**: 
   - `Set(varName, value)` → Global (tüm ekranlarda geçerli)
   - `UpdateContext({varName: value})` → Lokal (sadece o ekranda)

4. **Components**: Tekrar kullanılabilir UI parçaları (Blazor component gibi). Library olarak paylaşılabilir.

5. **Responsive Layout**: Containers kullanarak responsive tasarım yap. `App.Width` ve `App.Height` ile dinamik boyutlandırma.

---

## 📚 Kaynaklar

- [Power Apps Documentation](https://learn.microsoft.com/en-us/power-apps/)
- [Power Fx Reference](https://learn.microsoft.com/en-us/power-platform/power-fx/overview)
- [Power Automate Documentation](https://learn.microsoft.com/en-us/power-automate/)
- [Power Apps Training Path](https://learn.microsoft.com/en-us/training/browse/?products=power-apps)
