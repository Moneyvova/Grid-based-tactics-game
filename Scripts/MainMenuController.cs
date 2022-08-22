using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : UIRenderer
{
    public Image ImageFadeEffect;
    //Кнопка "Новая игра"
    public Button startButton,
        //Кнопка "Загрузить игру"
                  loadButton,
        //Кнопка "Настройки"
                  settingsButton,
        //Кнопка "Титры"
                  creditsButton,
        //Кнопка "Выход"
                  exitButton;
    //Модальные окна
    //Окно настройки
    public GameObject settingsWindow,
        //Окно загрузки
                      loadWindow,
        //Окно титров
                      creditsPanel;
    //Отслеживание эффекта затемнения
    private bool fadeStatus;
    //Отслеживание состояния текущего модального окна
    private GameObject modalWindow;

    // Start is called before the first frame update
    void Start()
    {
        fadeStatus = true;
        startButton.onClick.AddListener(LoadStartScene);
        startButton.GetComponentInChildren<Text>().text =
            GlobalVariables.localization.mainMenu.btns[0].MatchValue(startButton.name);

        loadButton.onClick.AddListener(ShowLoadScreen);
        loadButton.GetComponentInChildren<Text>().text =
            GlobalVariables.localization.mainMenu.btns[1].MatchValue(loadButton.name);

        settingsButton.onClick.AddListener(ShowSettings);
        settingsButton.GetComponentInChildren<Text>().text =
            GlobalVariables.localization.mainMenu.btns[2].MatchValue(settingsButton.name);

        creditsButton.onClick.AddListener(ShowCredits);
        creditsButton.GetComponentInChildren<Text>().text =
            GlobalVariables.localization.mainMenu.btns[3].MatchValue(creditsButton.name);

        exitButton.onClick.AddListener(ExitGame);
        exitButton.GetComponentInChildren<Text>().text =
            GlobalVariables.localization.mainMenu.btns[4].MatchValue(exitButton.name);
    }

    void FixedUpdate()
    {
        if (fadeStatus)
            fadeStatus = FadeEffect(fadeStatus, ImageFadeEffect);
        if (modalWindow == null)
            ToggleMenuItems(true);
        if (creditsPanel.activeSelf && Input.anyKey)
        {
            creditsPanel.SetActive(false);
            startButton.transform.parent.gameObject.SetActive(true);
        }
    }

    //Загрузка новой игры
    void LoadStartScene()
    {
        AsyncOperation loadingOperation = SceneManager.LoadSceneAsync("TacticalField");
    }

    //Вызов окна загрузки
    void ShowLoadScreen()
    {
        modalWindow = Instantiate(loadWindow, this.gameObject.transform);
        ToggleMenuItems(false);
    }

    //Вызов окна настройки
    void ShowSettings()
    {
        modalWindow = Instantiate(settingsWindow, this.gameObject.transform);
        ToggleMenuItems(false);
    }

    //Показать Титры
    void ShowCredits()
    {
        creditsPanel.SetActive(true);
        startButton.transform.parent.gameObject.SetActive(false);
    }

    //Выход из приложения
    void ExitGame()
    {
        Application.Quit();
    }

    //Блокировка кнопок при вызове окон
    private void ToggleMenuItems(bool interactability)
    {
        startButton.interactable = interactability;
        loadButton.interactable = interactability;
        settingsButton.interactable = interactability;
        creditsButton.interactable = interactability;
        exitButton.interactable = interactability;
    }
}
