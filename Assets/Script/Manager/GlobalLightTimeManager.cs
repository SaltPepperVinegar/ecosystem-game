using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GlobalLightTimeManager : MonoBehaviour
{
    [Header("Time Settings")]
    [Tooltip("Real-time minutes for a full 24-hour cycle")]
    public float dayLengthInRealMinutes = 6f; 
    
    [Range(0f, 24f)]
    public float timeOfDay; 
    public int daysPassed = 0;

    [Header("Global Lighting")]
    public Light2D globalLight;
    public Gradient ambientColor;
    public AnimationCurve ambientIntensity;


    public static event System.Action<float, float, float> OnSunPositionChanged;

    private void Start()
    {
        timeOfDay = UnityEngine.Random.Range(0f, 24f);
        
        UpdateLighting();
        BroadcastSunData();
    }

    private void Update()
    {
        UpdateTime();
        UpdateLighting();
        BroadcastSunData();
    }

    private void UpdateTime()
    {
        // Calculate time gained this frame (6 mins real time = 24 hrs game time)
        float timeGainedThisFrame = (Time.deltaTime / (dayLengthInRealMinutes * 60f)) * 24f;
        timeOfDay += timeGainedThisFrame;

        // Loop the day
        if (timeOfDay >= 24f)
        {
            timeOfDay %= 24f;
            daysPassed++;
        }
    }

    private void UpdateLighting()
    {
        if (globalLight == null) return;

        float normalizedTime = timeOfDay / 24f;
        globalLight.color = ambientColor.Evaluate(normalizedTime);
        globalLight.intensity = ambientIntensity.Evaluate(normalizedTime);
    }

    private void BroadcastSunData()
    {
        // Assuming daytime is 6:00 AM to 6:00 PM (18:00)
        if (timeOfDay >= 6f && timeOfDay <= 18f)
        {
            float dayProgress = (timeOfDay - 6f) / 12f; // 0 at dawn, 1 at dusk
            
            // Angle from East (0) to West (180)
            float angleDegrees = Mathf.Lerp(0f, 180f, dayProgress);
            
            // Peak intensity at noon (1.0), tapering to dawn/dusk (0.0)
            float timeFromNoon = 1f - (Mathf.Abs(dayProgress - 0.5f) * 2f); 
            
            // Shadows are shortest at noon (0.5 scale), longest at dawn/dusk (1.5 scale)
            float shadowScaleMultiplier = Mathf.Lerp(1.5f, 0.5f, timeFromNoon);

            // Shadows fade in at dawn, peak at noon (0.6 opacity), fade out at dusk
            float shadowAlpha = Mathf.Clamp(timeFromNoon, 0f, 0.6f); 

            OnSunPositionChanged?.Invoke(angleDegrees, shadowScaleMultiplier, shadowAlpha);
        }
        else
        {
            // Night time: tell all shadows to disappear (Alpha = 0)
            OnSunPositionChanged?.Invoke(0f, 1f, 0f); 
        }
    }

    private static Color GetColor(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out Color color))
            return color;
        return Color.white; 
    }


    private void Reset()
    {
        SetupDefaultPresets();
    }

    private void SetupDefaultPresets()
    {
        ambientColor = new Gradient();
        GradientColorKey[] colorKeys = new GradientColorKey[] {
            new GradientColorKey(GetColor("#1A1A3A"), 0.00f), // 1. Midnight
            new GradientColorKey(GetColor("#3A2E4D"), 0.20f), // 2. Pre-Dawn
            new GradientColorKey(GetColor("#FF8C66"), 0.25f), // 3. Sunrise
            new GradientColorKey(GetColor("#FFF4D6"), 0.40f), // 4. Morning (shifted slightly)
            new GradientColorKey(GetColor("#FFFFFF"), 0.50f), // 5. Noon
            new GradientColorKey(GetColor("#FFF4D6"), 0.60f), // 6. Afternoon (shifted slightly)
            new GradientColorKey(GetColor("#FF5E33"), 0.75f), // 7. Sunset
            new GradientColorKey(GetColor("#3A2E4D"), 0.85f)  // 8. Dusk (loops back to Midnight naturally)
        };
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[] {
            new GradientAlphaKey(1.0f, 0.0f),
            new GradientAlphaKey(1.0f, 1.0f)
        };
        ambientColor.SetKeys(colorKeys, alphaKeys);

        Keyframe[] intensityKeys = new Keyframe[] {
            new Keyframe(0.00f, 0.15f),
            new Keyframe(0.25f, 0.40f),
            new Keyframe(0.35f, 0.85f),
            new Keyframe(0.50f, 1.00f),
            new Keyframe(0.65f, 0.85f),
            new Keyframe(0.75f, 0.40f),
            new Keyframe(1.00f, 0.15f)
        };
        ambientIntensity = new AnimationCurve(intensityKeys);
        for (int i = 0; i < ambientIntensity.length; i++)
        {
            ambientIntensity.SmoothTangents(i, 0f);
        }
    }    
}