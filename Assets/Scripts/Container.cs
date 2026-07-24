using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class Container : MonoBehaviour
{
    [Header("Settings Box")]
    public BoxType acceptType;
    public Transform dropPoint;
    public int scoreValue = 10;

    [Header("Particle")]
    public ParticleSystem wrongParticlePrefab;
    public Transform particleSpawnPoint;

    [Header("Audio")]
    public AudioClip wrongSound;
    private AudioSource _audioSource;

    private Vector3 _initialScale;
    private ParticleSystem _spawnedWrongParticle;

    private void Awake()
    {
        _initialScale = transform.localScale;
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();

        if (wrongParticlePrefab != null)
        {
            Vector3 spawnPos = particleSpawnPoint != null ? particleSpawnPoint.position : transform.position;
            Quaternion spawnRot = particleSpawnPoint != null ? particleSpawnPoint.rotation : Quaternion.identity;
            _spawnedWrongParticle = Instantiate(wrongParticlePrefab, spawnPos, spawnRot, transform);
            _spawnedWrongParticle.Stop();
        }
    }

    public void TryAccept(Box box)
    {
        if (box.IsBonus)
        {
            box.IsHandled = true;

            int finalPoints = 0;

            if (box.Type == BoxType.BonusTime)
            {
                GameManager.Instance.Timer.AddExtraTime(box.bonusTimeAmount);
                FloatingTextSpawner.Instance.SpawnText(transform.position, box.bonusScoreAmount, true);
            }
            else if (box.Type == BoxType.BonusScore)
            {
                int multiplier = GameManager.Instance.Combo.Multiplier;
                finalPoints = box.bonusScoreAmount * multiplier;

                GameManager.Instance.OnCorrectSort(box.bonusScoreAmount);
                FloatingTextSpawner.Instance.SpawnText(transform.position, finalPoints, true);
            }

            AnimateCorrectContainer();
            box.PlayCorrectTween(dropPoint.position, () =>
            {
                GameManager.Instance.Conveyor.ReleaseBox(box);
            });
            return;
        }

        if (box.Type == acceptType)
        {
            box.IsHandled = true;

            int multiplier = GameManager.Instance.Combo.Multiplier;
            int totalScoreAdded = scoreValue * multiplier;

            FloatingTextSpawner.Instance.SpawnText(transform.position, totalScoreAdded, false);

            AnimateCorrectContainer();

            box.PlayCorrectTween(dropPoint.position, () =>
            {
                GameManager.Instance.Conveyor.ReleaseBox(box);
            });

            GameManager.Instance.OnCorrectSort(scoreValue);
        }
        else
        {
            AnimateWrongContainer();
            PlayWrongParticles();

            if (CameraShaker.Instance != null)
            {
                CameraShaker.Instance.ShakeOnWrong();
            }

            if (wrongSound != null) _audioSource.PlayOneShot(wrongSound);

            box.PlayThrowUpTween(() =>
            {
                GameManager.Instance.Conveyor.ReleaseBox(box);
            });

            GameManager.Instance.OnWrongSort();
        }
    }

    private void PlayWrongParticles()
    {
        if (_spawnedWrongParticle != null)
        {
            _spawnedWrongParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            _spawnedWrongParticle.Play();
        }
    }

    private void AnimateCorrectContainer()
    {
        transform.DOKill(true);
        transform.localScale = _initialScale;

        transform.DOScale(_initialScale * 1.15f, 0.15f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                transform.DOScale(_initialScale, 0.15f);
            });
    }

    private void AnimateWrongContainer()
    {
        transform.DOKill(true);
        transform.localScale = _initialScale;

        transform.DOShakePosition(0.35f, strength: new Vector3(0.15f, 0f, 0.15f), vibrato: 20, randomness: 90);
    }
}
