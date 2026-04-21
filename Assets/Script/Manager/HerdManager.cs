using System.Collections.Generic;
using UnityEngine;

public class HerdManager : MonoBehaviour
{
    public static HerdManager Instance { get; private set; }

    public Dictionary<int, HerdData> activeHerds = new Dictionary<int, HerdData>();
    private int nextHerdID = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public HerdData CreateHerd(EntityType type)
    {
        int id = nextHerdID++;
        HerdData newHerd = new HerdData(id, type);
        activeHerds.Add(id, newHerd);
        return newHerd;
    }

    public void AddEntityToHerd(EntityData entity, int herdID)
    {
        if (activeHerds.TryGetValue(herdID, out HerdData herd))
        {
            if (!herd.members.Contains(entity))
            {
                herd.members.Add(entity);
                entity.herdID = herdID;
            }
        }
    }

    public void RemoveEntityFromHerd(EntityData entity)
    {
        if (entity.herdID != -1 && activeHerds.TryGetValue(entity.herdID, out HerdData herd))
        {
            herd.members.Remove(entity);
            entity.herdID = -1;
            
            // Optionally clean up empty herds
            if (herd.members.Count == 0)
            {
                activeHerds.Remove(herd.herdID);
            }
        }
    }

    private void Update()
    {
        if (Time.frameCount % 30 == 0)
        {
            UpdateHerds();
        }
    }

    private void UpdateHerds()
    {
        foreach (var kvp in activeHerds)
        {
            HerdData herd = kvp.Value;
            if (herd.members.Count == 0) continue;

            Vector2 centerSum = Vector2.zero;
            foreach (var member in herd.members)
            {
                centerSum += member.worldPosition;
            }
            herd.herdCenter = centerSum / herd.members.Count;

            if (herd.herdDestination == Vector2.zero || Vector2.Distance(herd.herdCenter, herd.herdDestination) < 3f)
            {
                Vector2 randomDir = Random.insideUnitCircle.normalized * 15f;
                herd.herdDestination = herd.herdCenter + randomDir;
            }
        }
    }
}
