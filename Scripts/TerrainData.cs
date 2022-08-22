using UnityEngine;

public class TerrainData : ObjectRenderer
{
    //Перечисление меток для подсветки
    public enum States
    {
        NormalTile,
        MoveTile,
        AttackTile
    }
    //Метки класса клетки
    public enum TileType
    {
        Generic = 0,
        BanFootmen = 1,
        BanGroundVehicles = 2,
        BanAirVehicles = 4,
        BanGroundUnits = BanFootmen | BanGroundVehicles,
        BanVehicles = BanGroundVehicles | BanAirVehicles,
        Impassable = BanFootmen | BanGroundVehicles | BanAirVehicles
    }

    [Header("Terrain data")]
    public string tileName;
    //Метка для вычисления зон перемещения/атаки юнитов
    public States currentState = States.NormalTile;
    //Быстрая метка для ограничений
    public TileType hinderClass = TileType.Generic;
    //Дополнительный вес клетки
    public int moveCost = 0,
        //Бонус к атаке юнита на клетке
               bonusAtk = 0,
        //Бонус к точности юнита на клетке
               bonusAcc = 0,
        //Бонус к защите юнита на клетке
               bonusDef = 0;
    //Координата клетки
    public Vector2Int assignedCoord;
    

    //Возвращает разрешение на перемещение юнита на клетку
    public bool CheckForPassage(UnitData.MovementTypes type)
    {
        if (hinderClass != TileType.Impassable)
        {
            switch (type)
            {
                case UnitData.MovementTypes.Infantry:
                case UnitData.MovementTypes.InfantryLight:
                    if (hinderClass.HasFlag(TileType.BanFootmen))
                        return false;
                    else
                        goto default;
                case UnitData.MovementTypes.Vehicle:
                    if (hinderClass.HasFlag(TileType.BanGroundVehicles))
                        return false;
                    else
                        goto default;
                case UnitData.MovementTypes.FlyingVehicle:
                    if (hinderClass.HasFlag(TileType.BanAirVehicles))
                        return false;
                    else
                        goto default;
                default:
                    return true;
            }
        }
        else
            return false;
    }
}