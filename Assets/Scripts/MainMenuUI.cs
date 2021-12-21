using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour {
    [SerializeField] private Transform levelsSubMenu;
    [SerializeField] private Button levelsButton;
    [Space]
    [SerializeField] private Transform skinsSubMenu;
    [SerializeField] private Button skinsButton;

    private Transform currentSubMenu;
    private Button currentButton;

    private void Start() {
        currentSubMenu = levelsSubMenu;
        currentSubMenu.gameObject.SetActive(true);
        currentButton = levelsButton;
        currentButton.interactable = false;

        levelsButton.onClick.AddListener(() => OpenSubMenu(levelsSubMenu, levelsButton));
        skinsButton.onClick.AddListener(() => OpenSubMenu(skinsSubMenu, skinsButton));
    }

    private void OpenSubMenu(Transform menu, Button button) {
        currentSubMenu.gameObject.SetActive(false);
        currentButton.interactable = true;

        currentSubMenu = menu;
        currentSubMenu.gameObject.SetActive(true);
        currentButton = button;
        currentButton.interactable = false;
    }
}