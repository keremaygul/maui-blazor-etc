// ============================================================
// 📝 MONACO EDITOR DERSİ - ANA UYGULAMA
// ============================================================
// Bu dosya Monaco Editor'ü yükler, konfigüre eder ve
// özel bir AL dili syntax highlighting tanımlar.
// ============================================================

// ============================================================
// BÖLÜM 1: ÖRNEK KODLAR
// ============================================================

const SAMPLES = {
    'al-table': {
        language: 'al',
        fileName: 'ItemTracking.Table.al',
        code: `// Business Central - Table Definition
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
            NotBlank = true;
            TableRelation = Item."No.";

            trigger OnValidate()
            var
                Item: Record Item;
            begin
                if Item.Get("Item No.") then
                    Description := Item.Description;
            end;
        }
        field(3; "Description"; Text[100])
        {
            Caption = 'Description';
        }
        field(4; "Quantity"; Decimal)
        {
            Caption = 'Quantity';
            DecimalPlaces = 0 : 5;

            trigger OnValidate()
            begin
                if Quantity < 0 then
                    Error('Quantity cannot be negative!');
            end;
        }
        field(5; "Status"; Enum "Tracking Status")
        {
            Caption = 'Status';
            InitValue = Active;
        }
    }

    keys
    {
        key(PK; "Entry No.")
        {
            Clustered = true;
        }
    }

    trigger OnInsert()
    begin
        "Created Date" := Today;
        "Created By" := UserId;
    end;
}`
    },

    'al-page': {
        language: 'al',
        fileName: 'ItemTrackingList.Page.al',
        code: `// Business Central - Page Definition
page 50100 "Item Tracking List"
{
    PageType = List;
    ApplicationArea = All;
    UsageCategory = Lists;
    SourceTable = "Item Tracking";
    CardPageId = "Item Tracking Card";
    Caption = 'Item Tracking List';

    layout
    {
        area(Content)
        {
            repeater(Lines)
            {
                field("Entry No."; Rec."Entry No.")
                {
                    ApplicationArea = All;
                    ToolTip = 'The unique entry number.';
                }
                field("Item No."; Rec."Item No.")
                {
                    ApplicationArea = All;
                }
                field(Description; Rec.Description)
                {
                    ApplicationArea = All;
                }
                field(Quantity; Rec.Quantity)
                {
                    StyleExpr = QuantityStyle;
                }
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

                trigger OnAction()
                begin
                    Message('Creating new entry...');
                end;
            }
        }
    }

    var
        QuantityStyle: Text;

    trigger OnAfterGetRecord()
    begin
        if Rec.Quantity <= 0 then
            QuantityStyle := 'Unfavorable'
        else
            QuantityStyle := 'Favorable';
    end;
}`
    },

    'al-codeunit': {
        language: 'al',
        fileName: 'ItemTrackingMgt.Codeunit.al',
        code: `// Business Central - Codeunit (Service Layer)
codeunit 50100 "Item Tracking Mgt."
{
    procedure CreateTrackingEntry(
        ItemNo: Code[20];
        Qty: Decimal;
        LocationCode: Code[10]
    ): Integer
    var
        ItemTracking: Record "Item Tracking";
        Item: Record Item;
    begin
        if not Item.Get(ItemNo) then
            Error('Item %1 not found!', ItemNo);

        ItemTracking.Init();
        ItemTracking."Item No." := ItemNo;
        ItemTracking.Description := Item.Description;
        ItemTracking.Quantity := Qty;
        ItemTracking."Location Code" := LocationCode;
        ItemTracking.Status := ItemTracking.Status::Active;
        ItemTracking.Insert(true);

        exit(ItemTracking."Entry No.");
    end;

    procedure GetTotalQuantity(ItemNo: Code[20]): Decimal
    var
        ItemTracking: Record "Item Tracking";
    begin
        ItemTracking.SetRange("Item No.", ItemNo);
        ItemTracking.CalcSums(Quantity);
        exit(ItemTracking.Quantity);
    end;

    [IntegrationEvent(false, false)]
    local procedure OnBeforeCreateEntry(
        var ItemTracking: Record "Item Tracking";
        var IsHandled: Boolean)
    begin
    end;

    [EventSubscriber(ObjectType::Codeunit, 
        Codeunit::"Item Tracking Mgt.",
        'OnBeforeCreateEntry', '', false, false)]
    local procedure HandleBeforeCreate(
        var ItemTracking: Record "Item Tracking";
        var IsHandled: Boolean)
    begin
        if ItemTracking.Quantity > 1000 then
            Error('Quantity too large!');
    end;
}`
    },

    'csharp-blazor': {
        language: 'csharp',
        fileName: 'StockService.cs',
        code: `using System.Collections.Generic;
using System.Linq;

namespace StockTracker.Services;

/// <summary>
/// Stock management service — Blazor MAUI Hybrid App
/// </summary>
public class StockService
{
    private readonly List<Product> _products = new();
    private int _nextId = 1;

    public StockService()
    {
        // Seed data
        _products.AddRange(new[]
        {
            new Product { Id = _nextId++, Name = "Wireless Mouse", Barcode = "4012345678901", Quantity = 45, Price = 29.99m },
            new Product { Id = _nextId++, Name = "USB-C Cable", Barcode = "4012345678902", Quantity = 120, Price = 12.50m },
            new Product { Id = _nextId++, Name = "Mechanical Keyboard", Barcode = "4012345678903", Quantity = 3, Price = 89.90m },
        });
    }

    public List<Product> GetAll() => _products.OrderBy(p => p.Name).ToList();

    public Product? GetById(int id) => _products.FirstOrDefault(p => p.Id == id);

    public Product? GetByBarcode(string barcode) =>
        _products.FirstOrDefault(p => p.Barcode == barcode);

    public List<Product> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return GetAll();

        return _products
            .Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
                     || p.Barcode.Contains(query))
            .ToList();
    }

    public Product Add(Product product)
    {
        product.Id = _nextId++;
        product.LastUpdated = DateTime.Now;
        _products.Add(product);
        return product;
    }

    public bool Update(Product product)
    {
        var existing = GetById(product.Id);
        if (existing == null) return false;

        existing.Name = product.Name;
        existing.Quantity = product.Quantity;
        existing.Price = product.Price;
        existing.LastUpdated = DateTime.Now;
        return true;
    }

    public bool Delete(int id) =>
        _products.Remove(_products.FirstOrDefault(p => p.Id == id)!);
}`
    },

    'json-appjson': {
        language: 'json',
        fileName: 'app.json',
        code: `{
    "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "name": "Item Tracking Extension",
    "publisher": "Learning Developer",
    "version": "1.0.0.0",
    "brief": "A learning project for AL development",
    "description": "Stock/item tracking extension for Business Central. This extension adds tracking capabilities to the standard Item entity.",
    "privacyStatement": "",
    "EULA": "",
    "help": "",
    "url": "https://github.com/keremaygul/maui-blazor-etc",
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
    "target": "Cloud",
    "features": [
        "TranslationFile",
        "GenerateCaptions"
    ]
}`
    }
};

