using System.Text;
using System.Text.RegularExpressions;
namespace ABCDMall.Modules.FoodCourt.Application.Helpers;
public static class SlugHelper
{
    public static string GenerateSlug(string phrase)
    {
        string str = phrase.ToLower();

        // remove dấu tiếng Việt
        str = RemoveVietnameseSigns(str);

        // remove ký tự đặc biệt
        str = Regex.Replace(str, @"[^a-z0-9\s-]", "");

        // replace space -> -
        str = Regex.Replace(str, @"\s+", "-").Trim();

        // remove multiple -
        str = Regex.Replace(str, @"-+", "-");

        return str;
    }

    private static string RemoveVietnameseSigns(string text)
    {
        string[] vietnameseSigns = new string[]
        {
            "aàáạảãâầấậẩẫăằắặẳẵ",
            "eèéẹẻẽêềếệểễ",
            "iìíịỉĩ",
            "oòóọỏõôồốộổỗơờớợởỡ",
            "uùúụủũưừứựửữ",
            "yỳýỵỷỹ",
            "dđ"
        };

        for (int i = 1; i < vietnameseSigns.Length; i++)
        {
            for (int j = 0; j < vietnameseSigns[i].Length; j++)
                text = text.Replace(vietnameseSigns[i][j], vietnameseSigns[i][0]);
        }

        return text;
    }

    public static string NormalizeImage(string url)
{
    if (string.IsNullOrEmpty(url)) return "";

    return url.Replace("./img", "/img");
}

}