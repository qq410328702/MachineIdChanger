using Microsoft.Win32;

namespace MachineIdChanger;

/// <summary>
/// 注册表操作类 - 用于读取和修改 MachineGuid
/// </summary>
public class RegistryHelper
{
    private const string RegistryPath = @"SOFTWARE\Microsoft\Cryptography";
    private const string ValueName = "MachineGuid";

    /// <summary>
    /// 获取当前的 MachineGuid
    /// </summary>
    public static string? GetMachineGuid()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(RegistryPath);
            return key?.GetValue(ValueName)?.ToString();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"读取 MachineGuid 失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 设置新的 MachineGuid
    /// </summary>
    public static void SetMachineGuid(string newGuid)
    {
        if (!IsValidGuid(newGuid))
        {
            throw new ArgumentException("无效的 GUID 格式", nameof(newGuid));
        }

        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(RegistryPath, writable: true)
                ?? Registry.LocalMachine.CreateSubKey(RegistryPath);
            
            key?.SetValue(ValueName, newGuid, RegistryValueKind.String);
        }
        catch (UnauthorizedAccessException)
        {
            throw new InvalidOperationException("没有权限修改注册表，请以管理员身份运行程序");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"修改 MachineGuid 失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 验证 GUID 格式
    /// </summary>
    public static bool IsValidGuid(string guid)
    {
        return Guid.TryParseExact(guid, "D", out _)
            || Guid.TryParseExact(guid, "B", out _)
            || Guid.TryParseExact(guid, "P", out _)
            || Guid.TryParseExact(guid, "N", out _);
    }

    /// <summary>
    /// 生成新的 GUID（标准格式）
    /// </summary>
    public static string GenerateNewGuid()
    {
        return Guid.NewGuid().ToString("D");
    }

    /// <summary>
    /// 生成新的 GUID（无连字符格式）
    /// </summary>
    public static string GenerateNewGuidNoDash()
    {
        return Guid.NewGuid().ToString("N");
    }
}
