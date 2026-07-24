using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorSpawner : MonoBehaviour
{
    [Header("Base Setup")]
    public Box boxPrefab;
    public Transform spawnPoint;
    public Transform endPoint;
    public Transform boxesParent;
    public Material[] materialsByType;

    [Header("Adaptive Difficulty")]
    public float baseSpawnInterval = 1.5f;
    public float minSpawnInterval = 0.6f;
    public float baseTravelSpeed = 1.5f;
    public float maxTravelSpeed = 4.0f;
    public float scoreForMaxDifficulty = 500f;

    [Header("Spawn Limits & Smart Rules")]
    public int maxComplexBoxesOnBelt = 2;
    public float complexBoxIntervalMultiplier = 1.4f;

    [Header("Anti-Overlap Settings")]
    public float minBoxDistance = 1.2f;

    [Header("Bonus Box Settings")]
    [Range(0f, 1f)] public float bonusSpawnChance = 0.15f;
    public Material bonusScoreMaterial;
    public Material bonusTimeMaterial;

    [HideInInspector] public float currentTravelSpeed;
    [HideInInspector] public float currentSpawnInterval;

    private ObjectPool<Box> _pool;
    private Coroutine _spawnRoutine;
    private readonly List<Box> _activeBoxes = new List<Box>();

    private float _slowdownTimer = 0f;
    private float _freezeAbilityTimer = 0f;
    private float _nextSpawnDelayBonus = 0f;

    public bool IsFrozenByAbility => _freezeAbilityTimer > 0f;

    private void Awake()
    {
        _pool = new ObjectPool<Box>(boxPrefab, boxesParent, prewarm: 6);
        currentTravelSpeed = baseTravelSpeed;
        currentSpawnInterval = baseSpawnInterval;
    }

    public void StartSpawning()
    {
        StopSpawning();
        _spawnRoutine = StartCoroutine(SpawnLoop());
    }

    public void StopSpawning()
    {
        if (_spawnRoutine != null)
        {
            StopCoroutine(_spawnRoutine);
            _spawnRoutine = null;
        }
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            while (IsFrozenByAbility)
            {
                yield return null;
            }

            SpawnBox();

            float waitTime = currentSpawnInterval + _nextSpawnDelayBonus;
            _nextSpawnDelayBonus = 0f;

            yield return new WaitForSeconds(waitTime);
        }
    }

    private void SpawnBox()
    {
        BoxType type;
        Material mat;

        if (Random.value < bonusSpawnChance && bonusScoreMaterial != null && bonusTimeMaterial != null)
        {
            bool isTimeBonus = Random.value > 0.5f;
            type = isTimeBonus ? BoxType.BonusTime : BoxType.BonusScore;
            mat = isTimeBonus ? bonusTimeMaterial : bonusScoreMaterial;
        }
        else
        {
            int normalTypesCount = materialsByType.Length;
            int randomIndex = Random.Range(0, normalTypesCount);

            type = (BoxType)randomIndex;
            mat = materialsByType[randomIndex];
        }

        Box box = _pool.Get(spawnPoint.position, Quaternion.identity);
        box.Init(type, mat);

        if (GameManager.Instance != null && GameManager.Instance.Level != null)
        {
            BoxMechanicState mechanic = GameManager.Instance.Level.GetRandomMechanicForLevel();

            if (mechanic != BoxMechanicState.Normal && GetComplexBoxesCount() >= maxComplexBoxesOnBelt)
            {
                mechanic = BoxMechanicState.Normal;
            }

            box.InitMechanic(mechanic);

            if (mechanic != BoxMechanicState.Normal)
            {
                _nextSpawnDelayBonus = currentSpawnInterval * (complexBoxIntervalMultiplier - 1f);
            }
        }

        _activeBoxes.Add(box);
    }

    private int GetComplexBoxesCount()
    {
        int count = 0;
        foreach (var b in _activeBoxes)
        {
            if (b != null && b.mechanicState != BoxMechanicState.Normal)
            {
                count++;
            }
        }
        return count;
    }

    private void Update()
    {
        UpdateDifficulty();
        UpdateTimers();
        MoveBoxes();
    }

    private void MoveBoxes()
    {
        float effectiveSpeed = currentTravelSpeed;

        if (_freezeAbilityTimer > 0f)
        {
            effectiveSpeed = 0f;
        }
        else if (_slowdownTimer > 0f)
        {
            effectiveSpeed *= 0.4f;
        }

        float dt = Time.deltaTime;

        for (int i = _activeBoxes.Count - 1; i >= 0; i--)
        {
            Box box = _activeBoxes[i];

            if (box == null || box.IsHandled)
            {
                _activeBoxes.RemoveAt(i);
                continue;
            }

            if (box.IsDragging) continue;

            float boxSpeedFactor = 1f;

            Box boxAhead = GetBoxAheadInList(i);

            if (boxAhead != null)
            {
                float distSqr = (box.transform.position - boxAhead.transform.position).sqrMagnitude;
                float minDistSqr = minBoxDistance * minBoxDistance;

                if (distSqr < minDistSqr)
                {
                    float distanceToAhead = Mathf.Sqrt(distSqr);
                    boxSpeedFactor = Mathf.Clamp01((distanceToAhead - (minBoxDistance * 0.5f)) / (minBoxDistance * 0.5f));
                }
            }

            if (boxSpeedFactor > 0.01f)
            {
                box.transform.position = Vector3.MoveTowards(
                    box.transform.position,
                    endPoint.position,
                    effectiveSpeed * boxSpeedFactor * dt);
            }
        }
    }

    private Box GetBoxAheadInList(int currentIndex)
    {
        for (int i = currentIndex - 1; i >= 0; i--)
        {
            Box b = _activeBoxes[i];
            if (b != null && !b.IsHandled && !b.IsDragging)
            {
                return b;
            }
        }
        return null;
    }

    private void UpdateTimers()
    {
        if (_slowdownTimer > 0f) _slowdownTimer -= Time.deltaTime;
        if (_freezeAbilityTimer > 0f) _freezeAbilityTimer -= Time.deltaTime;
    }

    private void UpdateDifficulty()
    {
        if (GameManager.Instance == null || GameManager.Instance.Score == null) return;

        int currentScore = GameManager.Instance.Score.CurrentScore;
        float difficultyFactor = Mathf.Clamp01((float)currentScore / scoreForMaxDifficulty);

        currentTravelSpeed = Mathf.Lerp(baseTravelSpeed, maxTravelSpeed, difficultyFactor);
        currentSpawnInterval = Mathf.Lerp(baseSpawnInterval, minSpawnInterval, difficultyFactor);
    }

    public void ApplyMicroSlowdown(float duration)
    {
        _slowdownTimer = Mathf.Max(_slowdownTimer, duration);
    }

    public void ActivateFreezeAbility(float duration = 4f)
    {
        _freezeAbilityTimer = duration;
    }

    public void ReturnToConveyor(Box box)
    {
        if (box != null && !_activeBoxes.Contains(box))
        {
            _activeBoxes.Add(box);
        }
    }

    public void ReleaseBox(Box box)
    {
        if (box == null) return;

        _activeBoxes.Remove(box);
        _pool.Release(box);
    }
}
