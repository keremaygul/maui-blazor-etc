// ============================================================
// 📝 AL DERS 5: EXTENSIONS (Mevcut Objeleri Genişletme)
// ============================================================
// AL'ın EN ÖNEMLİ konsepti!
//
// Extension modeli şu anlama gelir:
// - BC'nin core koduna DOKUNMUYORSUN
// - Mevcut tablo/sayfalara yeni alanlar/özellikler EKLİYORSUN
// - Birden fazla extension aynı objeyi bağımsızca genişletebilir
// - Upgrade'ler güvenli olur (core değişmemiş)
//
// C# Karşılığı: partial class + dependency injection kombinasyonu
// Ama daha güçlü — runtime'da ekleniyor!
// ============================================================

// ============== TABLE EXTENSION ==============
// Mevcut "Item" tablosuna yeni alanlar ekleme
tableextension 50100 "Item Tracking Ext" extends Item
{
    fields
    {
        // 📝 Yeni alan ekliyoruz — mevcut tabloya dokunmadan!
        field(50100; "Tracking Enabled"; Boolean)
        {
            Caption = 'Tracking Enabled';
            DataClassification = CustomerContent;

            trigger OnValidate()
            begin
                if "Tracking Enabled" then
                    Message('Tracking has been enabled for item %1', "No.");
            end;
        }

        field(50101; "Default Tracking Location"; Code[10])
        {
            Caption = 'Default Tracking Location';
            DataClassification = CustomerContent;
            TableRelation = Location.Code;
        }

        field(50102; "Last Tracking Date"; Date)
        {
            Caption = 'Last Tracking Date';
            DataClassification = CustomerContent;
            Editable = false;
        }
    }

    // 📝 C# Karşılığı (kavramsal):
    // Bu TAM olarak böyle yapılamaz C#'ta, ama en yakın karşılık:
    //
    // // partial class ile extension (derleme zamanı)
    // public partial class Item
    // {
    //     public bool TrackingEnabled { get; set; }
    //     public string DefaultTrackingLocation { get; set; }
    //     public DateTime? LastTrackingDate { get; set; }
    // }
    //
    // FARK: AL'da bu RUNTIME'da eklenir, partial class compile-time!
}

// ============== PAGE EXTENSION ==============
// Mevcut "Item Card" sayfasına yeni alanları ekleme
pageextension 50100 "Item Card Tracking Ext" extends "Item Card"
{
    layout
    {
        // 📝 addafter → Mevcut bir alanın SONRASINA ekle
        // Diğer seçenekler: addbefore, addlast, addfirst
        addafter(Description)
        {
            group(TrackingInfo)
            {
                Caption = 'Tracking Settings';

                field("Tracking Enabled"; Rec."Tracking Enabled")
                {
                    ApplicationArea = All;
                    ToolTip = 'Enable or disable item tracking for this item.';
                }
                field("Default Tracking Location"; Rec."Default Tracking Location")
                {
                    ApplicationArea = All;
                    ToolTip = 'Default warehouse location for tracking entries.';
                    // Editable → Sadece tracking enabled ise düzenlenebilir
                    Editable = Rec."Tracking Enabled";
                }
                field("Last Tracking Date"; Rec."Last Tracking Date")
                {
                    ApplicationArea = All;
                    ToolTip = 'Date of the last tracking entry for this item.';
                }
            }
        }

        // 📝 modify → Mevcut bir alanın özelliklerini değiştir
        // modify(Description)
        // {
        //     Importance = Promoted;
        // }

        // 📝 moveafter → Mevcut bir alanı taşı
        // moveafter(Description; "No.")
    }

    actions
    {
        // 📝 Mevcut sayfaya yeni buton ekleme
        addlast(Processing)
        {
            action(ViewTrackingEntries)
            {
                Caption = 'View Tracking Entries';
                Image = ItemTrackingLines;
                Promoted = true;
                PromotedCategory = Process;

                trigger OnAction()
                var
                    ItemTracking: Record "Item Tracking";
                begin
                    // Filtrelenmiş sayfa aç
                    ItemTracking.SetRange("Item No.", Rec."No.");
                    Page.Run(Page::"Item Tracking List", ItemTracking);
                    // Page.Run → NavigateTo gibi
                end;
            }
        }
    }
}

// ============== PAGE EXTENSION (List Page) ==============
pageextension 50101 "Item List Tracking Ext" extends "Item List"
{
    layout
    {
        addafter(Description)
        {
            field("Tracking Enabled"; Rec."Tracking Enabled")
            {
                ApplicationArea = All;
                ToolTip = 'Indicates if tracking is enabled for this item.';
            }
        }
    }
}

// ============================================================
// 📝 DİĞER EXTENSION TİPLERİ:
// ============================================================
//
// enumextension 50100 "My Enum Ext" extends "Existing Enum"
// {
//     value(50100; "New Value") { Caption = 'New Value'; }
// }
//
// reportextension 50100 "My Report Ext" extends "Existing Report"
// {
//     // Rapora yeni sütun ekleme
// }
//
// codeunitextension → Mevcut codeunit'e event subscriber ekleme
//   (Aslında EventSubscriber attribute ile yapılır, ayrı extension gerekmez)
// ============================================================
