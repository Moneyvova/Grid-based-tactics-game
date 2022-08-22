using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    private enum MouseButtons
    {
        //Левая Кнопка Мыши
        LeftMB,
        //Правая Кнопка Мыши
        RightMB
    }
    
    private enum MouseTrigger
    {
        //Срабатывает пока нажата кнопка
        Down,
        //Срабатывает один раз за нажание
        Click,
        //Срабатывает когда кнопка отпущена
        Up
    }
    
    //Что попало в луч 
    private enum RayTarget
    {
        None, //Вне игрового поля
        Unit, //Любой юнит
        Tile  //Клетка поля
    }
    
    //Зона в пикселях от краёв экрана для перемещения камеры
    public float moveCameraThreshold = 50f;
    //Ссылка на объект камеры для выполнения действий 
    public CameraController cam;
    //Ссылка на объект интерфейса для выполнения действий
    public TacticalUI usedInterface;
    //Макет экрана для показа постановочных сцен
    public CutsceneController cutscenePrefab;
    
    //Хранилище для Объекта постановочной сцены
    private CutsceneController cutscene;
    //Переключатель для данных об объекте указанном курсором
    private RayTarget typeOfObject;
    //Битмаска для определения целей луча (юниты и клетки)
    private const int layerMask = (1 << 9) | (1 << 10);
    //Данные об указанном юните 
    private UnitData unitHover,
                     //Данные о выбранном юните
                     unitSelected;
    //Данные об указанной клетке
    private TerrainData tileHover;
    private bool inCutscene;
    //Отслеживание аниматоров
    private Animator unitAnimation;
    private Animator unitSelectedAnimation;

    public InputController()
    {
        moveCameraThreshold = 50f;
        typeOfObject = RayTarget.None;
        unitHover = null;
        unitSelected = null;
        tileHover = null;
        inCutscene = false;
    }

    void Start()
    {
        OpenCutscene("intro");
    }

    // Update is called once per frame
    void Update()
    {
        if (!inCutscene)
        {
            MouseHandler();
            KeyboardHandler();
        }
        else
        {
            if (unitSelected != null && unitSelected.finishedMovement)
            {
                inCutscene = false;
                usedInterface.showActionPanel = true;
                usedInterface.unitSelected = unitSelected;
            }
            else if (cutscene == null && unitSelected == null)
                inCutscene = false;
        }
    }

    //Открыть постановочную сцену
    public void OpenCutscene(string newCutscene = "test")
    {
        GlobalVariables.cutsceneName = newCutscene;
        cutscene = Instantiate(cutscenePrefab, this.transform);
        inCutscene = true;
    } 

    //Функция, вызывающая обработку событий мыши
    private void MouseHandler()
    {
        MouseInScrollZone();
        MouseZoomTrigger();
        CursorHover();
        ClickHandler();
    }

    //Функция для обработки ввода с клавиатуры
    private void KeyboardHandler()
    {
        //Определение перемещения камеры по горизонтали
        if (Input.GetKey(GlobalVariables.keymaps["CameraLeft"]))
            cam.moveCamera(Directions.Left);
        else if (Input.GetKey(GlobalVariables.keymaps["CameraRight"]))
            cam.moveCamera(Directions.Right);
        //Определение перемещения камеры по вертикали
        if (Input.GetKey(GlobalVariables.keymaps["CameraUp"]))
            cam.moveCamera(Directions.Up);
        else if (Input.GetKey(GlobalVariables.keymaps["CameraDown"]))
            cam.moveCamera(Directions.Down);

        if (Input.GetKeyUp(GlobalVariables.keymaps["OpenMenu"]))
            usedInterface.ToggleMenuPanel();

        if (Input.GetKeyUp(GlobalVariables.keymaps["EndTurn"]))
            usedInterface.EndTurn();

        if (Input.GetKeyUp(GlobalVariables.keymaps["CancelAction"]))
            usedInterface.UndoHandler();

    }

    //Обработка перемещения камеры от положения курсора на экране
    private void MouseInScrollZone()
    {
        Vector3 mouseCoords = Input.mousePosition;
        if (PointerInApp(mouseCoords))
        {
            KeyValuePair<Directions, float> cameraOffset = BoundaryCheck(mouseCoords.x, Screen.width, Directions.Left, Directions.Right);
            if (cameraOffset.Key != Directions.None)
                cam.moveCamera(cameraOffset.Key, cameraOffset.Value);

            cameraOffset = BoundaryCheck(mouseCoords.y, Screen.height, Directions.Down, Directions.Up);
            if (cameraOffset.Key != Directions.None)
                cam.moveCamera(cameraOffset.Key, cameraOffset.Value);
        }
    }

    //Определение соотношения скорости перемещения камеры от положения курсора
    private KeyValuePair<Directions, float> BoundaryCheck(float coord, int boundary, Directions lowerDirection, Directions upperDirection)
    {
        KeyValuePair<Directions, float> result = new KeyValuePair<Directions, float>(Directions.None, 0f);
        if (coord <= moveCameraThreshold)
        {
            float modifier = (moveCameraThreshold - coord) / moveCameraThreshold;
            result = new KeyValuePair<Directions, float>(lowerDirection, modifier);
        }
        //Курсор в правой части экрана?
        else if (boundary - coord <= moveCameraThreshold)
        {
            float modifier = (moveCameraThreshold + coord - boundary) / moveCameraThreshold;
            result = new KeyValuePair<Directions, float>(upperDirection, modifier);
        }
        return result;
    }

    //Обработка данных с колёсика мыши
    private void MouseZoomTrigger()
    {
        if (Input.mouseScrollDelta.y < 0)
            cam.zoomTarget += 1f;
        if (Input.mouseScrollDelta.y > 0)
            cam.zoomTarget -= 1f;
    }

    //Проверка на наличие курсора в приложении
    private bool PointerInApp(Vector3 mouseCoords)
    {
        if (mouseCoords.x >= 0 && mouseCoords.x <= Screen.width &&
            mouseCoords.y >= 0 && mouseCoords.y <= Screen.height)
            return true;
        else
            return false;
    }
    
    //Определение типа объекта, на который указывает курсор
    private void CursorHover()
    {
        Ray ray = cam.rayToCursor();
        ClearHover();
        if (Physics.Raycast(ray, out RaycastHit hit, 100, layerMask))
        {
            GameObject hitObject = hit.collider.gameObject;
            switch (hitObject.layer)
            {
                //Слой юнитов
                case 9:
                    typeOfObject = RayTarget.Unit;
                    break;
                //Слой Клеток
                case 10:
                    typeOfObject = RayTarget.Tile;
                    break;
                //По-умолчанию
                default:
                    typeOfObject = RayTarget.None;
                    break;
            }

            if (typeOfObject != RayTarget.None)
                SetHoverObject(hitObject);
        }
    }

    //Выдение объекта под курсором
    private void SetHoverObject(GameObject obj)
    {
        if (typeOfObject == RayTarget.Unit)
        {
            obj.TryGetComponent(out UnitData unitData);
            unitHover = unitData;
            //Подсветка юнита
            if (unitHover.activeColour == Colours.None)
                unitHover.ToggleColourChange(Colours.Hover);
            else
                unitHover.ToggleColourChange(unitHover.activeColour, Colours.Hover);
            //Показать окно данных юнита
            usedInterface.showUnitData = true;
            usedInterface.unitHit = unitHover;

            unitHover.transform.parent.TryGetComponent(out TerrainData unitHoverTile);
            tileHover = unitHoverTile;

            //Показать анимацию наведения
            unitAnimation = unitHover.GetComponentInChildren(typeof(Animator)) as Animator;
            unitAnimation.SetBool("Hover", true);
        }
        if (typeOfObject != RayTarget.None)
        {
            if (tileHover == null)
            {
                obj.TryGetComponent(out TerrainData terrainData);
                tileHover = terrainData;
            }

            //Подсветка клетки
            if (tileHover.activeColour == Colours.None)
                tileHover.ToggleEmission(Colours.Hover);
            else
                tileHover.ToggleEmission(tileHover.activeColour, Colours.Hover);
            //Подсветка данных клетки
            usedInterface.showTileData = true;
            usedInterface.tileHit = tileHover;
        }
    }

    //Очистка данных об объекте
    private void ClearHover()
    {
        typeOfObject = RayTarget.None;
        //Если есть данные о наведении на юнита
        if (unitHover != null)
        {
            //Сброс цвета
            if (unitHover.activeColour == Colours.Hover)
                unitHover.ToggleColourChange(Colours.None);
            else
                unitHover.ToggleColourChange(unitHover.activeColour);
            //Убрать окно данных
            unitHover = null;
            usedInterface.showUnitData = false;
        }
        //Если есть данные о клетке
        if (tileHover != null)
        {
            //Убрать подсветку
            if (tileHover.activeColour == Colours.Hover)
                tileHover.ToggleEmission();
            else
                tileHover.ToggleEmission(tileHover.activeColour);
            
            tileHover = null;
            //Убрать показ данных
            usedInterface.showTileData = false;
        }
        //Убрать произведение анимации
        if (unitAnimation!=null)
            unitAnimation.SetBool("Hover", false);
    }

    //Обработка нажатий кнопок мыши
    private void ClickHandler()
    {
        if (!usedInterface.showOutcome)
        {
            if (MouseTriggerCheck(MouseTrigger.Up, MouseButtons.LeftMB))
            {
                if (unitSelected == null)
                {
                    if (unitHover != null && !unitHover.acted &&
                        unitHover.unitSide == UnitData.Affiliations.PlayerUnit)
                    {
                        FocusUnit();
                    }
                }
                else
                {
                    if (unitHover == null || unitSelected == unitHover)
                    {
                        UnfocusUnit();
                    }
                    else if (unitHover != null && !unitHover.acted &&
                        unitHover.unitSide == UnitData.Affiliations.PlayerUnit)
                    {
                        UnfocusUnit();
                        FocusUnit();
                    }
                }
            }
            if (MouseTriggerCheck(MouseTrigger.Up, MouseButtons.RightMB))
            {
                if (unitSelected != null && tileHover != null && unitHover == null)
                {
                    inCutscene = unitSelected.MoveUnit(tileHover);
                    unitSelected.HideThreatArea();
                }
            }
        }
        else
        {
            if (MouseTriggerCheck(MouseTrigger.Up, MouseButtons.RightMB) && unitSelected != null)
            {
                List<UnitData> targets = unitSelected.TargetsInRange();
                if (targets.Contains(unitHover))
                {
                    unitSelected.AttackUnit(unitHover);
                    usedInterface.showOutcome = false;
                    usedInterface.unitSelected = null;
                    unitSelected.EndAction();
                    UnfocusUnit();
                }
            }
        }
    }

    //Проверка на нажатие кнопок мыши
    private bool MouseTriggerCheck(MouseTrigger trigger, MouseButtons clickedBtn)
    {
        int code = (int)clickedBtn;
        switch (trigger)
        {
            case MouseTrigger.Click:
                return Input.GetMouseButton(code);
            case MouseTrigger.Down:
                return Input.GetMouseButtonDown(code);
            case MouseTrigger.Up:
                return Input.GetMouseButtonUp(code);
        }
        return false;
    }

    //Выделение юнита после нажатия
    private void FocusUnit()
    {
        if (unitHover != null && unitHover.unitSide == UnitData.Affiliations.PlayerUnit)
        {
            unitSelected = unitHover;
            unitSelected.ToggleColourChange(Colours.Select);
            unitSelectedAnimation = unitAnimation;
            unitAnimation = null;
            unitSelectedAnimation.SetBool("Hover", false);
            unitSelectedAnimation.SetBool("Moving", true);

            unitSelected.ShowThreatArea();
        }
    }
    //Снятие выделения юнита после нажатия
    private void UnfocusUnit()
    {
        if (unitSelected.activeColour!= Colours.Moved)
            unitSelected.ToggleColourChange(Colours.None);
        unitSelected.HideThreatArea();
        unitSelectedAnimation.SetBool("Moving", false);
        if (unitSelected == unitHover)
            unitSelectedAnimation.SetBool("Hover", true);
        unitSelectedAnimation = null;
        unitSelected = null;
    }
}