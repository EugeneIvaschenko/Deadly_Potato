using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Levels : ISubMenuFiller {
    [SerializeField] private List<string> levelsList;
    [SerializeField] private GameObject LevelSelectUIPrefab;

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

    public override void UpdateSubMenuList(Transform UIListForm) {
        Debug.Log("UpdateSubMenuList in Levels");
        buttons = new List<Button>();
        foreach (string levelName in levelsList) {
            AddLevelCheck(levelName, UIListForm);
        }
    }

    private void AddLevelCheck(string levelName, Transform UIListForm) {
        GameObject LevelCheckUI = Instantiate(LevelSelectUIPrefab, UIListForm);
        LevelCheckUI.GetComponentInChildren<TMP_Text>().text = levelName;
        Button button = LevelCheckUI.GetComponentInChildren<Button>();
        button.onClick.AddListener(() => lobby.JoinRoom(levelName));
        buttons.Add(button);
        button.interactable = PhotonNetwork.IsConnected;
    }

    private void OnEnable() {
        lobby = GetComponent<LobbyManager>();
    }
}