namespace MachineIdChanger;

public partial class MainForm : Form
{
    private TextBox txtCurrentGuid = null!;
    private TextBox txtNewGuid = null!;
    private Button btnRefresh = null!;
    private Button btnGenerate = null!;
    private Button btnGenerateNoDash = null!;
    private Button btnApply = null!;
    private Label lblStatus = null!;
    private Button btnShowHardwareInfo = null!;
    private string? originalGuid;

    public MainForm()
    {
        InitializeComponent();
        LoadCurrentGuid();
    }

    private void InitializeComponent()
    {
        Text = "MachineGuid 修改工具";
        Size = new Size(650, 360);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;

        // 当前 GUID 标签和文本框
        var lblCurrent = new Label
        {
            Text = "当前 MachineGuid:",
            Location = new Point(20, 20),
            Size = new Size(120, 25),
            TextAlign = ContentAlignment.MiddleLeft
        };

        txtCurrentGuid = new TextBox
        {
            Location = new Point(150, 20),
            Size = new Size(400, 25),
            ReadOnly = true,
            BackColor = SystemColors.ControlLight
        };

        btnRefresh = new Button
        {
            Text = "刷新",
            Location = new Point(560, 18),
            Size = new Size(60, 28)
        };
        btnRefresh.Click += (s, e) => LoadCurrentGuid();

        // 新 GUID 标签和文本框
        var lblNew = new Label
        {
            Text = "新的 MachineGuid:",
            Location = new Point(20, 60),
            Size = new Size(120, 25),
            TextAlign = ContentAlignment.MiddleLeft
        };

        txtNewGuid = new TextBox
        {
            Location = new Point(150, 60),
            Size = new Size(470, 25)
        };

        // 生成按钮组
        btnGenerate = new Button
        {
            Text = "生成新 GUID",
            Location = new Point(150, 100),
            Size = new Size(120, 35)
        };
        btnGenerate.Click += (s, e) => txtNewGuid.Text = RegistryHelper.GenerateNewGuid();

        btnGenerateNoDash = new Button
        {
            Text = "生成无连字符 GUID",
            Location = new Point(290, 100),
            Size = new Size(140, 35)
        };
        btnGenerateNoDash.Click += (s, e) => txtNewGuid.Text = RegistryHelper.GenerateNewGuidNoDash();

        btnApply = new Button
        {
            Text = "应用修改",
            Location = new Point(450, 100),
            Size = new Size(120, 35),
            BackColor = Color.LightCoral
        };
        btnApply.Click += BtnApply_Click;

        // 硬件信息按钮
        btnShowHardwareInfo = new Button
        {
            Text = "查看硬件信息",
            Location = new Point(250, 150),
            Size = new Size(140, 35),
            BackColor = Color.LightBlue
        };
        btnShowHardwareInfo.Click += BtnShowHardwareInfo_Click;

        // 状态标签
        lblStatus = new Label
        {
            Location = new Point(20, 200),
            Size = new Size(600, 40),
            TextAlign = ContentAlignment.MiddleLeft,
            BorderStyle = BorderStyle.FixedSingle
        };

        // 说明标签
        var lblInfo = new Label
        {
            Text = "说明：\n" +
                   "• MachineGuid 存储在注册表 HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Cryptography\n" +
                   "• 修改前请确保已备份或记住原始值\n" +
                   "• 需要以管理员身份运行才能修改注册表",
            Location = new Point(20, 250),
            Size = new Size(600, 80),
            ForeColor = Color.DarkBlue
        };

        Controls.Add(lblCurrent);
        Controls.Add(txtCurrentGuid);
        Controls.Add(btnRefresh);
        Controls.Add(lblNew);
        Controls.Add(txtNewGuid);
        Controls.Add(btnGenerate);
        Controls.Add(btnGenerateNoDash);
        Controls.Add(btnApply);
        Controls.Add(btnShowHardwareInfo);
        Controls.Add(lblStatus);
        Controls.Add(lblInfo);
    }

