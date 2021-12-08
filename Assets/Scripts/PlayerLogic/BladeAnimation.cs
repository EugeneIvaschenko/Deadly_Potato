using System.Collections;
using UnityEngine;

public class BladeAnimation : MonoBehaviour {
    [SerializeField] private Transform bladeObject;
    private PlayerPhysics _physics;

    public float rotatesPerSecond = 5;
    public float growthTime = 0.2f;
    public float duration = 0.6f;
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
        StartCoroutine(Rotation());
    }

    public void Refresh() {
        transform.localScale = new Vector3(0.9f, 0.05f, 0.9f);
        StopAllCoroutines();
    }

    private IEnumerator GrowthUngrowth() {
        while (bladeGrowth == BladeGrowth.Growth) {
            float deltaSize = maxSize - minSize;
            curSize += deltaSize * Time.deltaTime / growthTime;
            transform.localScale = new Vector3(curSize, transform.lossyScale.y, curSize);
            if (curSize >= maxSize) {
                curSize = maxSize;
                bladeGrowth = BladeGrowth.Idle;
            }
            yield return null;
        }

        yield return new WaitForSeconds(duration - growthTime * 2);
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

    private IEnumerator Rotation() {
        float timeLeft = duration;
        Vector3 rotation = new Vector3(0, rotatesPerSecond * 360, 0);
        while (timeLeft > 0) {
            bladeObject.Rotate(rotation * Time.deltaTime);
            timeLeft -= Time.deltaTime;
            yield return null;
        }
    }
}

public enum BladeGrowth {
    Growth,
    Ungrowth,
    Idle
}