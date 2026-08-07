# 03 - Business Central Basics & ERP Simulation

Microsoft Dynamics 365 Business Central'ın temel mimarisi, modülleri ve iş süreçlerini simüle eden Blazor uygulaması.

## 🎯 Bu Projede Öğrenilen ERP Kavramları

### 1. Document-Centric Architecture (Belge Mimarisi)
Business Central'da veriler "Document" (Belge) mantığında tutulur:
- **Header (Başlık)**: Müşteri bilgisi, tarih, genel şartlar (`SalesOrder`)
- **Lines (Satırlar)**: Sipariş edilen ürünler, miktarlar, fiyatlar (`SalesOrderLine`)

### 2. Standard Business Central Document Flow (İş Akışı)
```
Sales Order → Release → Post Shipment → Post Invoice → Payment
  (Sipariş)    (Onay)     (Sevkiyat)     (Fatura)      (Ödeme)
```

- **Open**: Sipariş taslak halinde, düzenlenebilir.
- **Release**: Sipariş onaylandı, değişikliklere kilitlendi. Stok ve kredi limiti kontrolü yapılır.
- **Post Shipment**: Ürünler depodan sevk edildi. Stok düşer, sevkiyat kaydı oluşur.
- **Post Invoice**: Fatura kesildi. Müşteri açık bakiyesi artar, Posted Invoice belgesi oluşur.

### 3. Business Central Core Concepts
- **Posting**: Kaydı kesinleştirme (geri alınamaz, ters kayıt gerekir).
- **FlowFields**: Veritabanında depolanmayan, dinamik hesaplanan alanlar (örn. Toplam Sipariş Tutarı).
- **Credit Limit**: Müşteri borç riski kontrolü.

## 📁 Proje Yapısı

```
03-BusinessCentralBasics/
├── Models/
│   └── ErpModels.cs        ← Customer, Item, SalesOrder, Lines, PostedInvoice
├── Services/
│   └── ErpService.cs       ← BC Codeunit simülasyonu (Release, PostShipment, PostInvoice)
├── Components/
│   ├── Pages/
│   │   ├── Home.razor      ← RoleCenter Dashboard
│   │   └── SalesOrders.razor ← Order Card & Posting Operations
│   └── Layout/
│       ├── MainLayout.razor
│       └── NavMenu.razor
└── wwwroot/
    └── app.css             ← Dark theme ERP UI
```

## ▶️ Çalıştırma

```bash
cd 03-BusinessCentralBasics
dotnet run
```
