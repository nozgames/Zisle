using System;
using UnityEngine;

namespace Survival
{
    [CreateAssetMenu(fileName = "dayNightCycle", menuName = "Survival/DayNightCycle")]
    public class DayNightCycle : ScriptableObject
    {
        private const float MainLightAngleStart = -15.0f;
        private const float MainLightAngleEnd = 195.0f;
        private const float MaxSunSize = 0.5f;

        public struct TimeOfDay
        {
            public float normalizedTime;
            public Color fogColor;
            public Color horizonColor;
            public Color skyColor;
            public Color starColor;
            public Color ambientLightColor;
            public Color sunColor;
            public Color mainLightColor;
            public float sunSize;
            public float mainLightAngle;
            public float mainLightShadowStrength;
        }

        [Header("General")]
        [Tooltip("Duration of the entire cycle in seconds")]
        [SerializeField] private float _duration = 60.0f;
        [SerializeField] Material _skyboxMaterial = null;

        [Header("Day")]
        [SerializeField] private float _dayDurationRatio = 1.0f;
        [SerializeField] private Color _dayFogColor = Color.white;
        [SerializeField] [ColorUsage(showAlpha: false)] private Color _dayHorizonColor = Color.white;
        [SerializeField] [ColorUsage(showAlpha: false)] private Color _daySkyColor = Color.white;
        [SerializeField] [ColorUsage(showAlpha: false)] private Color _dayAmbientColor = Color.white;
        [SerializeField] [ColorUsage(showAlpha: false)] private Color _dayLightColor = Color.white;
        [SerializeField] [ColorUsage(showAlpha: true, hdr: true)] private Color _daySunColor = Color.white;
        [SerializeField] [Range(0, MaxSunSize)] private float _daySunSize = 0.2f;
        [SerializeField] [Range(0, 1.0f)] private float _dayShadowStrength = 0.2f;

        [Header("Sunset")]
        [SerializeField] private float _sunsetDurationRatio = 1.0f;
        [SerializeField] private Color _sunsetFogColor = Color.white;
        [SerializeField] [ColorUsage(showAlpha: false)] private Color _sunsetHorizonColor = Color.white;
        [SerializeField] [ColorUsage(showAlpha: false)] private Color _sunsetSkyColor = Color.white;
        [SerializeField] [ColorUsage(showAlpha: false)] private Color _sunsetAmbientColor = Color.white;
        [SerializeField] [ColorUsage(showAlpha: false)] private Color _sunsetLightColor = Color.white;
        [SerializeField] [ColorUsage(showAlpha: true, hdr: true)] private Color _sunsetSunColor = Color.white;
        [SerializeField] [Range(0, MaxSunSize)] private float _sunsetSunSize = 0.2f;
        [SerializeField] [Range(0, 1.0f)] private float _sunsetShadowStrength = 0.2f;

        [Header("Night")]
        [SerializeField] private float _nightDurationRatio = 1.0f;
        [SerializeField] private Color _nightFogColor = Color.white;
        [SerializeField] [ColorUsage(showAlpha: false)] private Color _nightHorizonColor = Color.white;
        [SerializeField] [ColorUsage(showAlpha: false)] private Color _nightSkyColor = Color.white;
        [SerializeField] [ColorUsage(showAlpha: false)] private Color _nightAmbientColor = Color.white;
        [SerializeField] [ColorUsage(showAlpha: false)] private Color _nightLightColor = Color.white;
        [SerializeField] [ColorUsage(showAlpha: true, hdr: true)] private Color _nightStarColor = Color.white;
        [SerializeField] [ColorUsage(showAlpha: true, hdr: true)] private Color _nightMoonColor = Color.white;
        [SerializeField] [Range(0, MaxSunSize)] private float _nightMoonSize = 0.2f;
        [SerializeField] [Range(0, 1.0f)] private float _nightShadowStrength = 0.2f;

        [Header("Sunrise")]
        [SerializeField] private float _sunriseDurationRatio = 1.0f;
        [SerializeField] private Color _sunriseFogColor = Color.white;
        [SerializeField] [ColorUsage(showAlpha: false)] private Color _sunriseHorizonColor = Color.white;
        [SerializeField] [ColorUsage(showAlpha: false)] private Color _sunriseSkyColor = Color.white;
        [SerializeField] [ColorUsage(showAlpha: false)] private Color _sunriseAmbientColor = Color.white;
        [SerializeField] [ColorUsage(showAlpha: false)] private Color _sunriseLightColor = Color.white;
        [SerializeField] [ColorUsage(showAlpha:true, hdr:true)] private Color _sunriseSunColor = Color.white;
        [SerializeField] [Range(0, MaxSunSize)] private float _sunriseSunSize = 0.2f;
        [SerializeField] [Range(0, 1.0f)] private float _sunriseShadowStrength = 0.2f;

