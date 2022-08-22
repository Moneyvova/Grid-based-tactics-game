//Используемые библиотеки в данном файле
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Mono.Data.Sqlite;
using UnityEngine;

//Класс для работы с таблицами БД
[System.Serializable]
public class DatabaseController
{
    //Перечисление разрешённых действий с БД
    public enum TableActions
    {
        Create,
        Drop,
        Select,
        Insert
    }
    //Новый путь до БД
    public string databaseNewPath,
                  //Название БД
                  databaseName;
    //Массив имён таблиц БД
    public List<string> tableNames;
    //Массив таблиц БД
    public List<DatabaseTableController> tables;
    //Переменная, отвечающая за связь с БД
    private static SqliteConnection connection;
    //Используемый путь к БД
    private static string databasePath;
    //Путь к БД по-умолчанию
    private static string defaultPath = Application.dataPath + "/Resources/";
    //Постоянное начало запроса на соединение
    private const string pathStart = "URI=file:";
    private const string baseSchemaFile = "DB_base";

    //При инициализации класса используется база по-умолчанию
    public DatabaseController()
    {
        databaseNewPath = defaultPath;
        tables = new List<DatabaseTableController>();
        this.ChangeDatabase("baseData");
    }

    //Переназначение базы
    public void ChangeDatabase(string newBase)
    {
        databaseName = newBase;
        databasePath = pathStart + databaseNewPath + databaseName + ".db";
        //Перезапись схемы таблиц на основную
        TextAsset dbFile = Resources.Load(baseSchemaFile) as TextAsset;
        tables.Clear();
        JsonUtility.FromJsonOverwrite(dbFile.text, this);
        foreach (string checkedTable in tableNames)
        {
            //Проверка на соответствие макету
            bool check = DatabaseAction(SelectData(checkedTable));
            if (!check)
            {
                DatabaseAction(DeleteTable(checkedTable));
                DatabaseAction(CreateTable(checkedTable));
                SeedTable(checkedTable);
            }
        }
    }
    //Заполнение таблицы данными по-умолчанию
    private void SeedTable(string table)
    {
        BaseDataSeeder seeder = new BaseDataSeeder();
        switch (table)
        {
            case "WeaponTypes":
                foreach (BaseDataSeeder.WeaponTypesTable row in seeder.WeaponTypes)
                    DatabaseAction(InsertData(table, row.parseData()));
                break;
            case "WeaponGrades":
                foreach (BaseDataSeeder.WeaponGradesTable row in seeder.WeaponGrades)
                    DatabaseAction(InsertData(table, row.parseData()));
                break;
            case "MoveTypes":
                foreach (BaseDataSeeder.MoveTypesTable row in seeder.MoveTypes)
                    DatabaseAction(InsertData(table, row.parseData()));
                break;
            case "Classes":
                foreach (BaseDataSeeder.ClassesTable row in seeder.Classes)
                    DatabaseAction(InsertData(table, row.parseData()));
                break;
            case "Units":
                foreach (BaseDataSeeder.UnitsTable row in seeder.Units)
                    DatabaseAction(InsertData(table, row.parseData()));
                break;
            case "VictoryConds":
                foreach (BaseDataSeeder.VictoryCondsTable row in seeder.VictoryConds)
                    DatabaseAction(InsertData(table, row.parseData()));
                break;
            case "Tiles":
                foreach (BaseDataSeeder.TilesTable row in seeder.Tiles)
                    DatabaseAction(InsertData(table, row.parseData()));
                break;
            case "Maps":
                foreach (BaseDataSeeder.MapsTable row in seeder.Maps)
                    DatabaseAction(InsertData(table, row.parseData()));
                break;
            case "TilePool":
                foreach (BaseDataSeeder.TilePoolTable row in seeder.TilePool)
                    DatabaseAction(InsertData(table, row.parseData()));
                break;
            case "UnitPool":
                foreach (BaseDataSeeder.UnitPoolTable row in seeder.UnitPool)
                    DatabaseAction(InsertData(table, row.parseData()));
                break;
        }
    }
    //Функция обработки события БД без получаемых данных
    public bool DatabaseAction(string action)
    {
        using (connection = new SqliteConnection(databasePath))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = action;
                try
                {
                    command.ExecuteNonQueryAsync();
                    connection.Close();
                    return true;
                }
                catch (SqliteException exception)
                {
                    Debug.Log(exception.ErrorCode);
                    connection.Close();
                    return false;
                }
            }
        }
    }
    //Функция получения данных из БД
    public List<ArrayList> DatabaseDataGet(string action)
    {
        using (connection = new SqliteConnection(databasePath))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = action;
                try
                {
                    IDataReader result = command.ExecuteReader();
                    List<ArrayList> receivedData = new List<ArrayList>();
                    while (result.Read())
                    {
                        ArrayList array = new ArrayList();
                        for (int i = 1; i < result.FieldCount; i++)
                            array.Add(result[i]);
                        receivedData.Add(array);
                    }
                    connection.Close();
                    return receivedData;
                }
                catch (SqliteException exception)
                {
                    Debug.Log(exception.ErrorCode);
                    connection.Close();
                    return null;
                }
            }
        }
    }
    //Функция создания таблицы
    public string CreateTable(string tableName)
    {
        int num = tableNames.IndexOf(tableName);
        string command = TableQueryString(tableName, TableActions.Create) + tables[num].GetAllColumnParams();
        return command;
    }
    //Функция удаления таблицы
    public string DeleteTable(string tableName)
    {
        return TableQueryString(tableName, TableActions.Drop);
    }
    //Функция добавления данных в таблицу
    public string InsertData(string tableName, ArrayList data)
    {
        int num = tableNames.IndexOf(tableName);
        string command = "INSERT INTO " + tableName + " " + tables[num].GetAllColumnNames();
        command += " VALUES (";
        for (int i = 0; i < data.Count; i++)
        {
            if (i > 0)
                command += ", ";
            if (data[i].GetType() == typeof(string))
                command += "'" + data[i] + "'";
            else if (data[i].GetType() == typeof(int))
                command += data[i].ToString();
        }
        command += ");";
        return command;
    }
    //Функция получения всех данных из таблицы
    public string SelectData(string tableName, int whereID = -1)
    {
        if (whereID < 0)
            return "SELECT * FROM " + tableName + ";";
        else
            return "SELECT * FROM " + tableName + " WHERE ID = " + whereID + ";";
    }
    //Функция получения всех данных из таблицы
    public string SelectData(string tableName, string column, int value)
    {
        return "SELECT * FROM " + tableName + " WHERE " + column + " = " + value + ";";
    }
    //Функция создания строки для запроса
    private string TableQueryString(string tableName, TableActions action)
    {
        string command = "";
        switch (action)
        {
            case TableActions.Create:
                command = "CREATE";
                break;
            case TableActions.Drop:
                command = "DROP";
                break;
        }
        command += " TABLE";
        if (action == TableActions.Drop)
            command += " IF EXISTS ";
        else
            command += " IF NOT EXISTS ";
        command += tableName + " ";
        if (action == TableActions.Drop)
            command += ";";
        else
            command += " ";
        return command;
    }
}
//Класс для работы с таблицами БД
[System.Serializable]
public class DatabaseTableController
{
    //Имя таблицы
    public string tableName;
    //Поля таблицы
    public List<DatabaseTableColumnData> columnData = new List<DatabaseTableColumnData>();

