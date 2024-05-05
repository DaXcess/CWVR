using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;

namespace CWVR;

internal static class OpenXR
{
    [DllImport("UnityOpenXR", EntryPoint = "DiagnosticReport_GenerateReport")]
    private static extern IntPtr GenerateReport();

    [DllImport("UnityOpenXR", EntryPoint = "DiagnosticReport_ReleaseReport")]
    private static extern void ReleaseReport(IntPtr report);

    [DllImport("UnityOpenXR", EntryPoint = "NativeConfig_GetRuntimeName")]
    private static extern bool GetRuntimeName(out IntPtr runtimeNamePtr);

    [DllImport("UnityOpenXR", EntryPoint = "NativeConfig_GetRuntimeVersion")]
    public static extern bool GetRuntimeVersion(out ushort major, out ushort minor, out ushort patch);

    public static string GenerateTextReport()
    {
        var report = GenerateReport();
        if (report == IntPtr.Zero)
            return "";

        var result = Marshal.PtrToStringAnsi(report);
        ReleaseReport(report);

        return result;
    }

    public static bool GetDiagnosticReport(out OpenXRReport report)
    {
        report = null;

        var sectionRegex = new Regex("^==== ([A-z0-9-_ ]+) ====$", RegexOptions.Multiline);
        var errorRegex = new Regex("^\\[FAILURE\\] [A-z]+: ([A-Z_]+) \\(\\d+x\\)$");

        var raw = GenerateTextReport();

        var rawSections = sectionRegex.Split(raw).Skip(1).Select(v => v.Trim()).ToArray();
        var sections = new Dictionary<string, string>();

        for (var i = 0; i < rawSections.Length; i += 2)
            sections.Add(rawSections[i], rawSections[i + 1]);

        if (!sections.TryGetValue("OpenXR Runtime Info", out string section))
            return false;

        var lines = section.Split('\n');

        var runtimeName = lines.FirstOrDefault(line => line.StartsWith("Runtime Name: "));
        var runtimeVersion = lines.FirstOrDefault(line => line.StartsWith("Runtime Version: "));

        if (runtimeName == default || runtimeVersion == default)
        {
            runtimeName = "<Missing>";
            runtimeVersion = "<Missing>";
        }
        else
        {
            runtimeName = runtimeName.Split(": ")[1];
            runtimeVersion = runtimeVersion.Split(": ")[1];
        }

        if (!sections.TryGetValue("Last 20 non-XR_SUCCESS returns", out section))
            return false;

        var match = errorRegex.Match(section.Split('\n')[0].Trim());
        if (match.Groups.Count == 0)
            return false;

        var error = match.Groups[1].Value;

        report = new OpenXRReport(runtimeName, runtimeVersion, error);

        return true;
    }

    public class OpenXRReport(string runtimeName, string runtimeVersion, string error)
    {
        public string RuntimeName { get; } = runtimeName;
        public string RuntimeVersion { get; } = runtimeVersion;
        public string Error { get; } = error;
    }

    public static Dictionary<string, string> DetectOpenXRRuntimes(out string @default)
    {
        var list = new Dictionary<string, string>();

        @default = null;

        var hKey = IntPtr.Zero;
        var cbData = 0u;

        try
        {
            if (Native.RegOpenKeyEx(Native.HKEY_LOCAL_MACHINE, "SOFTWARE\\Khronos\\OpenXR\\1", 0, 0x20019, out hKey) !=
                0)
                throw new Exception("Failed to open registry key HKLM\\SOFTWARE\\Khronos\\OpenXR\\1");

            if (Native.RegQueryValueEx(hKey, "ActiveRuntime", 0, out _, null, ref cbData) != 0)
                throw new Exception("Failed to query ActiveRuntime value");

            var data = new StringBuilder((int)cbData);

            if (Native.RegQueryValueEx(hKey, "ActiveRuntime", 0, out _, data, ref cbData) != 0)
                throw new Exception("Failed to query ActiveRuntime value");

            var path = data.ToString();
            @default = JSON.Deserialize<OpenXRRuntime>(File.ReadAllText(path)).Runtime.Name;

            if (Native.RegOpenKeyEx(hKey, "AvailableRuntimes", 0, 0x20019, out hKey) != 0)
                throw new Exception("Failed to open AvailableRuntimes registry key");

            if (Native.RegQueryInfoKey(hKey, null, IntPtr.Zero, IntPtr.Zero, out _, out _, out _, out var valueCount,
                    out var maxValueNameLength, out _, IntPtr.Zero, IntPtr.Zero) != 0)
                throw new Exception("Failed to query AvailableRuntimes registry key");

            var values = new List<string>();

            for (uint i = 0; i < valueCount; i++)
            {
                try
                {
                    var valueName = new StringBuilder((int)maxValueNameLength + 1);
                    var cbValueName = maxValueNameLength + 1;

                    var result = Native.RegEnumValue(hKey, i, valueName, ref cbValueName, IntPtr.Zero, IntPtr.Zero,
                        IntPtr.Zero, IntPtr.Zero);

                    if (result != 0)
                        continue;

                    values.Add(valueName.ToString());
                }
                catch
                {
                    // Failed to query runtime
                }
            }

            foreach (var file in values)
            {
                var i = 0;

                var name = JSON.Deserialize<OpenXRRuntime>(File.ReadAllText(file)).Runtime.Name;
                var resultName = name;

                while (list.ContainsKey(resultName))
                {
                    i++;
                    resultName = $"{name} ({i})";
                }


                list.Add(name, file);
            }

            return list;
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Failed to query runtimes: {ex.Message}");
            return null;
        }
        finally
        {
            if (hKey != IntPtr.Zero)
                Native.RegCloseKey(hKey);
        }
    }

    public static bool GetRuntimeName(out string name)
    {
        name = null;

        if (!GetRuntimeName(out IntPtr ptr))
            return false;

        if (ptr == IntPtr.Zero)
            return false;

        name = Marshal.PtrToStringAnsi(ptr);

        return true;
    }

    [DataContract]
    private struct OpenXRRuntime
    {
        [DataMember(Name = "runtime")] public RuntimeInfo Runtime { get; set; }
    }

    [DataContract]
    private struct RuntimeInfo
    {
        [DataMember(Name = "name")] public string Name { get; set; }
    }
}