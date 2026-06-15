using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public float MasterVolume { get; private set; } = 1f;
    public float MusicVolume  { get; private set; } = 0.75f;
    public float SFXVolume    { get; private set; } = 1f;

    private const string KEY_MASTER = "Vol_Master";
    private const string KEY_MUSIC  = "Vol_Music";
    private const string KEY_SFX    = "Vol_SFX";

    private AudioSource musicSource;
    private AudioSource sfxSource;

    private AudioClip mainMenuMusic;
    private AudioClip gameplayMusic;
    private AudioClip buttonClickSFX;
    private AudioClip plantSFX;
    private AudioClip harvestSFX;

    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoCreate()
    {
        var go = new GameObject("AudioManager");
        DontDestroyOnLoad(go);
        go.AddComponent<AudioManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource             = gameObject.AddComponent<AudioSource>();
        musicSource.loop        = true;
        musicSource.playOnAwake = false;

        sfxSource             = gameObject.AddComponent<AudioSource>();
        sfxSource.loop        = false;
        sfxSource.playOnAwake = false;

        LoadAll();
        LoadClips();

        SceneManager.sceneLoaded += OnSceneLoaded;
        
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (Instance == this) Instance = null;
    }

    private void LoadClips()
    {
        mainMenuMusic  = Resources.Load<AudioClip>("Audio/Music/MainMenuMusic");
        gameplayMusic  = Resources.Load<AudioClip>("Audio/Music/GameplayMusic");
        buttonClickSFX = Resources.Load<AudioClip>("Audio/SFX/ButtonClick");
        plantSFX       = Resources.Load<AudioClip>("Audio/SFX/Plant");
        harvestSFX     = Resources.Load<AudioClip>("Audio/SFX/Harvest");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AudioClip clip = scene.name == "MainMenu" ? mainMenuMusic : gameplayMusic;
        PlayMusic(clip);
    }

    private void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return;
        musicSource.clip = clip;
        musicSource.Play();
    }

    public void SetMasterVolume(float v)
    {
        MasterVolume         = v;
        AudioListener.volume = v;
        PlayerPrefs.SetFloat(KEY_MASTER, v);
    }

    public void SetMusicVolume(float v)
    {
        MusicVolume          = v;
        musicSource.volume   = v;
        PlayerPrefs.SetFloat(KEY_MUSIC, v);
    }

    public void SetSFXVolume(float v)
    {
        SFXVolume          = v;
        sfxSource.volume   = v;
        PlayerPrefs.SetFloat(KEY_SFX, v);
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    public void PlayButtonClick() => PlaySFX(buttonClickSFX);
    public void PlayPlant()       => PlaySFX(plantSFX);
    public void PlayHarvest()     => PlaySFX(harvestSFX);

    public void SaveAll() => PlayerPrefs.Save();

    private void LoadAll()
    {
        MasterVolume         = PlayerPrefs.GetFloat(KEY_MASTER, 1f);
        MusicVolume          = PlayerPrefs.GetFloat(KEY_MUSIC,  0.75f);
        SFXVolume            = PlayerPrefs.GetFloat(KEY_SFX,    1f);
        AudioListener.volume = MasterVolume;
        musicSource.volume   = MusicVolume;
        sfxSource.volume     = SFXVolume;
    }
}
