using System.Collections.Generic;
using UnityEngine;

//Основной класс для работы с локализацией приложения
[System.Serializable]
public class LocalizationController
{
    public string localizationName;
    public string saveLabel,
                  loadLabel,
                  saveBtnLabel,
                  loadBtnLabel,
                  deleteBtnLabel,
                  actor1,
                  actor2;
    public MainMenuScreen mainMenu = new MainMenuScreen();
    public SettingsScreen settings = new SettingsScreen();
    public TacticalScreen gameplayField = new TacticalScreen();
    public PreparationScreen preparation = new PreparationScreen();
    public List<UIButtons> tileNames = new List<UIButtons>();
    public List<UIButtons> unitNames = new List<UIButtons>();

    //Инициализация класса без параметров
    public LocalizationController()
    {
        GlobalVariables.locales = GetLocales();
        LoadLocalizationFile("ru");
    }
    //Инициализация класса с параметром
    public LocalizationController(string overrideLocale)
    {
        GlobalVariables.locales = GetLocales();
        LoadLocalizationFile(overrideLocale);
    }
    //Вывод списка доступных локализаций
    public List<string> GetLocales()
    {
        TextAsset[] locales = Resources.LoadAll<TextAsset>("Localization/");
        List<string> foundLocales = new List<string>();
        foreach (TextAsset locale in locales)
            foundLocales.Add(locale.name);
        return foundLocales;
    }
    //Функция перезаписи локализации класса
    public void LoadLocalizationFile(string name)
    {
        TextAsset localeFile = Resources.Load("Localization/" + name) as TextAsset;
        if (localeFile != null)
            JsonUtility.FromJsonOverwrite(localeFile.text, this);
    }
}

//Класс для локализации главного меню
[System.Serializable]
public class MainMenuScreen
{
    public List<UIButtons> btns = new List<UIButtons>();
}

//Класс для локализации кнопок в интерфейсе
[System.Serializable]
public class UIButtons
{
    public string internalName;
    public string value;
    public string MatchValue(string match)
    {
        if (internalName == match)
        {
            if (value != "")
                return value;
            else
                return internalName;
        }
        return match;
    }
}

//Класс для локализации панели настроек
[System.Serializable]
public class SettingsScreen
{
    public List<UIButtons> panelBtns = new List<UIButtons>();
    //Подменю настройки управления
    public string controlPanelName;
    public List<UIButtons> controls = new List<UIButtons>();
    //Подменю настройки звука
    public string audioPanelName;
    public List<UIButtons> audioBtns = new List<UIButtons>();
    //Подменю настройки графики
    public string gfxPanelName;
    public List<UIButtons> gfxBtns = new List<UIButtons>();

    public List<string> gfxPresets = new List<string>();

    //Подменю настройки игрового процесса
    public string gpPanelName;
    public List<UIButtons> gpBtns = new List<UIButtons>();
}

//Класс для локализации панели предсказания боя
[System.Serializable]
public class PreparationScreen
{
    public List<UIButtons> btns = new List<UIButtons>();
    public string mapName;
    public string mapHint;
}

//Класс для локализации панелей в игровом процессе
[System.Serializable]
public class TacticalScreen
{
    public CombatOutcome combatOutcome = new CombatOutcome();
    public TurnAnnouncements turn = new TurnAnnouncements();
    public GameplayMenu menu = new GameplayMenu();
}

//Класс для локализации меню в игровом процессе
[System.Serializable]
public class GameplayMenu
{
    public string name;
    public List<UIButtons> btns = new List<UIButtons>();
}

//Класс для локализации панели объявления о ходе
[System.Serializable]
public class TurnAnnouncements
{
    public string player;
    public string enemy;
}

//Класс для локализации панели предсказания боя
[System.Serializable]
public class CombatOutcome
{
    public string labelAccuracy;
    public string labelDamage;
    public string labelRetaliate;

    public string accuracyMin;
    public string accuracyAvg;
    public string accuracyGood;
    public string accuracyMax;

    public string retaliateYes;
    public string retaliateNo;

    public string damageMin;
    public string damageAvg;
    public string damageGood;
    public string damageMax;
}
