using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    //Кнопка "Управление"
    public Button controlsToggle,
        //Кнопка "Звук"
                  audioToggle,
        //Кнопка "Графика"
                  graphicsToggle,
        //Кнопка "Игровой процесс"
                  gameplayToggle,
        //Кнопка закрытия окна
                  closeSettings;
    //Макет объекта списка
    public GameObject settingItemPrefab,
        //Макет выпадающего меню выбора языка
                      languagePrefab;
    public RectTransform scrollViewport;
    //Текущий список позиций
    private List<GameObject> currentItemList = new List<GameObject>();
    //Текущее кач-во графики
    private int currentQuality;
    
    // Start is called before the first frame update
    void Start()
    {
        controlsToggle.onClick.AddListener(ControlsOptions);
        controlsToggle.GetComponentInChildren<Text>().text = 
            GlobalVariables.localization.settings.panelBtns[0].MatchValue(controlsToggle.name);
        
        audioToggle.onClick.AddListener(AudioOptions);
        audioToggle.GetComponentInChildren<Text>().text =
            GlobalVariables.localization.settings.panelBtns[1].MatchValue(audioToggle.name);
        
        graphicsToggle.onClick.AddListener(GraphicsOptions);
        graphicsToggle.GetComponentInChildren<Text>().text =
            GlobalVariables.localization.settings.panelBtns[2].MatchValue(graphicsToggle.name);
        
        gameplayToggle.onClick.AddListener(GameplayOptions);
        gameplayToggle.GetComponentInChildren<Text>().text =
            GlobalVariables.localization.settings.panelBtns[3].MatchValue(gameplayToggle.name);

        ControlsOptions();
        closeSettings.onClick.AddListener(CloseWindow);

        if (PlayerPrefs.HasKey("Quality"))
            currentQuality = PlayerPrefs.GetInt("Quality");
        else
            currentQuality = QualitySettings.GetQualityLevel();
    }

    //Загрузка настроек управления
    private void ControlsOptions()
    {
        controlsToggle.interactable = false;
        audioToggle.interactable = true;
        graphicsToggle.interactable = true;
        gameplayToggle.interactable = true;
        FlushOptions();

        int i = 0;
        foreach (KeyValuePair<string, string> keymap in GlobalVariables.keymaps)
        {
            MakeItem(keymap.Key, keymap.Value, GlobalVariables.localization.settings.controls,i);
            i++;
        }
    }

    //Загрузка настроек графики
    private void GraphicsOptions()
    {
        controlsToggle.interactable = true;
        audioToggle.interactable = true;
        graphicsToggle.interactable = false;
        gameplayToggle.interactable = true;
        FlushOptions();
        GameObject item = MakeItem(
            "unity_quality", 
            GlobalVariables.localization.settings.gfxPresets[currentQuality],
            GlobalVariables.localization.settings.gfxBtns);
        Button btn = item.GetComponentInChildren<Button>();
        btn.onClick.AddListener(ChangeQuality);
    }

    //Переключение настроек графики
    private void ChangeQuality()
    {
        Button btn = currentItemList[0].GetComponentInChildren<Button>();
        Text txt = btn.GetComponentInChildren<Text>();
        if (currentQuality <= 5)
            currentQuality++;
        else
            currentQuality = 0;
        txt.text = GlobalVariables.localization.settings.gfxPresets[currentQuality];
        PlayerPrefs.SetInt("Quality",currentQuality);
        QualitySettings.SetQualityLevel(currentQuality);
    }

    //Загрузка настроек звука
    private void AudioOptions()
    {
        controlsToggle.interactable = true;
        audioToggle.interactable = false;
        graphicsToggle.interactable = true;
        gameplayToggle.interactable = true;
        FlushOptions();
        MakeItem("unity_sound", "unity_make_sound", GlobalVariables.localization.settings.audioBtns);
    }

    //Загрузка настроек игрового процесса
    private void GameplayOptions()
    {
        controlsToggle.interactable = true;
        audioToggle.interactable = true;
        graphicsToggle.interactable = true;
        gameplayToggle.interactable = false;
        FlushOptions();

        GameObject item = Instantiate(languagePrefab, scrollViewport);
        item.transform.localPosition = new Vector3(item.transform.localPosition.x, -10f, item.transform.localPosition.z);
        currentItemList.Add(item);
        
        Dropdown langDropdown = item.GetComponentInChildren<Dropdown>();
        langDropdown.ClearOptions();
        Text label = item.GetComponentInChildren<Text>();
        foreach (UIButtons button in GlobalVariables.localization.settings.gpBtns)
        {
            string result = button.MatchValue(label.text);
            if (result != label.text)
                label.text = result;
        }

        foreach (string lang in GlobalVariables.locales)
        {
            string locName = new LocalizationController(lang).localizationName;
            langDropdown.options.Add(new Dropdown.OptionData(locName));

            if (lang == GlobalVariables.localeName)
                langDropdown.value = langDropdown.options.Count - 1;
        }

        langDropdown.onValueChanged.AddListener(delegate
        {
            GlobalVariables.localization.LoadLocalizationFile(GlobalVariables.locales[langDropdown.value]);
            GlobalVariables.localeName = GlobalVariables.locales[langDropdown.value];
        });

        MakeItem("unity_borderscroll", "unity_yes", GlobalVariables.localization.settings.gpBtns, 1);
        MakeItem("unity_highlight", "unity_no", GlobalVariables.localization.settings.gpBtns, 2);
    }

    //Добавить опцию в меню
    private GameObject MakeItem(string labelText, string btnText, List<UIButtons> labelList, float offset = 0)
    {
        GameObject item = Instantiate(settingItemPrefab, scrollViewport);
        item.transform.localPosition = new Vector3(item.transform.localPosition.x, -20f - 50f*offset, item.transform.localPosition.z);
        currentItemList.Add(item);
        Text label = item.GetComponentInChildren<Text>();
        Button btn = item.GetComponentInChildren<Button>();
        Text btnName = btn.GetComponentInChildren<Text>();
        
        foreach (UIButtons button in labelList)
        {
            string result = button.MatchValue(labelText);
            if (result != labelText)
            {
                label.text = result;
                break;
            }
            else
                label.text = labelText;
        }
        
        btnName.text = btnText;
        return item;
    }

    //Очистка опций
    private void FlushOptions()
    {
        if (currentItemList.Count > 0)
        {
            foreach (GameObject item in currentItemList)
                Destroy(item.gameObject);
        }
        currentItemList.Clear();
    }

    //Закрытие окна настроек
    private void CloseWindow()
    {
        Destroy(this.gameObject);
    }
}
