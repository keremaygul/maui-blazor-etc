// ============================================================
// 📝 AL DERS 4: CODEUNIT (İş Mantığı / Service Layer)
// ============================================================
// C#'taki Service class'ının AL karşılığı.
//
// Codeunit = static class + DI service karışımı
// Procedure'lar = public method'lar
// Local procedure'lar = private method'lar
// ============================================================

codeunit 50100 "Item Tracking Mgt."
{
    // ============================================================
    // CRUD İŞLEMLERİ
    // ============================================================

    /// <summary>
    /// Yeni bir tracking kaydı oluşturur.
    /// C#: public int CreateEntry(string itemNo, decimal qty, string location)
    /// </summary>
    procedure CreateTrackingEntry(
        ItemNo: Code[20];
        Qty: Decimal;
        LocationCode: Code[10]
    ): Integer
    var
        ItemTracking: Record "Item Tracking";
        Item: Record Item;
    begin
        // 📝 AL'da değişkenler "var" bloğunda ÖNCEDEN tanımlanır
        // C#'taki "var item = new Item()" gibi inline tanımlama YOK

        // Item'ın varlığını kontrol et
        if not Item.Get(ItemNo) then
            Error('Item %1 not found!', ItemNo);
        // Error() → throw new Exception() karşılığı
        // %1, %2... → string interpolation ($"{variable}" yerine)

        // Yeni kayıt başlat
        ItemTracking.Init();
        // Init() → Tüm field'ları default değerlere set et
        // C#: var entry = new ItemTracking();

        ItemTracking."Item No." := ItemNo;
        ItemTracking.Description := Item.Description;
        ItemTracking.Quantity := Qty;
        ItemTracking."Location Code" := LocationCode;
        ItemTracking.Status := ItemTracking.Status::Active;
        // Status::Active → Enum erişim syntax'ı
        // C#: entry.Status = TrackingStatus.Active;

        ItemTracking.Insert(true);
        // Insert(true) → trigger'ları çalıştır (OnInsert)
        // Insert(false) → trigger'ları ÇALIŞTIRMA
        // C#: DbContext.Add(entry); DbContext.SaveChanges();

        exit(ItemTracking."Entry No.");
        // exit(value) → return value;
    end;

    /// <summary>
    /// Miktarı güncelle.
    /// C#: public void UpdateQuantity(int entryNo, decimal newQty)
    /// </summary>
    procedure UpdateQuantity(EntryNo: Integer; NewQty: Decimal)
    var
        ItemTracking: Record "Item Tracking";
    begin
        // Get(PK) → Primary key ile bul
        // C#: var entry = DbContext.Find<ItemTracking>(entryNo);
        if not ItemTracking.Get(EntryNo) then
            Error('Entry %1 not found!', EntryNo);

        ItemTracking.Quantity := NewQty;
        ItemTracking.Modify(true);
        // Modify(true) → UPDATE + trigger çalıştır
    end;

    /// <summary>
    /// Kaydı sil.
    /// </summary>
    procedure DeleteEntry(EntryNo: Integer)
    var
        ItemTracking: Record "Item Tracking";
    begin
        if not ItemTracking.Get(EntryNo) then
            exit; // return; (void metod)

        // Kullanıcıya sor
        if not Confirm('Delete entry %1?', false, EntryNo) then
            exit;

        ItemTracking.Delete(true);
    end;

    // ============================================================
    // SORGULAMA (QUERYING)
    // ============================================================

    /// <summary>
    /// Bir item'ın toplam miktarını hesapla.
    /// C#: items.Where(x => x.ItemNo == itemNo && x.Qty > 0).Sum(x => x.Qty)
    /// </summary>
    procedure GetTotalQuantityByItem(ItemNo: Code[20]): Decimal
    var
        ItemTracking: Record "Item Tracking";
        Total: Decimal;
    begin
        // 📝 Filtreleme — LINQ'un Where'i gibi
        ItemTracking.SetRange("Item No.", ItemNo);
        // SetRange(Field, Value) → WHERE Field = Value
        // SetRange(Field, From, To) → WHERE Field BETWEEN From AND To

        ItemTracking.SetFilter(Quantity, '>%1', 0);
        // SetFilter → Daha karmaşık filtreler için
        // '>%1' → parameterized > operatörü
        // C#: .Where(x => x.Quantity > 0)

        // 📝 Yöntem 1: Döngü ile toplama
        if ItemTracking.FindSet() then
            // FindSet() → Sorguyu çalıştır, ilk kaydı al
            // C#: var list = query.ToList(); if (list.Any())
            repeat
                Total += ItemTracking.Quantity;
            until ItemTracking.Next() = 0;
            // repeat..until Next()=0 → foreach döngüsü
            // Next() = 0 → Başka kayıt yok

        exit(Total);
    end;

    /// <summary>
    /// DAHA HIZLI yöntem: CalcSums kullanarak SQL SUM() çalıştır.
    /// </summary>
    procedure GetTotalQuantityFast(ItemNo: Code[20]): Decimal
    var
        ItemTracking: Record "Item Tracking";
    begin
        ItemTracking.SetRange("Item No.", ItemNo);
        ItemTracking.CalcSums(Quantity);
        // CalcSums → Doğrudan SQL SUM() çalıştırır
        // Döngüden ÇOK daha hızlı!
        // C#: DbContext.ItemTrackings.Where(...).Sum(x => x.Quantity)
        exit(ItemTracking.Quantity);
    end;

    /// <summary>
    /// Duruma göre kayıtları say.
    /// C#: items.Count(x => x.Status == status)
    /// </summary>
    procedure CountByStatus(StatusFilter: Enum "Tracking Status"): Integer
    var
        ItemTracking: Record "Item Tracking";
    begin
        ItemTracking.SetRange(Status, StatusFilter);
        exit(ItemTracking.Count());
        // Count() → SQL COUNT(*)
    end;

    // ============================================================
    // TOPLU İŞLEMLER (BATCH OPERATIONS)
    // ============================================================

    /// <summary>
    /// Tüm active kayıtları completed yap.
    /// C#: await DbContext.ItemTrackings
    ///         .Where(x => x.Status == Active)
    ///         .ExecuteUpdateAsync(x => x.SetProperty(p => p.Status, Completed));
    /// </summary>
    procedure CompleteAllActive()
    var
        ItemTracking: Record "Item Tracking";
    begin
        ItemTracking.SetRange(Status, ItemTracking.Status::Active);

        // 📝 ModifyAll → Toplu güncelleme (tek SQL statement)
        ItemTracking.ModifyAll(Status, ItemTracking.Status::Completed);
        // C#: ExecuteUpdate (EF Core 7+)

        Message('%1 entries have been completed.', ItemTracking.Count());
    end;

    // ============================================================
    // EVENTS (Olaylar)
    // ============================================================

    // 📝 AL DERSİ - EVENT SYSTEM:
    //
    // Publisher/Subscriber pattern (C# event/delegate gibi)
    //
    // [IntegrationEvent] → Event tanımla (publisher)
    // [EventSubscriber] → Event'e abone ol (subscriber)
    //
    // Bu pattern sayesinde extension'lar birbirinin koduna
    // DOKUNMADAN davranış ekleyebilir!

    /// <summary>
    /// Event yayınla — başka extension'lar bu event'e subscribe olabilir.
    /// C#: public event EventHandler<ItemTrackingEventArgs> OnBeforeCreate;
    /// </summary>
    [IntegrationEvent(false, false)]
    local procedure OnBeforeCreateEntry(var ItemTracking: Record "Item Tracking"; var IsHandled: Boolean)
    begin
        // 📝 Event body BOŞ bırakılır!
        // Subscriber'lar kendi logic'lerini ekler.
    end;

    // 📝 Başka bir codeunit'te bu event'e subscribe olma:
    //
    // [EventSubscriber(ObjectType::Codeunit, Codeunit::"Item Tracking Mgt.",
    //     'OnBeforeCreateEntry', '', false, false)]
    // local procedure HandleBeforeCreate(var ItemTracking: Record "Item Tracking"; var IsHandled: Boolean)
    // begin
    //     // Kendi logic'ini ekle
    //     if ItemTracking.Quantity > 1000 then
    //         Error('Quantity too large for automatic processing!');
    // end;

    // ============================================================
    // ERROR HANDLING
    // ============================================================

    /// <summary>
    /// TryFunction — try-catch alternatifi.
    /// C#: try { DoSomething(); } catch { return false; }
    /// </summary>
    [TryFunction]
    procedure TryCreateEntry(ItemNo: Code[20]; Qty: Decimal)
    var
        EntryNo: Integer;
    begin
        // TryFunction:
        // - Hata olursa false döner (exception fırlatmaz)
        // - Başarılıysa true döner
        // - Return type OLMAZ (otomatik Boolean)
        EntryNo := CreateTrackingEntry(ItemNo, Qty, '');
    end;

    // Kullanımı:
    // if not ItemTrackingMgt.TryCreateEntry('ITEM-001', 10) then
    //     Message('Entry creation failed, but app continues!');

    // ============================================================
    // HELPER PROCEDURES
    // ============================================================

    /// <summary>
    /// Local procedure — sadece bu codeunit içinden çağrılabilir.
    /// C#: private void ValidateItem(...)
    /// </summary>
    local procedure ValidateItemExists(ItemNo: Code[20])
    var
        Item: Record Item;
    begin
        Item.SetRange("No.", ItemNo);
        if Item.IsEmpty() then
            Error('Item %1 does not exist.', ItemNo);
    end;

    /// <summary>
    /// Internal procedure — aynı extension içinden çağrılabilir.
    /// C#: internal void DoSomething()
    /// </summary>
    internal procedure GetTrackingCount(): Integer
    var
        ItemTracking: Record "Item Tracking";
    begin
        exit(ItemTracking.Count());
    end;
}
