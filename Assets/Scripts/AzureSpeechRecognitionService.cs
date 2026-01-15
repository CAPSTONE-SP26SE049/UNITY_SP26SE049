using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Service để tích hợp Azure Speech Services cho nhận diện giọng nói tiếng Việt
/// 
/// SETUP HƯỚNG DẪN:
/// 1. Đăng ký Azure Speech Services tại: https://azure.microsoft.com/services/cognitive-services/speech-services/
/// 2. Tạo Speech resource và lấy Subscription Key và Region
/// 3. Điền thông tin vào các field dưới đây hoặc sử dụng PlayerPrefs để lưu
/// </summary>
public class AzureSpeechRecognitionService : MonoBehaviour
{
    [Header("Azure Speech Services Configuration")]
    [Tooltip("Azure Speech Services Subscription Key (lấy từ Azure Portal)")]
    public string subscriptionKey = ""; // Điền subscription key của bạn
    
    [Tooltip("Azure Region (ví dụ: southeastasia, eastus, westeurope)")]
    public string region = "southeastasia"; // Region gần Việt Nam
    
    [Tooltip("Language code cho tiếng Việt")]
    public string language = "vi-VN";
    
    [Header("Settings")]
    [Tooltip("Timeout cho request (giây)")]
    public float requestTimeout = 30f; // Tăng timeout lên 30 giây

    private const string TOKEN_ENDPOINT = "https://{0}.api.cognitive.microsoft.com/sts/v1.0/issueToken";
    // Sử dụng REST API endpoint cho speech recognition (conversation mode)
    // Format: https://{region}.stt.speech.microsoft.com/speech/recognition/{mode}/cognitiveservices/v1
    // Thử dùng dictation mode thay vì conversation
    private const string SPEECH_ENDPOINT = "https://{0}.stt.speech.microsoft.com/speech/recognition/dictation/cognitiveservices/v1?language={1}&format=detailed";
    
    private string accessToken = "";
    private bool isTokenValid = false;

    /// <summary>
    /// Nhận diện giọng nói từ audio clip sử dụng Azure Speech Services
    /// </summary>
    public void RecognizeSpeech(AudioClip audioClip, Action<string, bool> onResult)
    {
        StartCoroutine(RecognizeSpeechCoroutine(audioClip, onResult));
    }

    private IEnumerator RecognizeSpeechCoroutine(AudioClip audioClip, Action<string, bool> onResult)
    {
        // Kiểm tra configuration
        if (string.IsNullOrEmpty(subscriptionKey))
        {
            // Thử lấy từ PlayerPrefs
            subscriptionKey = PlayerPrefs.GetString("AzureSpeechKey", "");
            
            if (string.IsNullOrEmpty(subscriptionKey))
            {
                Debug.LogError("Azure Speech Services Subscription Key chưa được cấu hình! Vui lòng điền vào Inspector hoặc PlayerPrefs.");
                onResult?.Invoke("", false);
                yield break;
            }
        }

        // Lấy access token
        yield return StartCoroutine(GetAccessToken());

        if (!isTokenValid || string.IsNullOrEmpty(accessToken))
        {
            Debug.LogError("Không thể lấy Azure Speech Services access token!");
            onResult?.Invoke("", false);
            yield break;
        }

        // Convert AudioClip sang WAV format
        string wavFilePath = SaveAudioClipToWav(audioClip);
        
        if (string.IsNullOrEmpty(wavFilePath) || !File.Exists(wavFilePath))
        {
            Debug.LogError("Không thể tạo file WAV từ audio clip!");
            onResult?.Invoke("", false);
            yield break;
        }

        // Gửi request đến Azure Speech Services
        yield return StartCoroutine(SendSpeechRequest(wavFilePath, onResult));

        // Xóa file WAV tạm
        try
        {
            if (File.Exists(wavFilePath))
            {
                File.Delete(wavFilePath);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Không thể xóa file WAV tạm: " + e.Message);
        }
    }

    private IEnumerator GetAccessToken()
    {
        isTokenValid = false;
        string tokenUrl = string.Format(TOKEN_ENDPOINT, region);

        using (UnityWebRequest request = UnityWebRequest.Post(tokenUrl, ""))
        {
            request.SetRequestHeader("Ocp-Apim-Subscription-Key", subscriptionKey);
            request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                accessToken = request.downloadHandler.text;
                isTokenValid = true;
                Debug.Log("Azure Speech Services token đã được lấy thành công!");
            }
            else
            {
                Debug.LogError($"Lỗi lấy token: {request.error} - {request.downloadHandler.text}");
                isTokenValid = false;
            }
        }
    }

