using System.Management;

namespace MachineIdChanger;

/// <summary>
/// 硬件信息读取类 - 用于获取硬盘、主板等硬件信息
/// </summary>
public class HardwareInfoHelper
{
    /// <summary>
    /// 获取硬盘序列号
    /// </summary>
    public static List<string> GetDiskSerialNumbers()
    {
        var serialNumbers = new List<string>();
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
            foreach (ManagementObject disk in searcher.Get())
            {
                var serialNumber = disk["SerialNumber"]?.ToString()?.Trim();
                var model = disk["Model"]?.ToString()?.Trim();
                var size = disk["Size"]?.ToString();
                
                if (!string.IsNullOrEmpty(model))
                {
                    var sizeGB = "";
                    if (!string.IsNullOrEmpty(size) && long.TryParse(size, out long sizeBytes))
                    {
                        sizeGB = $" ({sizeBytes / 1024 / 1024 / 1024} GB)";
                    }
                    
                    if (!string.IsNullOrEmpty(serialNumber))
                    {
                        serialNumbers.Add($"  {model}{sizeGB}\n  序列号: {serialNumber}");
                    }
                    else
                    {
                        serialNumbers.Add($"  {model}{sizeGB}\n  序列号: 未找到");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            serialNumbers.Add($"读取失败: {ex.Message}");
        }
        return serialNumbers;
    }

    /// <summary>
    /// 获取主板序列号
    /// </summary>
    public static string GetMotherboardSerialNumber()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard");
            foreach (ManagementObject board in searcher.Get())
            {
                var serialNumber = board["SerialNumber"]?.ToString()?.Trim();
                var manufacturer = board["Manufacturer"]?.ToString()?.Trim();
                var product = board["Product"]?.ToString()?.Trim();
                
                if (!string.IsNullOrEmpty(serialNumber))
                {
                    return $"{manufacturer} {product}: {serialNumber}";
                }
            }
        }
        catch (Exception ex)
        {
            return $"读取失败: {ex.Message}";
        }
        return "未找到";
    }

    /// <summary>
    /// 获取 BIOS 序列号
    /// </summary>
    public static string GetBiosSerialNumber()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS");
            foreach (ManagementObject bios in searcher.Get())
            {
                var serialNumber = bios["SerialNumber"]?.ToString()?.Trim();
                if (!string.IsNullOrEmpty(serialNumber))
                {
                    return serialNumber;
                }
            }
        }
        catch (Exception ex)
        {
            return $"读取失败: {ex.Message}";
        }
        return "未找到";
    }

    /// <summary>
    /// 获取 CPU ID
    /// </summary>
    public static string GetProcessorId()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
            foreach (ManagementObject processor in searcher.Get())
            {
                var processorId = processor["ProcessorId"]?.ToString()?.Trim();
                var name = processor["Name"]?.ToString()?.Trim();
                if (!string.IsNullOrEmpty(processorId))
                {
                    return $"{name}: {processorId}";
                }
            }
        }
        catch (Exception ex)
        {
            return $"读取失败: {ex.Message}";
        }
        return "未找到";
    }

    /// <summary>
    /// 获取 MAC 地址
    /// </summary>
    public static List<string> GetMacAddresses()
    {
        var macAddresses = new List<string>();
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter WHERE MACAddress IS NOT NULL");
            foreach (ManagementObject adapter in searcher.Get())
            {
                var mac = adapter["MACAddress"]?.ToString()?.Trim();
                var name = adapter["Name"]?.ToString()?.Trim();
                var adapterType = adapter["AdapterType"]?.ToString()?.Trim();
                
                if (!string.IsNullOrEmpty(mac) && !string.IsNullOrEmpty(name))
                {
                    // 过滤掉虚拟网卡、蓝牙PAN和WAN Miniport
                    if (name.Contains("Virtual", StringComparison.OrdinalIgnoreCase) ||
                        name.Contains("VMware", StringComparison.OrdinalIgnoreCase) ||
                        name.Contains("VirtualBox", StringComparison.OrdinalIgnoreCase) ||
                        name.Contains("Hyper-V", StringComparison.OrdinalIgnoreCase) ||
                        name.Contains("Personal Area Network", StringComparison.OrdinalIgnoreCase) ||
                        name.Contains("WAN Miniport", StringComparison.OrdinalIgnoreCase) ||
                        name.Contains("Teredo", StringComparison.OrdinalIgnoreCase) ||
                        name.Contains("isatap", StringComparison.OrdinalIgnoreCase) ||
                        name.Contains("6to4", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    
                    macAddresses.Add($"  {name}\n  MAC: {mac}");
                }
            }
        }
        catch (Exception ex)
        {
            macAddresses.Add($"读取失败: {ex.Message}");
        }
        return macAddresses;
    }
}
