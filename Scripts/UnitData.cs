using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitData : ObjectRenderer
{
    //Класс проходимости местности у юнитов
    public enum MovementTypes
    {
        //Пехота
        Infantry = 1,
        //Легкая пехота
        InfantryLight,
        //Передвижная техника
        Vehicle,
        //Летательные аппараты
        FlyingVehicle
    }
    //Перечисление фракций
    public enum Affiliations
    {
        //Юнит игрока
        PlayerUnit,
        //Юнит противника
        EnemyUnit
    }

    //Типы поведений ИИ
    public enum BehaviourTypes
    {
        Aggressive,
        Passive,
        Retreating,
        Guarding
    }

    [Header("Unit Data")]
    [Header("Map Stats")]
    //Принадлежность юнита к фракции
    public Affiliations unitSide;
    //Класс проходимости местности юнита
    public MovementTypes movementType;
    //Класс поведенияпроходимости местности юнита
    public BehaviourTypes behaviour;
    //Дальность перемещения на обычной местности
    public int moveRange,
               //Минимальная дальность атаки
               minAtkRange,
               //Максимальная дальность атаки
               maxAtkRange;
    //Юнит в конечной клетке?
    public bool finishedMovement,
                //Юнит ходил в текущем ходе?
                acted;
    [Header("Battle Stats")]
    //Имя класса вооружения для интерфейса
    public string weaponName;
    //Иконка оружия юнита
    public Sprite weaponImage;
    //Количество полученных атак за ход
    public int attacksReceived = 0,
    //Показатели максимального кол-ва единиц жизни и их остаток 
               health, healthRemaining,
               //Показатель атаки
               attack,
               //Показатель защиты
               defence,
               //Показатель базовой точности
               hitChance,
               //Показатель скорости
               speed;
    
    [Header("Misc Data")]
    //Картинка юнита для интерфейса
    public Sprite charImage;
    //Название юнита для интерфейса
    public string charName,
    //Описание юнита для интерфейса
                  charDesc,
    //Название класса юнита
                  charClassName,
    //Название класса юнита
                  charClassDesc;
    public TextMesh damageTextPrefab,
                    damageNumber;
    //Зона атаки для юнита
    public List<TerrainData> attackArea,
    //Зона перемещения для юнита
                             moveArea,
    //Зона атаки в клетке перемещения юнита 
                             attackAreaMoved;
    //Начальные координаты перед перемещением
    public Vector2Int startingPosition;
    //Позиция юнита над клеткой
    public Vector3 onTilePosition;
    //Вращения модели для правильного отображения в камере
    private Quaternion modelRotationUp,
                       modelRotationRight,
                       modelRotationDown,
                       modelRotationLeft;
    private const float positionOffsetSpeed = 0.5f;

    //Всегда вызывается при создании объекта класса
    public UnitData()
    {
        finishedMovement = false;
        acted = false;
        damageNumber = null;

        onTilePosition = new Vector3(0, 1f, 0);
        modelRotationUp = Quaternion.Euler(30, -90, 0);
        modelRotationRight = Quaternion.Euler(0, 10, 30);
        modelRotationDown = Quaternion.Euler(-30, 90, 0);
        modelRotationLeft = Quaternion.Euler(0, -190, -30);

        attackArea = new List<TerrainData>();
        moveArea = new List<TerrainData>();
        attackAreaMoved = new List<TerrainData>();
    }

    void FixedUpdate()
    {
        if (healthRemaining <= 0 && damageNumber == null)
        {
            DeleteUnit();
        }
        if ((int)this.transform.localPosition.x != (int)onTilePosition.x ||
            (int)this.transform.localPosition.z != (int)onTilePosition.z)
        {
            PositionCorrection();
            finishedMovement = false;
        }
        else
        {
            if (attackAreaMoved.Count <= 0)
            {
                attackAreaMoved = AreaCalculation(true);
                foreach (TerrainData tile in attackAreaMoved)
                {
                    tile.currentState = TerrainData.States.NormalTile;
                }
            }
            finishedMovement = true;
        }

        if (damageNumber != null)
        {
            damageNumber.transform.localPosition = 
                new Vector3(damageNumber.transform.localPosition.x, 
                damageNumber.transform.localPosition.y + 0.2f, 
                damageNumber.transform.localPosition.z);
            if (damageNumber.transform.localPosition.y >= 10f)
                Destroy(damageNumber.gameObject);
        }
    }
    
    //Удаление юнита с поля
    public void DeleteUnit()
    {
        if (GlobalVariables.playerUnits.Contains(this))
            GlobalVariables.playerUnits.Remove(this);
        if (GlobalVariables.enemyUnits.Contains(this))
            GlobalVariables.enemyUnits.Remove(this);
        Destroy(this.gameObject);
    }

    //Последовательное перемещение юнита на положение новой клетки
    private void PositionCorrection()
    {
        Vector3 initialPosition = this.transform.localPosition;
        float offsetX, offsetZ;
        if (this.transform.localPosition.x > positionOffsetSpeed)
            offsetX = -positionOffsetSpeed;
        else if (this.transform.localPosition.x < positionOffsetSpeed)
            offsetX = positionOffsetSpeed;
        else
            offsetX = 0;

        if (this.transform.localPosition.z > positionOffsetSpeed)
            offsetZ = -positionOffsetSpeed;
        else if (this.transform.localPosition.z < positionOffsetSpeed)
            offsetZ = positionOffsetSpeed;
        else
            offsetZ = 0;

        Vector3 updatedPosition = new Vector3(initialPosition.x + offsetX, initialPosition.y, initialPosition.z + offsetZ);
        if (Mathf.Abs(updatedPosition.x) <= positionOffsetSpeed)
            updatedPosition.x = onTilePosition.x;
        if (Mathf.Abs(updatedPosition.z) <= positionOffsetSpeed)
            updatedPosition.z = onTilePosition.z;

        //Вращение модели в соответсвии с самым тяжёлым направлением
        GameObject model = this.transform.GetChild(0).gameObject;

        if (Mathf.Abs(updatedPosition.z) > Mathf.Abs(updatedPosition.x))
        {
            if (updatedPosition.z >= 0)
                model.transform.rotation = modelRotationLeft;
            else
                model.transform.rotation = modelRotationRight;
        }
        else
        {
            if (updatedPosition.x >= 0)
                model.transform.rotation = modelRotationUp;
            else
                model.transform.rotation = modelRotationDown;
        }

        //Применение вычисленного перемещения
        if (Mathf.Abs(updatedPosition.x) < positionOffsetSpeed && Mathf.Abs(updatedPosition.z) < positionOffsetSpeed)
        {
            this.transform.localPosition = onTilePosition;
            model.transform.rotation = modelRotationDown;
        }
        else
            this.transform.localPosition = updatedPosition;
    }

    //Формула перемещения на клетки в зависимости от класса проходимости
    private int MoveFormula(int move, int moveCost)
    {
        if (movementType == MovementTypes.InfantryLight && moveCost > 0)
            move++;
        if (movementType != MovementTypes.FlyingVehicle)
            move -= moveCost;
        move--;
        return move;
    }

    //Цели в зоне атаки
    public List<UnitData> TargetsInRange()
    {
        List<UnitData> targets = new List<UnitData>();
        foreach (TerrainData tile in attackAreaMoved)
        {
            if (CheckTileForEnemies(tile))
                targets.Add(tile.GetComponentInChildren<UnitData>());
        }
        return targets;
    }

    //Показ зоны атаки
    public void ShowAttackArea()
    {
        foreach (TerrainData tile in attackAreaMoved)
        {
            if (tile != null)
                tile.ToggleEmission(Colours.Highlight);
        }
    }

    //Скрытие зоны атаки
    public void HideAttackArea()
    {
        foreach (TerrainData tile in attackAreaMoved)
        {
            if (tile != null)
                tile.ToggleEmission(Colours.None);
        }
    }

    //Отображение зоны поражения
    public void ShowThreatArea()
    {
        if (moveArea.Count <= 0)
            CalculateThreatArea();
        foreach (TerrainData tile in moveArea)
            tile.ToggleEmission(Colours.Moved);
        foreach (TerrainData tile in attackArea)
            tile.ToggleEmission(Colours.Highlight);
    }
    
    //Скрытие зоны поражения
    public void HideThreatArea()
    {
        foreach (TerrainData tile in moveArea)
            tile.ToggleEmission(Colours.None);
        foreach (TerrainData tile in attackArea)
            tile.ToggleEmission(Colours.None);
    }
    
    //Рассчёт зоны поражения
    public void CalculateThreatArea()
    {
        moveArea.Clear();
        attackArea.Clear();
        List<TerrainData> t_calculatedArea = AreaCalculation();
        foreach (TerrainData l_tile in t_calculatedArea)
        {
            if (l_tile.currentState == TerrainData.States.MoveTile)
                moveArea.Add(l_tile);
            else if (l_tile.currentState == TerrainData.States.AttackTile)
                attackArea.Add(l_tile);
            l_tile.currentState = TerrainData.States.NormalTile;
        }
    }
    
    //Функция агрегатор результата рассчёта
    private List<TerrainData> AreaCalculation(bool forceNoMove = false)
    {
        List<TerrainData> area = new List<TerrainData>();
        //Клетка
        this.transform.parent.TryGetComponent(out TerrainData startingTile);
        area.Add(MoveTileAssign(startingTile.assignedCoord));

        if (moveRange > 0 && !forceNoMove)
            area.AddRange(AreaFill(startingTile.assignedCoord, moveRange + maxAtkRange));
        else
            area.AddRange(AreaFill(startingTile.assignedCoord, maxAtkRange));
        return area;
    }
    
    /// <summary>
    /// Рекурсивно заполняет несортированный список клеток для перемещения и атаки
    /// </summary>
    /// <param name="coords">Координаты клетки, откуда пришёл вызов</param>
    /// <param name="remMove">Остатки очков движения</param>
    /// <param name="d">Направление движения рассчёта</param>
    /// <returns>Список клеток </returns>
    private List<TerrainData> AreaFill(Vector2Int coords, int remMove = -1, Directions d = Directions.None)
    {
        List<TerrainData> result = new List<TerrainData>();

        Vector2Int tileUp = new Vector2Int(coords.x, coords.y - 1);
        Vector2Int tileRight = new Vector2Int(coords.x + 1, coords.y);
        Vector2Int tileDown = new Vector2Int(coords.x, coords.y + 1);
        Vector2Int tileLeft = new Vector2Int(coords.x - 1, coords.y);

        if (d != Directions.Down && CheckCoords(tileUp))
            result.AddRange(ProbeDirection(tileUp, remMove, Directions.Up));
        if (d != Directions.Left && CheckCoords(tileRight))
            result.AddRange(ProbeDirection(tileRight, remMove, Directions.Right));
        if (d != Directions.Up && CheckCoords(tileDown))
            result.AddRange(ProbeDirection(tileDown, remMove, Directions.Down));
        if (d != Directions.Right && CheckCoords(tileLeft))
            result.AddRange(ProbeDirection(tileLeft, remMove, Directions.Left));
        return result;
    }
    
    /// <summary>
    /// Ветка для рассчета 
    /// </summary>
    /// <param name="curTileCoord">Текущая клетка для проверки</param>
    /// <param name="remMove">Количество очков движения</param>
    /// <param name="d">Направление для передачи</param>
    /// <returns>Список дочерних клеток</returns>
    private List<TerrainData> ProbeDirection(Vector2Int curTileCoord, int remMove, Directions d)
    {
        TerrainData tile;
        if (remMove - maxAtkRange > 0)
        {
            tile = MoveTileAssign(curTileCoord, remMove - maxAtkRange);
            if (tile.currentState == TerrainData.States.MoveTile)
                remMove = MoveFormula(remMove, tile.moveCost);
            else
                remMove = maxAtkRange;
        }
        else
        {
            tile = AttackTileAssign(curTileCoord, Mathf.Abs(maxAtkRange - remMove) + 1);
            remMove--;
        }
        List<TerrainData> furtherTiles = new List<TerrainData>();
        if (remMove > 0)
        {
            furtherTiles = AreaFill(curTileCoord, remMove, d);
        }
        if (tile != null && !furtherTiles.Contains(tile))
            furtherTiles.Add(tile);
        return furtherTiles;
    }
    
    //Функция определения клетки как доступной к перемещению
    private TerrainData MoveTileAssign(Vector2Int coords, int remMove = -1)
    {
        TerrainData tile = GlobalVariables.playfield[coords.x, coords.y];
        if (coords != startingPosition)
        {
            remMove = MoveFormula(remMove, tile.moveCost);
            if (!CheckTileForEnemies(tile) && tile.CheckForPassage(movementType) && remMove >= 0)
                tile.currentState = TerrainData.States.MoveTile;
            else
                AttackTileAssign(coords);
        }
        else
            tile.currentState = TerrainData.States.MoveTile;
        return tile;
    }
    
    //Функция определения клетки как доступной к атаке
    private TerrainData AttackTileAssign(Vector2Int coords, int curRange = 1)
    {
        TerrainData tile = GlobalVariables.playfield[coords.x, coords.y];
        if (curRange < minAtkRange || curRange > maxAtkRange || tile.currentState != TerrainData.States.NormalTile)
            return null;
        else
        {
            tile.currentState = TerrainData.States.AttackTile;
            return tile;
        }
    }
    
    //Проверка координат на легитимность в рамках текущего поля
    private bool CheckCoords(Vector2Int coords)
    {
        if (coords.x >= 0 && coords.y >= 0)
        {
            if (coords.x < GlobalVariables.playfieldDimentions.x && coords.y < GlobalVariables.playfieldDimentions.y)
                return true;
        }
        return false;
    }

    //Проверка на наличие юнитов противника на клетке
    private bool CheckTileForEnemies(TerrainData tile)
    {
        if (tile.gameObject.transform.childCount > 0)
        {
            Affiliations t_hostileSide = tile.GetComponentInChildren<UnitData>().unitSide;
            if (unitSide != t_hostileSide)
                return true;
        }
        return false;
    }
    
    //Перемещение юнита по клеткам поля
    public bool MoveUnit(TerrainData tile = null)
    {
        if (tile != null && moveArea.Contains(tile) &&
            (tile.transform.childCount == 0 || tile.assignedCoord == startingPosition))
        {
            this.transform.SetParent(tile.transform);
            attackAreaMoved.Clear();
            finishedMovement = false;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Просчитывает урон атаки юнита на цель с учётом местности
    /// </summary>
    /// <param name="target">Цель для атаки</param>
    /// <returns>Вычисленныый урон</returns>
    public int DamageFormula(UnitData target)
    {
        TerrainData unitTile = GlobalVariables.playfield[startingPosition.x, startingPosition.y];
        TerrainData targetTile = GlobalVariables.playfield[target.startingPosition.x, target.startingPosition.y];
        int damage = Mathf.CeilToInt(attack / (1 + target.defence)) - targetTile.bonusDef + unitTile.bonusAtk;
        return damage;
    }

    //Атака юнита
    public void AttackUnit(UnitData enemy, bool noRetaliate = false)
    {
        if (attacksReceived < 2 && InAttackRange(enemy))
        {
            int rnd1 = Random.Range(0, 99);
            int finalHitChance = HitChanceFormula(enemy);
            bool res;
            if (finalHitChance < 50)
                res = HitCheck(rnd1, finalHitChance);
            else
            {
                int rnd2 = Random.Range(0, 99);
                res = HitCheck(rnd1 + rnd2, finalHitChance * 2);
            }
            enemy.damageNumber = Instantiate(damageTextPrefab, enemy.transform);
            if (res)
            {
                enemy.damageNumber.text = DamageFormula(enemy).ToString();
                enemy.healthRemaining -= DamageFormula(enemy);
                if (enemy.healthRemaining <= 0)
                    enemy.damageNumber.color = Color.red;
                enemy.attacksReceived++;
            }
            else
                enemy.damageNumber.text = "MISSED";
        }
        if (enemy.attacksReceived < 2 && !noRetaliate)
            enemy.AttackUnit(this, true);
    }

    public int HitChanceFormula(UnitData enemy)
    {
        this.transform.parent.gameObject.TryGetComponent(out TerrainData tile);
        //TerrainData tile = GlobalVariables.playfield[startingPosition.x, startingPosition.y];
        int finalHitChance = Mathf.CeilToInt((hitChance + tile.bonusAcc) / (1 + enemy.speed));
        return finalHitChance;
    }
    
    //Проверка на попадение
    private bool HitCheck(int rnd, int hitPercent)
    {
        return rnd < hitPercent;
    }

    //Завершение хода для юнита
    public void EndAction()
    {
        attacksReceived = 0;
        this.transform.parent.TryGetComponent(out TerrainData data);
        startingPosition = data.assignedCoord;
        acted = true;
        this.ToggleColourChange(Colours.Moved);
        HideAttackArea();
        CalculateThreatArea();
    }

    //
    //СИСТЕМА ИсИг
    //

    //Действия системы Искуственный Игрок
    public IEnumerator TakeAction()
    {
        List<UnitData> targets;
        TerrainData targetTile;
        switch (behaviour)
        {
            case BehaviourTypes.Aggressive:
                targets = CheckForTargetsInArea();
                UnitData target;
                if (targets.Count == 0)
                    target = FindTarget();
                else
                    target = CalculateOptimalTarget(targets);
                targetTile = CalculateOptimalTile(target);
                MoveUnit(targetTile);
                yield return new WaitUntil(() => finishedMovement == true);
                if (InAttackRange(target))
                    AttackUnit(target);
                break;
            case BehaviourTypes.Passive:
                targets = CheckForTargetsInArea();
                if (targets.Count > 0)
                    goto case BehaviourTypes.Aggressive;
                break;
            case BehaviourTypes.Retreating:
                int x = startingPosition.x;
                int y = startingPosition.y;
                targetTile = GlobalVariables.playfield[x, y];
                foreach (UnitData enemy in GlobalVariables.playerUnits)
                {
                    if (enemy.moveArea.Contains(targetTile) || enemy.attackArea.Contains(targetTile))
                        targetTile = CalculateOptimalTile(enemy);
                }
                MoveUnit(targetTile);
                break;
            case BehaviourTypes.Guarding:
                targets = ShowAttackTargets();
                if (targets.Count > 0)
                    AttackUnit(CalculateOptimalTarget(targets));
                break;
            default:
                break;
        }
        EndAction();
        yield return null;
    }
    
    //Функция нахождения цели
    private UnitData FindTarget()
    {
        UnitData result = CalculateOptimalTarget(GlobalVariables.playerUnits);
        return result;
    }
    
    //Формула приоритета на цель
    private UnitData CalculateOptimalTarget(List<UnitData> targets)
    {
        int currentPriority = -1;
        UnitData target = null;
        foreach (UnitData potentialTarget in targets)
        {
            int calculatedPriority = AttackPriority(potentialTarget);
            if (currentPriority == -1 || calculatedPriority > currentPriority)
            {
                currentPriority = calculatedPriority;
                target = potentialTarget;
            }
        }
        return target;
    }
    
    //Функция нахождения оптимальной клетки
    private TerrainData CalculateOptimalTile(UnitData target)
    {
        int currentPriority = -1;
        TerrainData targetTile = null;
        foreach (TerrainData moveTile in moveArea)
        {
            int calculatedPriority = TilePriority(moveTile, target);
            if (currentPriority == -1 || calculatedPriority > currentPriority)
            {
                currentPriority = calculatedPriority;
                targetTile = moveTile;
            }
        }
        return targetTile;
    }
    
    //Функция определения приоритета клетки
    public int TilePriority(TerrainData tile, UnitData target)
    {
        int priority = GlobalVariables.playfieldDimentions.x * GlobalVariables.playfieldDimentions.y;
        foreach (UnitData unit in GlobalVariables.playerUnits)
        {
            if (behaviour != BehaviourTypes.Retreating)
            {
                if (InAttackRange(target, tile.assignedCoord))
                    priority += 60;
            }
            if (unit.moveArea.Contains(tile) || unit.attackArea.Contains(tile))
            {
                priority += GetRangeOfTiles(unit, tile.assignedCoord);
                if (unit.DamageFormula(this) >= healthRemaining)
                    priority -= 100;
                else
                    priority -= unit.DamageFormula(this);
            }
            if (tile.transform.childCount > 0 && tile.assignedCoord != startingPosition)
            {
                priority = -1;
            }
        }
        return priority;
    }
    
    //Функция приоритета атаки
    public int AttackPriority(UnitData target)
    {
        int priority = GlobalVariables.playfieldDimentions.x * GlobalVariables.playfieldDimentions.y;
        if (DamageFormula(target) >= target.healthRemaining)
            priority += 60;
        else
            priority += (DamageFormula(target) - target.healthRemaining);
        priority -= GetRangeOfTiles(target);
        return priority;
    }

    //Найти противников в текущей зоне поражения
    private List<UnitData> CheckForTargetsInArea()
    {
        List<UnitData> targets = new List<UnitData>();
        foreach (TerrainData tile in moveArea)
        {
            if (CheckTileForEnemies(tile))
                targets.Add(tile.GetComponentInChildren<UnitData>());
        }
        foreach (TerrainData tile in attackArea)
        {
            if (CheckTileForEnemies(tile))
                targets.Add(tile.GetComponentInChildren<UnitData>());
        }
        return targets;
    }

    //Показать цели для атаки
    public List<UnitData> ShowAttackTargets()
    {
        List<UnitData> targets = new List<UnitData>();
        foreach (UnitData unit in GlobalVariables.playerUnits)
        {
            if (InAttackRange(unit))
                targets.Add(unit);
        }
        return targets;
    }

    //Находится ли цель в зоне атаки?
    public bool InAttackRange(UnitData target)
    {
        int res = GetRangeOfTiles(target);
        return res <= maxAtkRange && res >= minAtkRange;
    }
    //Находится ли цель в зоне атаки c указанием координат
    public bool InAttackRange(UnitData target, Vector2Int coords)
    {
        int res = GetRangeOfTiles(target, coords);
        return res <= maxAtkRange && res >= minAtkRange;
    }

    //Расстояние между юнитом и целью
    protected int GetRangeOfTiles(UnitData target)
    {
        TerrainData tile = this.transform.parent.GetComponent<TerrainData>();
        TerrainData targetTile = target.transform.parent.GetComponent<TerrainData>();
        Vector2Int unitCoordsDelta = targetTile.assignedCoord - tile.assignedCoord;
        int tileDelta = Mathf.Abs(unitCoordsDelta.x) + Mathf.Abs(unitCoordsDelta.y);
        return tileDelta;
    }
    
    //Расстояние между юнитом и целью из указанной координаты
    protected int GetRangeOfTiles(UnitData target, Vector2Int startCoords)
    {
        TerrainData targetTile = target.transform.parent.GetComponent<TerrainData>();
        Vector2Int unitCoordsDelta = targetTile.assignedCoord - startCoords;
        int tileDelta = Mathf.Abs(unitCoordsDelta.x) + Mathf.Abs(unitCoordsDelta.y);
        return tileDelta;
    }
}