using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Skins : ISubMenuFiller {
    [SerializeField] private SkinsSO skins;
    [SerializeField] private GameObject SkinSelectUIPrefab;

    public override void UpdateSubMenuList(Transform UIListForm) {
        foreach (SkinSO skin in skins.skinList) {
            GameObject SkinSelectUI = Instantiate(SkinSelectUIPrefab, UIListForm);
            SkinSelectUI.GetComponentInChildren<TMP_Text>().text = skin.displayName;
            Button button = SkinSelectUI.GetComponentInChildren<Button>();
            button.onClick.AddListener(() => PlayerSettings.selectedSkinID = skin.uniqID);
        }
    }
}