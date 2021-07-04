using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UtilityStaticFunctions
{
    // perform a cross fade on a canvas group (Unity, why isn't there a function for this?)
    public static IEnumerator CanvasGroupCrossFadeAlpha(CanvasGroup group, float alpha, float duration)
    {
        float passedTime = 0.0f;
        float endTime = Time.time + duration;

        while (Time.time < endTime)
        {
            passedTime += Time.deltaTime;

            group.alpha = Mathf.Lerp(group.alpha, alpha, passedTime / duration);

            yield return new WaitForEndOfFrame();
        }

        group.alpha = alpha;
    }

    // lerp a GameObject's position to a specified position over a specified duration
    public static IEnumerator MoveGameObjectOverTime(GameObject toMove, Vector3 toMoveTo, float duration)
    {
        float timer = 0.0f;

        while (timer < duration)
        {
            toMove.transform.position = Vector3.Lerp(toMove.transform.position, toMoveTo, timer / duration);

            timer += Time.deltaTime;
            yield return null;
        }

        toMove.transform.position = toMoveTo;
    }
}