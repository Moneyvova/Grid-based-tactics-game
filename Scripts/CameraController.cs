using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed,                 //Скорость перемещения камеры
                 zoomTarget;                //Конечная точка для приближения/отдаления
    public const int minZoom = 1;           //Минимальное отдаление камеры от объектов
    public int maxZoom = 15;                //Максимальное отдаление камеры от объектов
    public Camera additionalCamera;
    
    private float cameraCurrentZoom;        //Переменная для отслеживания текущего приближения
    private bool scrollAllowed = true;      //Флаг отмечающий возможность перемещать камеру
    private Camera usedCamera;              //Переменная для хранения основного объекта-камеры

    void Awake()
    {
        usedCamera = Camera.main;
        this.transform.rotation = Quaternion.Euler(80, -90, 0);
        cameraCurrentZoom = usedCamera.orthographicSize;
        additionalCamera.orthographicSize = usedCamera.orthographicSize;
        zoomTarget = cameraCurrentZoom;
        scrollAllowed = scrollToggle();
    }
    // FixedUpdate is called before Update within fixed intervals (default is 50 fps)
    void FixedUpdate()
    {
        if (cameraCurrentZoom != zoomTarget)
            zoomCamera();
    }

    //Создание луча из камеры к курсору
    public Ray rayToCursor()
    {
        return usedCamera.ScreenPointToRay(Input.mousePosition);
    }

    //Функция перемещения камеры
    public void moveCamera(Directions d, float mod = 1f)
    {
        if (scrollAllowed) //Если разрешено перемещение
        {
            float t_distance = moveSpeed * mod * Time.deltaTime; //Расстояние на которое изменится положение камеры
            switch (d)                                           //Распределяется по сторонам света. Нужно чтобы хоть 
            {                                                    //Нужно чтобы хоть один луч из onGamePlane давал true
                case Directions.Up:
                    if (onGamePlane(d) || onGamePlane(Directions.UpLeft) || onGamePlane(Directions.UpRight))
                        usedCamera.transform.position += new Vector3(-t_distance, 0, 0);
                    break;
                case Directions.Right:
                    if (onGamePlane(d) || onGamePlane(Directions.UpRight) || onGamePlane(Directions.DownRight))
                        usedCamera.transform.position += new Vector3(0, 0, t_distance);
                    break;
                case Directions.Down:
                    if (onGamePlane(d) || onGamePlane(Directions.DownLeft) || onGamePlane(Directions.DownRight))
                        usedCamera.transform.position += new Vector3(t_distance, 0, 0);
                    break;
                case Directions.Left:
                    if (onGamePlane(d) || onGamePlane(Directions.UpLeft) || onGamePlane(Directions.DownLeft))
                        usedCamera.transform.position += new Vector3(0, 0, -t_distance);
                    break;
            }
        }
    }
    //Определение возможности скролла экрана
    private bool scrollToggle()
    {
        Vector3 t_rayAngle = new Vector3();
        foreach (Directions d in Enum.GetValues(typeof(Directions)))
        {
            if (d != Directions.None)
            {
                //Получение параметров направления луча в зависимости от текущего значения направления
                switch (d)
                {
                    case Directions.UpLeft:
                        t_rayAngle = new Vector3(0, Screen.height - 1, 0);
                        break;
                    case Directions.Up:
                        t_rayAngle = new Vector3(Mathf.FloorToInt(Screen.width / 2), Screen.height - 1, 0);
                        break;
                    case Directions.UpRight:
                        t_rayAngle = new Vector3(Screen.width - 1, Screen.height - 1, 0);
                        break;
                    case Directions.Left:
                        t_rayAngle = new Vector3(0, Mathf.FloorToInt(Screen.height / 2), 0);
                        break;
                    case Directions.Right:
                        t_rayAngle = new Vector3(Screen.width - 1, Mathf.FloorToInt(Screen.height / 2), 0);
                        break;
                    case Directions.DownLeft:
                        t_rayAngle = new Vector3(0, 0, 0);
                        break;
                    case Directions.Down:
                        t_rayAngle = new Vector3(Mathf.FloorToInt(Screen.width / 2), 0, 0);
                        break;
                    case Directions.DownRight:
                        t_rayAngle = new Vector3(Screen.width - 1, 0, 0);
                        break;
                }
                Ray t_ray = usedCamera.ScreenPointToRay(t_rayAngle);                        //Создаётся луч для проверки перемещения
                bool hitConfirm = Physics.Raycast(t_ray, out RaycastHit t_hit, 100, 1 << 8);//Ответ попадения на зону игрового поля
                Debug.DrawLine(t_ray.origin, t_hit.point, Color.blue, 5f);
                if (hitConfirm)
                    return true;
            }
        }
        return false;
    }

    //Проверка лучами возможности движения обзора камеры
    private bool onGamePlane(Directions d)
    {
        int t_positionAdjust = Mathf.CeilToInt(moveSpeed * 2);
        Vector3 t_baseRay = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        switch (d)
        {
            case Directions.UpLeft:
                t_baseRay += new Vector3(-t_positionAdjust, t_positionAdjust, 0);
                break;
            case Directions.Up:
                t_baseRay += new Vector3(0, t_positionAdjust, 0);
                break;
            case Directions.UpRight:
                t_baseRay += new Vector3(t_positionAdjust, t_positionAdjust, 0);
                break;
            case Directions.Left:
                t_baseRay += new Vector3(-t_positionAdjust, 0, 0);
                break;
            case Directions.Right:
                t_baseRay += new Vector3(t_positionAdjust, 0, 0);
                break;
            case Directions.DownLeft:
                t_baseRay += new Vector3(-t_positionAdjust, -t_positionAdjust, 0);
                break;
            case Directions.Down:
                t_baseRay += new Vector3(0, -t_positionAdjust, 0);
                break;
            case Directions.DownRight:
                t_baseRay += new Vector3(t_positionAdjust, -t_positionAdjust, 0);
                break;
        }
        Ray t_ray = usedCamera.ScreenPointToRay(t_baseRay);
        
        bool check = Physics.Raycast(t_ray, out RaycastHit t_hit, 100, 1 << 8);

        return check;
    }

    //Формула для пошагового приближения объектива камеры
    private void zoomCamera()
    {
        zoomTarget = zoomBorders(zoomTarget, minZoom, maxZoom);
        
        if (zoomTarget < cameraCurrentZoom)
            cameraCurrentZoom -= cameraZoomFormula(cameraCurrentZoom, zoomTarget);
        else if (zoomTarget > cameraCurrentZoom)
            cameraCurrentZoom += cameraZoomFormula(cameraCurrentZoom, zoomTarget);
        
        cameraCurrentZoom = zoomBorders(cameraCurrentZoom, minZoom, maxZoom);
        usedCamera.orthographicSize = cameraCurrentZoom;
        additionalCamera.orthographicSize = usedCamera.orthographicSize;

        scrollAllowed = scrollToggle();
    }

    //Проверка границ приближения/отдаления
    private float zoomBorders(float var, float min, float max)
    {
        if (var < min)
            var = min;
        else if (var > max)
            var = max;
        return var;
    }

    //Формула для расчёта используемого приближения за шаг
    private float cameraZoomFormula(float start, float finish)
    {
        float diff = Mathf.Abs(start - finish),
              res  = 6f * (Time.deltaTime);
        if (diff < res)
        {
            for (int i = 0; i < 8; i++)
            {
                res = ((Time.deltaTime) / Mathf.Pow(2, i));
                if (diff > res) 
                    break;
            }
        }
        if (diff < res)
            return diff;
        return res;
    }
}
