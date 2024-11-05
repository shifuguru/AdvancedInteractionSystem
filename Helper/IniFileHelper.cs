using System.Runtime.InteropServices;
using System.Text;

public static class IniFileHelper
{
    [DllImport("kernel32", CharSet = CharSet.Unicode)]
    private static extern long WritePrivateProfileString(string section, string key, string value, string filePath);

    [DllImport("kernel32", CharSet = CharSet.Unicode)]
    private static extern int GetPrivateProfileString(string section, string key, string defaultValue, StringBuilder returnValue, int size, string filePath);

    public static void WriteValue(string section, string key, string value, string filePath)
    {
        WritePrivateProfileString(section, key, value, filePath);
    }

    public static string ReadValue(string section, string key, string filePath)
    {
        StringBuilder returnValue = new StringBuilder(255);
        GetPrivateProfileString(section, key, "", returnValue, 255, filePath);
        return returnValue.ToString();
    }
}