    private void LoadCurrentGuid()
    {
        try
        {
            originalGuid = RegistryHelper.GetMachineGuid();
            if (originalGuid != null)
            {
                txtCurrentGuid.Text = originalGuid;
                lblStatus.Text = $"状态: 成功读取当前 MachineGuid | 原始值已备份";
                lblStatus.ForeColor = Color.Green;
            }
            else
            {
                lblStatus.Text = "状态: 未找到 MachineGuid，将创建新值";
                lblStatus.ForeColor = Color.Orange;
            }
        }
        catch (Exception ex)
        {
            lblStatus.Text = $"错误: {ex.Message}";
            lblStatus.ForeColor = Color.Red;
            MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnApply_Click(object? sender, EventArgs e)
    {
        var newGuid = txtNewGuid.Text.Trim();
        
        if (string.IsNullOrEmpty(newGuid))
        {
            MessageBox.Show("请输入新的 GUID", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!RegistryHelper.IsValidGuid(newGuid))
        {
            MessageBox.Show("GUID 格式无效，请使用标准 GUID 格式\n例如: 12345678-1234-1234-1234-123456789012", 
                "格式错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var confirmMsg = $"确定要修改 MachineGuid 吗?\n\n" +
                         $"原值: {originalGuid ?? "无"}\n" +
                         $"新值: {newGuid}\n\n" +
                         $"此操作将影响系统识别码，请谨慎操作!";

        if (MessageBox.Show(confirmMsg, "确认修改", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
        {
            return;
        }

        try
        {
            RegistryHelper.SetMachineGuid(newGuid);
            lblStatus.Text = $"状态: MachineGuid 修改成功 | 原值: {originalGuid} | 新值: {newGuid}";
            lblStatus.ForeColor = Color.Green;
            
            MessageBox.Show("MachineGuid 修改成功!\n建议重启计算机以确保更改生效。", 
                "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            
            LoadCurrentGuid();
        }
        catch (InvalidOperationException ex)
        {
            lblStatus.Text = $"错误: {ex.Message}";
            lblStatus.ForeColor = Color.Red;
            
            if (ex.Message.Contains("管理员"))
            {
                MessageBox.Show("请以管理员身份重新运行此程序。\n右键点击程序图标选择'以管理员身份运行'", 
                    "需要管理员权限", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            lblStatus.Text = $"错误: {ex.Message}";
            lblStatus.ForeColor = Color.Red;
            MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnShowHardwareInfo_Click(object? sender, EventArgs e)
    {
        try
        {
            lblStatus.Text = "正在读取硬件信息...";
            lblStatus.ForeColor = Color.Blue;
            Application.DoEvents();

            var info = new System.Text.StringBuilder();
            info.AppendLine("========== 硬件信息 ==========\n");

            // 主板信息
            info.AppendLine("【主板序列号】");
            info.AppendLine(HardwareInfoHelper.GetMotherboardSerialNumber());
            info.AppendLine();

            // BIOS 信息
            info.AppendLine("【BIOS 序列号】");
            info.AppendLine(HardwareInfoHelper.GetBiosSerialNumber());
            info.AppendLine();

            // CPU 信息
            info.AppendLine("【处理器 ID】");
            info.AppendLine(HardwareInfoHelper.GetProcessorId());
            info.AppendLine();

            // 硬盘信息
            info.AppendLine("【硬盘序列号】");
            var disks = HardwareInfoHelper.GetDiskSerialNumbers();
            if (disks.Count > 0)
            {
                foreach (var disk in disks)
                {
                    info.AppendLine(disk);
                }
            }
            else
            {
                info.AppendLine("未找到硬盘信息");
            }
            info.AppendLine();

            // MAC 地址
            info.AppendLine("【网卡 MAC 地址】");
            var macs = HardwareInfoHelper.GetMacAddresses();
            if (macs.Count > 0)
            {
                foreach (var mac in macs)
                {
                    info.AppendLine(mac);
                }
            }
            else
            {
                info.AppendLine("未找到网卡信息");
            }

            // 显示硬件信息窗口
            var infoForm = new Form
            {
                Text = "硬件信息",
                Size = new Size(700, 500),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.Sizable,
                MinimizeBox = false,
                MaximizeBox = true
            };

            var txtInfo = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 10),
                Text = info.ToString()
            };

            var btnCopy = new Button
            {
                Text = "复制到剪贴板",
                Dock = DockStyle.Bottom,
                Height = 40
            };
            btnCopy.Click += (s, ev) =>
            {
                Clipboard.SetText(info.ToString());
                MessageBox.Show("硬件信息已复制到剪贴板", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            infoForm.Controls.Add(txtInfo);
            infoForm.Controls.Add(btnCopy);
            infoForm.ShowDialog();

            lblStatus.Text = "状态: 硬件信息读取完成";
            lblStatus.ForeColor = Color.Green;
        }
        catch (Exception ex)
        {
            lblStatus.Text = $"错误: {ex.Message}";
            lblStatus.ForeColor = Color.Red;
            MessageBox.Show($"读取硬件信息失败:\n{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
