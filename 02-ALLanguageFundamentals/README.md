# 02 - AL Programming Language Fundamentals

Microsoft Dynamics 365 Business Central'ın programlama dili **AL** (Application Language) hakkında kapsamlı bir rehber.

## 🎯 AL Nedir?

AL, Microsoft Dynamics 365 Business Central için extension (uzantı) geliştirmek için kullanılan programlama dilidir.

### Tarihçe
```
C/SIDE → C/AL → AL
(Eski)    (Geçiş)  (Güncel)
```

- **C/SIDE**: Dynamics NAV'ın eski geliştirme ortamı (2018 öncesi)
- **C/AL**: C/SIDE içinde kullanılan eski dil
- **AL**: Modern VS Code tabanlı dil (2018+, Business Central ile)

> **Önemli**: Yeni projeler %100 AL ile yazılır. C/AL sadece eski (legacy) sistemlerde karşına çıkar.

---

## 📁 AL Proje Yapısı

```
MyExtension/
├── .vscode/
│   └── launch.json          ← Debug konfigürasyonu
├── src/
│   ├── table/
│   │   └── ItemTracking.Table.al
│   ├── page/
│   │   ├── ItemTrackingList.Page.al
│   │   └── ItemTrackingCard.Page.al
│   ├── codeunit/
│   │   └── ItemTrackingMgt.Codeunit.al
│   ├── report/
│   │   └── ItemTrackingReport.Report.al
│   ├── enum/
│   │   └── TrackingStatus.Enum.al
│   └── pageextension/
│       └── ItemListExt.PageExtension.al
├── app.json                  ← Extension manifest (NuGet'teki .csproj gibi)
├── .editorconfig
└── README.md
```

---

## 🧩 AL'ın Temel Objeleri

### 1. Table (Tablo) — Veritabanı Tablosu

C#'taki Entity/Model class'ına benzer. Veritabanı tablosunu tanımlar.

```al
// 📝 AL DERSİ - TABLE:
// Her table'ın benzersiz bir ID'si olmalı (50000-99999 arası custom range)
// field() içinde: ID, isim, veri tipi

table 50100 "Item Tracking"
{
    Caption = 'Item Tracking';
    DataClassification = CustomerContent;

    fields
    {
        field(1; "Entry No."; Integer)
        {
            Caption = 'Entry No.';
            AutoIncrement = true;
        }
        field(2; "Item No."; Code[20])
        {
            Caption = 'Item No.';
            // TableRelation → Foreign Key (C#'taki navigation property gibi)
            TableRelation = Item."No.";
        }
        field(3; "Description"; Text[100])
        {
            Caption = 'Description';
        }
        field(4; "Quantity"; Decimal)
        {
            Caption = 'Quantity';
            // Trigger → field değiştiğinde otomatik çalışır
            trigger OnValidate()
            begin
                if Quantity < 0 then
                    Error('Quantity cannot be negative!');
            end;
        }
        field(5; "Location Code"; Code[10])
        {
            Caption = 'Location Code';
            TableRelation = Location.Code;
        }
        field(6; "Status"; Enum "Tracking Status")
        {
            Caption = 'Status';
        }
        field(7; "Created Date"; Date)
        {
            Caption = 'Created Date';
        }
        field(8; "Created By"; Code[50])
        {
            Caption = 'Created By';
        }
    }

    keys
    {
        // Primary Key
        key(PK; "Entry No.")
        {
            Clustered = true;
        }
        // Secondary Key (sorgularda performans için)
        key(ItemNo; "Item No.", "Location Code") { }
    }

    // Table-level trigger'lar
    trigger OnInsert()
    begin
        "Created Date" := Today;
        "Created By" := UserId;
    end;

    trigger OnModify()
    begin
        // Her güncelleme loglanabilir
    end;

    trigger OnDelete()
    begin
        // Silme öncesi kontroller
    end;
}
```

**C# Karşılığı:**
```csharp
// AL table ≈ C# Entity + FluentAPI + Trigger
[Table("ItemTracking")]
public class ItemTracking
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int EntryNo { get; set; }

    [MaxLength(20)]
    [ForeignKey("Item")]
    public string ItemNo { get; set; }

    [MaxLength(100)]
    public string Description { get; set; }

    public decimal Quantity { get; set; }
    // ... vs.
}
```

---

### 2. Page (Sayfa) — UI Sayfası

Blazor'daki Razor Page'e benzer. Kullanıcıya gösterilen arayüz.

