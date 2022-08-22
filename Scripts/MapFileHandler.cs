using System.Collections.Generic;
using UnityEngine;

//Класс для работы с файлами JSON описывающие карты
[System.Serializable]
public class MapFile
{//Максимальное отдаление камеры на карте
    public int maxZoom,
        //Ширина игровой карты в клетках
               width,
        //Высота игровой карты в клетках
               height; 

    //Список клеток с особенными параметрами
    public List<TileParameters> modifiedTiles = new List<TileParameters>();
    
    public List<UnitParameters> usedUnits = new List<UnitParameters>();

    //Алгоритм действий при инициализации класса
    public MapFile(string mapName)
    {
        LoadMapFile(mapName);
    }

    //Загрузить карту из файла
    public void LoadMapFile(string map)
    {
        TextAsset mapFile = Resources.Load("MapData/" + map) as TextAsset;
        modifiedTiles.Clear();
        JsonUtility.FromJsonOverwrite(mapFile.text, this);
    }

    //Сохранить карту в файл
    public void SaveMapToFile(string newName)
    {
        string output = JsonUtility.ToJson(this, true);
        System.IO.File.WriteAllText("Assets/Resources/MapData/" + newName + ".json", output);
    }

    //Получение пресета клетки
    public int GetPreset(Vector2Int coords)
    {
        foreach (TileParameters tile in modifiedTiles)
        {
            if (tile.coordX == coords.x & tile.coordY == coords.y)
                return tile.preset;
        }
        return 1;
    }
}

//Класс обработки клетки карты из файла
[System.Serializable]
public class TileParameters
{
    public int coordX,//Координата Х
               coordY;//Координата У
    //Используемый макет данных клетки
    public int preset;

    //Объявление класса
    public TileParameters(int x, int y, int usedPreset)
    {
        coordX = x;
        coordY = y;
        preset = usedPreset;
    }
}
// Класс обработки юнита на карте из файла
[System.Serializable]
public class UnitParameters
{
    public int coordX,      //Координата Х
               coordY,      //Координата У
               preset;      //Используемый макет юнита на карте
    //Принадлежность юнита
    public UnitData.Affiliations affiliation;

    //Объявление класса
    public UnitParameters(int x, int y, int usedPreset)
    {
        coordX = x;
        coordY = y;
        preset = usedPreset;
        affiliation = UnitData.Affiliations.PlayerUnit;
    }
}