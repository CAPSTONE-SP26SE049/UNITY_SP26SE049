using UnityEngine;

/// <summary>
/// Helper script để load font VT323
/// Nếu bạn có font VT323, đặt nó vào thư mục Assets/Resources/Fonts/
/// </summary>
public static class FontHelper
{
    private static Font _vt323Font;
    
    public static Font GetVT323Font()
    {
        if (_vt323Font != null)
            return _vt323Font;
        
        // Tìm font VT323 trong Resources
        _vt323Font = Resources.Load<Font>("Fonts/VT323");
        
        // Nếu không tìm thấy, tìm trong toàn bộ project
        if (_vt323Font == null)
        {
            Font[] fonts = Resources.FindObjectsOfTypeAll<Font>();
            foreach (Font f in fonts)
            {
                if (f.name.Contains("VT323") || f.name.Contains("vt323"))
                {
                    _vt323Font = f;
                    break;
                }
            }
        }
        
        // Nếu vẫn không tìm thấy, dùng font mặc định
        if (_vt323Font == null)
        {
            _vt323Font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
        
        return _vt323Font;
    }
}