```al
// 📝 AL DERSİ - PAGE:
// PageType: List, Card, Document, Worksheet, RoleCenter...
// List → Tablo görünümü (DataGrid)
// Card → Detay/form görünümü

// === LIST PAGE (Tablo Listesi) ===
page 50100 "Item Tracking List"
{
    PageType = List;
    ApplicationArea = All;
    UsageCategory = Lists;
    SourceTable = "Item Tracking";    // Hangi table'dan veri çekecek
    CardPageId = "Item Tracking Card"; // Satıra tıklayınca açılacak card page
    Caption = 'Item Tracking List';
    Editable = false;

    layout
    {
        area(Content)
        {
            repeater(Lines)  // repeater = her satır için tekrarla (foreach gibi)
            {
                field("Entry No."; Rec."Entry No.") { }
                field("Item No."; Rec."Item No.") { }
                field(Description; Rec.Description) { }
                field(Quantity; Rec.Quantity)
                {
                    // Koşullu stil
                    StyleExpr = QuantityStyle;
                }
                field("Location Code"; Rec."Location Code") { }
                field(Status; Rec.Status) { }
            }
        }
    }

    actions
    {
        area(Processing)
        {
            action(CreateNew)
            {
                Caption = 'Create New Entry';
                Image = New;
                Promoted = true;
                PromotedCategory = Process;

                trigger OnAction()
                begin
                    // Yeni kayıt oluşturma logic'i
                    Message('New entry will be created!');
                end;
            }
        }
    }

    var
        QuantityStyle: Text;

    trigger OnAfterGetRecord()
    begin
        // Her satır render edilirken çalışır
        if Rec.Quantity <= 0 then
            QuantityStyle := 'Unfavorable'
        else if Rec.Quantity < 10 then
            QuantityStyle := 'Ambiguous'
        else
            QuantityStyle := 'Favorable';
    end;
}

// === CARD PAGE (Detay Sayfası) ===
page 50101 "Item Tracking Card"
{
    PageType = Card;
    ApplicationArea = All;
    SourceTable = "Item Tracking";
    Caption = 'Item Tracking Card';

    layout
    {
        area(Content)
        {
            group(General)
            {
                Caption = 'General';
                field("Entry No."; Rec."Entry No.")
                {
                    Editable = false;  // Auto-increment olduğu için
                }
                field("Item No."; Rec."Item No.") { }
                field(Description; Rec.Description) { }
                field(Status; Rec.Status) { }
            }
            group(Details)
            {
                Caption = 'Details';
                field(Quantity; Rec.Quantity) { }
                field("Location Code"; Rec."Location Code") { }
                field("Created Date"; Rec."Created Date")
                {
                    Editable = false;
                }
                field("Created By"; Rec."Created By")
                {
                    Editable = false;
                }
            }
        }
    }
}
```

---

### 3. Codeunit — İş Mantığı (Service Layer)

C#'taki Service class'ına birebir karşılık gelir.

```al
// 📝 AL DERSİ - CODEUNIT:
// İş mantığını table/page'den ayırır (Separation of Concerns)
// DI benzeri: Codeunit'leri birbirinden çağırabilirsin

codeunit 50100 "Item Tracking Mgt."
{
    // Public prosedür — diğer codeunit'lerden çağrılabilir
    procedure CreateTrackingEntry(
        ItemNo: Code[20];
        Qty: Decimal;
        LocationCode: Code[10]
    ): Integer
    var
        ItemTracking: Record "Item Tracking";
        Item: Record Item;
    begin
        // Item var mı kontrol et
        if not Item.Get(ItemNo) then
            Error('Item %1 not found!', ItemNo);

        // Yeni kayıt oluştur
        ItemTracking.Init();
        ItemTracking."Item No." := ItemNo;
        ItemTracking.Description := Item.Description;
        ItemTracking.Quantity := Qty;
        ItemTracking."Location Code" := LocationCode;
        ItemTracking.Status := ItemTracking.Status::Active;
        ItemTracking.Insert(true);  // true = trigger'ları çalıştır

        exit(ItemTracking."Entry No.");
    end;

    // 📝 AL DERSİ - Record İŞLEMLERİ:
    // Record = C#'taki DbSet<T> entity instance'ı
    procedure UpdateQuantity(EntryNo: Integer; NewQty: Decimal)
    var
        ItemTracking: Record "Item Tracking";
    begin
        // Get → Primary key ile bul (SingleOrDefault gibi)
        if not ItemTracking.Get(EntryNo) then
            Error('Entry %1 not found!', EntryNo);

        ItemTracking.Quantity := NewQty;
        ItemTracking.Modify(true);  // UPDATE + trigger
    end;

    // 📝 AL DERSİ - FILTERING & LOOPING:
    procedure GetTotalQuantityByItem(ItemNo: Code[20]): Decimal
    var
        ItemTracking: Record "Item Tracking";
        Total: Decimal;
    begin
        // SetRange/SetFilter = WHERE clause
        ItemTracking.SetRange("Item No.", ItemNo);
        ItemTracking.SetFilter(Quantity, '>%1', 0);

        // FindSet + repeat..until = foreach döngüsü
        if ItemTracking.FindSet() then
            repeat
                Total += ItemTracking.Quantity;
            until ItemTracking.Next() = 0;

        exit(Total);
    end;

    // 📝 AL DERSİ - CalcSums (SQL SUM() gibi):
    procedure GetTotalQuantityFast(ItemNo: Code[20]): Decimal
    var
        ItemTracking: Record "Item Tracking";
    begin
        ItemTracking.SetRange("Item No.", ItemNo);
        ItemTracking.CalcSums(Quantity);
        exit(ItemTracking.Quantity);
    end;

    // Event yayınlama (C#'taki event/delegate gibi)
    [IntegrationEvent(false, false)]
    local procedure OnBeforeCreateEntry(var ItemTracking: Record "Item Tracking")
    begin
        // Boş bırakılır — subscriber'lar doldurur
    end;
}
```

