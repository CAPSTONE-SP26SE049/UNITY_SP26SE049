using System;
using System.Collections;
using System.IO;
using UnityEngine;

/// <summary>
/// Helper class để nhận diện giọng nói từ audio clip
/// Sử dụng Azure Speech Services để nhận diện giọng nói tiếng Việt
/// </summary>
public class SpeechRecognitionHelper : MonoBehaviour
{
    private static SpeechRecognitionHelper instance;
    public static SpeechRecognitionHelper Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("SpeechRecognitionHelper");
                instance = obj.AddComponent<SpeechRecognitionHelper>();
                DontDestroyOnLoad(obj);
            }
            return instance;
        }
    }

    private AzureSpeechRecognitionService azureService;

    private void Awake()
    {
        // Tìm Azure Speech Recognition Service trong scene trước
        azureService = FindObjectOfType<AzureSpeechRecognitionService>();
        
        // Nếu không tìm thấy trong scene, tự động tạo
        if (azureService == null)
        {
            GameObject azureObj = new GameObject("AzureSpeechRecognitionService");
            azureService = azureObj.AddComponent<AzureSpeechRecognitionService>();
            DontDestroyOnLoad(azureObj);
            Debug.Log("AzureSpeechRecognitionService đã được tạo tự động. Để cấu hình Subscription Key, hãy tạo GameObject thủ công trong scene.");
        }
        else
        {
            Debug.Log("Đã tìm thấy AzureSpeechRecognitionService trong scene.");
        }
    }

    /// <summary>
    /// Nhận diện giọng nói từ audio clip sử dụng Azure Speech Services
    /// </summary>
    /// <param name="audioClip">Audio clip cần nhận diện</param>
    /// <param name="onResult">Callback khi có kết quả (string result, bool success)</param>
    public void RecognizeSpeech(AudioClip audioClip, Action<string, bool> onResult)
    {
        if (azureService == null)
        {
            Debug.LogError("Azure Speech Recognition Service chưa được khởi tạo!");
            onResult?.Invoke("", false);
            return;
        }

        // Sử dụng Azure Speech Services để nhận diện
        azureService.RecognizeSpeech(audioClip, onResult);
    }

    private string SaveAudioClipToWav(AudioClip clip)
    {
        try
        {
            string filePath = Path.Combine(Application.temporaryCachePath, "temp_recording.wav");
            
            // Convert AudioClip sang WAV
            float[] samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);
            
            byte[] pcmData = ConvertToPCM16(samples, clip.frequency, clip.channels);
            MemoryStream wavStream = CreateWavFile(new MemoryStream(pcmData), clip.frequency, clip.channels, clip.samples);
            
            // Lưu file
            File.WriteAllBytes(filePath, wavStream.ToArray());
            
            return filePath;
        }
        catch (Exception e)
        {
            Debug.LogError("Lỗi lưu WAV file: " + e.Message);
            return "";
        }
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
    /// So sánh text đã nhận diện với text đúng (khớp chính xác bao gồm dấu)
    /// Cải thiện để linh hoạt hơn: bỏ qua dấu chấm, dấu phẩy, khoảng trắng thừa
    /// </summary>
    public bool CompareText(string recognizedText, string targetText)
    {
        if (string.IsNullOrEmpty(recognizedText) || string.IsNullOrEmpty(targetText))
            return false;

        // Chuẩn hóa: loại bỏ khoảng trắng thừa, chuyển về lowercase
        string normalizedRecognized = NormalizeText(recognizedText);
        string normalizedTarget = NormalizeText(targetText);

        // So khớp chính xác (bao gồm dấu tiếng Việt)
        bool exactMatch = normalizedRecognized == normalizedTarget;
        
        // Nếu không khớp chính xác, thử kiểm tra xem có chứa target text không (cho trường hợp có thêm từ)
        if (!exactMatch)
        {
            // Kiểm tra xem recognized text có chứa target text không
            if (normalizedRecognized.Contains(normalizedTarget))
            {
                return true;
            }
            
            // Kiểm tra ngược lại: target text có chứa recognized text không (cho trường hợp nói thiếu)
            // Nhưng chỉ khi recognized text đủ dài (ít nhất 70% độ dài của target)
            if (normalizedRecognized.Length >= normalizedTarget.Length * 0.7f)
            {
                if (normalizedTarget.Contains(normalizedRecognized))
                {
                    return true;
                }
            }
        }
        
        return exactMatch;
    }

    /// <summary>
    /// Chuẩn hóa text: loại bỏ dấu chấm, dấu phẩy, khoảng trắng thừa, chuyển về lowercase
    /// </summary>
    private string NormalizeText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return "";

        // Loại bỏ dấu chấm, dấu phẩy, dấu chấm hỏi, dấu chấm than ở cuối
        text = text.Trim().ToLower();
        text = text.TrimEnd('.', ',', '?', '!', ';', ':');
        
        // Loại bỏ khoảng trắng thừa (nhiều khoảng trắng liên tiếp thành 1)
        while (text.Contains("  "))
        {
            text = text.Replace("  ", " ");
        }
        
        return text.Trim();
    }
}

