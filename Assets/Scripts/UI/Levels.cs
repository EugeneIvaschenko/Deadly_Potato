using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Levels : MonoBehaviour {
    [SerializeField] private List<string> levelsList;
    [SerializeField] private GameObject LevelCheckUIPrefab;
    [SerializeField] private Transform LevelsListObject;

    private LobbyManager lobby;
    private List<Button> buttons;

    public void OnConnected() {
        SetInteractables(true);
    }

    public void OnDisconnected() {
        SetInteractables(false);
    }

    private void SetInteractables(bool isInteractable) {
        foreach(Button button in buttons) {
            button.interactable = isInteractable;
        }
    }

    private void UpdateLevelList() {
        buttons = new List<Button>();
        LevelsListObject.DetachChildren();
        foreach (string levelName in levelsList) {
            AddLevelCheck(levelName);
        }
    }

    private void AddLevelCheck(string levelName) {
        GameObject LevelCheckUI = Instantiate(LevelCheckUIPrefab, LevelsListObject);
        LevelCheckUI.GetComponentInChildren<TMP_Text>().text = levelName;
        Button button = LevelCheckUI.GetComponentInChildren<Button>();
        button.onClick.AddListener(() => lobby.JoinRoom(levelName));
        buttons.Add(button);
        button.interactable = false;
    }

    private void OnEnable() {
        lobby = GetComponent<LobbyManager>();
    }

    private void Start() {
        UpdateLevelList();
    }
}