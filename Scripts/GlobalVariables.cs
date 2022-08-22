using System.Collections.Generic;
using UnityEngine;

//Перечисление направлений на 2D пространстве
public enum Directions
{
    None,
    Up        = 1,
    Left      = 2,
    Right     = 4,
    Down      = 8,
    UpLeft    = Up   | Left,
    UpRight   = Up   | Right,
    DownLeft  = Down | Left,
    DownRight = Down | Right
}

//Используемые обозначения модификатора цвета у объектов
public enum Colours
{
    //Без изменений
    None,
    //Объект выбран
    Select,
    //Объект выделен/подсвечен
    Highlight,
    //Курсор находся на объекте
    Hover,
    //Объект закончил движение
    Moved
}

public static class GlobalVariables
{
    //Название вызываемой карты
    public static string mapName = "map1";
    //Название для вызова постановочной сцены
    public static string cutsceneName;
    //Список юнитов игрока и юнитов-противников
    public static List<UnitData> playerUnits = new List<UnitData>(),   
                                 enemyUnits = new List<UnitData>();    
    //Переменные для работы функций локализации
    public static string localeName = "ru";
    public static List<string> locales = new List<string>();
    public static LocalizationController localization = new LocalizationController();
    //Переменная для обработки данных из БД
    public static DatabaseController database = new DatabaseController();
    //Переменная для хранения размеров игрового поля для быстрого доступа
    public static Vector2Int playfieldDimentions;
    //Хранилище данных о клетках
    public static TerrainData[,] playfield;
    //Массив с кнопками управления с клавиатуры
    public static Dictionary<string, string> keymaps = new Dictionary<string, string> 
    {
        { "CameraUp"    , "w"},
        { "CameraDown"  , "s"},
        { "CameraLeft"  , "a"},
        { "CameraRight" , "d"},
        { "EndTurn"     , "e"},
        { "OpenMenu"    , "f1"},
        { "CancelAction", "escape"}
    };
}