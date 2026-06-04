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
            ["NewGame_StartingResources"] = "початкові ресурси",
            ["NewGame_GameSettings"] = "налаштування гри",
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
            ["Game_Error_NotEnoughResources"] = "Недостатньо:",

            ["Resource_Metal"] = "метал",
            ["Resource_Organic"] = "органіка",
            ["Resource_Meat"] = "м'ясо",
            ["Resource_Wood"] = "дерево",
            ["Resource_Coal"] = "вугілля",
            ["Resource_Bones"] = "кісточки",
            ["Resource_Diamonds"] = "діаманти",
            ["Resource_Generic"] = "Ресурс",

            ["Panel_Build"] = "будівництво",
            ["Panel_Research"] = "дослідження",
            ["Panel_Trade"] = "торгівля",

            ["Building_Base"] = "основна база",
            ["Building_Factory"] = "фабрика",
            ["Building_Mine"] = "шахта",
            ["Building_MeatFactory"] = "м'ясофабрика",
            ["Building_Sawmill"] = "лісопилка",
            ["Building_Bank"] = "банк",
            ["Building_Marketplace"] = "маркет",
            ["Building_Furnace"] = "печка",
            ["Building_Altar"] = "вівтар",
            ["Building_Crystallizer"] = "кристалізатор",
            ["Building_Generic"] = "БУДІВЛЯ",

            ["BuildingDesc_Base"] = "центр управління колонією",
            ["BuildingDesc_Factory"] = "виробляє органіку",
            ["BuildingDesc_Mine"] = "видобуває метал",
            ["BuildingDesc_MeatFactory"] = "виробляє м'ясо",
            ["BuildingDesc_Sawmill"] = "видобуває дерево",
            ["BuildingDesc_Bank"] = "інвестує ресурси для прибутку",
            ["BuildingDesc_Marketplace"] = "дозволяє торгувати ресурсами",
            ["BuildingDesc_Furnace"] = "переплавляє дерево у вугілля",
            ["BuildingDesc_Altar"] = "перероблює вугілля, органіку та м'ясо у кісточки",
            ["BuildingDesc_Crystallizer"] = "перероблює вугілля, органіку та метал у діаманти",
            ["BuildingDesc_Generic"] = "будівля",

            ["Research_Remaining"] = "залишилось ще",
            ["Research_Available"] = "доступні дослідження:",
            ["Research_Cost"] = "вартість:",
            ["Research_Button"] = "дослідити",

            ["Trade_NeedMarket"] = "...потрібно побудувати Маркет для торгівлі",
            ["Trade_Step1"] = "оберіть ресурс для обміну",
            ["Trade_Rate"] = "курс: 100% → 60%",
            ["Trade_Back"] = "← назад",
            ["Trade_Step2"] = "оберіть кількість",
            ["Trade_Available"] = "доступно:",
            ["Trade_Amount"] = "кількість:",
            ["Trade_Next"] = "далі →",
            ["Trade_Step3"] = "оберіть ресурс для отримання",
            ["Trade_Giving"] = "віддаєте:",
            ["Trade_Receive"] = "отримаєте:",
            ["Trade_RateDisplay"] = "курс:",
            ["Trade_Confirm"] = "підтвердити обмін",
            ["Trade_Has"] = "є:",
            ["Trade_WillReceive"] = "отримаєте:",

            ["BuildingPanel_Level"] = "рівень:",
            ["BuildingPanel_Upgrade"] = "апгрейд",
            ["BuildingPanel_Delete"] = "знищити",
            ["BuildingPanel_Sure"] = "впевнені?",
            ["BuildingPanel_Production"] = "виробництво:",
            ["BuildingPanel_Investment"] = "інвестиція:",
            ["BuildingPanel_Invested"] = "інвестовано:",
            ["BuildingPanel_Remaining"] = "залишилось:",
            ["BuildingPanel_Cooldown"] = "кулдаун:",
            ["BuildingPanel_Invest"] = "інвестувати 100",

            ["BatchProduction_Amount"] = "кількість для виробництва:",
            ["BatchProduction_Max"] = "максимум:",
            ["BatchProduction_Start"] = "почати виробництво",
            ["BatchProduction_Cancel"] = "скасувати",
            ["BatchProduction_Remaining"] = "залишилось ще:",
            ["BatchProduction_CancelWarning"] = "скасування поверне лише 50% ресурсів!",
            ["BatchProduction_Input"] = "витрати на 1 шт:",
            ["BatchProduction_Total"] = "всього витрат:",
            ["BatchProduction_Producing"] = "виробництво:",

            ["Research_ImprovedProduction"] = "покращене виробництво",
            ["Research_EfficientConstruction"] = "ефективне будівництво",
            ["Research_FastLearning"] = "швидке навчання",
            ["Research_ExtendedRadius"] = "розширений радіус",
            ["Research_AdvancedMining"] = "покращене видобування",
            ["Research_OrganicBoost"] = "органічний бум",

            ["ResearchDesc_ImprovedProduction"] = "збільшує швидкість виробництва всіх будівель на 15%",
            ["ResearchDesc_EfficientConstruction"] = "зменшує вартість будівництва на 20%",
            ["ResearchDesc_FastLearning"] = "зменшує час досліджень на 25%",
            ["ResearchDesc_ExtendedRadius"] = "збільшує радіус будівництва на 30%",
            ["ResearchDesc_AdvancedMining"] = "шахти виробляють на 50% більше металу",
            ["ResearchDesc_OrganicBoost"] = "фабрики виробляють на 40% більше органіки"
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
            ["NewGame_StartingResources"] = "starting resources",
            ["NewGame_GameSettings"] = "game settings",
            ["NewGame_DisableSaving"] = "disable game saving",
            ["NewGame_EnableConsole"] = "enable console",

            ["Dialog_Yes"] = "yes",
            ["Dialog_No"] = "no",

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
            ["Game_Error_NotEnoughResources"] = "Not enough:",

            ["Resource_Metal"] = "metal",
            ["Resource_Organic"] = "organic",
            ["Resource_Meat"] = "meat",
            ["Resource_Wood"] = "wood",
            ["Resource_Coal"] = "coal",
            ["Resource_Bones"] = "bones",
            ["Resource_Diamonds"] = "diamonds",
            ["Resource_Generic"] = "resource",

            ["Panel_Build"] = "construction",
            ["Panel_Research"] = "research",
            ["Panel_Trade"] = "trading",

            ["Building_Base"] = "base",
            ["Building_Factory"] = "factory",
            ["Building_Mine"] = "mine",
            ["Building_MeatFactory"] = "meat factory",
            ["Building_Sawmill"] = "sawmill",
            ["Building_Bank"] = "bank",
            ["Building_Marketplace"] = "market",
            ["Building_Furnace"] = "furnace",
            ["Building_Altar"] = "altar",
            ["Building_Crystallizer"] = "crystallizer",
            ["Building_Generic"] = "building",

            ["BuildingDesc_Base"] = "colony control center",
            ["BuildingDesc_Factory"] = "produces organic",
            ["BuildingDesc_Mine"] = "extracts metal",
            ["BuildingDesc_MeatFactory"] = "produces meat",
            ["BuildingDesc_Sawmill"] = "extracts wood",
            ["BuildingDesc_Bank"] = "invests resources for profit",
            ["BuildingDesc_Marketplace"] = "allows trading resources",
            ["BuildingDesc_Furnace"] = "smelts wood into coal",
            ["BuildingDesc_Altar"] = "converts coal, organic and meat into bones",
            ["BuildingDesc_Crystallizer"] = "converts coal, organic and metal into diamonds",
            ["BuildingDesc_Generic"] = "building",

            ["Research_Remaining"] = "remaining:",
            ["Research_Available"] = "available research:",
            ["Research_Cost"] = "cost:",
            ["Research_Button"] = "research",

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
            ["BuildingPanel_Upgrade"] = "upgrade",
            ["BuildingPanel_Delete"] = "destroy",
            ["BuildingPanel_Sure"] = "sure?",
            ["BuildingPanel_Production"] = "production:",
            ["BuildingPanel_Investment"] = "investment:",
            ["BuildingPanel_Invested"] = "invested:",
            ["BuildingPanel_Remaining"] = "remaining:",
            ["BuildingPanel_Cooldown"] = "cooldown:",
            ["BuildingPanel_Invest"] = "invest 100",

            ["BatchProduction_Amount"] = "amount to produce:",
            ["BatchProduction_Max"] = "maximum:",
            ["BatchProduction_Start"] = "start production",
            ["BatchProduction_Cancel"] = "undo",
            ["BatchProduction_Remaining"] = "remaining:",
            ["BatchProduction_CancelWarning"] = "cancellation will return only 50% of resources",
            ["BatchProduction_Input"] = "cost per 1 unit:",
            ["BatchProduction_Total"] = "total cost:",
            ["BatchProduction_Producing"] = "producing:",

            ["Research_ImprovedProduction"] = "improved production",
            ["Research_EfficientConstruction"] = "efficient construction",
            ["Research_FastLearning"] = "fast learning",
            ["Research_ExtendedRadius"] = "extended radius",
            ["Research_AdvancedMining"] = "advanced mining",
            ["Research_OrganicBoost"] = "organic boost",

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
