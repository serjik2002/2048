using UnityEngine;
using GoogleMobileAds.Api;
using System;

public class AdManager : MonoBehaviour
{
    public static AdManager Instance { get; private set; }

    private bool _isInitialized = false;

    [Header("Ad Unit IDs")]
    [SerializeField] private string _interstitialAdId = "ca-app-pub-8968740975401720/1778481179";
    [SerializeField] private string _bannerAdId = "ca-app-pub-8968740975401720/9649749253";
    [SerializeField] private string _rewardedAdId = "ca-app-pub-8968740975401720/8636037480";


    private InterstitialAd _interstitialAd;
    private BannerView _bannerView;
    private RewardedAd _rewardedAd;


    [SerializeField] private float adFreeDurationMinutes = 3f;
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
        LoadBannerAd();
        LoadRewardedAd(); // --- Загружаем рекламу с вознаграждением ---
    }

    private void OnDestroy()
    {
        // Очистка Баннера
        if (_bannerView != null)
        {
            _bannerView.Destroy();
            _bannerView = null;
        }

        // Очистка Interstitial
        if (_interstitialAd != null)
        {
            _interstitialAd.Destroy();
            _interstitialAd = null;
        }

        // --- Очистка Rewarded ---
        if (_rewardedAd != null)
        {
            _rewardedAd.Destroy();
            _rewardedAd = null;
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
        if (_interstitialAd != null)
        {
            _interstitialAd.Destroy();
            _interstitialAd = null;
        }

        var request = new AdRequest();
        InterstitialAd.Load(_interstitialAdId, request, (ad, error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("Ошибка загрузки межстраничной рекламы: " + error);
                Invoke(nameof(LoadInterstitialAd), 10f); // Повторная попытка
                return;
            }

            _interstitialAd = ad;
            _interstitialAd.OnAdFullScreenContentClosed += () => LoadInterstitialAd();
        });
    }

    public void ShowInterstitialAd()
    {
        if (_interstitialAd != null && _interstitialAd.CanShowAd())
        {
            if (Time.time - startTime >= adFreeDurationSeconds)
                _interstitialAd.Show();
        }
        else
        {
            Debug.Log("Межстраничная реклама не готова.");
        }
    }

    #endregion

    #region Banner Ad Logic

    public void LoadBannerAd()
    {
        if (_bannerView != null)
        {
            _bannerView.Destroy();
            _bannerView = null; // Хорошая практика обнулять ссылку
        }

        _bannerView = new BannerView(_bannerAdId, AdSize.Banner, AdPosition.Bottom);
        var request = new AdRequest();
        _bannerView.LoadAd(request);
    }

    public void HideBannerAd()
    {
        if (_bannerView != null) _bannerView.Hide();
    }

    public void ShowBannerAd()
    {
        if (_bannerView != null) _bannerView.Show();
    }

    #endregion

    // --- ДОБАВЛЕНО: Логика Rewarded Ad ---
    #region Rewarded Ad Logic

    public void LoadRewardedAd()
    {
        // Очищаем старую рекламу перед загрузкой новой
        if (_rewardedAd != null)
        {
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }

        Debug.Log("Загрузка Rewarded рекламы...");

        var request = new AdRequest();

        RewardedAd.Load(_rewardedAdId, request, (ad, error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("Ошибка загрузки Rewarded рекламы: " + error);
                // Можно попробовать перезагрузить через время, если нужно, но осторожно с циклами
                return;
            }

            Debug.Log("Rewarded реклама загружена!");
            _rewardedAd = ad;

            // Регистрируем обработчики событий (например, чтобы загрузить новую после закрытия)
            RegisterReloadHandler(_rewardedAd);
        });
    }

    private void RegisterReloadHandler(RewardedAd ad)
    {
        // Вызывается, когда реклама закрывается (посмотрели или нет)
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Rewarded реклама закрыта. Загружаем следующую.");
            LoadRewardedAd();
        };

        // Вызывается, если показать не удалось
        ad.OnAdFullScreenContentFailed += (adError) =>
        {
            Debug.LogError("Ошибка показа Rewarded рекламы: " + adError);
            LoadRewardedAd();
        };
    }

    /// <summary>
    /// Метод показа рекламы. Принимает функцию (callback), которая выполнится при успешном просмотре.
    /// </summary>
    /// <param name="onRewardEarned">Действие, которое нужно выполнить (дать монеты, жизнь и т.д.)</param>
    public void ShowRewardedAd(Action<Reward> onRewardEarned)
    {
        if (_rewardedAd != null && _rewardedAd.CanShowAd())
        {
            _rewardedAd.Show((Reward reward) =>
            {
                // Пользователь досмотрел рекламу до конца
                Debug.Log($"Награда получена! Тип: {reward.Type}, кол-во: {reward.Amount}");

                // Выполняем действие, переданное из другого скрипта
                onRewardEarned?.Invoke(reward);
            });
        }
        else
        {
            Debug.Log("Rewarded реклама еще не готова.");
            // Опционально: Можно показать сообщение пользователю "Реклама загружается..."
        }
    }

    #endregion
}