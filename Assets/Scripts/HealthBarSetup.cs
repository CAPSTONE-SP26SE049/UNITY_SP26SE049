using UnityEngine;

/// <summary>
/// Script để tự động setup Health Bar trong scene
/// Attach script này vào bất kỳ GameObject nào trong scene để tự động tạo Health Bar
/// </summary>
public class HealthBarSetup : MonoBehaviour
{
    [Header("Auto Setup")]
    public bool setupOnStart = true;
    public int maxHealth = 100;

    private HealthManager healthManager;
    private HealthBar healthBar;

    private void Start()
    {
        if (setupOnStart)
        {
            SetupHealthSystem();
        }
    }

    public void SetupHealthSystem()
    {
        // Find or create HealthManager on Player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            // Try to find Player by name
            player = GameObject.Find("Player");
        }

        if (player != null)
        {
            healthManager = player.GetComponent<HealthManager>();
            if (healthManager == null)
            {
                healthManager = player.AddComponent<HealthManager>();
            }
            healthManager.maxHealth = maxHealth;
        }
        else
        {
            // Create HealthManager on this object if no player found
            healthManager = GetComponent<HealthManager>();
            if (healthManager == null)
            {
                healthManager = gameObject.AddComponent<HealthManager>();
            }
            healthManager.maxHealth = maxHealth;
        }

        // HealthManager will automatically create the HealthBar in its Start method
    }
}

