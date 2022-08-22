using UnityEngine;
using UnityEngine.UI;

public class UIRenderer : MonoBehaviour
{
    //Перечисление каналов цвета в Unity
    private enum UnityColorVars
    {
        RedChannel = 0,
        GreenChannel = 1,
        BlueChannel = 2,
        Transparency = 3
    }
    //Скорость смещения выезжающих элементов
    private const int showSpeed = 775;    
    
    //Эффект нарастающей прозрачности объекта UI
    public bool FadeEffect(bool fadeInOut, Image img, bool instant = false)
    {
        float fadeLevel = img.color[(int)UnityColorVars.Transparency],
              offset;
        if (instant)
        {
            if (fadeInOut)
                offset = 0f;
            else
                offset = 1f;
            img.color = new Color(
                img.color[(int)UnityColorVars.RedChannel],
                img.color[(int)UnityColorVars.GreenChannel],
                img.color[(int)UnityColorVars.BlueChannel],
                offset);
            return !fadeInOut;
        }

        if (fadeInOut == true)
            offset = fadeLevel - Time.deltaTime;
        else
            offset = fadeLevel + Time.deltaTime;

        if (offset >= 0f && offset <= 1f)
        {
            img.color = new Color(
                img.color[(int)UnityColorVars.RedChannel],
                img.color[(int)UnityColorVars.GreenChannel],
                img.color[(int)UnityColorVars.BlueChannel],
                offset);
            return fadeInOut;
        }
        else
            return !fadeInOut;
    }
    //Отзеркаливание объекта UI
    public bool MirrorUIObject(RectTransform obj, bool instant = false)
    {
        if (instant)
        {
            obj.localScale = new Vector3(-obj.localScale.x, obj.localScale.y, obj.localScale.z);
            return true;
        }
        return false;
    }
    //Перемещение объекта UI
    public bool SlideUIObject(RectTransform obj, Vector3 finalCoords, Directions direction = Directions.None, bool instant = false)
    {
        Vector3 currentPos = obj.position;
        if (instant || direction == Directions.None)
        {
            obj.position = finalCoords;
            return true;
        }
        else
        {
            if (direction.HasFlag(Directions.Right))
            {
                currentPos.x += (Time.fixedDeltaTime * showSpeed);
                if (Mathf.Ceil(currentPos.x) < Mathf.Floor(finalCoords.x))
                {
                    obj.position = currentPos;
                    return false;
                }
                else
                {
                    obj.position = finalCoords;
                    return true;
                }
            }
            else if (direction.HasFlag(Directions.Left))
            {
                currentPos.x -= (Time.fixedDeltaTime * showSpeed);
                if (Mathf.Ceil(finalCoords.x) < Mathf.Floor(currentPos.x))
                {
                    obj.position = currentPos;
                    return false;
                }
                else
                {
                    obj.position = finalCoords;
                    return true;
                }
            }

            if (direction.HasFlag(Directions.Up))
            {
                currentPos.y += (Time.fixedDeltaTime * showSpeed);
                if (Mathf.Ceil(currentPos.y) < Mathf.Floor(finalCoords.y))
                {
                    obj.position = currentPos;
                    return false;
                }
                else
                {
                    obj.position = finalCoords;
                    return true;
                }
            }
            else if (direction.HasFlag(Directions.Down))
            {
                currentPos.y -= (Time.fixedDeltaTime * showSpeed);
                if (Mathf.Ceil(finalCoords.y) < Mathf.Floor(currentPos.y))
                {
                    obj.position = currentPos;
                    return false;
                }
                else
                {
                    obj.position = finalCoords;
                    return true;
                }
            }
        }
        return false;
    }
}