    private IEnumerator SendSpeechRequest(string wavFilePath, Action<string, bool> onResult)
    {
        string speechUrl = string.Format(SPEECH_ENDPOINT, region, language);
        byte[] audioData = File.ReadAllBytes(wavFilePath);

        // Kiểm tra file size (Azure giới hạn ~10MB cho REST API)
        if (audioData.Length > 10 * 1024 * 1024)
        {
            Debug.LogError($"File audio quá lớn: {audioData.Length / 1024}KB. Azure REST API giới hạn ~10MB.");
            onResult?.Invoke("", false);
            yield break;
        }

        Debug.Log($"Đang gửi request đến Azure... File size: {audioData.Length / 1024}KB, URL: {speechUrl}");

        // Tạo UnityWebRequest với POST method và upload raw data
        using (UnityWebRequest request = new UnityWebRequest(speechUrl, "POST"))
        {
            request.SetRequestHeader("Authorization", "Bearer " + accessToken);
            request.SetRequestHeader("Content-Type", "audio/wav; codec=audio/pcm; samplerate=16000");
            request.SetRequestHeader("Accept", "application/json");
            // Disable Expect: 100-continue để tránh Response Code 100
            request.SetRequestHeader("Expect", "");
            request.uploadHandler = new UploadHandlerRaw(audioData);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.timeout = (int)requestTimeout;
            
            // Disable chunked transfer encoding
            request.useHttpContinue = false;

            // Gửi request với progress tracking
            var operation = request.SendWebRequest();
            
            while (!operation.isDone)
            {
                Debug.Log($"Đang xử lý... Progress: {operation.progress * 100:F1}%");
                yield return null;
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                Debug.Log($"Azure Response: {responseText}");
                
                AzureSpeechResponse response = JsonUtility.FromJson<AzureSpeechResponse>(responseText);

                if (response != null && response.RecognitionStatus == "Success" && !string.IsNullOrEmpty(response.DisplayText))
                {
                    Debug.Log($"Nhận diện thành công: {response.DisplayText}");
                    onResult?.Invoke(response.DisplayText, true);
                }
                else
                {
                    Debug.LogWarning($"Nhận diện không thành công. Status: {response?.RecognitionStatus}, Response: {responseText}");
                    onResult?.Invoke("", false);
                }
            }
            else
            {
                string errorDetails = request.downloadHandler?.text ?? "No error details";
                Debug.LogError($"Lỗi gửi request đến Azure: {request.error}\nResponse Code: {request.responseCode}\nDetails: {errorDetails}");
                onResult?.Invoke("", false);
            }
        }
    }

