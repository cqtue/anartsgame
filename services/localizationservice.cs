using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace anartsgame.services;

public class LocalizationService : INotifyPropertyChanged
{
    private static LocalizationService? _instance;
    private static readonly object _lock = new();
    private string _currentLanguage = "Українська";

    public event PropertyChangedEventHandler? PropertyChanged;

    public static LocalizationService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new LocalizationService();
                }
            }
            return _instance;
        }
    }

    private readonly Dictionary<string, Dictionary<string, string>> _translations = new()
    {
        ["Українська"] = new()
        {
            // Main Menu
            ["MainMenu_Title"] = "anarts",
            ["MainMenu_NewGame"] = "нова гра",
            ["MainMenu_Continue"] = "продовжити",
            ["MainMenu_Settings"] = "налаштування",
            ["MainMenu_Exit"] = "вихід",

            // Settings
            ["Settings_Title"] = "налаштування",
            ["Settings_Language"] = "мова",
            ["Settings_MasterVolume"] = "загальна гучність",
            ["Settings_SoundVolume"] = "гучність звуків",
            ["Settings_MusicVolume"] = "гучність музики",
            ["Settings_Fullscreen"] = "повноекранний режим",
            ["Settings_Vsync"] = "вертикальна синхронізація",
            ["Settings_Save"] = "зберегти",
            ["Settings_Back"] = "назад",
            ["Settings_UnsavedChanges"] = "у вас є незбережені зміни. відхилити їх?",

            // Game View - Pause Menu
            ["Game_Pause"] = "ПАУЗА",
            ["Game_Resume"] = "продовжити",
            ["Game_Settings"] = "налаштування",
            ["Game_SaveAndExit"] = "зберегти та вийти",

            // New Game Setup
            ["NewGame_Title"] = "новий світ",
            ["NewGame_Start"] = "почати",

            // Dialog
            ["Dialog_Yes"] = "так",
            ["Dialog_No"] = "ні",

            // Game - Error Messages
            ["Game_Error_TooFar"] = "Занадто далеко від будівель",
            ["Game_Error_MineRocksFar"] = "Шахта: занадто далеко від будівель та каменів",
            ["Game_Error_MineBuildingsFar"] = "Шахта: занадто далеко від будівель",
            ["Game_Error_MineNoRocks"] = "Шахта: має бути біля каменів",
            ["Game_Error_Overlap"] = "Перетинається з іншою будівлею",
            ["Game_Error_SawmillTreesFar"] = "Лісопилка: занадто далеко від будівель та дерев",
            ["Game_Error_SawmillBuildingsFar"] = "Лісопилка: занадто далеко від будівель",
            ["Game_Error_SawmillNoTrees"] = "Лісопилка: має бути біля дерев",
            ["Game_Error_BankLimit"] = "Банк може бути лише один",
            ["Game_Error_MarketLimit"] = "Маркет може бути лише один",

            // Game - Resources
            ["Resource_Metal"] = "Метал",
            ["Resource_Organic"] = "Органіка",
            ["Resource_Meat"] = "М'ясо",
            ["Resource_Wood"] = "Дерево",
            ["Resource_Coal"] = "Вугілля",
            ["Resource_Generic"] = "Ресурс",

            // Game - Panel Titles
            ["Panel_Build"] = "БУДІВНИЦТВО",
            ["Panel_Research"] = "ДОСЛІДЖЕННЯ",
            ["Panel_Trade"] = "ТРЕЙДИНГ",

            // Game - Building Names
            ["Building_Base"] = "БАЗА",
            ["Building_Factory"] = "ФАБРИКА",
            ["Building_Mine"] = "ШАХТА",
            ["Building_MeatFactory"] = "М'ЯСОФАБРИКА",
            ["Building_Sawmill"] = "ЛІСОПИЛКА",
            ["Building_Bank"] = "БАНК",
            ["Building_Marketplace"] = "МАРКЕТ",
            ["Building_Furnace"] = "ПЕЧКА",
            ["Building_Generic"] = "БУДІВЛЯ",

            // Game - Building Descriptions
            ["BuildingDesc_Base"] = "Центр управління колонією",
            ["BuildingDesc_Factory"] = "Виробляє органіку",
            ["BuildingDesc_Mine"] = "Видобуває метал",
            ["BuildingDesc_MeatFactory"] = "Виробляє м'ясо",
            ["BuildingDesc_Sawmill"] = "Видобуває дерево",
            ["BuildingDesc_Bank"] = "Інвестує ресурси для прибутку",
            ["BuildingDesc_Marketplace"] = "Дозволяє торгувати ресурсами",
            ["BuildingDesc_Furnace"] = "Переплавляє дерево у вугілля",
            ["BuildingDesc_Generic"] = "Будівля",

            // Game - Research Panel
            ["Research_Remaining"] = "Залишилось:",
            ["Research_Available"] = "ДОСТУПНІ ДОСЛІДЖЕННЯ:",
            ["Research_Cost"] = "Вартість:",
            ["Research_Button"] = "ДОСЛІДИТИ",

            // Game - Trading Panel
            ["Trade_NeedMarket"] = "Потрібно побудувати Маркет для торгівлі",
            ["Trade_Step1"] = "Крок 1: Оберіть ресурс для обміну",
            ["Trade_Rate"] = "Курс обміну: 100% → 60%",
            ["Trade_Back"] = "← Назад",
            ["Trade_Step2"] = "Крок 2: Оберіть кількість",
            ["Trade_Available"] = "Доступно:",
            ["Trade_Amount"] = "Кількість:",
            ["Trade_Next"] = "Далі →",
            ["Trade_Step3"] = "Крок 3: Оберіть ресурс для отримання",
            ["Trade_Giving"] = "Віддаєте:",
            ["Trade_Receive"] = "Отримаєте:",
            ["Trade_RateDisplay"] = "Курс:",
            ["Trade_Confirm"] = "✓ Підтвердити обмін",
            ["Trade_Has"] = "є:",
            ["Trade_WillReceive"] = "отримаєте:",

            // Game - Building Panel
            ["BuildingPanel_Level"] = "Рівень:",
            ["BuildingPanel_Upgrade"] = "АПГРЕЙД",
            ["BuildingPanel_Delete"] = "ВИДАЛИТИ",
            ["BuildingPanel_Sure"] = "впевнені?",
            ["BuildingPanel_Production"] = "ВИРОБНИЦТВО:",
            ["BuildingPanel_Investment"] = "ІНВЕСТИЦІЯ:",
            ["BuildingPanel_Invested"] = "Інвестовано:",
            ["BuildingPanel_Remaining"] = "Залишилось:",
            ["BuildingPanel_Cooldown"] = "Кулдаун:",
            ["BuildingPanel_Invest"] = "Інвестувати 100",

            // Game - Research Names
            ["Research_ImprovedProduction"] = "Покращене виробництво",
            ["Research_EfficientConstruction"] = "Ефективне будівництво",
            ["Research_FastLearning"] = "Швидке навчання",
            ["Research_ExtendedRadius"] = "Розширений радіус",
            ["Research_AdvancedMining"] = "Покращена видобування",
            ["Research_OrganicBoost"] = "Органічний бум",

            // Game - Research Descriptions
            ["ResearchDesc_ImprovedProduction"] = "Збільшує швидкість виробництва всіх будівель на 15%",
            ["ResearchDesc_EfficientConstruction"] = "Зменшує вартість будівництва на 20%",
            ["ResearchDesc_FastLearning"] = "Зменшує час досліджень на 25%",
            ["ResearchDesc_ExtendedRadius"] = "Збільшує радіус будівництва на 30%",
            ["ResearchDesc_AdvancedMining"] = "Шахти виробляють на 50% більше металу",
            ["ResearchDesc_OrganicBoost"] = "Фабрики виробляють на 40% більше органіки"
        },
        ["English"] = new()
        {
            // Main Menu
            ["MainMenu_Title"] = "anarts",
            ["MainMenu_NewGame"] = "new game",
            ["MainMenu_Continue"] = "continue",
            ["MainMenu_Settings"] = "settings",
            ["MainMenu_Exit"] = "exit",

            // Settings
            ["Settings_Title"] = "settings",
            ["Settings_Language"] = "language",
            ["Settings_MasterVolume"] = "master volume",
            ["Settings_SoundVolume"] = "sound volume",
            ["Settings_MusicVolume"] = "music volume",
            ["Settings_Fullscreen"] = "fullscreen mode",
            ["Settings_Vsync"] = "vertical sync",
            ["Settings_Save"] = "save",
            ["Settings_Back"] = "back",
            ["Settings_UnsavedChanges"] = "you have unsaved changes. discard them?",

            // Game View - Pause Menu
            ["Game_Pause"] = "PAUSE",
            ["Game_Resume"] = "resume",
            ["Game_Settings"] = "settings",
            ["Game_SaveAndExit"] = "save and exit",

            // New Game Setup
            ["NewGame_Title"] = "new world",
            ["NewGame_Start"] = "start",

            // Dialog
            ["Dialog_Yes"] = "yes",
            ["Dialog_No"] = "no",

            // Game - Error Messages
            ["Game_Error_TooFar"] = "too far from buildings",
            ["Game_Error_MineRocksFar"] = "mine: too far from buildings and rocks",
            ["Game_Error_MineBuildingsFar"] = "mine: too far from buildings",
            ["Game_Error_MineNoRocks"] = "mine: must be near rocks",
            ["Game_Error_Overlap"] = "overlaps with another building",
            ["Game_Error_SawmillTreesFar"] = "sawmill: too far from buildings and trees",
            ["Game_Error_SawmillBuildingsFar"] = "sawmill: too far from buildings",
            ["Game_Error_SawmillNoTrees"] = "sawmill: must be near trees",
            ["Game_Error_BankLimit"] = "bank can only be one",
            ["Game_Error_MarketLimit"] = "market can only be one",

            // Game - Resources
            ["Resource_Metal"] = "metal",
            ["Resource_Organic"] = "organic",
            ["Resource_Meat"] = "meat",
            ["Resource_Wood"] = "wood",
            ["Resource_Coal"] = "coal",
            ["Resource_Generic"] = "resource",

            // Game - Panel Titles
            ["Panel_Build"] = "CONSTRUCTION",
            ["Panel_Research"] = "RESEARCH",
            ["Panel_Trade"] = "TRADING",

            // Game - Building Names
            ["Building_Base"] = "BASE",
            ["Building_Factory"] = "FACTORY",
            ["Building_Mine"] = "MINE",
            ["Building_MeatFactory"] = "MEAT FACTORY",
            ["Building_Sawmill"] = "SAWMILL",
            ["Building_Bank"] = "BANK",
            ["Building_Marketplace"] = "MARKET",
            ["Building_Furnace"] = "FURNACE",
            ["Building_Generic"] = "BUILDING",

            // Game - Building Descriptions
            ["BuildingDesc_Base"] = "colony control center",
            ["BuildingDesc_Factory"] = "produces organic",
            ["BuildingDesc_Mine"] = "extracts metal",
            ["BuildingDesc_MeatFactory"] = "produces meat",
            ["BuildingDesc_Sawmill"] = "extracts wood",
            ["BuildingDesc_Bank"] = "invests resources for profit",
            ["BuildingDesc_Marketplace"] = "allows trading resources",
            ["BuildingDesc_Furnace"] = "smelts wood into coal",
            ["BuildingDesc_Generic"] = "building",

            // Game - Research Panel
            ["Research_Remaining"] = "remaining:",
            ["Research_Available"] = "AVAILABLE RESEARCH:",
            ["Research_Cost"] = "cost:",
            ["Research_Button"] = "RESEARCH",

            // Game - Trading Panel
            ["Trade_NeedMarket"] = "need to build market for trading",
            ["Trade_Step1"] = "step 1: choose resource to exchange",
            ["Trade_Rate"] = "exchange rate: 100% → 60%",
            ["Trade_Back"] = "← back",
            ["Trade_Step2"] = "step 2: choose amount",
            ["Trade_Available"] = "available:",
            ["Trade_Amount"] = "amount:",
            ["Trade_Next"] = "next →",
            ["Trade_Step3"] = "step 3: choose resource to receive",
            ["Trade_Giving"] = "giving:",
            ["Trade_Receive"] = "will receive:",
            ["Trade_RateDisplay"] = "rate:",
            ["Trade_Confirm"] = "✓ confirm exchange",
            ["Trade_Has"] = "has:",
            ["Trade_WillReceive"] = "will receive:",

            // Game - Building Panel
            ["BuildingPanel_Level"] = "level:",
            ["BuildingPanel_Upgrade"] = "UPGRADE",
            ["BuildingPanel_Delete"] = "DELETE",
            ["BuildingPanel_Sure"] = "sure?",
            ["BuildingPanel_Production"] = "PRODUCTION:",
            ["BuildingPanel_Investment"] = "INVESTMENT:",
            ["BuildingPanel_Invested"] = "invested:",
            ["BuildingPanel_Remaining"] = "remaining:",
            ["BuildingPanel_Cooldown"] = "cooldown:",
            ["BuildingPanel_Invest"] = "invest 100",

            // Game - Research Names
            ["Research_ImprovedProduction"] = "improved production",
            ["Research_EfficientConstruction"] = "efficient construction",
            ["Research_FastLearning"] = "fast learning",
            ["Research_ExtendedRadius"] = "extended radius",
            ["Research_AdvancedMining"] = "advanced mining",
            ["Research_OrganicBoost"] = "organic boom",

            // Game - Research Descriptions
            ["ResearchDesc_ImprovedProduction"] = "increases production speed of all buildings by 15%",
            ["ResearchDesc_EfficientConstruction"] = "reduces construction cost by 20%",
            ["ResearchDesc_FastLearning"] = "reduces research time by 25%",
            ["ResearchDesc_ExtendedRadius"] = "increases construction radius by 30%",
            ["ResearchDesc_AdvancedMining"] = "mines produce 50% more metal",
            ["ResearchDesc_OrganicBoost"] = "factories produce 40% more organic"
        }
    };

    private LocalizationService()
    {
        _currentLanguage = SettingsService.Instance.Language;
    }

    public string CurrentLanguage
    {
        get => _currentLanguage;
        set
        {
            if (_currentLanguage != value)
            {
                _currentLanguage = value;
                OnPropertyChanged();
                OnPropertyChanged("Item[]");
            }
        }
    }

    public string this[string key]
    {
        get
        {
            if (_translations.TryGetValue(_currentLanguage, out var languageDict))
            {
                if (languageDict.TryGetValue(key, out var translation))
                {
                    return translation;
                }
            }
            return $"[{key}]";
        }
    }

    public void SetLanguage(string language)
    {
        if (_translations.ContainsKey(language))
        {
            CurrentLanguage = language;
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
