using UnityEngine;

public class WeaponSpawner : MonoBehaviour
{
    [SerializeField]
    GameObject shotgunPrefab;
    [SerializeField]
    GameObject sniperPrefab;
    [SerializeField]
    Vector2 spawnAreaMin; // Minimum X and Y coordinates for the spawn area
    [SerializeField]
    Vector2 spawnAreaMax; // Maximum X and Y coordinates for the spawn area

    public static bool playerHasShotgun = false;
    public static bool playerHasSniper = false;
    public static bool enemyHasShotgun = false;
    public static bool enemyHasSniper = false;

    void Start()
    {
        SpawnWeapons();
    }

    void SpawnWeapons()
    {
        // Spawn two shotguns if the player or enemy does not have one
        for (int i = 0; i < 2; i++)
        {
            if (!playerHasShotgun || !enemyHasShotgun)
            {
                SpawnWeaponAtRandomLocation(shotgunPrefab);
            }
        }

        // Spawn two snipers if the player or enemy does not have one
        for (int i = 0; i < 2; i++)
        {
            if (!playerHasSniper || !enemyHasSniper)
            {
                SpawnWeaponAtRandomLocation(sniperPrefab);
            }
        }
    }

    void SpawnWeaponAtRandomLocation(GameObject weaponPrefab)
    {
        // Generate a random position within the defined area
        float x = Random.Range(spawnAreaMin.x, spawnAreaMax.x);
        float y = Random.Range(spawnAreaMin.y, spawnAreaMax.y);
        Vector3 randomPosition = new Vector3(x, y, 0);

        if (weaponPrefab != null)
        {
            Debug.Log($"Spawning {weaponPrefab.name} at position {randomPosition}");
            Instantiate(weaponPrefab, randomPosition, Quaternion.identity);
        }
        else
        {
            Debug.LogError("Weapon prefab is null.");
        }
    }
}