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
            ["MainMenu_Title"] = "anarts",
            ["MainMenu_NewGame"] = "нова гра",
            ["MainMenu_Continue"] = "продовжити",
            ["MainMenu_Settings"] = "налаштування",
            ["MainMenu_Exit"] = "вихід",

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

            ["Game_Pause"] = "ПАУЗА",
            ["Game_Resume"] = "продовжити",
            ["Game_Settings"] = "налаштування",
            ["Game_SaveAndExit"] = "зберегти та вийти",

            ["NewGame_Title"] = "новий світ",
            ["NewGame_Start"] = "почати",
            ["NewGame_StartingResources"] = "ПОЧАТКОВІ РЕСУРСИ",
            ["NewGame_GameSettings"] = "НАЛАШТУВАННЯ ГРИ",
            ["NewGame_DisableSaving"] = "вимкнути збереження гри",
            ["NewGame_EnableConsole"] = "увімкнути консоль",

            ["Dialog_Yes"] = "так",
            ["Dialog_No"] = "ні",

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

            ["Resource_Metal"] = "Метал",
            ["Resource_Organic"] = "Органіка",
            ["Resource_Meat"] = "М'ясо",
            ["Resource_Wood"] = "Дерево",
            ["Resource_Coal"] = "Вугілля",
            ["Resource_Bones"] = "Кісточки",
            ["Resource_Diamonds"] = "Діаманти",
            ["Resource_Generic"] = "Ресурс",

            ["Panel_Build"] = "БУДІВНИЦТВО",
            ["Panel_Research"] = "ДОСЛІДЖЕННЯ",
            ["Panel_Trade"] = "ТРЕЙДИНГ",

            ["Building_Base"] = "БАЗА",
            ["Building_Factory"] = "ФАБРИКА",
            ["Building_Mine"] = "ШАХТА",
            ["Building_MeatFactory"] = "М'ЯСОФАБРИКА",
            ["Building_Sawmill"] = "ЛІСОПИЛКА",
            ["Building_Bank"] = "БАНК",
            ["Building_Marketplace"] = "МАРКЕТ",
            ["Building_Furnace"] = "ПЕЧКА",
            ["Building_Altar"] = "ВІВТАР",
            ["Building_Crystallizer"] = "КРИСТАЛІЗАТОР",
            ["Building_Generic"] = "БУДІВЛЯ",

            ["BuildingDesc_Base"] = "Центр управління колонією",
            ["BuildingDesc_Factory"] = "Виробляє органіку",
            ["BuildingDesc_Mine"] = "Видобуває метал",
            ["BuildingDesc_MeatFactory"] = "Виробляє м'ясо",
            ["BuildingDesc_Sawmill"] = "Видобуває дерево",
            ["BuildingDesc_Bank"] = "Інвестує ресурси для прибутку",
            ["BuildingDesc_Marketplace"] = "Дозволяє торгувати ресурсами",
            ["BuildingDesc_Furnace"] = "Переплавляє дерево у вугілля",
            ["BuildingDesc_Altar"] = "Перероблює вугілля, органіку та м'ясо у кісточки",
            ["BuildingDesc_Crystallizer"] = "Перероблює вугілля, органіку та метал у діаманти",
            ["BuildingDesc_Generic"] = "Будівля",

            ["Research_Remaining"] = "Залишилось:",
            ["Research_Available"] = "ДОСТУПНІ ДОСЛІДЖЕННЯ:",
            ["Research_Cost"] = "Вартість:",
            ["Research_Button"] = "ДОСЛІДИТИ",

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

            ["BatchProduction_Amount"] = "Кількість для виробництва:",
            ["BatchProduction_Max"] = "Максимум:",
            ["BatchProduction_Start"] = "ПОЧАТИ ВИРОБНИЦТВО",
            ["BatchProduction_Cancel"] = "СКАСУВАТИ",
            ["BatchProduction_Remaining"] = "Залишилось:",
            ["BatchProduction_CancelWarning"] = "Скасування поверне лише 50% ресурсів",
            ["BatchProduction_Input"] = "Витрати на 1 шт:",
            ["BatchProduction_Total"] = "Всього витрат:",
            ["BatchProduction_Producing"] = "ВИРОБНИЦТВО:",

            ["Research_ImprovedProduction"] = "Покращене виробництво",
            ["Research_EfficientConstruction"] = "Ефективне будівництво",
            ["Research_FastLearning"] = "Швидке навчання",
            ["Research_ExtendedRadius"] = "Розширений радіус",
            ["Research_AdvancedMining"] = "Покращена видобування",
            ["Research_OrganicBoost"] = "Органічний бум",

            ["ResearchDesc_ImprovedProduction"] = "Збільшує швидкість виробництва всіх будівель на 15%",
            ["ResearchDesc_EfficientConstruction"] = "Зменшує вартість будівництва на 20%",
            ["ResearchDesc_FastLearning"] = "Зменшує час досліджень на 25%",
            ["ResearchDesc_ExtendedRadius"] = "Збільшує радіус будівництва на 30%",
            ["ResearchDesc_AdvancedMining"] = "Шахти виробляють на 50% більше металу",
            ["ResearchDesc_OrganicBoost"] = "Фабрики виробляють на 40% більше органіки"
        },
        ["English"] = new()
        {
            ["MainMenu_Title"] = "anarts",
            ["MainMenu_NewGame"] = "new game",
            ["MainMenu_Continue"] = "continue",
            ["MainMenu_Settings"] = "settings",
            ["MainMenu_Exit"] = "exit",

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

            ["Game_Pause"] = "PAUSE",
            ["Game_Resume"] = "resume",
            ["Game_Settings"] = "settings",
            ["Game_SaveAndExit"] = "save and exit",

            ["NewGame_Title"] = "new world",
            ["NewGame_Start"] = "start",
            ["NewGame_StartingResources"] = "STARTING RESOURCES",
            ["NewGame_GameSettings"] = "GAME SETTINGS",
            ["NewGame_DisableSaving"] = "disable game saving",
            ["NewGame_EnableConsole"] = "enable console",

            ["Dialog_Yes"] = "yes",
            ["Dialog_No"] = "no",

            ["Game_Error_TooFar"] = "Too far from buildings",
            ["Game_Error_MineRocksFar"] = "Mine: too far from buildings and rocks",
            ["Game_Error_MineBuildingsFar"] = "Mine: too far from buildings",
            ["Game_Error_MineNoRocks"] = "Mine: must be near rocks",
            ["Game_Error_Overlap"] = "Overlaps with another building",
            ["Game_Error_SawmillTreesFar"] = "Sawmill: too far from buildings and trees",
            ["Game_Error_SawmillBuildingsFar"] = "Sawmill: too far from buildings",
            ["Game_Error_SawmillNoTrees"] = "Sawmill: must be near trees",
            ["Game_Error_BankLimit"] = "Bank can only be one",
            ["Game_Error_MarketLimit"] = "Market can only be one",

            ["Resource_Metal"] = "Metal",
            ["Resource_Organic"] = "Organic",
            ["Resource_Meat"] = "Meat",
            ["Resource_Wood"] = "Wood",
            ["Resource_Coal"] = "Coal",
            ["Resource_Bones"] = "Bones",
            ["Resource_Diamonds"] = "Diamonds",
            ["Resource_Generic"] = "Resource",

            ["Panel_Build"] = "CONSTRUCTION",
            ["Panel_Research"] = "RESEARCH",
            ["Panel_Trade"] = "TRADING",

            ["Building_Base"] = "BASE",
            ["Building_Factory"] = "FACTORY",
            ["Building_Mine"] = "MINE",
            ["Building_MeatFactory"] = "MEAT FACTORY",
            ["Building_Sawmill"] = "SAWMILL",
            ["Building_Bank"] = "BANK",
            ["Building_Marketplace"] = "MARKET",
            ["Building_Furnace"] = "FURNACE",
            ["Building_Altar"] = "ALTAR",
            ["Building_Crystallizer"] = "CRYSTALLIZER",
            ["Building_Generic"] = "BUILDING",

            ["BuildingDesc_Base"] = "Colony control center",
            ["BuildingDesc_Factory"] = "Produces organic",
            ["BuildingDesc_Mine"] = "Extracts metal",
            ["BuildingDesc_MeatFactory"] = "Produces meat",
            ["BuildingDesc_Sawmill"] = "Extracts wood",
            ["BuildingDesc_Bank"] = "Invests resources for profit",
            ["BuildingDesc_Marketplace"] = "Allows trading resources",
            ["BuildingDesc_Furnace"] = "Smelts wood into coal",
            ["BuildingDesc_Altar"] = "Converts coal, organic and meat into bones",
            ["BuildingDesc_Crystallizer"] = "Converts coal, organic and metal into diamonds",
            ["BuildingDesc_Generic"] = "Building",

            ["Research_Remaining"] = "remaining:",
            ["Research_Available"] = "AVAILABLE RESEARCH:",
            ["Research_Cost"] = "cost:",
            ["Research_Button"] = "RESEARCH",

            ["Trade_NeedMarket"] = "needs to build market for trading",
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

            ["BatchProduction_Amount"] = "amount to produce:",
            ["BatchProduction_Max"] = "maximum:",
            ["BatchProduction_Start"] = "START PRODUCTION",
            ["BatchProduction_Cancel"] = "CANCEL",
            ["BatchProduction_Remaining"] = "remaining:",
            ["BatchProduction_CancelWarning"] = "cancellation will return only 50% of resources",
            ["BatchProduction_Input"] = "cost per 1 unit:",
            ["BatchProduction_Total"] = "total cost:",
            ["BatchProduction_Producing"] = "PRODUCING:",

            ["Research_ImprovedProduction"] = "Improved production",
            ["Research_EfficientConstruction"] = "Efficient construction",
            ["Research_FastLearning"] = "Fast learning",
            ["Research_ExtendedRadius"] = "Extended radius",
            ["Research_AdvancedMining"] = "Advanced mining",
            ["Research_OrganicBoost"] = "Organic boost",

            ["ResearchDesc_ImprovedProduction"] = "Increases production speed of all buildings by 15%",
            ["ResearchDesc_EfficientConstruction"] = "Reduces construction cost by 20%",
            ["ResearchDesc_FastLearning"] = "Reduces research time by 25%",
            ["ResearchDesc_ExtendedRadius"] = "Increases construction radius by 30%",
            ["ResearchDesc_AdvancedMining"] = "Mines produce 50% more metal",
            ["ResearchDesc_OrganicBoost"] = "Factories produce 40% more organic"
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
