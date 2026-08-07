// ============================================================
// 📝 AL DERS 1: ENUM (Sabit Değerler)
// ============================================================
// C#'taki enum'un AL karşılığı.
// 
// Fark: AL enum'ları ID tabanlıdır ve Caption (görünen ad) içerir.
// Extensible = true → Başka extension'lar yeni value ekleyebilir.
// ============================================================

enum 50100 "Tracking Status"
{
    Extensible = true;
    Caption = 'Tracking Status';

    value(0; " ")
    {
        Caption = ' ';
    }
    value(1; "Active")
    {
        Caption = 'Active';
    }
    value(2; "Completed")
    {
        Caption = 'Completed';
    }
    value(3; "Cancelled")
    {
        Caption = 'Cancelled';
    }
}

// ============================================================
// C# Karşılığı:
// ============================================================
// public enum TrackingStatus
// {
//     None = 0,
//     Active = 1,
//     Completed = 2,
//     Cancelled = 3
// }
// ============================================================
