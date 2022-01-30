using UnityEngine;
using NoZ;
using UnityEditor;

namespace Survival
{
    public class TimeManager : Singleton<TimeManager>
    {
        [Header("General")]
        [SerializeField] private DayNightCycle _dayNightCycle = null;

        [Range(0.1f, 10.0f)] [SerializeField] private float _timeScale = 1.0f;

        [Header("Editor")]
        [Range(0,1)] [SerializeField] private float _editorTime = 0.0f;

        private float _time;
        private float _normalizedTime;
        private Material _skyboxMaterial;

        protected override void Awake()
        {
            base.Awake();
            UpdateSkyboxMaterial();
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
        }

        protected override void OnShutdown()
        {
            base.OnShutdown();
        }

        private void UpdateSkyboxMaterial()
        {
            _skyboxMaterial = new Material(_dayNightCycle.skyboxMaterial);
            RenderSettings.skybox = _skyboxMaterial;
        }

        private void LateUpdate() => UpdateTimeOfDay();

        public void UpdateTimeOfDay()
        {
            if (RenderSettings.sun == null)
                return;

            //_time = (NetzTime.serverTime * _timeScale) % _dayNightCycle.duration;

            _normalizedTime = _time / _dayNightCycle.duration;

            var timeOfDay = _dayNightCycle.CalculateTimeOfDay(_normalizedTime);


            var sun = RenderSettings.sun;
            sun.transform.eulerAngles = new Vector3(timeOfDay.mainLightAngle, 0, 0);
            sun.color = timeOfDay.mainLightColor;
            sun.shadowStrength = timeOfDay.mainLightShadowStrength;

            RenderSettings.ambientLight = timeOfDay.ambientLightColor;
            RenderSettings.fogColor = timeOfDay.fogColor;

            _skyboxMaterial.SetColor(ShaderPropertyID.HorizonColor, timeOfDay.horizonColor);
            _skyboxMaterial.SetColor(ShaderPropertyID.SkyColor, timeOfDay.skyColor);
            _skyboxMaterial.SetColor(ShaderPropertyID.FogColor, timeOfDay.fogColor);
            _skyboxMaterial.SetColor(ShaderPropertyID.StarColor, timeOfDay.starColor);
            _skyboxMaterial.SetFloat(ShaderPropertyID.SunSize, timeOfDay.sunSize);
            _skyboxMaterial.SetColor(ShaderPropertyID.SunColor, timeOfDay.sunColor);
        }

#if UNITY_EDITOR
        public void OnValidate()
        {
            if (!EditorApplication.isPlaying)
                return;

            UpdateSkyboxMaterial();
            UpdateTimeOfDay();
        }
#endif
    }
}
