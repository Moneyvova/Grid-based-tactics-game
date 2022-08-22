using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CutsceneFile
{
    //Используемая папка
    public string folder,
        //Имя используемого файла
                  fileName;
    public CutsceneContent content;
    //Порядковый номер шага
    private int currentAction = 0;
    //Используемый файл
    private TextAsset file;

    //Инициализация касса по-умолчанию
    public CutsceneFile(string newFolder = "", string newName = "test")
    {
        content = new CutsceneContent();
        folder = newFolder;
        fileName = newName;
        file = LoadFile();
        if (file != null)
            JsonUtility.FromJsonOverwrite(file.text, content);
    }

    //Загрузка файла постановочной сцены
    private TextAsset LoadFile()
    {
        TextAsset newFile;
        if (folder == "")
            newFile = Resources.Load("CutsceneData/" + fileName) as TextAsset;
        else
            newFile = Resources.Load("CutsceneData/" + folder + "/" + fileName) as TextAsset;
        if (newFile != null)
            return newFile;
        else
            return null;
    }

    //Выдача следующего шага
    public CutsceneStep NextAction()
    {
        CutsceneStep data = null;
        if (currentAction < content.actions.Count)
            data = content.actions[currentAction];
        currentAction++;
        return data;
    }
}
[System.Serializable]
public class CutsceneContent
{
    //Список акторов текущей сцены
    public List<CutsceneActors> actors = new List<CutsceneActors>();
    //Шаги текущей сцены
    public List<CutsceneStep> actions = new List<CutsceneStep>();
    //Путь до рисунков по-умолчанию
    private const string bustPath = "ImagesCutscene/";

    //Загрузить рисунок актора
    public Sprite LoadActorImage(int actorIndex, string emotion = "base")
    {
        string imageName;
        if (emotion!="" && emotion != null)
            imageName = actors[actorIndex].baseBustName + "_" + emotion;
        else
            imageName = actors[actorIndex].baseBustName + "_base";
        Sprite img = Resources.Load<Sprite>(bustPath+imageName);
        return img;
    }
}
[System.Serializable]
public class CutsceneActors
{
    //Имя актора
    public string internalName,
        //Набор рисунков с эмоциями
                  baseBustName;
}
[System.Serializable]
public class CutsceneStep
{
    //Ссылка на актора и его речь
    public int actor;
    public string screenText;
    //Параметры картинки актора
    public string emotion;
    public float emotionWidth;
    public bool mirrored;
    //Положение картинки актора
    public float offsetX;
    public float offsetY;
    //Параметры анимации
    public Directions slideDirection;
}

public class AnimationFinish{
    public enum Actions
    {
        None,
        Rotation,
        Slide,
        Mirror,
        Fade
    }
    public Actions appliedAction;
    public RectTransform lastObject;
    public Vector3 targetCoords;
    public Directions actionDirection;
}

public class CutsceneController : UIRenderer
{
    public Text speakBubble,
                speakShadow,
                actorBubble;
    public Image speakerImage;
    public Image waitImage;
    public CutsceneFile cutscene;
    public bool showNext = true;
    // Start is called before the first frame update
    public int currentAction = 0;

    private Vector3 ImgPosition = new Vector3(-840f, -100f, 0);
    private List<Image> actorImages = new List<Image>();
    private AnimationFinish finisher = new AnimationFinish();

    void Start()
    {
        cutscene = new CutsceneFile(GlobalVariables.mapName, GlobalVariables.cutsceneName);
        for (int i = 0; i < cutscene.content.actors.Count; i++)
        {
            Image temp = Instantiate(speakerImage, this.gameObject.transform);
            int posMod = (int) Mathf.Pow(-1, i);
            //Сделать первым на отображении
            temp.transform.SetSiblingIndex(0);
            //Сделать видимым
            temp.gameObject.SetActive(true);
            temp.transform.localPosition = new Vector3(ImgPosition.x * posMod, ImgPosition.y, 0);
            actorImages.Add(temp);
        }
        StartOfCutscene();
    }

