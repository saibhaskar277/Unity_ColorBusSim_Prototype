using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetMover : MonoBehaviour
{
    public List<Transform> cubes;
    public float duration = 2f;

    private void Start()
    {
        StartCoroutine(MoveLoop());
    }

    IEnumerator MoveLoop()
    {
        while (true)
        {
            // Store current positions
            List<Vector3> positions = new List<Vector3>();
            foreach (var cube in cubes)
            {
                positions.Add(cube.position);
            }

            int completed = 0;

            // Move each cube to next cube's position
            for (int i = 0; i < cubes.Count; i++)
            {
                Vector3 targetPos = positions[(i + 1) % positions.Count];
                StartCoroutine(MoveCube(cubes[i], targetPos, () => completed++));
            }

            // Wait until all cubes finish moving
            yield return new WaitUntil(() => completed == cubes.Count);
        }
    }

    IEnumerator MoveCube(Transform cube, Vector3 targetPos, System.Action onComplete)
    {
        float time = 0f;
        Vector3 startPos = cube.position;

        while (time < duration)
        {
            float t = time / duration;

            cube.position = Vector3.Lerp(startPos, targetPos, t);

            time += Time.deltaTime;
            yield return null;
        }

        cube.position = targetPos;
        onComplete?.Invoke();
    }
}