---

### 4. Enum (Sabit Değerler)

```al
enum 50100 "Tracking Status"
{
    Extensible = true;  // Diğer extension'lar yeni değer ekleyebilir
    
    value(0; " ") { Caption = ' '; }
    value(1; Active) { Caption = 'Active'; }
    value(2; Completed) { Caption = 'Completed'; }
    value(3; Cancelled) { Caption = 'Cancelled'; }
}
```

---

### 5. Page Extension & Table Extension

**En önemli kavram!** Mevcut BC objelerini DEĞİŞTİRMEDEN genişletme.

```al
// 📝 AL DERSİ - EXTENSION MODEL:
// BC'nin core koduna DOKUNMADAN yeni alanlar/özellikler eklenir.
// Bu, upgrade'leri güvenli yapar ve birden fazla extension'ın
// aynı objeyi genişletmesine izin verir.

// Mevcut Item tablosuna yeni alan ekleme
tableextension 50100 "Item Tracking Ext" extends Item
{
    fields
    {
        field(50100; "Tracking Enabled"; Boolean)
        {
            Caption = 'Tracking Enabled';
            DataClassification = CustomerContent;
        }
    }
}

// Mevcut Item Card sayfasına yeni alan ekleme
pageextension 50100 "Item Card Tracking Ext" extends "Item Card"
{
    layout
    {
        // 'addafter' → Mevcut bir alanın SONRASINA ekle
        addafter(Description)
        {
            field("Tracking Enabled"; Rec."Tracking Enabled")
            {
                ApplicationArea = All;
            }
        }
    }
}
```

---

### 6. Report (Rapor)

```al
report 50100 "Item Tracking Report"
{
    Caption = 'Item Tracking Report';
    DefaultRenderingLayout = LayoutRDLC;
    
    dataset
    {
        dataitem(ItemTracking; "Item Tracking")
        {
            column(EntryNo; "Entry No.") { }
            column(ItemNo; "Item No.") { }
            column(Description; Description) { }
            column(Quantity; Quantity) { }
            column(Status; Status) { }
        }
    }

    rendering
    {
        layout(LayoutRDLC)
        {
            Type = RDLC;
            LayoutFile = './src/report/ItemTrackingReport.rdl';
        }
    }
}
```

---

## 🔄 AL vs C# Karşılaştırma Tablosu

| Kavram | C# | AL |
|--------|----|----|
| Sınıf | `class` | `codeunit` |
| Model/Entity | `class Product {...}` | `table 50100 "Product" {...}` |
| UI Sayfası | Razor Page / Blazor Component | `page 50100 "Product List" {...}` |
| İlişki (FK) | `[ForeignKey]` / Navigation Property | `TableRelation = Item."No."` |
| LINQ Where | `.Where(x => x.Name == "abc")` | `SetRange(Name, 'abc')` |
| LINQ Select | `.Select(x => x.Price)` | `CalcSums(Price)` |
| foreach | `foreach (var item in list)` | `if FindSet() then repeat...until Next() = 0` |
| try-catch | `try { } catch { }` | `if not TryFunction() then...` |
| Event | `event EventHandler` | `[IntegrationEvent]` |
| Subscribe | `obj.Event += Handler` | `[EventSubscriber]` |
| Enum | `enum Status { Active, Done }` | `enum 50100 "Status" { value(0; Active)... }` |
| NULL check | `if (obj == null)` | `if not Record.Get(ID) then` |
| String format | `$"Hello {name}"` | `StrSubstNo('Hello %1', name)` |
| Print/Log | `Console.WriteLine()` | `Message()` |
| Exception | `throw new Exception()` | `Error()` |
| Metod | `public void DoSomething()` | `procedure DoSomething()` |
| Lokal değişken | `var x = 5;` | `var x: Integer;` (var bloğu) |

