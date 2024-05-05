using System;
using System.Runtime.InteropServices;
using System.Text;

namespace CWVR;

internal static class Native
{
    public static readonly IntPtr HKEY_LOCAL_MACHINE = new(0x80000002);

    [DllImport("Advapi32.dll", EntryPoint = "RegOpenKeyExA", CharSet = CharSet.Ansi)]
    public static extern int RegOpenKeyEx(IntPtr hKey, [In] string lpSubKey, int ulOptions, int samDesired, out IntPtr phkResult);

    [DllImport("advapi32.dll", CharSet = CharSet.Ansi)]
    public static extern int RegQueryValueEx(IntPtr hKey, string lpValueName, int lpReserved, out uint lpType, StringBuilder lpData, ref uint lpcbData);

    [DllImport("advapi32.dll", CharSet = CharSet.Ansi)]
    public static extern int RegQueryInfoKey(IntPtr hKey, StringBuilder lpClass, IntPtr lpcbClass, IntPtr lpReserved, out uint lpcSubKeys, out uint lpcbMaxSubKeyLen, out uint lpcbMaxClassLen, out uint lpcValues, out uint lpcbMaxValueNameLen, out uint lpcbMaxValueLen, IntPtr lpSecurityDescriptor, IntPtr lpftLastWriteTime);

    [DllImport("advapi32.dll", EntryPoint = "RegEnumValueA", CharSet = CharSet.Ansi)]
    public static extern int RegEnumValue(IntPtr hKey, uint dwIndex, StringBuilder lpValueName, ref uint lpcchValueName, IntPtr lpReserved, IntPtr lpType, IntPtr lpData, IntPtr lpcbData);

    [DllImport("advapi32.dll")]
    public static extern int RegCloseKey(IntPtr hKey);
}