    private string SaveAudioClipToWav(AudioClip clip)
    {
        try
        {
            string filePath = Path.Combine(Application.temporaryCachePath, "temp_recording.wav");
            
            // Convert AudioClip sang WAV với sample rate 16000 (yêu cầu của Azure)
            float[] samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);
            
            // Trim silence ở đầu và cuối để giảm file size
            samples = TrimSilence(samples, clip.channels);
            
            // Resample nếu cần (Azure yêu cầu 16000 Hz)
            int targetSampleRate = 16000;
            if (clip.frequency != targetSampleRate)
            {
                samples = ResampleAudio(samples, clip.frequency, targetSampleRate, clip.channels);
            }
            
            // Kiểm tra lại sau khi trim
            int estimatedSize = samples.Length * 2; // 16-bit = 2 bytes per sample
            if (estimatedSize > 10 * 1024 * 1024)
            {
                Debug.LogWarning($"File vẫn quá lớn sau khi trim: {estimatedSize / 1024}KB. Cắt bớt xuống 10MB.");
                // Cắt xuống còn ~10MB (khoảng 5 triệu samples)
                int maxSamples = 5 * 1024 * 1024;
                if (samples.Length > maxSamples)
                {
                    float[] trimmed = new float[maxSamples];
                    Array.Copy(samples, 0, trimmed, 0, maxSamples);
                    samples = trimmed;
                }
            }
            
            byte[] pcmData = ConvertToPCM16(samples, targetSampleRate, clip.channels);
            MemoryStream wavStream = CreateWavFile(new MemoryStream(pcmData), targetSampleRate, clip.channels, samples.Length / clip.channels);
            
            File.WriteAllBytes(filePath, wavStream.ToArray());
            
            Debug.Log($"File WAV đã được tạo: {filePath}, Size: {wavStream.Length / 1024}KB");
            
            return filePath;
        }
        catch (Exception e)
        {
            Debug.LogError("Lỗi lưu WAV file: " + e.Message);
            return "";
        }
    }

    /// <summary>
    /// Trim silence ở đầu và cuối audio để giảm file size
    /// </summary>
    private float[] TrimSilence(float[] samples, int channels)
    {
        if (samples == null || samples.Length == 0) return samples;

        float silenceThreshold = 0.01f; // Ngưỡng để coi là silence
        int startIndex = 0;
        int endIndex = samples.Length - 1;

        // Tìm điểm bắt đầu (bỏ qua silence ở đầu)
        for (int i = 0; i < samples.Length; i += channels)
        {
            float maxAmplitude = 0f;
            for (int c = 0; c < channels; c++)
            {
                if (i + c < samples.Length)
                {
                    maxAmplitude = Mathf.Max(maxAmplitude, Mathf.Abs(samples[i + c]));
                }
            }
            
            if (maxAmplitude > silenceThreshold)
            {
                startIndex = i;
                break;
            }
        }

        // Tìm điểm kết thúc (bỏ qua silence ở cuối)
        for (int i = samples.Length - channels; i >= 0; i -= channels)
        {
            float maxAmplitude = 0f;
            for (int c = 0; c < channels; c++)
            {
                if (i + c < samples.Length)
                {
                    maxAmplitude = Mathf.Max(maxAmplitude, Mathf.Abs(samples[i + c]));
                }
            }
            
            if (maxAmplitude > silenceThreshold)
            {
                endIndex = i + channels;
                break;
            }
        }

        // Đảm bảo có ít nhất 0.5 giây audio (để tránh trim quá nhiều)
        int minSamples = 8000; // 0.5 giây ở 16000 Hz
        if (endIndex - startIndex < minSamples)
        {
            endIndex = Mathf.Min(startIndex + minSamples, samples.Length);
        }

        // Tạo array mới với phần đã trim
        int trimmedLength = endIndex - startIndex;
        if (trimmedLength <= 0 || trimmedLength >= samples.Length)
        {
            return samples; // Không cần trim
        }

        float[] trimmed = new float[trimmedLength];
        Array.Copy(samples, startIndex, trimmed, 0, trimmedLength);
        
        Debug.Log($"Đã trim audio: {samples.Length} -> {trimmedLength} samples (giảm {((1f - (float)trimmedLength / samples.Length) * 100f):F1}%)");
        
        return trimmed;
    }

    private float[] ResampleAudio(float[] samples, int sourceRate, int targetRate, int channels)
    {
        // Simple resampling - có thể cải thiện bằng thuật toán tốt hơn
        float ratio = (float)targetRate / sourceRate;
        int newLength = Mathf.RoundToInt(samples.Length * ratio);
        float[] resampled = new float[newLength];

        for (int i = 0; i < newLength; i++)
        {
            float sourceIndex = i / ratio;
            int index1 = Mathf.FloorToInt(sourceIndex);
            int index2 = Mathf.Min(index1 + 1, samples.Length - 1);
            float t = sourceIndex - index1;
            
            if (channels == 1)
            {
                resampled[i] = Mathf.Lerp(samples[index1], samples[index2], t);
            }
            else
            {
                // Mono conversion cho đơn giản
                resampled[i] = Mathf.Lerp(samples[index1], samples[index2], t);
            }
        }

        return resampled;
    }

    private byte[] ConvertToPCM16(float[] samples, int sampleRate, int channels)
    {
        short[] intData = new short[samples.Length];
        byte[] bytesData = new byte[samples.Length * 2];

        int rescaleFactor = 32767;

        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)(samples[i] * rescaleFactor);
            byte[] byteArr = BitConverter.GetBytes(intData[i]);
            byteArr.CopyTo(bytesData, i * 2);
        }

        return bytesData;
    }

    private MemoryStream CreateWavFile(MemoryStream pcmStream, int sampleRate, int channels, int samples)
    {
        MemoryStream wavStream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(wavStream);

        int subchunk1Size = 16;
        int bitsPerSample = 16;
        int subchunk2Size = samples * channels * bitsPerSample / 8;
        int chunkSize = 4 + (8 + subchunk1Size) + (8 + subchunk2Size);

        // WAV header
        writer.Write("RIFF".ToCharArray());
        writer.Write(chunkSize);
        writer.Write("WAVE".ToCharArray());
        writer.Write("fmt ".ToCharArray());
        writer.Write(subchunk1Size);
        writer.Write((short)1); // Audio format (PCM)
        writer.Write((short)channels);
        writer.Write(sampleRate);
        writer.Write(sampleRate * channels * bitsPerSample / 8); // Byte rate
        writer.Write((short)(channels * bitsPerSample / 8)); // Block align
        writer.Write((short)bitsPerSample);
        writer.Write("data".ToCharArray());
        writer.Write(subchunk2Size);
        writer.Write(pcmStream.ToArray());

        return wavStream;
    }

    /// <summary>
    /// Lưu subscription key vào PlayerPrefs (để không cần điền lại mỗi lần)
    /// </summary>
    public void SaveSubscriptionKey(string key)
    {
        PlayerPrefs.SetString("AzureSpeechKey", key);
        PlayerPrefs.Save();
        subscriptionKey = key;
    }
}

/// <summary>
/// Response model từ Azure Speech Services
/// </summary>
[Serializable]
public class AzureSpeechResponse
{
    public string RecognitionStatus;
    public string DisplayText;
    public int Offset;
    public int Duration;
}

