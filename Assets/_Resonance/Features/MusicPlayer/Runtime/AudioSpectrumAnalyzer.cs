using UnityEngine;

[DisallowMultipleComponent]
public sealed class AudioSpectrumAnalyzer : MonoBehaviour
{
    private const int SpectrumSampleCount = 512;
    private const int FirstSpectrumSample = 1;

    [Header("Audio Source")]
    [Tooltip("The same AudioSource that MusicPlaybackService uses to play the active track.")]
    [SerializeField] private AudioSource audioSource;

    [Header("Visualizer Response")]
    [Tooltip("Number of normalized frequency bands exposed to the visualizer.")]
    [Range(8, 64)]
    [SerializeField] private int bandCount = 28;

    [Tooltip("Strength applied to analyzed audio before each band is clamped to its visible range.")]
    [Min(0f)]
    [SerializeField] private float sensitivity = 65f;

    [Tooltip("How quickly a bar responds when its incoming level rises.")]
    [Min(0.01f)]
    [SerializeField] private float attackSpeed = 24f;

    [Tooltip("How quickly a bar settles when its incoming level falls or playback pauses.")]
    [Min(0.01f)]
    [SerializeField] private float releaseSpeed = 7f;

    [Tooltip("Keeps upper frequency bands visually useful without changing the raw spectrum data.")]
    [Range(0f, 2f)]
    [SerializeField] private float highFrequencyCompensation = 0.65f;

    private float[] _spectrumSamples;
    private float[] _bandValues;

    public int BandCount => _bandValues != null ? _bandValues.Length : 0;

    public float GetBandValue(int index)
    {
        return index >= 0 && index < BandCount ? _bandValues[index] : 0f;
    }

    private void Awake()
    {
        EnsureBuffers();
    }

    private void Update()
    {
        EnsureBuffers();

        bool hasActiveAudio = audioSource != null && audioSource.isPlaying;
        if (hasActiveAudio)
            audioSource.GetSpectrumData(_spectrumSamples, 0, FFTWindow.BlackmanHarris);

        float deltaTime = Time.unscaledDeltaTime;
        for (int bandIndex = 0; bandIndex < _bandValues.Length; bandIndex++)
        {
            float target = hasActiveAudio ? GetMappedBandLevel(bandIndex) : 0f;
            float smoothingSpeed = target > _bandValues[bandIndex] ? attackSpeed : releaseSpeed;
            float smoothingFactor = 1f - Mathf.Exp(-smoothingSpeed * deltaTime);
            _bandValues[bandIndex] = Mathf.Lerp(_bandValues[bandIndex], target, smoothingFactor);
        }
    }

    private float GetMappedBandLevel(int bandIndex)
    {
        // Wider logarithmic bands mirror the way audible music is distributed more closely than one FFT bin per bar.
        float lowerProgress = bandIndex / (float)_bandValues.Length;
        float upperProgress = (bandIndex + 1) / (float)_bandValues.Length;
        int maxSample = _spectrumSamples.Length / 2 - 1;
        int startSample = Mathf.Clamp(Mathf.FloorToInt(Mathf.Pow(maxSample, lowerProgress)), FirstSpectrumSample, maxSample);
        int endSample = Mathf.Clamp(Mathf.CeilToInt(Mathf.Pow(maxSample, upperProgress)), startSample + 1, maxSample + 1);

        float sum = 0f;
        for (int sampleIndex = startSample; sampleIndex < endSample; sampleIndex++)
            sum += _spectrumSamples[sampleIndex] * _spectrumSamples[sampleIndex];

        float rootMeanSquare = Mathf.Sqrt(sum / (endSample - startSample));
        float frequencyCompensation = Mathf.Lerp(1f, 1f + highFrequencyCompensation, upperProgress);
        return Mathf.Clamp01(rootMeanSquare * sensitivity * frequencyCompensation);
    }

    private void EnsureBuffers()
    {
        if (_spectrumSamples == null)
            _spectrumSamples = new float[SpectrumSampleCount];

        int clampedBandCount = Mathf.Clamp(bandCount, 8, 64);
        if (_bandValues == null || _bandValues.Length != clampedBandCount)
            _bandValues = new float[clampedBandCount];
    }

    private void OnValidate()
    {
        bandCount = Mathf.Clamp(bandCount, 8, 64);
        sensitivity = Mathf.Max(0f, sensitivity);
        attackSpeed = Mathf.Max(0.01f, attackSpeed);
        releaseSpeed = Mathf.Max(0.01f, releaseSpeed);
    }
}
