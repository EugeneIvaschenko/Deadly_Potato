using System.Collections;
using UnityEngine;

public class Blade : MonoBehaviour {
    public float growthTime = 0.2f;
    public float lifetime = 0.6f;
    public float maxSize = 2f;
    private float minSize = 1f;
    private float curSize;
    public BladeRB _bladeRB;

    private BladeGrowth bladeGrowth = BladeGrowth.Idle;

    private void Start() {
        //_bladeRB = GetComponentInParent<BladeRB>();
        gameObject.SetActive(false);
    }

    private void FixedUpdate() {
        if (gameObject.activeSelf) {
            _bladeRB.transform.localRotation = transform.localRotation;
            //_bladeRB.transform.localScale = transform.localScale;
            _bladeRB.transform.position = transform.position;
        }
    }

    public void Attack() {
        bladeGrowth = BladeGrowth.Growth;
        curSize = minSize;
        gameObject.SetActive(true);
        StartCoroutine(BladeAnimation());
    }

    public void Refresh() {
        transform.localScale = new Vector3(0.9f, 0.05f, 0.9f);
        StopAllCoroutines();
    }

    private IEnumerator BladeAnimation() {
        while(bladeGrowth == BladeGrowth.Growth) {
            float deltaSize = maxSize - minSize;
            curSize += deltaSize * Time.deltaTime / growthTime;
            transform.localScale = new Vector3(curSize, transform.lossyScale.y, curSize);
            if (curSize >= maxSize) {
                curSize = maxSize;
                bladeGrowth = BladeGrowth.Idle;
            }
            yield return null;
        }

        yield return new WaitForSeconds(lifetime - growthTime * 2);
        bladeGrowth = BladeGrowth.Ungrowth;

        while (bladeGrowth == BladeGrowth.Ungrowth) {
            float deltaSize = maxSize - minSize;
            curSize -= deltaSize * Time.deltaTime / growthTime;
            transform.localScale = new Vector3(curSize, transform.lossyScale.y, curSize);
            if (curSize <= minSize) {
                curSize = minSize;
                bladeGrowth = BladeGrowth.Idle;
            }
            yield return null;
        }
        gameObject.SetActive(false);
    }
}

public enum BladeGrowth {
    Growth,
    Ungrowth,
    Idle
}