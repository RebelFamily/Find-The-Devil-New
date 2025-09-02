using UnityEngine;
public class AppmetricaUsage : MonoBehaviour
{
    void Start()
    {
        EventsExample();
    }

    void EventsExample()
    {
        int levelNo = 4;
        AppmetricaAnalytics.ReportCustomEvent(AnalyticsType.GameData, $"Level_{levelNo}", "Start");
        AppmetricaAnalytics.ReportCustomEvent(AnalyticsType.GameData, $"Level_{levelNo}", "Fail");
        AppmetricaAnalytics.ReportCustomEvent(AnalyticsType.GameData, $"Level_{levelNo}", "Complete");

        AppmetricaAnalytics.ReportCustomEvent(AnalyticsType.Extras, "DeviceInfo", "ProcessorCount", SystemInfo.processorCount.ToString());





        //Remember
        //Only five payloads are allowed in custom events
        //that means, params length should not exceed 5, for example
        AppmetricaAnalytics.ReportCustomEvent(AnalyticsType.GameData, "event_1", "event_2", "event_3", "event_4", "event_5"); // Valid Event
        AppmetricaAnalytics.ReportCustomEvent(AnalyticsType.GameData, "event_1", "event_2", "event_3", "event_4", "event_5", "event_6"); // Invalid event
    }
}
