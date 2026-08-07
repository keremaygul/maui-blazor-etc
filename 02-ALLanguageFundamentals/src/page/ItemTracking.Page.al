// ============================================================
// 📝 AL DERS 3: PAGE - List & Card
// ============================================================
// Blazor/Razor Page'lerin AL karşılığı.
//
// Page Tipleri:
//   List     → DataGrid/Tablo (listeleme)
//   Card     → Form (detay/düzenleme)
//   Document → Sipariş belgesi (header + lines)
//   Worksheet → Excel benzeri çalışma tablosu
//   RoleCenter → Ana dashboard sayfası
// ============================================================

// ============== LIST PAGE ==============
page 50100 "Item Tracking List"
{
    PageType = List;
    ApplicationArea = All;
    UsageCategory = Lists;
    SourceTable = "Item Tracking";
    CardPageId = "Item Tracking Card";
    Caption = 'Item Tracking List';
    Editable = false;

    // 📝 layout → HTML/Razor yapısının AL karşılığı
    layout
    {
        area(Content)
        {
            // repeater → foreach döngüsü gibi, her kayıt için tekrarlanır
            repeater(Lines)
            {
                field("Entry No."; Rec."Entry No.")
                {
                    ApplicationArea = All;
                    ToolTip = 'The unique entry number.';
                    // ToolTip zorunlu — accessibility ve kullanıcı yardımı için
                }
                field("Item No."; Rec."Item No.")
                {
                    ApplicationArea = All;
                    ToolTip = 'The item number this tracking entry belongs to.';
                }
                field(Description; Rec.Description)
                {
                    ApplicationArea = All;
                    ToolTip = 'The description of the tracked item.';
                }
                field(Quantity; Rec.Quantity)
                {
                    ApplicationArea = All;
                    ToolTip = 'The tracked quantity.';
                    StyleExpr = QuantityStyle;
                    // StyleExpr → Koşullu CSS gibi
                    // Favorable = Yeşil, Unfavorable = Kırmızı, Ambiguous = Sarı
                }
                field("Location Code"; Rec."Location Code")
                {
                    ApplicationArea = All;
                    ToolTip = 'The warehouse location code.';
                }
                field(Status; Rec.Status)
                {
                    ApplicationArea = All;
                    ToolTip = 'The current tracking status.';
                }
            }
        }

        // 📝 FactBox → Sağ taraftaki bilgi paneli
        area(FactBoxes)
        {
            systempart(Notes; Notes)
            {
                ApplicationArea = All;
            }
        }
    }

    // 📝 actions → Toolbar butonları (Blazor'daki <button @onclick>)
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
                PromotedIsBig = true;

                trigger OnAction()
                var
                    ItemTrackingCard: Page "Item Tracking Card";
                begin
                    ItemTrackingCard.RunModal();
                    // RunModal → Yeni pencere aç, kapanmasını bekle
                    // Run → Yeni pencere aç, bekleme
                    // C#: var dialog = new ItemForm(); dialog.ShowDialog();
                end;
            }

            action(MarkCompleted)
            {
                Caption = 'Mark as Completed';
                Image = Completed;
                Promoted = true;
                PromotedCategory = Process;

                trigger OnAction()
                begin
                    if not Confirm('Mark entry %1 as completed?', false, Rec."Entry No.") then
                        exit;
                    // Confirm → MessageBox.Show (Yes/No)

                    Rec.Status := Rec.Status::Completed;
                    Rec.Modify(true);
                    CurrPage.Update(false);
                    // CurrPage.Update → Sayfayı yenile (StateHasChanged gibi)
                end;
            }
        }

        area(Navigation)
        {
            action(ViewItem)
            {
                Caption = 'View Item';
                Image = Item;
                RunObject = page "Item Card";
                RunPageLink = "No." = field("Item No.");
                // RunObject + RunPageLink → Başka sayfaya navigate et
                // C#: Navigation.NavigateTo($"/items/{ItemNo}")
            }
        }
    }

    // 📝 Page Trigger'ları
    trigger OnOpenPage()
    begin
        // Sayfa açıldığında çalışır (OnInitialized gibi)
        Rec.SetCurrentKey("Entry No.");
        Rec.Ascending(false);
        // En yeni kayıtlar üstte
    end;

    var
        QuantityStyle: Text;

    trigger OnAfterGetRecord()
    begin
        // Her satır render edilirken çalışır
        // Blazor'daki @foreach içindeki koşullu rendering gibi
        if Rec.Quantity <= 0 then
            QuantityStyle := 'Unfavorable'    // Kırmızı
        else if Rec.Quantity < 10 then
            QuantityStyle := 'Ambiguous'      // Sarı
        else
            QuantityStyle := 'Favorable';     // Yeşil
    end;
}

// ============== CARD PAGE ==============
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
            // 📝 group → Form section (fieldset gibi)
            group(General)
            {
                Caption = 'General Information';

                field("Entry No."; Rec."Entry No.")
                {
                    ApplicationArea = All;
                    ToolTip = 'Auto-generated entry number.';
                    Editable = false;
                    Importance = Promoted;
                    // Importance: Standard, Promoted (her zaman göster), Additional (gizle)
                }
                field("Item No."; Rec."Item No.")
                {
                    ApplicationArea = All;
                    ToolTip = 'Select the item to track.';
                    ShowMandatory = true;
                    // ShowMandatory → Zorunlu alan işareti (kırmızı yıldız)
                }
                field(Description; Rec.Description)
                {
                    ApplicationArea = All;
                    ToolTip = 'Description is auto-filled from the item.';
                }
                field(Status; Rec.Status)
                {
                    ApplicationArea = All;
                    ToolTip = 'Current tracking status.';
                }
            }

            group(Details)
            {
                Caption = 'Tracking Details';

                field(Quantity; Rec.Quantity)
                {
                    ApplicationArea = All;
                    ToolTip = 'Enter the tracked quantity.';
                }
                field("Location Code"; Rec."Location Code")
                {
                    ApplicationArea = All;
                    ToolTip = 'The warehouse location.';
                }
            }

            group(Audit)
            {
                Caption = 'Audit Information';

                field("Created Date"; Rec."Created Date")
                {
                    ApplicationArea = All;
                    ToolTip = 'Date when this entry was created.';
                    Editable = false;
                }
                field("Created By"; Rec."Created By")
                {
                    ApplicationArea = All;
                    ToolTip = 'User who created this entry.';
                    Editable = false;
                }
            }
        }
    }

    actions
    {
        area(Processing)
        {
            action(Activate)
            {
                Caption = 'Activate';
                Image = Start;

                trigger OnAction()
                begin
                    Rec.Status := Rec.Status::Active;
                    Rec.Modify(true);
                    Message('Entry %1 has been activated.', Rec."Entry No.");
                end;
            }
        }
    }
}