    //Создание таблицы с определённым именем без полей кроме ключевого
    public DatabaseTableController(string newName)
    {
        tableName = newName;
        columnData.Add(new DatabaseTableColumnData());
    }

    //Передача данных обо всех колонках
    public string GetAllColumnParams()
    {
        string output = "(";
        //Создание вывода данных о колонках
        for (int i = 0; i < columnData.Count; i++)
        {
            if (i > 0)
                output += ", " + columnData[i].GetColumnData();
            else
                output += columnData[i].GetColumnData();
        }
        //Дополнение вывода данными о внешних ключах
        foreach (DatabaseTableColumnData column in columnData)
        {
            if (column.keyType == DatabaseTableColumnData.KeyTypes.Foreign)
                output += column.GetForeignParams();
        }
        output += ");";
        return output;
    }
    //Передача перечисления всех колонок
    public string GetAllColumnNames()
    {
        string output = "(";
        for (int i = 1; i < columnData.Count; i++)
        {
            if (i > 1)
                output += ", " + columnData[i].columnName;
            else
                output += columnData[i].columnName;
        }
        output += ")";
        return output;
    }
    //Порядковый номер искомой колонки
    public int ColumnIndex(string searchColumn)
    {
        int i = 0;
        foreach (DatabaseTableColumnData column in columnData)
        {
            if (column.columnName == searchColumn)
                return i - 1;
            else
                i++;
        }
        return -1;
    }
}
//Класс для работы с столбцами БД
[System.Serializable]
public class DatabaseTableColumnData
{
    //Перечисление поддерживаемых типов ключей
    public enum KeyTypes
    {
        None,
        Primary,
        Foreign
    }
    //Поддерживаемые типы данных
    public enum DataTypes
    {
        Integer,
        Varchar
    }
    //Параметры столбца
    public KeyTypes keyType = KeyTypes.None;
    public string columnName;
    public DataTypes dataType;
    public int length = -1;
    public bool autoIncrement = false;
    public bool columnNullable = false;
    public string foreignTable;
    public string foreignColumn;