    void FixedUpdate()
    {
        if (finisher.appliedAction != AnimationFinish.Actions.None)
            KeepAnimatingObject();
        else
            showNext = FadeEffect(showNext, waitImage);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.Space))
            PlayCutsceneStep();
    }

    //Переход на первый шаг
    private void StartOfCutscene()
    {
        PlayCutsceneStep();
    }

    //Воспроизведение шага сцены
    private void PlayCutsceneStep()
    {
        if (finisher.appliedAction != AnimationFinish.Actions.None)
        {
            finisher.actionDirection = Directions.None;
            KeepAnimatingObject();
        }
        else
        {
            showNext = FadeEffect(true, waitImage, true);
            CutsceneStep currentStep = cutscene.NextAction();
            if (currentStep != null)
            {
                UnfocusActors(currentStep.actor);
                ChangeImage(currentStep);
                ChangeText(currentStep);
            }
            else
                Destroy(this.gameObject);
        }
    }
    //Функция затемнения неактивного актёра в поставновочной сцене
    private void UnfocusActors(int exception = -1)
    {
        for (int i=0; i < actorImages.Count; i++)
        {
            if (i != exception)
                actorImages[i].color = Color.gray;
            else
                actorImages[i].color = Color.white;
        }
    }

    //Функция замены кадра у актёра
    private void ChangeImage(CutsceneStep usedStep)
    {
        Sprite newImage = cutscene.content.LoadActorImage(usedStep.actor, usedStep.emotion);

        actorImages[usedStep.actor].sprite = newImage;
        actorImages[usedStep.actor].transform.SetSiblingIndex(cutscene.content.actors.Count);

        if (usedStep.mirrored)
            MirrorUIObject(actorImages[usedStep.actor].rectTransform, true);
        if (usedStep.emotionWidth > 0 && usedStep.emotionWidth != 2f)
            FixImageAspectRatio(actorImages[usedStep.actor], usedStep.emotionWidth);
        else
            FixImageAspectRatio(actorImages[usedStep.actor], 2);
        ApplyOffset(usedStep);
    }
    //Исправление соотношения сторон эмоции
    private void FixImageAspectRatio(Image img, float width)
    {
        img.TryGetComponent(out AspectRatioFitter fitter);
        fitter.aspectRatio = width / 3;
    }
    //Изменение положения актёра на экране постановочной сцены.
    private void ApplyOffset(CutsceneStep usedStep)
    {
        if (usedStep.offsetX != 0f || usedStep.offsetY != 0f)
        {
            Vector3 initialCoords = actorImages[usedStep.actor].transform.position;

            float mod = (Screen.width / 1280f);

            initialCoords += new Vector3(usedStep.offsetX*mod, usedStep.offsetY, 0);
            
            if (usedStep.slideDirection != Directions.None)
            {
                finisher.appliedAction = AnimationFinish.Actions.Slide;
                finisher.lastObject = actorImages[usedStep.actor].rectTransform;
                finisher.targetCoords = initialCoords;
                finisher.actionDirection = usedStep.slideDirection;
            }
            else
                SlideUIObject(actorImages[usedStep.actor].rectTransform, initialCoords);
        }
    }

    //Изменение текста в окне
    private void ChangeText(CutsceneStep usedStep)
    {
        if (usedStep.screenText == "" || usedStep.screenText == null)
        {
            speakShadow.transform.parent.gameObject.SetActive(false);
            actorBubble.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            speakShadow.transform.parent.gameObject.SetActive(true);
            actorBubble.transform.parent.gameObject.SetActive(true);

            speakBubble.text = usedStep.screenText;
            speakShadow.text = usedStep.screenText;
            if (usedStep.actor == 0)
                actorBubble.text = GlobalVariables.localization.actor1;
            else
                actorBubble.text = GlobalVariables.localization.actor2;
        }
    }

    //Функция для доведения описанной анимации действия до конца
    private void KeepAnimatingObject()
    {
        bool result;
        switch (finisher.appliedAction)
        {
            case AnimationFinish.Actions.Slide:
                result = SlideUIObject(finisher.lastObject, finisher.targetCoords, finisher.actionDirection);
                if (result)
                    finisher.appliedAction = AnimationFinish.Actions.None;
                return;
        }
    }
}
