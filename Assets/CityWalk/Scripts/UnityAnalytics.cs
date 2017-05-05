using System;
using UnityEngine;
using System.Collections.Generic;
using AmberGarage.Trajen;
using Steamworks;
using UnityEngine.Analytics;

public static class UnityAnalytics {
    
    public static void Teleport(Vector3 target)
    {
        locationEvent("teleport", target);
    }

    public static void TakePicture(Vector3 position) {
        // TODO: Record target building
        locationEvent("take_picture", position);
    }

    public static void TimeOfDay(long timestamp, float hour) {
        customEvent("time_of_day", new Dictionary<string, object>{
                {"timestamp", timestamp},
                {"hour", hour}
        });
    }

    public static void Zoom(Vector3 position, float targetScale) {
        locationEvent("zoom", position, new Dictionary<string, object>{
            { "scale", targetScale }
        });
    }

    public static void Sunrise() {
        customEvent("sunrise", new Dictionary<string, object>());
    }

    private static void locationEvent(string eventName, Vector3? positionInGame, IDictionary<string, object> customData = null) {
        if (customData == null) {
            customData = new Dictionary<string, object>();
        }

        if (positionInGame.HasValue) {
            customData.Add("x", positionInGame.Value.x);
            customData.Add("y", positionInGame.Value.y);
            customData.Add("z", positionInGame.Value.z);

            if ((MapModelSwitcher.Instance != null) &&
                (MapModelSwitcher.Instance.currentMapModel != null)) {
                try {
                    MapModel mapModel = MapModelSwitcher.Instance.currentMapModel;
                    Vector3 eun =
                        MapModelSwitcher.Instance.currentMapModel.transform.InverseTransformPoint(positionInGame.Value);

                    double latitude = 0;
                    double longitude = 0;
                    double altitude = 0;
                    GeodeticConverter.enu_to_geodetic(
                        eun.x,
                        eun.z,
                        eun.y,
                        mapModel.latitude,
                        mapModel.longitude,
                        mapModel.altitude,
                        out latitude,
                        out longitude,
                        out altitude
                        );

                    customData.Add("lat", latitude);
                    customData.Add("long", longitude);
                    customData.Add("alt", altitude);
                } catch (Exception) {
                }
            }
        }

        customEvent(eventName, customData);
    }

    private static void setUserId(string userId) {
        Analytics.SetUserId(userId);
    }

    private static void customEvent(string customEventName, IDictionary<string, object> customData) {
        uint userId = 0;
        try {
            userId = SteamUser.GetSteamID().GetAccountID().m_AccountID;
            setUserId(userId.ToString());
            customData.Add("user_id", userId);
        } catch (Exception) {
        }

        AnalyticsResult result = Analytics.CustomEvent(customEventName, customData);
        if (result != AnalyticsResult.Ok) {
            Debug.LogError("Analytics failed. " + result);
        }

#if UNITY_EDITOR
        Debug.Log("Analytics: " + customEventName + ", " + customData);
#endif
    }
}