    //Создаваемые данные класса по-умолчанию
    public DatabaseTableColumnData()
    {
        columnName = "ID";
        dataType = DataTypes.Integer;
        autoIncrement = true;
        keyType = KeyTypes.Primary;
    }
    //Перегрузка для создания обычных полей
    public DatabaseTableColumnData(string newName, DataTypes type, int newLength = -1, bool nullable = false)
    {
        columnName = newName;
        dataType = type;
        length = newLength;
        columnNullable = nullable;
    }
    //Перегрузка для создания ключевых полей
    public DatabaseTableColumnData(string newName, DataTypes type, KeyTypes key, bool autoInc = false)
    {
        columnName = newName;
        this.keyType = key;
        if (type == DataTypes.Integer)
        {
            dataType = type;
            autoIncrement = autoInc;
        }
        else
            dataType = type;
    }
    //Перегрузка для создания внешних ключей
    public DatabaseTableColumnData(string newColumnName, KeyTypes key, string foreignTableName, string foreignColumnName)
    {
        columnName = newColumnName;
        dataType = DataTypes.Integer;
        keyType = key;
        foreignTable = foreignTableName;
        foreignColumn = foreignColumnName;
    }

    //Получить данные о структуре колонки
    public string GetColumnData()
    {
        return columnName + " " + FormatColumnParams();
    }
    //Создание определителя колонки на языке SQL
    private string FormatColumnParams()
    {
        string result = "";
        switch (dataType)
        {
            case DataTypes.Integer:
                result = "INTEGER";
                break;
            case DataTypes.Varchar:
                result = "VARCHAR";
                if (length > 0)
                    result += ("(" + length.ToString() + ")");
                else
                {
                    length = 255;
                    result += "(255)";
                }
                break;
        }
        if (keyType == KeyTypes.Primary)
            result += " PRIMARY KEY";
        if (autoIncrement)
            result += " AUTOINCREMENT";
        if (!columnNullable)
            result += " NOT NULL";
        return result;
    }
    //Дополнение для колонок с внешним ключом
    public string GetForeignParams()
    {
        return ", FOREIGN KEY (" + columnName + ") REFERENCES " +
               foreignTable + " (" + foreignColumn + ") ON DELETE SET NULL";
    }
}

