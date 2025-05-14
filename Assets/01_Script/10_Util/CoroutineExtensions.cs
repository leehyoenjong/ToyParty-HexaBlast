using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CoroutineExtensions
{
    public static IEnumerator WaitForAll(this MonoBehaviour mono, params IEnumerator[] coroutines)
    {
        var routines = new List<Coroutine>(coroutines.Length);

        // 모든 코루틴 시작
        foreach (var coroutine in coroutines)
        {
            routines.Add(mono.StartCoroutine(coroutine));
        }

        // 모든 코루틴이 완료될 때까지 대기
        foreach (var routine in routines)
        {
            yield return routine;
        }
    }
}