# MachineGuid 修改工具

一个 Windows 平台的 MachineGuid 修改工具，使用 C# 和 WinForms 开发。

## 功能

- 读取当前系统的 MachineGuid
- 生成新的 GUID（标准格式或无连字符格式）
- 修改注册表中的 MachineGuid
- 自动请求管理员权限

## 技术栈

- .NET 8.0
- Windows Forms
- Microsoft.Win32.Registry

## 注册表位置

```
HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Cryptography\MachineGuid
```

## 使用方法

### 编译运行

```bash
# 进入项目目录
cd MachineId

# 构建项目
dotnet build

# 运行（会自动请求管理员权限）
dotnet run
```

### 发布

```bash
# 发布单文件可执行程序
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## 注意事项

1. **必须以管理员身份运行** - 修改 HKEY_LOCAL_MACHINE 需要管理员权限
2. **修改前请备份** - 工具会显示原始值，请记录备用
3. **建议重启系统** - 修改后建议重启计算机以确保更改生效

## 界面说明

- **当前 MachineGuid**: 显示当前系统的机器码
- **新的 MachineGuid**: 输入或生成新的 GUID
- **生成新 GUID**: 生成标准格式 GUID (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx)
- **生成无连字符 GUID**: 生成无连字符格式 GUID (xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx)
- **应用修改**: 将新 GUID 写入注册表

## 安全提示

MachineGuid 是 Windows 系统的重要标识符，某些软件授权或激活机制可能依赖此值。修改前请确认了解其影响。
