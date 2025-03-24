using UnityEngine;

public class TrapShovel : Trap
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
