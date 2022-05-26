using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VFXCustomization : ISubMenuFiller {
    [SerializeField] private EffectsSO effectsList;
    [SerializeField] private GameObject VFXSelectUIPrefab;
    [SerializeField] private GameObject SeparatortUIPrefab;

    private List<List<Button>> buttonGroups;

    public override void UpdateSubMenuList(Transform UIListForm) {
        buttonGroups = new List<List<Button>>();
        List<Button> buttonGroup = new List<Button>();
        effectsList.list = effectsList.list.OrderBy(t => t.type).ToList();
        SpecialEffectType prevType = effectsList.list[0].type;
        GameObject SeparatorUI = Instantiate(SeparatortUIPrefab, UIListForm);
        SeparatorUI.GetComponentInChildren<TMP_Text>().text = effectsList.list[0].type.ToString() + ":";

        foreach (EffectSO effect in effectsList.list) {
            if(prevType != effect.type) {
                prevType = effect.type;
                if (buttonGroup.Count > 0) {
                    buttonGroups.Add(buttonGroup);
                    buttonGroup = new List<Button>();
                }
                else Destroy(SeparatorUI);
                SeparatorUI = Instantiate(SeparatortUIPrefab, UIListForm);
                SeparatorUI.GetComponentInChildren<TMP_Text>().text = effect.type.ToString() + ":";
            }

            GameObject VFXSelectUI = Instantiate(VFXSelectUIPrefab, UIListForm);
            VFXSelectUI.GetComponentInChildren<TMP_Text>().text = effect.displayName;
            Button button = VFXSelectUI.GetComponentInChildren<Button>();
            buttonGroup.Add(button);
            if (PlayerSettings.VFXPlayerCustomizations.ContainsKey(effect.type) && PlayerSettings.VFXPlayerCustomizations[effect.type] == effect.uniqID) button.interactable = false;
            button.onClick.AddListener(() => {
                if (PlayerSettings.VFXPlayerCustomizations.ContainsKey(effect.type)) PlayerSettings.VFXPlayerCustomizations[effect.type] = effect.uniqID;
                else PlayerSettings.VFXPlayerCustomizations.Add(effect.type, effect.uniqID);
                SelectNewButton(button);
            });
        }
    }

    private void SelectNewButton(Button selectedButton) {
        foreach(List<Button> buttonGroup in buttonGroups) {
            if (buttonGroup.Contains(selectedButton)) {
                foreach(Button button in buttonGroup) {
                    if (button == selectedButton) button.interactable = false;
                    else button.interactable = true;
                }
                return;
            }
        }
    }
}
