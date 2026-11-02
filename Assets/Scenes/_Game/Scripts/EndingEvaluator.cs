using UnityEngine;
using UnityEngine.Playables; // PENTING: Untuk panggil Timeline
using UnityEngine.SceneManagement;
using TMPro;

public class EndingEvaluator : MonoBehaviour
{
    [Header("Zone Colliders")]
    public BoxCollider roomZoneCollider;
    public BoxCollider cabinetZoneCollider;

    [Header("Cabinet Doors Reference")]
    public CabinetDoor[] cabinetDoors;

    [Header("UI Ending References")]
    public GameObject endingPanel;
    public TextMeshProUGUI endingTitleText;
    public TextMeshProUGUI endingDescText;

    [Header("Timeline Reference")]
    public PlayableDirector cutsceneTimeline; // Drag GameObject CutsceneTimeline ke sini
    public FirstPersonController playerController;
    public PhysicsGrabber physicsGrabber;

    // Dipanggil dari InspectionManager pas timer habis
    public void StartCutscene()
    {
        // Matikan kontrol player & lepas barang
        if (physicsGrabber != null)
        {
            physicsGrabber.ReleaseObject();
            physicsGrabber.enabled = false;
        }
        if (playerController != null) playerController.enabled = false;

        // Jalankan Timeline!
        if (cutsceneTimeline != null)
        {
            cutsceneTimeline.Play();
        }
        else
        {
            // Fallback jika tidak ada timeline
            EvaluateEnding();
        }
    }

    // Dipanggil otomatis oleh Signal Emitter di akhir Timeline!
    public void EvaluateEnding()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        GameObject[] allItems = GameObject.FindGameObjectsWithTag("MessyItem");
        int totalItemsInGame = allItems.Length;

        int itemsStoredInCabinet = 0;
        int itemsMessyInRoom = 0;
        int itemsOutsideRoom = 0;

        bool isCabinetClosed = AreCabinetDoorsClosed();

        foreach (GameObject item in allItems)
        {
            Vector3 itemPos = item.transform.position;

            bool isInCabinet = cabinetZoneCollider != null && cabinetZoneCollider.bounds.Contains(itemPos);
            bool isInRoom = roomZoneCollider != null && roomZoneCollider.bounds.Contains(itemPos);

            if (isInCabinet && isCabinetClosed)
            {
                itemsStoredInCabinet++;
            }
            else if (isInRoom)
            {
                itemsMessyInRoom++;
            }
            else
            {
                itemsOutsideRoom++;
            }
        }

        endingPanel.SetActive(true);

        if (totalItemsInGame > 0 && itemsOutsideRoom == totalItemsInGame)
        {
            endingTitleText.text = "<color=yellow>SECRET ENDING</color>";
            endingDescText.text = "\"Ini kamar kos atau lapangan? Kok kosong melompong begini? Kasur kamu mana?!\"";
        }
        else if (itemsMessyInRoom <= 1)
        {
            endingTitleText.text = "<color=green>GOOD ENDING</color>";
            endingDescText.text = $"Sisa barang berantakan: {itemsMessyInRoom} item.\n\n\"Nah, rapi begini kan enak dilihat. Bulan depan Ibu potong uang sewanya ya.\"";
        }
        else
        {
            endingTitleText.text = "<color=red>BAD ENDING</color>";
            endingDescText.text = $"Sisa barang berantakan: {itemsMessyInRoom} item.\n\n\"Kamar berantakan seperti kapal pecah begini! Besok kamu harus pindah dari sini!\"";
        }

        Time.timeScale = 0f;
    }

    private bool AreCabinetDoorsClosed()
    {
        if (cabinetDoors == null || cabinetDoors.Length == 0) return true;

        foreach (CabinetDoor door in cabinetDoors)
        {
            if (door != null && !door.IsClosed())
            {
                return false;
            }
        }
        return true;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}