        private float _totalDurationRatio;
        private float _dayStart;
        private float _nightStart;
        private float _sunriseStart;
        private float _sunsetStart;
        private float _sunAngleStart;
        private float _sunAngleDuration;

        public Material skyboxMaterial => _skyboxMaterial;
        public float duration => _duration;

        private void OnEnable() => PreCalculate();

#if UNITY_EDITOR
        private void OnValidate()
        {
            PreCalculate();
            var timeManager = FindObjectOfType<TimeManager>();
            if(null != timeManager)
                timeManager.OnValidate();
        }
#endif

        private void PreCalculate()
        {
            _totalDurationRatio = _dayDurationRatio + _nightDurationRatio + _sunriseDurationRatio + _sunsetDurationRatio;
            _dayStart = 0.0f;
            _sunsetStart = _dayDurationRatio;
            _nightStart = (_dayDurationRatio + _sunsetDurationRatio);
            _sunriseStart = (_dayDurationRatio + _sunsetDurationRatio + _nightDurationRatio);
            _sunAngleStart = _dayStart - (_sunriseDurationRatio);
            _sunAngleDuration = _dayDurationRatio + (_sunriseDurationRatio) + (_sunsetDurationRatio);
        }

        private float GetMainLightDarkenByAngle (float mainLightAngle)
        {
            if (mainLightAngle < 0)
                return 1.0f - (mainLightAngle / MainLightAngleStart);

            if (mainLightAngle > 180)
                return ((MainLightAngleEnd - mainLightAngle) / (MainLightAngleEnd - 180));

            return 1.0f;
        }

