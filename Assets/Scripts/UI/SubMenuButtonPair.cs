using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class SubMenuButtonPair {
    [SerializeField] public Button button;
    [SerializeField] public ISubMenuFiller subMenuFiller;
}