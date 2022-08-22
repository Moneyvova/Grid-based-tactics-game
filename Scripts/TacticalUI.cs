using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TacticalUI : UIRenderer
{
    [Header("UnitDataPanel Variables")]
    //Панель данных юнита
    public RectTransform unitDataPanel;
    //Запрос на отображение панели данных юнита
    public bool showUnitData;
    //Данные для отображения на панели
    public UnitData unitHit;

    public Text UnitDataNameBox;
    public Image UnitDataPortrait,
                 UnitDataWeapon,
                 UnitDataMoveType,
                 UnitDataMinRange,
                 UnitDataMaxRange;
    public Slider UnitDataHealth,
                  UnitDataAttack,
                  UnitDataDefence,
                  UnitDataSpeed;
    //Значение координат видимой панели
    private Vector3 unitDataPanelPos_s;
    //Значение координат невидимой панели
    private Vector3 unitDataPanelPos_h;
    
    [Header("TileDataPanel Variables")]
    //Панель данных клетки
    public RectTransform tileDataPanel;
    //Запрос на отображение панели данных юнита
    public bool showTileData;
    //Данные для отображения на панели
    public TerrainData tileHit;
    //Значение координат видимой панели
    private Vector3 tileDataPanelPos_s;
    //Значение координат невидимой панели
    private Vector3 tileDataPanelPos_h;

    [Header("ActionPanel variables")]
    //Панель данных клетки на сцене
    public RectTransform actionPanel;
    //Запрос на отображение панели данных юнита
    public bool showActionPanel;
    //Кнопка отмены действий
    public Button actionUndoButton,
        //Команда ожидать юниту 
                  waitButton,
        //Команда атаковать юниту
                  attackButton;
    //Выбранный юнит
    public UnitData unitSelected;
    //Значение координат видимой панели
    private Vector3 actionPanelPos_s;
    //Значение координат невидимой панели
    private Vector3 actionPanelPos_h;
    
    [Header("MenuPanel items")]
    public RectTransform menuPanel;
    public Button menuButton,
                  endTurnButton,
                  saveButton,
                  loadButton,
                  settingsButton,
                  exitButton;
    public GameObject settingsWindow,
                      saveWindow;
    
    [Header("TurnAnnouncement panel")]
    public Image turnAnnouncement;
    public Text announcementText,
                announcementShadow;
    public bool showAnnouncement;

    [Header("Outcome panel")]
    public RectTransform outcomePanel;
    public bool showOutcome,
                outcomeWindowVisible;
    //Ссылка на класс игрового процесса
    public PlayfieldController field;
    //Звуки UI
    public AudioClip slideSounds,
                     finishSound;

    //Объект для удержания панелей
    private Dictionary<RectTransform, int> heldPanels;
    //Время удержания панелей
    private const int holdTime = 60;

    private UnitData.Affiliations turnHolder = UnitData.Affiliations.PlayerUnit;

    public TacticalUI()
    {
        heldPanels = new Dictionary<RectTransform, int>();
        
        showUnitData = false;
        unitHit = null;
        showTileData = false;
        tileHit = null;
        showActionPanel = false;
        unitSelected = null;

        showOutcome = false;
        outcomeWindowVisible = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        PrepareUI();
    }

    // FixedUpdate is called before Update within fixed intervals (default is 50 fps)
    void FixedUpdate()
    {
        OutcomePanelHandler();
        UnitPanelHandler();
        TilePanelHandler();
        ActionPanelHandler();
        AnnouncementHandler();
    }

    //Преднастройка интерфейса
    protected void PrepareUI()
    {
        float mod = (Screen.width / 1280f);
        Debug.Log(mod);
        unitDataPanelPos_s = unitDataPanel.transform.position;
        unitDataPanelPos_h = new Vector3(-unitDataPanel.sizeDelta.x*mod,
                                         unitDataPanel.transform.position.y,
                                         unitDataPanel.transform.position.z);
        SlideUIObject(unitDataPanel, unitDataPanelPos_h);

        tileDataPanelPos_s = tileDataPanel.transform.position;
        tileDataPanelPos_h = new Vector3(-tileDataPanel.sizeDelta.x * mod,
                                         tileDataPanel.transform.position.y,
                                         tileDataPanel.transform.position.z);
        SlideUIObject(tileDataPanel, tileDataPanelPos_h);

        actionPanelPos_s = actionPanel.transform.position;
        actionPanelPos_h = new Vector3(actionPanel.transform.position.x + actionPanel.sizeDelta.x * mod,
                                       actionPanel.transform.position.y,
                                       actionPanel.transform.position.z);
        SlideUIObject(actionPanel, actionPanelPos_h);
        actionUndoButton.onClick.AddListener(UndoMovement);
        waitButton.onClick.AddListener(WaitOnPosition);
        attackButton.onClick.AddListener(ShowTargets);
        actionUndoButton.gameObject.SetActive(false);
        //Установка обработчика и локализированного обозначения у кнопки меню
        menuButton.onClick.AddListener(ToggleMenuPanel);
        menuButton.GetComponentInChildren<Text>().text =
            GlobalVariables.localization.gameplayField.menu.name;


        //Установка обработчика и локализированного обозначения у кнопки сохранения
        endTurnButton.onClick.AddListener(EndTurn);
        endTurnButton.GetComponentInChildren<Text>().text =
            GlobalVariables.localization.gameplayField.menu.btns[0].MatchValue(endTurnButton.name);

        //Установка обработчика и локализированного обозначения у кнопки сохранения
        saveButton.onClick.AddListener(OpenSaveMenu);
        saveButton.GetComponentInChildren<Text>().text =
            GlobalVariables.localization.gameplayField.menu.btns[1].MatchValue(saveButton.name);

        //Установка обработчика и локализированного обозначения у кнопки загрузки
        loadButton.onClick.AddListener(OpenLoadMenu);
        loadButton.GetComponentInChildren<Text>().text =
            GlobalVariables.localization.gameplayField.menu.btns[2].MatchValue(loadButton.name);

        //Установка обработчика и локализированного обозначения у кнопки настройки
        settingsButton.onClick.AddListener(OpenSettingsMenu);
        settingsButton.GetComponentInChildren<Text>().text =
            GlobalVariables.localization.gameplayField.menu.btns[3].MatchValue(settingsButton.name);

        //Установка обработчика и локализированного обозначения у кнопки меню
        exitButton.onClick.AddListener(GoToMainMenu);
        exitButton.GetComponentInChildren<Text>().text =
            GlobalVariables.localization.gameplayField.menu.btns[4].MatchValue(exitButton.name);

        ToggleMenuPanel();
    }

    //Управление функций связанных с панелью данных юнита
    protected void UnitPanelHandler()
    {
        if (unitHit != null)
            UnitDataUpdate();
        UnitPanelToggle();
    }
    
    //Обновление данных панели юнита
    private void UnitDataUpdate()
    {
        UnitDataPortrait.sprite = unitHit.charImage;
        UnitDataWeapon.sprite = unitHit.weaponImage;
        UnitDataMinRange.sprite = Resources.Load<Sprite>("ImagesUI/number_" + unitHit.minAtkRange);
        UnitDataMaxRange.sprite = Resources.Load<Sprite>("ImagesUI/number_" + unitHit.maxAtkRange);
        UnitDataMoveType.sprite = Resources.Load<Sprite>("ImagesUI/moveType" + (int)unitHit.movementType);

        UnitDataNameBox.text = unitHit.charName;
        UnitDataHealth.maxValue = unitHit.health;
        UnitDataHealth.value = unitHit.healthRemaining;
        UnitDataAttack.value = unitHit.attack;
        UnitDataDefence.value = unitHit.defence;
        UnitDataSpeed.value = unitHit.speed;

        unitHit = null;
    }
    
    //Видимость панели юнита
    private void UnitPanelToggle()
    {
        var sounds = unitDataPanel.GetComponentInChildren<AudioSource>();
        bool result;
        if (showUnitData || (heldPanels.ContainsKey(unitDataPanel) && heldPanels[unitDataPanel] < holdTime))
        {
            result = SlideUIObject(unitDataPanel, unitDataPanelPos_s, Directions.Right);

            //Удержание панели на экране
            if (!heldPanels.ContainsKey(unitDataPanel))
                heldPanels.Add(unitDataPanel, 0);
            else if (showUnitData)
                heldPanels[unitDataPanel] = 0;
            else
                heldPanels[unitDataPanel]++;
        }
        else
        {
            result = SlideUIObject(unitDataPanel, unitDataPanelPos_h, Directions.Left);
            if (heldPanels.ContainsKey(unitDataPanel))
                heldPanels.Remove(unitDataPanel);
        }

        if (result && sounds.clip != finishSound)
        {
            sounds.clip = finishSound;
            sounds.Play();
        }
        else if (!result && sounds.clip != slideSounds)
        {
            sounds.clip = slideSounds;
            sounds.Play();
        }
    }
    
    //Управление функций связанных с панелью данных клетки
    protected void TilePanelHandler()
    {
        if (tileHit != null)
            TileDataUpdate();
        TilePanelToggle();
    }
    
    //Видимость панели юнита
    private void TileDataUpdate()
    {
        ColorCodeIcon("AtkImg", tileHit.bonusAtk);
        ColorCodeIcon("DefImg", tileHit.bonusDef);
        ColorCodeIcon("AccImg", tileHit.bonusAcc);
        GameObject obj = GameObject.Find("TileName");
        obj.TryGetComponent(out Text TileText);
        TileText.text = tileHit.tileName;
    }
    
    //Цветовое обозначение иконок
    private void ColorCodeIcon(string iconName, int compareValue)
    {
        GameObject obj = GameObject.Find(iconName);
        obj.TryGetComponent(out Image icon);
        if (compareValue > 0)
            icon.color = Color.green;
        else if (compareValue < 0)
            icon.color = Color.red;
        else
            icon.color = Color.white;
    }
    
    //Видимость панели клетки
    private void TilePanelToggle()
    {
        var sounds = tileDataPanel.GetComponentInChildren<AudioSource>();
        bool result;
        if (showTileData || (heldPanels.ContainsKey(tileDataPanel) && heldPanels[tileDataPanel] < holdTime))
        {
            result = SlideUIObject(tileDataPanel, tileDataPanelPos_s, Directions.Right);
            //Удержание панели на экране
            if (!heldPanels.ContainsKey(tileDataPanel))
                heldPanels.Add(tileDataPanel, 0);
            else if (showTileData)
                heldPanels[tileDataPanel] = 0;
            else
                heldPanels[tileDataPanel]++;
        }
        else
        {
            result = SlideUIObject(tileDataPanel, tileDataPanelPos_h, Directions.Left);
            if (heldPanels.ContainsKey(tileDataPanel))
                heldPanels.Remove(tileDataPanel);
        }

        if (result && sounds.clip != finishSound)
        {
            sounds.clip = finishSound;
            sounds.Play();
        }
        else if (!result && sounds.clip != slideSounds)
        {
            sounds.clip = slideSounds;
            sounds.Play();
        }
    }

    //Обработка действий с панелью 
    protected void ActionPanelHandler()
    {
        if (showActionPanel)
        {
            SlideUIObject(actionPanel, actionPanelPos_s, Directions.Left);
            actionUndoButton.gameObject.SetActive(true);
            if (unitSelected != null)
            {
                unitSelected.ShowAttackArea();
                List<UnitData> targets = unitSelected.TargetsInRange();
                if (targets.Count > 0)
                    attackButton.interactable = true;
                else
                    attackButton.interactable = false;
            }
        }
        else
        {
            SlideUIObject(actionPanel, actionPanelPos_h, Directions.Right);
            if (!showOutcome)
                actionUndoButton.gameObject.SetActive(false);
        }
    }
    
    public void UndoHandler()
    {
        if (showOutcome)
            UndoTargets();
        else if (showActionPanel)
            UndoMovement();
    }

    //Откат перемещения юнита игрока
    private void UndoMovement()
    {
        TerrainData tile = GlobalVariables.playfield[unitSelected.startingPosition.x, unitSelected.startingPosition.y];
        unitSelected.transform.SetParent(tile.transform);
        unitSelected.transform.localPosition = unitSelected.onTilePosition;
        if (unitSelected != null)
            unitSelected.HideAttackArea();
        showActionPanel = false;
    }

    //Показать цели 
    private void ShowTargets()
    {
        showOutcome = true;
        showActionPanel = false;
        actionUndoButton.onClick.RemoveListener(UndoMovement);
        actionUndoButton.onClick.AddListener(UndoTargets);
    }

    //Откат на панель действий
    private void UndoTargets()
    {
        showOutcome = false;
        showActionPanel = true;
        actionUndoButton.onClick.RemoveListener(UndoTargets);
        actionUndoButton.onClick.AddListener(UndoMovement);
    }

    //Завершение перемещения юнита
    private void WaitOnPosition()
    {
        showActionPanel = false;
        unitSelected.EndAction();
    }

    //Обработка панели резульатата боя
    private void OutcomePanelHandler()
    {
        if (unitHit != null && unitSelected != null)
        {
            OutcomePanelToggle();
            if (outcomeWindowVisible)
            {
                outcomePanel.position = Input.mousePosition;

                GameObject obj = GameObject.Find("LabelAccuracy");
                obj.TryGetComponent(out Text textLabel);
                textLabel.text = GlobalVariables.localization.gameplayField.combatOutcome.labelAccuracy;


                obj = GameObject.Find("Accuracy");
                textLabel = obj.GetComponent<Text>();
                int hitChance = unitSelected.HitChanceFormula(unitHit);
                
                if (hitChance < 20)
                    textLabel.text = GlobalVariables.localization.gameplayField.combatOutcome.accuracyMin;
                else if (hitChance >= 20 && hitChance < 75)
                    textLabel.text = GlobalVariables.localization.gameplayField.combatOutcome.accuracyAvg;
                else if (hitChance >= 75 && hitChance < 100)
                    textLabel.text = GlobalVariables.localization.gameplayField.combatOutcome.accuracyGood;
                else
                    textLabel.text = GlobalVariables.localization.gameplayField.combatOutcome.accuracyMax;

                obj = GameObject.Find("LabelDamage");
                textLabel = obj.GetComponent<Text>();
                textLabel.text = GlobalVariables.localization.gameplayField.combatOutcome.labelDamage;

                obj = GameObject.Find("Damage");
                textLabel = obj.GetComponent<Text>();
                int result = unitHit.healthRemaining - unitSelected.DamageFormula(unitHit);

                float percent = ((float)(unitHit.healthRemaining - result) / unitHit.healthRemaining);

                if (result <= 0)
                    textLabel.text = GlobalVariables.localization.gameplayField.combatOutcome.damageMax;
                else if (percent > 0.7f)
                    textLabel.text = GlobalVariables.localization.gameplayField.combatOutcome.damageGood;
                else if (percent <= 0.7f && percent > 0.3f)
                    textLabel.text = GlobalVariables.localization.gameplayField.combatOutcome.damageAvg;
                else
                    textLabel.text = GlobalVariables.localization.gameplayField.combatOutcome.damageMin;

                obj = GameObject.Find("LabelRetaliate");
                textLabel = obj.GetComponent<Text>();
                textLabel.text = GlobalVariables.localization.gameplayField.combatOutcome.labelRetaliate;

                obj = GameObject.Find("Retaliate");
                textLabel = obj.GetComponent<Text>();

                if (unitHit.attacksReceived < 1)
                    textLabel.text = GlobalVariables.localization.gameplayField.combatOutcome.retaliateYes;
                else
                    textLabel.text = GlobalVariables.localization.gameplayField.combatOutcome.retaliateNo;
            }
        }
        else
        {
            outcomePanel.gameObject.SetActive(false);
            outcomeWindowVisible = false;
        }
    }

    //Выидмость панели результата боя
    private void OutcomePanelToggle()
    {
        if (unitSelected.TargetsInRange().Contains(unitHit))
        {
            outcomePanel.gameObject.SetActive(true);
            outcomeWindowVisible = true;
        }
        else
        {
            outcomePanel.gameObject.SetActive(false);
            outcomeWindowVisible = false;
        }
    }

    //Завершение хода
    public void EndTurn()
    {
        if (turnHolder == UnitData.Affiliations.PlayerUnit)
        {
            turnHolder = UnitData.Affiliations.EnemyUnit;
            StartCoroutine(field.RefreshUnits(GlobalVariables.playerUnits));
        }
        else
        {
            turnHolder = UnitData.Affiliations.PlayerUnit;
            StartCoroutine(field.RefreshUnits(GlobalVariables.enemyUnits));
        }
        showAnnouncement = true;
    }

    //Функция обработки панели объявления хода
    private void AnnouncementHandler()
    {
        if (showAnnouncement)
        {
            if (heldPanels.ContainsKey(turnAnnouncement.rectTransform))
            {
                heldPanels[turnAnnouncement.rectTransform]++;
                if (heldPanels[turnAnnouncement.rectTransform] >= holdTime)
                {
                    showAnnouncement = false;
                    heldPanels.Remove(turnAnnouncement.rectTransform);
                    turnAnnouncement.gameObject.SetActive(false);
                    if (turnHolder == UnitData.Affiliations.EnemyUnit)
                        StartCoroutine(field.EnemyAction());
                }
            }
            else
            {
                UpdateAnnouncement();
                turnAnnouncement.gameObject.SetActive(true);
            }
        }
    }

    //Изменение данных панели объявления хода
    private void UpdateAnnouncement()
    {
        switch (turnHolder)
        {
            case UnitData.Affiliations.PlayerUnit:
                announcementText.text = GlobalVariables.localization.gameplayField.turn.player;
                announcementShadow.text = announcementText.text;
                turnAnnouncement.color = new Color32(50, 120, 200, 100);
                break;
            case UnitData.Affiliations.EnemyUnit:
                announcementText.text = GlobalVariables.localization.gameplayField.turn.enemy;
                announcementShadow.text = announcementText.text;
                turnAnnouncement.color = new Color32(115, 25, 20, 100);
                break;
        }
        heldPanels.Add(turnAnnouncement.rectTransform, 0);
    }

    //Вызов панели меню
    public void ToggleMenuPanel()
    {
        GameObject menuPanel = saveButton.transform.parent.gameObject;
        menuPanel.SetActive(!menuPanel.activeSelf);
    }

    //Вызов окна сохранения
    private void OpenSaveMenu()
    {
        GameObject window = Instantiate(saveWindow, this.transform);
        window.TryGetComponent(out CheckpointController checkpoint);
        checkpoint.operationMode = true;
    }

    //Вызов окна загрузки
    private void OpenLoadMenu()
    {
        Instantiate(saveWindow, this.transform);
    }

    //Вызов окна настроек
    private void OpenSettingsMenu()
    {
        Instantiate(settingsWindow, this.transform);
    }

    //Возврат в главное меню
    private void GoToMainMenu()
    {
        AsyncOperation loadingOperation = SceneManager.LoadSceneAsync("MainMenu");
    }
}