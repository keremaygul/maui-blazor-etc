// ============================================================
// 📝 AL DERS 2: TABLE (Veritabanı Tablosu)
// ============================================================
// C#'taki Entity class + EF Configuration birleşimi.
//
// Her field'ın:
//   - Benzersiz bir ID'si (1, 2, 3...)
//   - Bir adı ("Entry No.")
//   - Bir veri tipi (Integer, Code, Text, Decimal, Date...)
//
// Trigger'lar: OnInsert, OnModify, OnDelete, OnRename
// Field trigger: OnValidate (field değiştiğinde)
// ============================================================

table 50100 "Item Tracking"
{
    Caption = 'Item Tracking';
    DataClassification = CustomerContent;
    // DataClassification → GDPR uyumluluğu için veri sınıflandırması

    fields
    {
        // 📝 Primary Key alanı — AutoIncrement ile otomatik artar
        field(1; "Entry No."; Integer)
        {
            Caption = 'Entry No.';
            AutoIncrement = true;
            // C#: [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        }

        // 📝 Code tipi — büyük harf, sabit uzunluk (varchar gibi ama uppercase)
        field(2; "Item No."; Code[20])
        {
            Caption = 'Item No.';
            NotBlank = true;
            // TableRelation → Foreign Key ilişkisi
            // C#: [ForeignKey("Item")] public string ItemNo { get; set; }
            TableRelation = Item."No.";

            trigger OnValidate()
            var
                Item: Record Item;
            begin
                // Item tablosundan bilgileri otomatik çek
                if Item.Get("Item No.") then
                    Description := Item.Description;
            end;
        }

        // 📝 Text tipi — serbest metin alanı (nvarchar gibi)
        field(3; "Description"; Text[100])
        {
            Caption = 'Description';
        }

        field(4; "Quantity"; Decimal)
        {
            Caption = 'Quantity';
            DecimalPlaces = 0 : 5;
            // DecimalPlaces = min : max → ondalık basamak sınırı
            // C#'ta [Column(TypeName = "decimal(18,5)")] gibi

            trigger OnValidate()
            begin
                if Quantity < 0 then
                    Error('Quantity cannot be negative! Current value: %1', Quantity);
                // C#: throw new ValidationException(...)
            end;
        }

        field(5; "Location Code"; Code[10])
        {
            Caption = 'Location Code';
            TableRelation = Location.Code;
        }

        // 📝 Enum tipi — kendi tanımladığımız enum'u kullanma
        field(6; "Status"; Enum "Tracking Status")
        {
            Caption = 'Status';
            InitValue = Active;
            // InitValue → Default değer
            // C#: public TrackingStatus Status { get; set; } = TrackingStatus.Active;
        }

        field(7; "Created Date"; Date)
        {
            Caption = 'Created Date';
            Editable = false;
            // Editable = false → UI'da düzenlenemez
        }

        field(8; "Created By"; Code[50])
        {
            Caption = 'Created By';
            Editable = false;
            TableRelation = User."User Name";
        }

        // 📝 FlowField — Hesaplanmış alan (SQL View gibi)
        // Veritabanında DEPOLANMAZ, her seferinde hesaplanır
        field(9; "Total Value"; Decimal)
        {
            Caption = 'Total Value';
            FieldClass = FlowField;
            CalcFormula = sum("Item Tracking"."Quantity" where("Item No." = field("Item No.")));
            Editable = false;
            // C# karşılığı: 
            // public decimal TotalValue => DbContext.ItemTrackings
            //     .Where(x => x.ItemNo == this.ItemNo)
            //     .Sum(x => x.Quantity);
        }
    }

    keys
    {
        // 📝 Primary Key — her table'da zorunlu
        key(PK; "Entry No.")
        {
            Clustered = true;
        }
        // 📝 Secondary Key — sorgu performansı için index
        key(ItemLocation; "Item No.", "Location Code")
        {
            // C#: [Index(nameof(ItemNo), nameof(LocationCode))]
        }
        // 📝 SumIndexField — CalcSums ile hızlı toplam için
        key(ItemQty; "Item No.")
        {
            SumIndexFields = Quantity;
        }
    }

    // ============================================================
    // TABLE TRIGGER'LARI
    // ============================================================

    // 📝 OnInsert → C#'taki SaveChanges() override (Added state)
    trigger OnInsert()
    begin
        "Created Date" := Today;       // Bugünün tarihi
        "Created By" := CopyStr(UserId, 1, MaxStrLen("Created By"));  // Aktif kullanıcı
    end;

    // 📝 OnModify → Entity state = Modified
    trigger OnModify()
    begin
        // Güncelleme loglaması yapılabilir
    end;

    // 📝 OnDelete → Entity state = Deleted
    trigger OnDelete()
    begin
        // İlişkili kayıtları temizle
        // Veya Error() ile silmeyi engelle
    end;

    // 📝 OnRename → Primary key değiştiğinde
    trigger OnRename()
    begin
        Error('Entry No. cannot be changed!');
    end;
}
