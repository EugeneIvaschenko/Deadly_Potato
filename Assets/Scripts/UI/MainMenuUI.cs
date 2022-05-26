using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour {
    [SerializeField] private Transform subMenuForm;
    [Space]
    [SerializeField] private List<SubMenuButtonPair> subMenuButtonPairs;
    

    private Button currentButton;

    private void Start() {
        bool isFirst = true;
        foreach (SubMenuButtonPair pair in subMenuButtonPairs) {
            pair.button.onClick.AddListener(() => {
                foreach (Transform child in subMenuForm.transform) {
                    Destroy(child.gameObject);
                }
                OpenSubMenu(pair.button);
                pair.subMenuFiller.UpdateSubMenuList(subMenuForm);
            });
            if (isFirst) {
                isFirst = false;
                OpenSubMenu(pair.button);
                pair.subMenuFiller.UpdateSubMenuList(subMenuForm);
            }
        }
    }

    private void OpenSubMenu(Button button) {
        if(currentButton != null) currentButton.interactable = true;
        currentButton = button;
        currentButton.interactable = false;
    }
}