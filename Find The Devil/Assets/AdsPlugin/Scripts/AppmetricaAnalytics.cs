using System.Text;
using GoogleMobileAds.Api;
public static class AppmetricaAnalytics
{
    #region PaidEvent

    public static void ReportRevenue_Admob(AdValue admobAd, AdFormat format, string adUnit, string PlacementName = null)
    {
        if (admobAd == null) return;
        double revenue = (admobAd.Value / 1000000f);

        YandexAppMetricaAdRevenue rev = new YandexAppMetricaAdRevenue(revenue, "USD");
        rev.AdType = GetAdType(format);
        rev.AdUnitId = adUnit;
        rev.AdNetwork = "Admob_Native";
        if (!string.IsNullOrEmpty(PlacementName) && isValidPlacementName(format))
            rev.AdPlacementName = PlacementName.ToLower();

        AppMetrica.Instance.ReportAdRevenue(rev);
    }

    public static void ReportRevenue_Applovin(MaxSdkBase.AdInfo maxAd, AdFormat format, string PlacementName = null)
    {
        if (maxAd == null) return;
        double revenue = maxAd.Revenue;

        YandexAppMetricaAdRevenue rev = new YandexAppMetricaAdRevenue(revenue, "USD");
        rev.AdType = GetAdType(format);
        rev.AdNetwork = maxAd.NetworkName;
        rev.AdUnitId = maxAd.AdUnitIdentifier;
        if (!string.IsNullOrEmpty(PlacementName) && isValidPlacementName(format))
            rev.AdPlacementName = PlacementName.ToLower();

        AppMetrica.Instance.ReportAdRevenue(rev);
    }

    static YandexAppMetricaAdRevenue.AdTypeEnum GetAdType(AdFormat format)
    {
        switch (format)
        {
            case AdFormat.Banner: return YandexAppMetricaAdRevenue.AdTypeEnum.Banner;
            case AdFormat.Interstitial: return YandexAppMetricaAdRevenue.AdTypeEnum.Interstitial;
            case AdFormat.Rewarded: return YandexAppMetricaAdRevenue.AdTypeEnum.Rewarded;
            case AdFormat.AppOpen: return YandexAppMetricaAdRevenue.AdTypeEnum.Other;
            case AdFormat.MREC: return YandexAppMetricaAdRevenue.AdTypeEnum.Mrec;
            case AdFormat.NativeAd: return YandexAppMetricaAdRevenue.AdTypeEnum.Native;
            default: return YandexAppMetricaAdRevenue.AdTypeEnum.Other;
        }
    }

    static bool isValidPlacementName(AdFormat format)
    {
        return (format == AdFormat.Interstitial || format == AdFormat.Rewarded);
    }

    #endregion

    #region CustomEvents
    /// <summary>
    /// Do not use more than 5 elements in params as appmetrica supports only 5 payloads.
    /// </summary>
    public static void ReportCustomEvent(AnalyticsType type, params string[] data)
    {
        if (!data.IsNullOrEmpty())
            AppMetrica.Instance.ReportEvent(type.ToString(), GetJsonFromParams(data));
    }

    static bool IsNullOrEmpty(this string[] value)
    {
        if (value != null)
        {
            return value.Length == 0;
        }
        return true;
    }

    static StringBuilder stringBuilder = new StringBuilder();
    static string GetJsonFromParams(params string[] data)
    {
        stringBuilder.Clear();
        int dataLength = data.Length;
        for (int i = 0; i < dataLength; i++)
        {
            if (i == dataLength - 1)
            {
                stringBuilder.Append($"\"{data[i]}\"");
                break;
            }
            stringBuilder.Append($"{{\"{data[i]}\":");
        }
        stringBuilder.Append('}', data.Length - 1);
        return stringBuilder.ToString();
    }
    #endregion

    public enum AdFormat { Banner, MREC, Interstitial, Rewarded, AppOpen, NativeAd }
}
public enum AnalyticsType { Extras, GameData, AdsData }