        public TimeOfDay CalculateTimeOfDay (float normalizedTime)
        {
            var timeRatio = _totalDurationRatio * normalizedTime;
            var timeOfDay = new TimeOfDay { normalizedTime = normalizedTime };            

            // Day
            if (timeRatio < _sunsetStart)
            {
                var t = (timeRatio - _dayStart) / _dayDurationRatio;
                var tt = (timeRatio - _sunAngleStart) / _sunAngleDuration;
                timeOfDay.fogColor = _dayFogColor;
                timeOfDay.horizonColor = _dayHorizonColor;
                timeOfDay.skyColor = _daySkyColor;
                timeOfDay.starColor = new Color(0, 0, 0, 0);
                timeOfDay.sunColor = _daySunColor;
                timeOfDay.sunSize = tt > 0.5f ? Mathf.Lerp(_daySunSize, _sunsetSunSize, (tt - 0.5f) / 0.5f) : Mathf.Lerp(_sunriseSunSize, _daySunSize, tt / 0.5f);
                timeOfDay.ambientLightColor = _dayAmbientColor;
                timeOfDay.mainLightAngle = Mathf.Lerp(MainLightAngleStart, MainLightAngleEnd, tt);
                timeOfDay.mainLightColor = _dayLightColor;
                timeOfDay.mainLightShadowStrength = _dayShadowStrength;
            }
            // Sunset
            else if (timeRatio < _nightStart)
            {
                var t = (timeRatio - _sunsetStart) / _sunsetDurationRatio;
                var tt = Mathf.Clamp01((timeRatio - _sunAngleStart) / _sunAngleDuration);
                timeOfDay.mainLightAngle = Mathf.Lerp(MainLightAngleStart, MainLightAngleEnd, tt);
                timeOfDay.sunSize = Mathf.Lerp(_daySunSize, _sunsetSunSize, (tt - 0.5f) / 0.5f);

                if (t < 0.5f)
                {
                    t /= 0.5f;
                    timeOfDay.fogColor = Color.Lerp(_dayFogColor, _sunsetFogColor, t);
                    timeOfDay.horizonColor = Color.Lerp(_dayHorizonColor, _sunsetHorizonColor, t);
                    timeOfDay.skyColor = Color.Lerp(_daySkyColor, _sunsetSkyColor, t);
                    timeOfDay.starColor = new Color(0, 0, 0, 0);
                    timeOfDay.ambientLightColor = Color.Lerp(_dayAmbientColor, _sunsetAmbientColor, t);
                    timeOfDay.sunColor = Color.Lerp(_daySunColor, _sunsetSunColor, t);
                    timeOfDay.mainLightColor = Color.Lerp(_dayLightColor, _sunsetLightColor, t);
                    timeOfDay.mainLightShadowStrength = Mathf.Lerp(_dayShadowStrength, _sunsetShadowStrength, t);
                }
                else
                {
                    t = (t - 0.5f) / 0.5f;
                    timeOfDay.fogColor = Color.Lerp(_sunsetFogColor, _nightFogColor, t);
                    timeOfDay.horizonColor = Color.Lerp(_sunsetHorizonColor, _nightHorizonColor, t);
                    timeOfDay.skyColor = Color.Lerp(_sunsetSkyColor, _nightSkyColor, t);
                    timeOfDay.starColor = Color.Lerp(new Color(0, 0, 0, 0), _nightStarColor, t);
                    timeOfDay.ambientLightColor = Color.Lerp(_sunsetAmbientColor, _nightAmbientColor, t);
                    timeOfDay.sunColor = _sunsetSunColor;
                    timeOfDay.mainLightColor = Color.Lerp(_sunsetLightColor, _nightLightColor, t);
                    timeOfDay.mainLightShadowStrength = Mathf.Lerp(_sunsetShadowStrength, _nightShadowStrength, t);
                }
            }
            // Night
            else if (timeRatio < _sunriseStart)
            {
                var t = (timeRatio - _nightStart) / _nightDurationRatio;

                timeOfDay.fogColor = _nightFogColor;
                timeOfDay.horizonColor = _nightHorizonColor;
                timeOfDay.skyColor = _nightSkyColor;
                timeOfDay.starColor = _nightStarColor;
                timeOfDay.ambientLightColor = _nightAmbientColor;

                timeOfDay.mainLightColor = _nightLightColor;
                timeOfDay.mainLightAngle = Mathf.Lerp(MainLightAngleStart, MainLightAngleEnd, t);
                timeOfDay.sunColor = _nightMoonColor;
                timeOfDay.sunSize = _nightMoonSize;
                timeOfDay.mainLightShadowStrength = _nightShadowStrength;
            }
            // Sunrise
            else
            {
                var t = (timeRatio - _sunriseStart) / _sunriseDurationRatio;
                var tt = Mathf.Clamp01(((timeRatio - _sunriseStart)) / _sunAngleDuration);
                timeOfDay.sunSize = Mathf.Lerp(_sunriseSunSize, _daySunSize, tt / 0.5f);
                timeOfDay.mainLightAngle = Mathf.Lerp(MainLightAngleStart,MainLightAngleEnd,tt);

                if (t < 0.5f)
                {
                    t /= 0.5f;
                    timeOfDay.fogColor = Color.Lerp(_nightFogColor, _sunriseFogColor, t);
                    timeOfDay.horizonColor = Color.Lerp(_nightHorizonColor, _sunriseHorizonColor, t);
                    timeOfDay.skyColor = Color.Lerp(_nightSkyColor, _sunriseSkyColor, t);
                    timeOfDay.starColor = Color.Lerp(_nightStarColor, new Color(0, 0, 0, 0), t);
                    timeOfDay.ambientLightColor = Color.Lerp(_nightAmbientColor, _sunriseAmbientColor, t);
                    timeOfDay.mainLightColor = Color.Lerp(_nightLightColor, _sunriseLightColor, t);
                    timeOfDay.sunColor = _sunriseSunColor;
                    timeOfDay.mainLightShadowStrength = Mathf.Lerp(_nightShadowStrength, _sunriseShadowStrength, t);
                }
                else
                {
                    t = (t - 0.5f) / 0.5f;
                    timeOfDay.fogColor = Color.Lerp(_sunriseFogColor, _dayFogColor, t);
                    timeOfDay.horizonColor = Color.Lerp(_sunriseHorizonColor, _dayHorizonColor, t);
                    timeOfDay.skyColor = Color.Lerp(_sunriseSkyColor, _daySkyColor, t);
                    timeOfDay.ambientLightColor = Color.Lerp(_sunriseAmbientColor, _dayAmbientColor, t);
                    timeOfDay.sunColor = Color.Lerp(_sunriseSunColor, _daySunColor, t);
                    timeOfDay.mainLightColor = Color.Lerp(_sunriseLightColor, _dayLightColor, t);
                    timeOfDay.mainLightShadowStrength = Mathf.Lerp(_sunsetShadowStrength, _dayShadowStrength, t);
                }
            }

            timeOfDay.mainLightColor *= GetMainLightDarkenByAngle(timeOfDay.mainLightAngle);

            return timeOfDay;
        }
    }
}

