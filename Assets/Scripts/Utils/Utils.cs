using UnityEngine;

public static class Utils {
    public static float ClampAngle(float x) {
        return x - 360 * Mathf.Floor(x / 360);
    }
}
