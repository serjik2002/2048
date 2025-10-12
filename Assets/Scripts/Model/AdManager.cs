using UnityEngine;
using GoogleMobileAds.Api;
using System;

public class AdManager : MonoBehaviour
{
    public static AdManager Instance { get; private set; }

    private bool _isInitialized = false;

    // --- Interstitial Ad ---
    private InterstitialAd _interstitialAd;
    private const string InterstitialAdUnitId = "ca-app-pub-8968740975401720/1778481179"; // тестовый

    // --- ДОБАВЛЕНО ДЛЯ БАННЕРА ---
    private BannerView _bannerView;
    private const string BannerAdUnitId = "ca-app-pub-8968740975401720/9649749253";


    [SerializeField] private float adFreeDurationMinutes = 5f;
    private float adFreeDurationSeconds;
    private float startTime;

    private void Awake()
    {
        InititalizeSingleton();
    }

    private void InititalizeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            startTime = Time.time;
            adFreeDurationSeconds = adFreeDurationMinutes * 60f;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeAds();
        LoadInterstitialAd();
        LoadBannerAd(); // --- ДОБАВЛЕНО ДЛЯ БАННЕРА ---
    }

    // --- ДОБАВЛЕНО ДЛЯ БАННЕРА ---
    // Важно очищать ресурсы, когда объект уничтожается
    private void OnDestroy()
    {
        if (_bannerView != null)
        {
            _bannerView.Destroy();
        }
    }

    private void InitializeAds()
    {
        if (_isInitialized) return;
        MobileAds.Initialize(initStatus =>
        {
            Debug.Log("Google Mobile Ads SDK initialized.");
            _isInitialized = true;
        });
    }

    #region Interstitial Ad Logic

    private void LoadInterstitialAd()
    {
        var request = new AdRequest();
        InterstitialAd.Load(InterstitialAdUnitId, request, (ad, error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("Ошибка загрузки межстраничной рекламы: " + error);
                // Повторная попытка загрузки через 10 секунд
                Invoke(nameof(LoadInterstitialAd), 10f);
                return;
            }

            _interstitialAd = ad;
            // Подписываемся на событие закрытия для автоперезагрузки
            _interstitialAd.OnAdFullScreenContentClosed += () => LoadInterstitialAd();
        });
    }

    public void ShowInterstitialAd()
    {
        if (_interstitialAd != null && _interstitialAd.CanShowAd())
        {
            if(Time.time - startTime >= adFreeDurationSeconds)
                _interstitialAd.Show();
        }
        else
        {
            Debug.Log("Межстраничная реклама не готова.");
        }
    }

    #endregion

    // --- ДОБАВЛЕНО ДЛЯ БАННЕРА ---
    #region Banner Ad Logic

    public void LoadBannerAd()
    {
        // Если баннер уже существует, уничтожаем его, чтобы создать новый
        if (_bannerView != null)
        {
            _bannerView.Destroy();
        }

        // Создаем стандартный баннер (320x50) внизу экрана
        _bannerView = new BannerView(BannerAdUnitId, AdSize.Banner, AdPosition.Bottom);

        var request = new AdRequest();

        // Загружаем и показываем баннер
        _bannerView.LoadAd(request);
        Debug.Log("Загрузка баннера...");
    }

    public void HideBannerAd()
    {
        if (_bannerView != null)
        {
            _bannerView.Hide();
        }
    }

    public void ShowBannerAd()
    {
        if (_bannerView != null)
        {
            _bannerView.Show();
        }
    }

    #endregion
}