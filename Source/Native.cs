using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace CWVR;

internal static class Native
{
    public static readonly IntPtr HKEY_LOCAL_MACHINE = new(0x80000002);

    [DllImport("Advapi32.dll", EntryPoint = "RegOpenKeyExA", CharSet = CharSet.Ansi)]
    public static extern int RegOpenKeyEx(IntPtr hKey, [In] string lpSubKey, int ulOptions, int samDesired,
        out IntPtr phkResult);

    [DllImport("advapi32.dll", CharSet = CharSet.Ansi)]
    public static extern int RegQueryValueEx(IntPtr hKey, string lpValueName, int lpReserved, out uint lpType,
        StringBuilder lpData, ref uint lpcbData);

    [DllImport("advapi32.dll", CharSet = CharSet.Ansi)]
    public static extern int RegQueryInfoKey(IntPtr hKey, StringBuilder lpClass, IntPtr lpcbClass, IntPtr lpReserved,
        out uint lpcSubKeys, out uint lpcbMaxSubKeyLen, out uint lpcbMaxClassLen, out uint lpcValues,
        out uint lpcbMaxValueNameLen, out uint lpcbMaxValueLen, IntPtr lpSecurityDescriptor, IntPtr lpftLastWriteTime);

    [DllImport("advapi32.dll", EntryPoint = "RegEnumValueA", CharSet = CharSet.Ansi)]
    public static extern int RegEnumValue(IntPtr hKey, uint dwIndex, StringBuilder lpValueName, ref uint lpcchValueName,
        IntPtr lpReserved, IntPtr lpType, IntPtr lpData, IntPtr lpcbData);

    [DllImport("advapi32.dll")]
    public static extern int RegCloseKey(IntPtr hKey);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetCurrentProcess();

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool OpenProcessToken(IntPtr hProcess, uint dwAccess, out IntPtr hToken);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool GetTokenInformation(IntPtr hToken, uint tokenInformationClass, IntPtr lpData,
        uint tokenInformationLength, out uint returnLength);

    [DllImport("kernel32.dll", SetLastError = true)]
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    [SuppressUnmanagedCodeSecurity]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(IntPtr handle);

    public static bool RegOpenSubKey(ref IntPtr hKey, string lpSubKey, int samDesired)
    {
        var result = RegOpenKeyEx(hKey, lpSubKey, 0, samDesired, out var hNewKey) == 0;
        if (!result)
            return false;

        RegCloseKey(hKey);
        hKey = hNewKey;

        return true;
    }

    public static bool IsElevated()
    {
        var hToken = IntPtr.Zero;
        var data = IntPtr.Zero;

        try
        {
            if (!OpenProcessToken(GetCurrentProcess(), 0x0008, out hToken))
                return false;

            data = Marshal.AllocHGlobal(4);
            if (!GetTokenInformation(hToken, 20, data, 4, out _))
                return false;

            return Marshal.ReadIntPtr(data).ToInt32() != 0;
        }
        finally
        {
            if (hToken != IntPtr.Zero)
                CloseHandle(hToken);

            if (data != IntPtr.Zero)
                Marshal.FreeHGlobal(data);
        }
    }
}