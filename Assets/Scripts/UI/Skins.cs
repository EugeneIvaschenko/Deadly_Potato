using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Skins : MonoBehaviour {
    [SerializeField] private SkinsSO skins;
    [SerializeField] private GameObject SkinCheckUIPrefab;
    [SerializeField] private Transform SkinListObject;

    void OnEnable()
    {
        UpdateList();
    }

    public void UpdateList() {
        foreach(SkinSO skin in skins.skinList) {
            GameObject LevelCheckUI = Instantiate(SkinCheckUIPrefab, SkinListObject);
            LevelCheckUI.GetComponentInChildren<TMP_Text>().text = skin.displayName;
            Button button = LevelCheckUI.GetComponentInChildren<Button>();
            button.onClick.AddListener(() => PlayerSettings.selectedSkinID = skin.uniqID);
        }
    }
}