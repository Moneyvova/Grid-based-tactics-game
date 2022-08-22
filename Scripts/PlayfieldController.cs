using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayfieldController : MonoBehaviour
{
    //Файл, описывающий карту для геймплея
    public MapFile usedMap;
    //Объект-поле для работы камеры. Задан в редакторе Unity
    public GameObject gameplayPlane,
                      //Объект для хранения графических клеток поля
                      terrainContainer,
                      //Объект-базис клетки. Задан в редакторе Unity
                      tilePrefab,
                      //Объект-базис юнита.Задан в редакторе Unity
                      unitPrefab;
    //Файлы анимторов для анимаций юнитов
    //Пехота
    public RuntimeAnimatorController infantryAnimator,
        //Артилерия
                                     vehicleAnimator,
        //Вертолеты
                                     copterAnimator;
    //Основная камера сцены для введения координат из файла
    public CameraController usedCam;

    //Поправка местоположения на сцене
    private const float offset = 0.5f;  
    //Счёт ходов
    private int turnCount;
    //Списки используемых юнитов и клеток
    private List<int> tileList;
    private List<int> unitList;

    private TacticalUI usedUI;

    public PlayfieldController()
    {
        turnCount = 1;
        tileList = new List<int>();
        unitList = new List<int>();
    }

    // Start is called before the first frame update
    void Awake()
    {
        GetLists();
        usedMap = new MapFile(GlobalVariables.mapName);
        usedUI = usedCam.GetComponentInChildren<TacticalUI>();
        GlobalVariables.playfield = new TerrainData[usedMap.height, usedMap.width];
        GenerateField();
        PopulateField();
    }
    
    void FixedUpdate()
    {
        if (GlobalVariables.playerUnits.Count <= 0)
        {
            AsyncOperation loadingOperation = SceneManager.LoadSceneAsync(0);
        }
        else if (GlobalVariables.enemyUnits.Count <= 0)
        {
            AsyncOperation loadingOperation = SceneManager.LoadSceneAsync(1);
        }
    }

    //Возврат очков действий для юнитов
    public IEnumerator RefreshUnits(List<UnitData> list)
    {
        foreach (UnitData unit in list)
        {
            unit.acted = false;
            unit.ToggleColourChange(Colours.None);
        }
        yield break;
    }

    //Отработка действий ИсИг
    public IEnumerator EnemyAction()
    {
        bool forceQuit = true;
        while (forceQuit)
        {
            forceQuit = false;
            foreach (UnitData unit in GlobalVariables.enemyUnits)
            {
                StartCoroutine(unit.TakeAction());
                yield return new WaitForSeconds(3f);
                if (unit == null)
                {
                    forceQuit = true;
                    break;
                }
            }
        }
        turnCount++;
        usedUI.EndTurn();
        yield break;
    }

    //Подготовка списков перед генерацией поля
    private void GetLists()
    {
        List<ArrayList> tilePool;
        tilePool = GlobalVariables.database.DatabaseDataGet(GlobalVariables.database.SelectData("TilePool", "MapID", 1));
        foreach (ArrayList row in tilePool)
            tileList.Add(Convert.ToInt32(row[1]));
        tilePool = GlobalVariables.database.DatabaseDataGet(GlobalVariables.database.SelectData("UnitPool", "MapID", 1));
        foreach (ArrayList row in tilePool)
            unitList.Add(Convert.ToInt32(row[1]));
    }

    //Создание игрового поля из полученной карты
    public void GenerateField()
    {
        AdjustPlaneSize();
        for (int i = 0; i < usedMap.height; i++)
        {
            for (int j = 0; j < usedMap.width; j++)
                GlobalVariables.playfield[i,j] = CreateTile(new Vector2Int(i, j));
        }
        usedCam.maxZoom = usedMap.maxZoom;
        usedCam.zoomTarget = usedMap.maxZoom;
    }
    
    //Настройка плана поля под размеры карты
    private void AdjustPlaneSize()
    {
        GlobalVariables.playfieldDimentions = new Vector2Int(usedMap.height, usedMap.width);
        float height = (float)usedMap.height;
        float width = (float)usedMap.width;

        gameplayPlane.transform.localScale = new Vector3(height / 10, 1f, width / 10);
        gameplayPlane.transform.position   = new Vector3(height / 2, 0, width / 2);
    }
    
    //Функция создания клетки и присвоение ей стандартных значений
    private TerrainData CreateTile(Vector2Int coords)
    {
        //Создание объекта клетки на сцене
        GameObject tile = Instantiate(tilePrefab, gameplayPlane.transform, true);
        tile.name = "Tile" + coords.x + "." + coords.y;
        tile.transform.position = new Vector3(coords.x + offset, offset, coords.y + offset);
        //Получение класса данных клетки
        tile.TryGetComponent(out TerrainData terrainSettings);
        //Установка данных о координатах клетки
        terrainSettings.assignedCoord = coords;
        //Использование пресета клетки
        ArrayList tilePreset = GetTileData(coords);

        foreach (UIButtons tileName in GlobalVariables.localization.tileNames)
        {
            string result = tileName.MatchValue(tilePreset[0].ToString());
            if (result != tilePreset[0].ToString())
            {
                terrainSettings.tileName = result;
                break;
            }
            else
                terrainSettings.tileName = result;
        }

        terrainSettings.hinderClass = (TerrainData.TileType)Convert.ToInt32(tilePreset[2]);
        terrainSettings.moveCost = Convert.ToInt32(tilePreset[3]);
        terrainSettings.bonusAtk = Convert.ToInt32(tilePreset[4]);
        terrainSettings.bonusDef = Convert.ToInt32(tilePreset[5]);
        terrainSettings.bonusAcc = Convert.ToInt32(tilePreset[6]);
        CreateVisualTerrain(coords, tilePreset[1].ToString());

        return terrainSettings;
    }

    //Получение данных клетки из БД
    private ArrayList GetTileData(Vector2Int coords)
    {
        int tilePreset = tileList.IndexOf(usedMap.GetPreset(coords));
        List<ArrayList> data;
        ArrayList row;
        if (tilePreset > 0)
        {
            data = GlobalVariables.database.DatabaseDataGet(GlobalVariables.database.SelectData("Tiles", tilePreset + 1));
            row = data[0];
        }
        else
        {
            data = GlobalVariables.database.DatabaseDataGet(GlobalVariables.database.SelectData("Tiles", 1));
            row = data[0];
        }
        return row;
    }

    //Создание на сцене визульного офромления клетки
    private void CreateVisualTerrain(Vector2Int coords, string terrainName)
    {
        GameObject terrain = Resources.Load("TerrainGFX/"+ terrainName) as GameObject;
        Vector3 terrainPosition = new Vector3(coords.x, offset, coords.y);
        terrain = Instantiate(terrain, terrainContainer.transform, true);
        terrain.transform.localPosition = terrainPosition;
    }

    //Функция для заполнения юнитами поля
    public void PopulateField()
    {
        foreach (UnitParameters parameters in usedMap.usedUnits)
            CreateUnit(parameters);
    }

    /// <summary>
    /// Получение данных из таблицы Units БД и создание объектов сцены
    /// </summary>
    /// <param name="parameters">Параметры из файла карты</param>
    private void CreateUnit(UnitParameters parameters)
    {
        TerrainData tile = GlobalVariables.playfield[parameters.coordX, parameters.coordY];
        GameObject unitContainer = Instantiate(unitPrefab, tile.gameObject.transform);
        unitContainer.transform.localPosition = new Vector3(0, 1f, 0);
        unitContainer.TryGetComponent(out UnitData unit);

        unit.startingPosition = new Vector2Int(parameters.coordX, parameters.coordY);
        switch (parameters.affiliation)
        {
            case UnitData.Affiliations.PlayerUnit:
                GlobalVariables.playerUnits.Add(unit);
                goto default; 
            case UnitData.Affiliations.EnemyUnit:
                GlobalVariables.enemyUnits.Add(unit);
                goto default;
            default:
                unit.unitSide = parameters.affiliation;
                break;
        }
        //Параметры из таблицы Units
        int linkValue = SetUnitData(parameters.preset, unit);
        //Параметры из таблицы Classes
        linkValue = SetClassData(linkValue, unit);
        //Параметры из таблицы WeaponTypes
        linkValue = SetWeaponTypeData(linkValue, unit);
        //Параметры из таблицы WeaponGrades
        SetWeaponData(linkValue, unit);

        unit.CalculateThreatArea();
    }

    //Получение данных юнита из БД
    private int SetUnitData(int preset, UnitData unit)
    {
        int unitPreset = unitList.IndexOf(preset);
        List<ArrayList> data;
        ArrayList row;
        if (unitPreset > 0)
        {
            data = GlobalVariables.database.DatabaseDataGet(GlobalVariables.database.SelectData("Units", unitPreset + 1));
            row = data[0];
        }
        else
        {
            data = GlobalVariables.database.DatabaseDataGet(GlobalVariables.database.SelectData("Units", 1));
            row = data[0];
        }
        foreach (UIButtons unitName in GlobalVariables.localization.unitNames)
        {
            string result = unitName.MatchValue(row[0].ToString());
            if (result != row[0].ToString())
            {
                unit.charName = result;
                break;
            }
            else
                unit.charName = result;
        }
        unit.charDesc = row[1].ToString();
        unit.health = Convert.ToInt32(row[3]);
        unit.attack = Convert.ToInt32(row[4]);
        unit.hitChance = Convert.ToInt32(row[5]);
        unit.defence = Convert.ToInt32(row[6]);
        unit.speed = Convert.ToInt32(row[7]);

        SetUpVisual(row[0].ToString(), unit);

        return Convert.ToInt32(row[2]);
    }
    
    /// <summary>
    /// Добавление на сцену графических компонент
    /// </summary>
    /// <param name="strParam">Строка, которая представляет все параметры</param>
    /// <param name="unit">Объект-ссылка на юнита</param>
    private void SetUpVisual(string strParam, UnitData unit)
    {
        int symCount = strParam.IndexOf("_", 2);
        string usedPortrait = strParam.Substring(0, symCount);
        Sprite img = Resources.Load<Sprite>("ImagesUI/" + usedPortrait);
        if (img != null)
            unit.charImage = img;
        else
        {
            img = Resources.Load<Sprite>("ImagesUI/w_pistol_test");
            unit.charImage = img;
        }

        GameObject model = Resources.Load("Models/" + usedPortrait) as GameObject;
        model = Instantiate(model, unit.gameObject.transform);
        model.transform.rotation = Quaternion.Euler(new Vector3(-30, 90, 0));
        
        model.transform.GetChild(1).gameObject.layer = 9;
        if (unit.unitSide == UnitData.Affiliations.EnemyUnit)
            model.transform.localScale = new Vector3(-1f, 1f, 1f);
        model.TryGetComponent(out Animator anim);

        int symCount2 = strParam.IndexOf("_", symCount + 1) - symCount - 1;
        string weapon;
        if (symCount2 > 0)
            weapon = strParam.Substring(symCount + 1, symCount2);
        else
            weapon = strParam.Substring(symCount + 1);

        anim.runtimeAnimatorController = infantryAnimator;
        if (weapon == "rifle")
            anim.SetBool("rifle", true);
    }

    //Получение данных класса юнита из БД
    private int SetClassData(int rowNumber, UnitData unit)
    {
        List<ArrayList> data;
        ArrayList row;
        data = GlobalVariables.database.DatabaseDataGet(GlobalVariables.database.SelectData("Classes", rowNumber));
        row = data[0];

        unit.charClassName = row[0].ToString();
        unit.charClassDesc = row[1].ToString();
        unit.movementType = (UnitData.MovementTypes)(Convert.ToInt32(row[3]));
        unit.moveRange = Convert.ToInt32(row[4]);
        unit.health += Convert.ToInt32(row[5]);
        unit.healthRemaining = unit.health;
        unit.attack += Convert.ToInt32(row[6]);
        unit.hitChance += Convert.ToInt32(row[7]);
        unit.defence += Convert.ToInt32(row[8]);
        unit.speed += Convert.ToInt32(row[9]);

        return Convert.ToInt32(row[2]);
    }
    
    //Получение данных класса вооружения из БД
    private int SetWeaponTypeData(int rowNumber, UnitData unit)
    {
        List<ArrayList> data;
        ArrayList row;
        data = GlobalVariables.database.DatabaseDataGet(GlobalVariables.database.SelectData("WeaponTypes", rowNumber));
        row = data[0];

        unit.weaponName = row[0].ToString();
        unit.minAtkRange = Convert.ToInt32(row[2]);
        unit.maxAtkRange = Convert.ToInt32(row[3]);

        return rowNumber;
    }
    
    //Получение данных единиц вооружения из БД
    private void SetWeaponData(int rowNumber, UnitData unit)
    {
        List<ArrayList> data;
        ArrayList row;
        data = GlobalVariables.database.DatabaseDataGet(GlobalVariables.database.SelectData("WeaponGrades", "WeaponTypeID", rowNumber));
        row = data[0];

        Sprite img = Resources.Load<Sprite>("ImagesUI/" + row[1].ToString());
        if (img != null)
            unit.weaponImage = img;
        else
        {
            img = Resources.Load<Sprite>("ImagesUI/w_pistol_test");
            unit.weaponImage = img;
        }
        unit.attack += Convert.ToInt32(row[2]);
        unit.hitChance = (int)Mathf.Ceil(unit.hitChance * Convert.ToInt32(row[3]) / 100);
    }
}