// ============================================================
// BÖLÜM 2: AL DİLİ TANIMLAMASI (Custom Language)
// ============================================================

/**
 * 📝 MONACO EDITOR DERSİ - CUSTOM LANGUAGE:
 * 
 * Monaco Editor'e özel bir dil eklemek için 3 adım:
 * 1. monaco.languages.register() → Dili kaydet
 * 2. monaco.languages.setMonarchTokensProvider() → Syntax kuralları
 * 3. monaco.languages.registerCompletionItemProvider() → IntelliSense
 * 
 * Monarch, Monaco'nun tokenizer formatıdır.
 * Her token bir regex kuralı ile eşleştirilir ve bir CSS class'ı alır.
 */
function registerALLanguage(monaco) {
    // Dili kaydet
    monaco.languages.register({ id: 'al' });

    // Syntax highlighting kuralları (Monarch tokenizer)
    monaco.languages.setMonarchTokensProvider('al', {
        // Büyük-küçük harf duyarsız
        ignoreCase: true,

        // AL anahtar kelimeleri
        keywords: [
            'table', 'page', 'codeunit', 'report', 'query', 'xmlport',
            'enum', 'interface', 'controladdin', 'entitlement', 'permissionset',
            'tableextension', 'pageextension', 'enumextension', 'reportextension',
            'fields', 'field', 'keys', 'key', 'fieldgroups', 'fieldgroup',
            'layout', 'actions', 'area', 'group', 'repeater', 'part',
            'action', 'trigger', 'procedure', 'local', 'internal',
            'var', 'begin', 'end', 'if', 'then', 'else', 'case', 'of',
            'for', 'to', 'downto', 'do', 'while', 'repeat', 'until',
            'with', 'exit', 'break', 'return',
            'true', 'false', 'not', 'and', 'or', 'xor', 'mod', 'div',
            'extends', 'implements'
        ],

        // AL veri tipleri
        typeKeywords: [
            'Integer', 'BigInteger', 'Decimal', 'Boolean', 'Text', 'Code',
            'Date', 'Time', 'DateTime', 'DateFormula', 'Duration',
            'Char', 'Byte', 'Option', 'Guid', 'Blob', 'Media', 'MediaSet',
            'Record', 'Page', 'Codeunit', 'Enum', 'Label', 'TextConst',
            'RecordId', 'RecordRef', 'FieldRef', 'KeyRef',
            'List', 'Dictionary', 'Array', 'JsonObject', 'JsonArray',
            'HttpClient', 'HttpContent', 'HttpHeaders', 'HttpResponseMessage'
        ],

        // AL property keywords
        propertyKeywords: [
            'Caption', 'Description', 'DataClassification', 'TableRelation',
            'SourceTable', 'PageType', 'ApplicationArea', 'UsageCategory',
            'Editable', 'AutoIncrement', 'NotBlank', 'InitValue',
            'DecimalPlaces', 'Promoted', 'PromotedCategory', 'Image',
            'RunObject', 'RunPageLink', 'ToolTip', 'StyleExpr',
            'Importance', 'ShowMandatory', 'Extensible', 'Clustered',
            'CalcFormula', 'FieldClass', 'SumIndexFields'
        ],

        // Operatörler
        operators: [
            ':=', '+=', '-=', '*=', '/=',
            '=', '<>', '<', '>', '<=', '>=',
            '+', '-', '*', '/', '..',
            '::', '.'
        ],

        // Tokenizer kuralları
        tokenizer: {
            root: [
                // Yorumlar
                [/\/\/.*$/, 'comment'],
                [/\/\*/, 'comment', '@comment'],

                // String'ler
                [/'[^']*'/, 'string'],

                // Sayılar
                [/\d+\.\d+/, 'number.float'],
                [/\d+/, 'number'],

                // Attribute'lar (köşeli parantez içinde)
                [/\[.*?\]/, 'annotation'],

                // Anahtar kelimeler ve tanımlayıcılar
                [/[a-zA-Z_]\w*/, {
                    cases: {
                        '@keywords': 'keyword',
                        '@typeKeywords': 'type',
                        '@propertyKeywords': 'variable.property',
                        '@default': 'identifier'
                    }
                }],

                // Tırnaklı tanımlayıcılar ("Item No." gibi)
                [/"[^"]*"/, 'string.identifier'],

                // Operatörler
                [/[{}()\[\]]/, '@brackets'],
                [/:=|[<>]=?|<>|[+\-*/]/, 'operator'],
                [/[;,.]/, 'delimiter'],
            ],

            comment: [
                [/[^/*]+/, 'comment'],
                [/\*\//, 'comment', '@pop'],
                [/[/*]/, 'comment']
            ]
        }
    });

    // 📝 MONACO EDITOR DERSİ - INTELLISENSE (Auto-Complete)
    monaco.languages.registerCompletionItemProvider('al', {
        provideCompletionItems: function (model, position) {
            const word = model.getWordUntilPosition(position);
            const range = {
                startLineNumber: position.lineNumber,
                endLineNumber: position.lineNumber,
                startColumn: word.startColumn,
                endColumn: word.endColumn
            };

            // AL snippet'leri
            const suggestions = [
                {
                    label: 'table',
                    kind: monaco.languages.CompletionItemKind.Snippet,
                    insertText: [
                        'table ${1:50100} "${2:TableName}"',
                        '{',
                        '    Caption = \'${2:TableName}\';',
                        '    DataClassification = CustomerContent;',
                        '',
                        '    fields',
                        '    {',
                        '        field(1; "${3:FieldName}"; ${4:Integer})',
                        '        {',
                        '            Caption = \'${3:FieldName}\';',
                        '        }',
                        '    }',
                        '',
                        '    keys',
                        '    {',
                        '        key(PK; "${3:FieldName}")',
                        '        {',
                        '            Clustered = true;',
                        '        }',
                        '    }',
                        '}'
                    ].join('\n'),
                    insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
                    documentation: 'Create a new AL table',
                    range: range
                },
                {
                    label: 'page',
                    kind: monaco.languages.CompletionItemKind.Snippet,
                    insertText: [
                        'page ${1:50100} "${2:PageName}"',
                        '{',
                        '    PageType = ${3|List,Card,Document|};',
                        '    ApplicationArea = All;',
                        '    SourceTable = "${4:TableName}";',
                        '    Caption = \'${2:PageName}\';',
                        '',
                        '    layout',
                        '    {',
                        '        area(Content)',
                        '        {',
                        '            ${0}',
                        '        }',
                        '    }',
                        '}'
                    ].join('\n'),
                    insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
                    documentation: 'Create a new AL page',
                    range: range
                },
                {
                    label: 'procedure',
                    kind: monaco.languages.CompletionItemKind.Snippet,
                    insertText: [
                        'procedure ${1:ProcedureName}(${2:})',
                        'var',
                        '    ${3:VarName}: ${4:Type};',
                        'begin',
                        '    ${0}',
                        'end;'
                    ].join('\n'),
                    insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
                    documentation: 'Create a new AL procedure',
                    range: range
                },
                {
                    label: 'field',
                    kind: monaco.languages.CompletionItemKind.Snippet,
                    insertText: [
                        'field(${1:1}; "${2:FieldName}"; ${3|Integer,Code[20],Text[100],Decimal,Boolean,Date|})',
                        '{',
                        '    Caption = \'${2:FieldName}\';',
                        '}'
                    ].join('\n'),
                    insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
                    documentation: 'Add a new field to a table',
                    range: range
                },
                {
                    label: 'trigger',
                    kind: monaco.languages.CompletionItemKind.Snippet,
                    insertText: [
                        'trigger ${1|OnInsert,OnModify,OnDelete,OnValidate,OnAction,OnAfterGetRecord|}()',
                        'begin',
                        '    ${0}',
                        'end;'
                    ].join('\n'),
                    insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
                    documentation: 'Add a trigger',
                    range: range
                },
                {
                    label: 'Message',
                    kind: monaco.languages.CompletionItemKind.Function,
                    insertText: 'Message(\'${1:Text}\');',
                    insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
                    documentation: 'Display a message dialog',
                    range: range
                },
                {
                    label: 'Error',
                    kind: monaco.languages.CompletionItemKind.Function,
                    insertText: 'Error(\'${1:ErrorText}\');',
                    insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
                    documentation: 'Throw an error (stops execution)',
                    range: range
                },
                {
                    label: 'if-then-else',
                    kind: monaco.languages.CompletionItemKind.Snippet,
                    insertText: [
                        'if ${1:condition} then',
                        '    ${2}',
                        'else',
                        '    ${0};'
                    ].join('\n'),
                    insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
                    documentation: 'If-then-else statement',
                    range: range
                },
                {
                    label: 'repeat-until',
                    kind: monaco.languages.CompletionItemKind.Snippet,
                    insertText: [
                        'if ${1:Record}.FindSet() then',
                        '    repeat',
                        '        ${0}',
                        '    until ${1:Record}.Next() = 0;'
                    ].join('\n'),
                    insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
                    documentation: 'Iterate over records (foreach equivalent)',
                    range: range
                }
            ];

            return { suggestions };
        }
    });
}

// ============================================================
// BÖLÜM 3: EDITOR BAŞLATMA
// ============================================================

/**
 * 📝 MONACO EDITOR DERSİ - AMD LOADER:
 * 
 * Monaco Editor AMD (Asynchronous Module Definition) kullanır.
 * require.config() ile CDN path'ini belirtiyoruz.
 * require(['vs/editor/editor.main'], callback) ile editörü yüklüyoruz.
 */
require.config({
    paths: {
        'vs': 'https://cdn.jsdelivr.net/npm/monaco-editor@0.52.2/min/vs'
    }
});

require(['vs/editor/editor.main'], function () {
    // AL dilini kaydet
    registerALLanguage(monaco);

    // 📝 Editor instance oluştur
    const editor = monaco.editor.create(document.getElementById('monacoEditor'), {
        value: SAMPLES['al-table'].code,
        language: 'al',
        
        // Tema
        theme: 'vs-dark',
        
        // Editor özellikleri
        automaticLayout: true,          // Container resize olunca otomatik uyum
        fontSize: 14,
        fontFamily: "'Cascadia Code', 'Fira Code', 'Consolas', monospace",
        fontLigatures: true,            // Font ligature'ları (=>, !== gibi)
        
        // Minimap (sağdaki küçük önizleme)
        minimap: {
            enabled: true,
            renderCharacters: true,
            maxColumn: 80
        },
        
        // Satır numaraları
        lineNumbers: 'on',
        lineNumbersMinChars: 3,
        
        // Girinti
        tabSize: 4,
        insertSpaces: true,
        
        // Scroll
        smoothScrolling: true,
        scrollBeyondLastLine: false,
        
        // Bracket
        bracketPairColorization: { enabled: true },
        guides: {
            bracketPairs: true,
            indentation: true
        },
        
        // Diğer
        wordWrap: 'off',
        renderWhitespace: 'selection',
        cursorBlinking: 'smooth',
        cursorSmoothCaretAnimation: 'on',
        padding: { top: 12, bottom: 12 },
        
        // Suggest (IntelliSense)
        suggestOnTriggerCharacters: true,
        quickSuggestions: true,
    });

    // ============================================================
    // BÖLÜM 4: EVENT HANDLERS
    // ============================================================

    // İmleç pozisyonu göster
    editor.onDidChangeCursorPosition(function (e) {
        document.getElementById('cursorLine').textContent = e.position.lineNumber;
        document.getElementById('cursorCol').textContent = e.position.column;
    });

    // Tema değişimi
    document.getElementById('themeSelector').addEventListener('change', function () {
        monaco.editor.setTheme(this.value);
    });

    // Dil değişimi
    document.getElementById('languageSelector').addEventListener('change', function () {
        const model = editor.getModel();
        monaco.editor.setModelLanguage(model, this.value);
        document.getElementById('languageDisplay').textContent = this.value.toUpperCase();
    });

    // Örnek kod değişimi
    document.getElementById('sampleSelector').addEventListener('change', function () {
        const sample = SAMPLES[this.value];
        if (sample) {
            editor.setValue(sample.code);
            document.getElementById('fileName').textContent = sample.fileName;
            
            const model = editor.getModel();
            monaco.editor.setModelLanguage(model, sample.language);
            document.getElementById('languageSelector').value = sample.language;
            document.getElementById('languageDisplay').textContent = sample.language.toUpperCase();
        }
    });

    // Format butonu
    document.getElementById('formatBtn').addEventListener('click', function () {
        editor.getAction('editor.action.formatDocument')?.run();
    });

    // Minimap toggle
    let minimapEnabled = true;
    document.getElementById('minimapToggle').addEventListener('click', function () {
        minimapEnabled = !minimapEnabled;
        editor.updateOptions({ minimap: { enabled: minimapEnabled } });
    });

    // 📝 MONACO EDITOR DERSİ - API ÖZETİ:
    //
    // editor.getValue()           → Tüm içeriği al (string)
    // editor.setValue(str)         → İçeriği değiştir
    // editor.getModel()           → Model objesini al
    // editor.updateOptions({...}) → Ayarları güncelle
    // editor.getAction(id).run()  → Bir action çalıştır
    // editor.addAction({...})     → Özel action ekle
    // editor.addCommand(...)      → Kısayol ekle
    //
    // monaco.editor.create()      → Yeni editor oluştur
    // monaco.editor.setTheme()    → Tema değiştir
    // monaco.editor.setModelLanguage() → Dil değiştir
    // monaco.languages.register() → Yeni dil kaydet
    // monaco.languages.setMonarchTokensProvider() → Syntax kuralları
    // monaco.languages.registerCompletionItemProvider() → IntelliSense
});
