using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PreparationScreenController : MonoBehaviour
{
    //Кнопки окна подготовки
    public Button[] menuButtons;
    //Надиписи данных о карте
    //Название карты
    public Text mapHeader,
        //Подсказка по карте
                mapHint;
    //Объект для визуализации карты
    public Image mapImage;
    //Макет модульного окна сохранения
    public GameObject saveScreenPrefab;
    //Используемая карта
    private MapFile map;
    //Отслеживание состояния окна сохранения
    private GameObject saveWindow = null;

    // Start is called before the first frame update
    void Start()
    {
        map = new MapFile(GlobalVariables.mapName);
        mapImage.sprite = GenerateMapTexture();
        SetUpUI();
    }

    //Подготовка интерфейса
    private void SetUpUI()
    {
        int i = 0;
        foreach (Button btn in menuButtons)
        {
            Text btnLabel = btn.GetComponentInChildren<Text>();
            btnLabel.text = GlobalVariables.localization.preparation.btns[i].MatchValue(btn.name);
            switch (i)
            {
                case 0:
                    btn.onClick.AddListener(delegate {
                        AsyncOperation loading = SceneManager.LoadSceneAsync(2); 
                    });
                    break;
                case 1:
                    btn.onClick.AddListener(delegate {
                        ToggleNextMapInfo(true);
                    });
                    break;
                case 2:
                    btn.onClick.AddListener(delegate {
                        ToggleNextMapInfo(false);
                    });
                    break;
                case 3:
                    btn.onClick.AddListener(delegate {
                        ToggleDataScreen(true);
                    });
                    break;
                case 4:
                    btn.onClick.AddListener(delegate {
                        ToggleDataScreen(false);
                    });
                    break;
                case 5:
                    btn.onClick.AddListener(delegate {
                        AsyncOperation loading = SceneManager.LoadSceneAsync(0);
                    });
                    break;
            }
            i++;
        }

        mapHeader.text = GlobalVariables.localization.preparation.mapName;
        mapHint.text = GlobalVariables.localization.preparation.mapHint;
    }

    //Переключение данных о следующей карте
    private void ToggleNextMapInfo(bool state)
    {
        mapHeader.gameObject.SetActive(state);
        mapImage.gameObject.SetActive(state);
        mapHint.gameObject.SetActive(state);
    }

    //Открытие модульного окна сохранения/загрузки 
    private void ToggleDataScreen(bool operationMode)
    {
        if (saveWindow == null)
        {
            saveWindow = Instantiate(saveScreenPrefab, this.transform);
            saveWindow.TryGetComponent(out CheckpointController controller);
            controller.operationMode = operationMode;
        }
    }

    //Генерация текстуры для отображения
    private Sprite GenerateMapTexture()
    {
        Texture2D mapTexture = new Texture2D(map.width, map.height);
        for (int i = 0; i < map.height; i++)
        {
            for (int j = 0; j < map.width; j++)
            {
                switch (map.GetPreset(new Vector2Int(i, j) ))
                {
                    case 2:
                        mapTexture.SetPixel(i, j, Color.gray);
                        break;
                    case 3:
                        mapTexture.SetPixel(i, j, Color.cyan);
                        break;
                    case 4:
                        mapTexture.SetPixel(i, j, Color.blue);
                        break;
                    case 5:
                        mapTexture.SetPixel(i, j, Color.green);
                        break;
                    case 6:
                        mapTexture.SetPixel(i, j, Color.red);
                        break;
                    case 7:
                        mapTexture.SetPixel(i, j, Color.black);
                        break;
                    default:
                        mapTexture.SetPixel(i, j, Color.white);
                        break;

                }
            }
        }
        mapTexture.filterMode = FilterMode.Point;
        mapTexture.Apply();
        return Sprite.Create(mapTexture, new Rect(0.0f, 0.0f, mapTexture.width, mapTexture.height), new Vector2(0.5f, 0.5f), 100f);
    }
}