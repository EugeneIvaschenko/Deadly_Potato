using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour {
    public float growthTime = 0.1f;
    public float maxSize = 2f;
    private float minSize = 1f;
    private float curSize;
    [HideInInspector]public float lifetime = 3f;

    private ShieldGrowth shieldGrowth = ShieldGrowth.Idle;

    private void Start() {
        gameObject.SetActive(false);
    }

    public void ActivateShield() {
        shieldGrowth = ShieldGrowth.Growth;
        curSize = minSize;
        gameObject.SetActive(true);
        StartCoroutine(ShieldAnimation());
    }

    private IEnumerator ShieldAnimation() {
        while(shieldGrowth == ShieldGrowth.Growth) {
            float deltaSize = maxSize - minSize;
            curSize += deltaSize * Time.deltaTime / growthTime;
            transform.localScale = new Vector3(curSize, curSize, curSize);
            if (curSize >= maxSize) {
                curSize = maxSize;
                shieldGrowth = ShieldGrowth.Idle;
            }
            yield return null;
        }

        yield return new WaitForSeconds(lifetime - growthTime * 2);
        shieldGrowth = ShieldGrowth.Ungrowth;

        while (shieldGrowth == ShieldGrowth.Ungrowth) {
            float deltaSize = maxSize - minSize;
            curSize -= deltaSize * Time.deltaTime / growthTime;
            transform.localScale = new Vector3(curSize, curSize, curSize);
            if (curSize <= minSize) {
                curSize = minSize;
                shieldGrowth = ShieldGrowth.Idle;
            }
            yield return null;
        }
        gameObject.SetActive(false);
    }
}

public enum ShieldGrowth {
    Growth,
    Ungrowth,
    Idle
}