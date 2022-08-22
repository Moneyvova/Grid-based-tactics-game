using UnityEngine;

public class ObjectRenderer : MonoBehaviour
{
    [Header("GFX variables")]
    //Переменная для отслеживания используемого цвета
    public Colours activeColour;
    //Цвет выделения объекта
    public Color highlight,
        //Цвет выбора объекта
                 select,
        //Цвет наведения на объект
                 hover,
        //Цвет после перемещения
                 moved;
    //Набор свойств материала Unity
    private MaterialPropertyBlock matPropBlock;

    // Awake is called when GameObject initialized or loaded
    void Awake()
    {
        matPropBlock = new MaterialPropertyBlock();
    }

    //Настройка цвета объектов
    public void SetColour(Color clr)
    {
        this.TryGetComponent(out Renderer render);
        matPropBlock.SetColor("_Color", clr);
        render.SetPropertyBlock(matPropBlock);
    }
    
    //Изменение цвета у объектов без свечения
    public void ToggleColourChange(Colours newColour, Colours colourMod = Colours.None)
    {
        Renderer render = this.gameObject.GetComponentInChildren(typeof(Renderer)) as Renderer;

        Color offset = new Color();
        switch (colourMod)
        {
            case Colours.Hover:
                offset = hover;
                break;
            default:
                break;
        }
        switch (newColour)
        {
            case Colours.Highlight:
                activeColour = Colours.Highlight;
                matPropBlock.SetColor("_Color", highlight + offset);
                break;
            case Colours.Select:
                activeColour = Colours.Select;
                matPropBlock.SetColor("_Color", select + offset);
                break;
            case Colours.Hover:
                activeColour = Colours.Hover;
                matPropBlock.SetColor("_Color", hover);
                break;
            case Colours.Moved:
                activeColour = Colours.Moved;
                matPropBlock.SetColor("_Color", moved + offset);
                break;
            default:
                activeColour = Colours.None;
                matPropBlock.SetColor("_Color", Color.white);
                break;
        }
        render.SetPropertyBlock(matPropBlock);
    }

    //Переключение свечения у объектов
    public void ToggleEmission(Colours colour = Colours.None, Colours colourMod = Colours.None)
    {
        Renderer render = this.GetComponent<Renderer>();
        Material mat = render.material;
        if (!mat.IsKeywordEnabled("_EMISSION"))
            mat.EnableKeyword("_EMISSION"); //Если свечение отключено - включить
        
        //Добавление модификатора цвета свечения
        Color offset = new Color();
        switch (colourMod)
        {
            case Colours.Hover:
                offset = hover;
                break;
            default:
                break;
        }
        //Переключение свечения по переданному colour
        switch (colour)
        {
            case Colours.Highlight:
                matPropBlock.SetColor("_EmissionColor", highlight + offset);
                activeColour = Colours.Highlight;
                break;
            case Colours.Select:
                matPropBlock.SetColor("_EmissionColor", select + offset);
                activeColour = Colours.Select;
                break;
            case Colours.Hover:
                matPropBlock.SetColor("_EmissionColor", hover);
                activeColour = Colours.Hover;
                break;
            case Colours.Moved:
                matPropBlock.SetColor("_EmissionColor", moved + offset);
                activeColour = Colours.Moved;
                break;
            default:
                mat.DisableKeyword("_EMISSION");
                activeColour = Colours.None;
                break;
        }
        render.SetPropertyBlock(matPropBlock);
    }
}