---

## 🔑 Sık Kullanılan AL Fonksiyonları

```al
// Tarih & Zaman
Today                    // DateTime.Today
Time                     // DateTime.Now.TimeOfDay
WorkDate()               // İş tarihi (kullanıcı ayarlı)
CurrentDateTime          // DateTime.Now
CalcDate('1M', Today)    // Bugüne 1 ay ekle

// String İşlemleri
StrSubstNo('Merhaba %1, yaşınız %2', 'Kerem', 25)  // String.Format
CopyStr(Text, 1, 10)    // Substring(0, 10)
StrLen(Text)             // Text.Length
UpperCase(Text)          // Text.ToUpper()
LowerCase(Text)          // Text.ToLower()
DelChr(Text, '<>', ' ')  // Text.Trim()

// Sayısal
Round(3.456, 0.01)       // Math.Round(3.456, 2)
Abs(-5)                  // Math.Abs(-5)
Power(2, 10)             // Math.Pow(2, 10)

// Dialog & Kullanıcı
Message('Bilgilendirme: %1', Value)     // Console.WriteLine / MessageBox
Error('Hata: %1', ErrorMsg)              // throw new Exception()
Confirm('Emin misiniz?')                // MessageBox.Show (Yes/No)
StrMenu('Seçenek 1,Seçenek 2,Seçenek 3') // ComboBox seçimi

// Kayıt İşlemleri (CRUD)
Rec.Init()               // new Entity()
Rec.Insert(true)         // DbContext.Add() + SaveChanges() + triggers
Rec.Modify(true)         // DbContext.Update() + SaveChanges() + triggers  
Rec.Delete(true)         // DbContext.Remove() + SaveChanges() + triggers
Rec.Get(PrimaryKey)      // DbContext.Find(id)
Rec.FindFirst()          // .First()
Rec.FindLast()           // .Last()
Rec.FindSet()            // .ToList() - iterasyon için
Rec.IsEmpty()            // !.Any()
Rec.Count()              // .Count()
Rec.SetRange(Field, Value)        // .Where(x => x.Field == Value)
Rec.SetFilter(Field, '>%1', 100)  // .Where(x => x.Field > 100)
Rec.SetCurrentKey(Field1, Field2) // .OrderBy()
```

---

## 🛠 Geliştirme Ortamı Kurulumu

### Gereksinimler
1. **VS Code** + **AL Language** extension (Microsoft)
2. **Business Central Sandbox** (Microsoft 365 Developer Program veya Docker)

### Docker ile Lokal BC Kurulumu (Opsiyonel)
```powershell
# BcContainerHelper modülünü yükle
Install-Module BcContainerHelper -Force

# Sandbox container oluştur
New-BcContainer `
    -accept_eula `
    -containerName "bcsandbox" `
    -imageName "mcr.microsoft.com/businesscentral/sandbox" `
    -auth UserPassword `
    -credential (Get-Credential) `
    -updateHosts
```

### app.json (Extension Manifest)
```json
{
    "id": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "name": "Item Tracking Extension",
    "publisher": "Learning",
    "version": "1.0.0.0",
    "brief": "A learning project for AL development",
    "description": "Stock/item tracking extension for Business Central",
    "privacyStatement": "",
    "EULA": "",
    "help": "",
    "url": "",
    "logo": "",
    "dependencies": [],
    "screenshots": [],
    "platform": "25.0.0.0",
    "application": "25.0.0.0",
    "idRanges": [
        {
            "from": 50100,
            "to": 50149
        }
    ],
    "resourceExposurePolicy": {
        "allowDebugging": true,
        "allowDownloadingSource": true,
        "includeSourceInSymbolFile": true
    },
    "runtime": "14.0",
    "target": "Cloud"
}
```

---

## 💡 İpuçları

1. **ID Aralıkları**: Her müşteri/proje için ayrı ID range kullanılır (çakışma olmasın diye)
2. **CamelCase değil**: AL'da alan isimleri "Quoted Identifiers" kullanır: `"Item No."`, `"Location Code"`
3. **Rec**: Page içinde `Rec` otomatik olarak current record'u temsil eder
4. **trigger vs procedure**: Trigger'lar otomatik çalışır, procedure'lar elle çağrılır
5. **true parametresi**: `Insert(true)` → trigger'ları çalıştır, `Insert(false)` → çalıştırma

---

## 📚 Kaynaklar

- [AL Language Documentation](https://learn.microsoft.com/en-us/dynamics365/business-central/dev-itpro/developer/devenv-reference-overview)
- [Business Central Learning Path](https://learn.microsoft.com/en-us/training/browse/?products=dynamics-business-central)
- [AL GitHub Samples](https://github.com/microsoft/AL)
