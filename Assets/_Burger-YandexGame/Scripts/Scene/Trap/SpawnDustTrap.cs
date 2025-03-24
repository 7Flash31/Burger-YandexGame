using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnDustTrap : MonoBehaviour
{
    [SerializeField] private Transform _spawner;
    [SerializeField] private GameObject _dust;
    [SerializeField] private float _timeToDestroy;

    public void SpawnDust()
    {
        GameObject dust = Instantiate(_dust, _spawner.position, _spawner.rotation);
        Destroy(dust, _timeToDestroy);
    }
}