//Класс-описание для вставки данных по умолчанию в БД
[System.Serializable]
public class BaseDataSeeder
{
    public List<WeaponTypesTable> WeaponTypes = new List<WeaponTypesTable>();
    public List<WeaponGradesTable> WeaponGrades = new List<WeaponGradesTable>();
    public List<MoveTypesTable> MoveTypes = new List<MoveTypesTable>();
    public List<ClassesTable> Classes = new List<ClassesTable>();
    public List<UnitsTable> Units = new List<UnitsTable>();
    public List<VictoryCondsTable> VictoryConds = new List<VictoryCondsTable>();
    public List<TilesTable> Tiles = new List<TilesTable>();
    public List<MapsTable> Maps = new List<MapsTable>();
    public List<TilePoolTable> TilePool = new List<TilePoolTable>();
    public List<UnitPoolTable> UnitPool = new List<UnitPoolTable>();
    //Использование сидирования по-умолчанию 
    public BaseDataSeeder()
    {
        TextAsset file = Resources.Load("DB_data") as TextAsset;
        JsonUtility.FromJsonOverwrite(file.text, this);
    }
    //Макеты таблиц
    [System.Serializable]
    public class WeaponTypesTable
    {
        public string WeaponTypeName,
                      ModelName;
        public int MinRange,
                   MaxRange;
        public ArrayList parseData()
        {
            ArrayList result = new ArrayList() {
                WeaponTypeName,
                ModelName,
                MinRange,
                MaxRange
            };
            return result;
        }
    }
    [System.Serializable]
    public class WeaponGradesTable
    {
        public int WeaponTypeID;
        public string IconName;
        public int Power,
                   Accuracy;
        public ArrayList parseData()
        {
            ArrayList result = new ArrayList() {
                WeaponTypeID,
                IconName,
                Power,
                Accuracy
            };
            return result;
        }
    }
    [System.Serializable]
    public class MoveTypesTable
    {
        public int Value;
        public string MoveTypeName;
        public ArrayList parseData()
        {
            ArrayList result = new ArrayList() {
                MoveTypeName,
                Value
            };
            return result;
        }
    }
    [System.Serializable]
    public class ClassesTable
    {
        public string ClassName,
                      ClassDesc;
        public int WeaponTypeID,
                   MoveTypeID,
                   MoveRange,
                   HealthBase,
                   AttackBase,
                   AccuracyBase,
                   DefenceBase,
                   SpeedBase;
        public ArrayList parseData()
        {
            ArrayList result = new ArrayList() {
                ClassName,
                ClassDesc,
                WeaponTypeID,
                MoveTypeID,
                MoveRange,
                HealthBase,
                AttackBase,
                AccuracyBase,
                DefenceBase,
                SpeedBase
            };
            return result;
        }
    }
    [System.Serializable]
    public class UnitsTable
    {
        public string UnitName,
                      UnitDesc;
        public int ClassID,
                   HealthModifier,
                   AttackModifier,
                   AccuracyModifier,
                   DefenceModifier,
                   SpeedModifier;
        public ArrayList parseData()
        {
            ArrayList result = new ArrayList() {
                UnitName,
                UnitDesc,
                ClassID,
                HealthModifier,
                AttackModifier,
                AccuracyModifier,
                DefenceModifier,
                SpeedModifier
            };
            return result;
        }
    }
    [System.Serializable]
    public class VictoryCondsTable
    {
        public string CondName;
        public ArrayList parseData()
        {
            ArrayList result = new ArrayList() {
                CondName
            };
            return result;
        }
    }
    [System.Serializable]
    public class MapsTable
    {
        public string MapName,
                      MapFile;
        public int VictoryCond;
        public ArrayList parseData()
        {
            ArrayList result = new ArrayList() {
                MapName,
                MapFile,
                VictoryCond
            };
            return result;
        }
    }
    [System.Serializable]
    public class TilesTable
    {
        public string TileName,
                      TerrainPreset;
        public int TileHinderType,
                   MoveCost,
                   AtkModifier,
                   DefModifier,
                   AccModifier;
        public ArrayList parseData()
        {
            ArrayList result = new ArrayList() {
                TileName,
                TerrainPreset,
                TileHinderType,
                MoveCost,
                AtkModifier,
                DefModifier,
                AccModifier
            };
            return result;
        }
    }
    [System.Serializable]
    public class TilePoolTable
    {
        public int MapID,
                   TileID;
        public ArrayList parseData()
        {
            ArrayList result = new ArrayList() {
                MapID,
                TileID
            };
            return result;
        }
    }
    [System.Serializable]
    public class UnitPoolTable
    {
        public string MapID,
                      UnitID;
        public ArrayList parseData()
        {
            ArrayList result = new ArrayList() {
                MapID,
                UnitID
            };
            return result;
        }
    }
}