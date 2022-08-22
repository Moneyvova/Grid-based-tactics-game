using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheckpointController : MonoBehaviour
{
    public Button closeButton;
    public Text windowLabel;
    public GameObject savePointPrefab;
    public Transform scrollViewport;

    public bool operationMode;

    private List<string> blurbs = new List<string>()
    {
        "Карта: 1\nРежим: Тактика\nХод: 3",
        "Карта: 1\nРежим: Тактика\nХод: 1",
        "Карта: 1\nРежим: Подготовка"
    };
    // Start is called before the first frame update
    void Start()
    {
        if (operationMode)
            windowLabel.text = GlobalVariables.localization.saveLabel;
        else
            windowLabel.text = GlobalVariables.localization.loadLabel;

        closeButton.onClick.AddListener(delegate {
            Destroy(this.gameObject);
        });

        for (int i=0; i < 3; i++)
        {
            GameObject obj = Instantiate(savePointPrefab, scrollViewport);
            if (i == 0)
                Destroy(obj.transform.Find("DeleteButton").gameObject);
            else
                Destroy(obj.transform.Find("AutoSaveMark").gameObject);

            obj.transform.localPosition = new Vector3(0, -20f -130f * i, 0);
            Text blurb = obj.transform.Find("StateDescription").GetComponent<Text>();
            blurb.text = blurbs[i];

            if (i > 0)
            {
                Text btnLabel = obj.transform.Find("DeleteButton").gameObject.GetComponentInChildren<Text>();
                btnLabel.text = GlobalVariables.localization.deleteBtnLabel;
            }

            if (operationMode)
            {
                Text btnLabel = obj.transform.Find("ActionButton").gameObject.GetComponentInChildren<Text>();
                btnLabel.text = GlobalVariables.localization.saveBtnLabel;
            }
            else
            {
                Text btnLabel = obj.transform.Find("ActionButton").gameObject.GetComponentInChildren<Text>();
                btnLabel.text = GlobalVariables.localization.loadBtnLabel;
            }
        }
    }
}