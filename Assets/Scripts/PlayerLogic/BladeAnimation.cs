using System.Collections;
using UnityEngine;

public class BladeAnimation : MonoBehaviour {
    private PlayerPhysics _physics;

    public float growthTime = 0.2f;
    public float lifetime = 0.6f;
    public float maxSize = 2f;
    private float minSize = 1f;
    private float curSize;

    private BladeGrowth bladeGrowth = BladeGrowth.Idle;

    private void Awake() {
        _physics = gameObject.GetComponentInParent<PlayerPhysics>();
    }

    private void Start() {
        gameObject.SetActive(false);
    }

    public void Run() {
        bladeGrowth = BladeGrowth.Growth;
        curSize = minSize;
        Refresh();
        gameObject.SetActive(true);
        StartCoroutine(GrowthUngrowth());
    }

    public void Refresh() {
        transform.localScale = new Vector3(0.9f, 0.05f, 0.9f);
        StopAllCoroutines();
    }

    private IEnumerator GrowthUngrowth() {
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