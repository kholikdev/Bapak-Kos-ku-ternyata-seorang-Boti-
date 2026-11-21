using UnityEngine;
using UnityEngine.Playables;
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
    public PlayableDirector cutsceneTimeline;
    public FirstPersonController playerController;
    public PhysicsGrabber physicsGrabber;

    [Header("Audio Settings")]
    public AudioSource endingAudioSource;
    public AudioClip goodEndingSound;
    public AudioClip badEndingSound;
    public AudioClip secretEndingSound;

    void Start()
    {
        if (endingAudioSource == null)
        {
            endingAudioSource = GetComponent<AudioSource>();
            if (endingAudioSource == null)
            {
                endingAudioSource = gameObject.AddComponent<AudioSource>();
                endingAudioSource.playOnAwake = false;
                endingAudioSource.spatialBlend = 0f;
            }
        }
        if (endingAudioSource != null)
        {
            endingAudioSource.ignoreListenerPause = true;
        }
    }

    public void StartCutscene()
    {
        // Ganti BGM ke musik mencekam
        if (AudioManager.Instance != null)
            AudioManager.Instance.SwitchToTenseBGM();

        // lepas barang dan kunci player
        if (physicsGrabber != null)
        {
            physicsGrabber.ReleaseObject();
            physicsGrabber.enabled = false;
        }
        if (playerController != null) playerController.enabled = false;

        // putar cutscene
        if (cutsceneTimeline != null)
            cutsceneTimeline.Play();
        else
            EvaluateEnding();
    }

    public void EvaluateEnding()
    {
        // Hentikan BGM saat panel ending tampil
        if (AudioManager.Instance != null)
            AudioManager.Instance.StopBGM();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        GameObject[] allItems = GameObject.FindGameObjectsWithTag("MessyItem");
        int totalItemsInGame = allItems.Length;

        int itemsStoredInCabinet = 0;
        int itemsMessyInRoom = 0;
        int itemsOutsideRoom = 0;

        bool isCabinetClosed = AreCabinetDoorsClosed();

        // hitung posisi barang
        foreach (GameObject item in allItems)
        {
            Vector3 itemPos = item.transform.position;

            bool isInCabinet = cabinetZoneCollider != null && cabinetZoneCollider.bounds.Contains(itemPos);
            bool isInRoom = roomZoneCollider != null && roomZoneCollider.bounds.Contains(itemPos);

            if (isInCabinet && isCabinetClosed)
                itemsStoredInCabinet++;
            else if (isInRoom)
                itemsMessyInRoom++;
            else
                itemsOutsideRoom++;
        }

        endingPanel.SetActive(true);

        AudioClip stingerToPlay = null;

        // tentukan ending
        if (totalItemsInGame > 0 && itemsOutsideRoom == totalItemsInGame)
        {
            endingTitleText.text = "<color=yellow>SECRET ENDING</color>";
            endingDescText.text = "\"Ini kamar kos? Kok kosong melompong begini? Barang-barangnya kemana?!\"";
            stingerToPlay = secretEndingSound;
        }
        else if (itemsMessyInRoom <= 2)
        {
            endingTitleText.text = "<color=green>GOOD ENDING</color>";
            endingDescText.text = $"Sisa barang berantakan: {itemsMessyInRoom} item.\n\n\"Nah, rapi begini kan enak dilihat. Cepet mandi, nanti ku-kasih hadiah spesial.\"";
            stingerToPlay = goodEndingSound;
        }
        else
        {
            endingTitleText.text = "<color=red>BAD ENDING</color>";
            endingDescText.text = $"Sisa barang berantakan: {itemsMessyInRoom} item.\n\n\"Kamar berantakan seperti kapal pecah begini! Besok kamu harus pindah dari sini!\"";
            stingerToPlay = badEndingSound;
        }

        if (stingerToPlay != null)
        {
            if (endingAudioSource != null)
            {
                endingAudioSource.PlayOneShot(stingerToPlay, 1f);
            }
            else if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(stingerToPlay, 1f);
            }
        }

        Time.timeScale = 0f;
    }

    private bool AreCabinetDoorsClosed()
    {
        if (cabinetDoors == null || cabinetDoors.Length == 0) return true;

        foreach (CabinetDoor door in cabinetDoors)
        {
            if (door != null && !door.IsClosed()) return false;
        }
        return true;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
