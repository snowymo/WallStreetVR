using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(TOD_Sky), typeof(TOD_Animation))]
[ExecuteInEditMode]
public class TimeOfDayController : MonoBehaviour {

    public Transform particleCity;
    public float TouchPanSpeed = 24f / (360f * 4f); // 4 circles per day

    public float Hour;
    public float WindSpeed;

    public AnimationCurve HeightToWindSpeedCurve;
    public TrackingSpaceController TrackingSpaceController;

    private TOD_Sky sky;
    private TOD_Animation skyAnimation;
    private float targetWindSpeed;
    private float previousHour;

    void Start() {
        sky = GetComponent<TOD_Sky>();
        skyAnimation = GetComponent<TOD_Animation>();
        updateParticleCity();
    }

    void Update() {
        if (!Application.isPlaying || OpeningController.Instance.State == OpeningController.GameState.Opening) {
            sky.Cycle.Hour = Hour;
            skyAnimation.WindSpeed = WindSpeed;
        } else if (OpeningController.Instance.State == OpeningController.GameState.Running) {
            var timeOfDayDelta = InputManager.Instance.GetAxis(InputManager.Axis.TimeOfDayDelta);
            if (timeOfDayDelta.HasValue) {
                DateTime dateTime = sky.Cycle.DateTime;
                float hoursDelta = timeOfDayDelta.Value.x * TouchPanSpeed;
                sky.Cycle.DateTime = dateTime.AddHours(hoursDelta);
                Hour = sky.Cycle.Hour;

                // Wind speed
                targetWindSpeed = hoursDelta * 500;
            } else {
                targetWindSpeed = HeightToWindSpeedCurve.Evaluate(TrackingSpaceController.PlayerHeight);
            }

            // Lerp wind speed
            skyAnimation.WindSpeed = Mathf.Clamp(Mathf.Lerp(skyAnimation.WindSpeed, targetWindSpeed, 0.01f), -100, 100);

            // Send touch-up event
            if (InputManager.Instance.GetButtonUp(InputManager.Button.TimeOfDayTouch)) {
                UnityAnalytics.TimeOfDay(sky.Cycle.Ticks, Hour);
                Debug.Log("Time of day: " + Hour);
            }
        }

        // Update hour-related components
        if (Application.isPlaying && Mathf.Abs(Hour - previousHour) > 1e-6f)
        {
            updateParticleCity();
            SoundManager.Instance.UpdateHourOfDay(Hour);
        }

        previousHour = Hour;
    }

    private void updateParticleCity() {
        var hour = sky.Cycle.Hour;

        if (particleCity != null && particleCity.gameObject.activeSelf) {
            float a = 0;
            if (hour >= 21 && hour <= 24) {
                a = (hour - 21f) / (24f - 21f);
            } else if (hour >= 0 && hour <= 3) {
                a = 1;
            } else if (hour >= 3 && hour <= 5) {
                a = 1 - (hour - 3f) / (5f - 3f);
            } else {
                a = 0;
            }

            var renderers = particleCity.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++) {
                renderers[i].material.SetColor("_SpriteColor", new Color(1, 1, 1, a));
            }
        }
    }
}
