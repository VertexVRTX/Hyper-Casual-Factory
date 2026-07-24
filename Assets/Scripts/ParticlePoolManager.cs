using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ParticlePoolManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private PooledParticle _prefab;
    [SerializeField] private int _defaultCapacity = 10;
    [SerializeField] private int _maxSize = 50;

    private UnityEngine.Pool.IObjectPool<PooledParticle> _pool;

    private void Awake()
    {
        _pool = new UnityEngine.Pool.ObjectPool<PooledParticle>(
            CreateParticle,
            OnGetFromPool,
            OnReleaseToPool,
            OnDestroyPoolObject,
            false,
            _defaultCapacity,
            _maxSize
        );
    }

    private PooledParticle CreateParticle()
    {
        PooledParticle particle = Instantiate(_prefab);
        particle.SetPool(_pool);
        return particle;
    }

    private void OnGetFromPool(PooledParticle particle)
    {
        particle.gameObject.SetActive(true);
    }

    private void OnReleaseToPool(PooledParticle particle)
    {
        particle.gameObject.SetActive(false);
    }

    private void OnDestroyPoolObject(PooledParticle particle)
    {
        Destroy(particle.gameObject);
    }

    public void PlayEffect(Vector3 position, Quaternion rotation)
    {
        PooledParticle particle = _pool.Get();
        particle.transform.SetPositionAndRotation(position, rotation);
        particle.GetComponent<ParticleSystem>().Play();
    }
}
