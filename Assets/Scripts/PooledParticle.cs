using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(ParticleSystem))]
public class PooledParticle : MonoBehaviour
{
    private ParticleSystem _particleSystem;
    private IObjectPool<PooledParticle> _pool;

    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();

        var main = _particleSystem.main;
        main.stopAction = ParticleSystemStopAction.Callback;
    }

    public void SetPool(IObjectPool<PooledParticle> pool)
    {
        _pool = pool;
    }

    private void OnParticleSystemStopped()
    {
        _pool?.Release(this);
    }
}