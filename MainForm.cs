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
            info.AppendLine("========================================");
            info.AppendLine("           硬件信息详情");
            info.AppendLine("========================================\n");

            // 主板信息
            info.AppendLine("【主板信息】");
            info.AppendLine(HardwareInfoHelper.GetMotherboardSerialNumber());
            info.AppendLine("\n----------------------------------------\n");

            // BIOS 信息
            info.AppendLine("【BIOS 序列号】");
            info.AppendLine(HardwareInfoHelper.GetBiosSerialNumber());
            info.AppendLine("\n----------------------------------------\n");

            // CPU 信息
            info.AppendLine("【处理器信息】");
            info.AppendLine(HardwareInfoHelper.GetProcessorId());
            info.AppendLine("\n----------------------------------------\n");

            // 硬盘信息
            info.AppendLine("【硬盘信息】");
            var disks = HardwareInfoHelper.GetDiskSerialNumbers();
            if (disks.Count > 0)
            {
                for (int i = 0; i < disks.Count; i++)
                {
                    info.AppendLine($"硬盘 {i + 1}:");
                    info.AppendLine(disks[i]);
                    if (i < disks.Count - 1)
                    {
                        info.AppendLine();
                    }
                }
            }
            else
            {
                info.AppendLine("  未找到硬盘信息");
            }
            info.AppendLine("\n----------------------------------------\n");

            // MAC 地址
            info.AppendLine("【网卡信息】");
            var macs = HardwareInfoHelper.GetMacAddresses();
            if (macs.Count > 0)
            {
                for (int i = 0; i < macs.Count; i++)
                {
                    info.AppendLine($"网卡 {i + 1}:");
                    info.AppendLine(macs[i]);
                    if (i < macs.Count - 1)
                    {
                        info.AppendLine();
                    }
                }
            }
            else
            {
                info.AppendLine("  未找到网卡信息");
            }
            info.AppendLine("\n========================================");

            // 显示硬件信息窗口
            var infoForm = new Form
            {
                Text = "硬件信息",
                Size = new Size(750, 550),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.Sizable,
                MinimizeBox = false,
                MaximizeBox = true
            };

            var richTextInfo = new RichTextBox
            {
                ReadOnly = true,
                ScrollBars = RichTextBoxScrollBars.Both,
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 10),
                BackColor = Color.FromArgb(250, 250, 250),
                BorderStyle = BorderStyle.None
            };

            // 添加带颜色的文本
            AddColoredHardwareInfo(richTextInfo);

            var btnCopy = new Button
            {
                Text = "复制到剪贴板",
                Dock = DockStyle.Bottom,
                Height = 40,
                Font = new Font("Microsoft YaHei UI", 10)
            };
            btnCopy.Click += (s, ev) =>
            {
                Clipboard.SetText(richTextInfo.Text);
                MessageBox.Show("硬件信息已复制到剪贴板", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            infoForm.Controls.Add(richTextInfo);
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

    private void AddColoredHardwareInfo(RichTextBox rtb)
    {
        rtb.Clear();

        // 标题
        AppendText(rtb, "========================================\n", Color.DarkBlue, true);
        AppendText(rtb, "           硬件信息详情\n", Color.DarkBlue, true);
        AppendText(rtb, "========================================\n\n", Color.DarkBlue, true);

        // 主板信息
        AppendText(rtb, "【主板信息】\n", Color.DarkGreen, true);
        var motherboard = HardwareInfoHelper.GetMotherboardSerialNumber();
        var mbParts = motherboard.Split(':');
        if (mbParts.Length == 2)
        {
            AppendText(rtb, mbParts[0] + ": ", Color.Black, false);
            AppendText(rtb, mbParts[1].Trim() + "\n", Color.Red, true);
        }
        else
        {
            AppendText(rtb, motherboard + "\n", Color.Black, false);
        }
        AppendText(rtb, "\n----------------------------------------\n\n", Color.Gray, false);

        // BIOS 信息
        AppendText(rtb, "【BIOS 序列号】\n", Color.DarkGreen, true);
        AppendText(rtb, HardwareInfoHelper.GetBiosSerialNumber() + "\n", Color.Red, true);
        AppendText(rtb, "\n----------------------------------------\n\n", Color.Gray, false);

        // CPU 信息
        AppendText(rtb, "【处理器信息】\n", Color.DarkGreen, true);
        var cpu = HardwareInfoHelper.GetProcessorId();
        var cpuParts = cpu.Split(':');
        if (cpuParts.Length == 2)
        {
            AppendText(rtb, cpuParts[0] + ": ", Color.Black, false);
            AppendText(rtb, cpuParts[1].Trim() + "\n", Color.Red, true);
        }
        else
        {
            AppendText(rtb, cpu + "\n", Color.Black, false);
        }
        AppendText(rtb, "\n----------------------------------------\n\n", Color.Gray, false);

        // 硬盘信息
        AppendText(rtb, "【硬盘信息】\n", Color.DarkGreen, true);
        var disks = HardwareInfoHelper.GetDiskSerialNumbers();
        if (disks.Count > 0)
        {
            for (int i = 0; i < disks.Count; i++)
            {
                AppendText(rtb, $"硬盘 {i + 1}:\n", Color.DarkBlue, false);
                var diskLines = disks[i].Split('\n');
                foreach (var line in diskLines)
                {
                    if (line.Contains("序列号:"))
                    {
                        var parts = line.Split(':');
                        AppendText(rtb, parts[0] + ": ", Color.Black, false);
                        AppendText(rtb, parts[1].Trim() + "\n", Color.Red, true);
                    }
                    else
                    {
                        AppendText(rtb, line + "\n", Color.Black, false);
                    }
                }
                if (i < disks.Count - 1)
                {
                    AppendText(rtb, "\n", Color.Black, false);
                }
            }
        }
        else
        {
            AppendText(rtb, "  未找到硬盘信息\n", Color.Gray, false);
        }
        AppendText(rtb, "\n----------------------------------------\n\n", Color.Gray, false);

        // 网卡信息
        AppendText(rtb, "【网卡信息】\n", Color.DarkGreen, true);
        var macs = HardwareInfoHelper.GetMacAddresses();
        if (macs.Count > 0)
        {
            for (int i = 0; i < macs.Count; i++)
            {
                AppendText(rtb, $"网卡 {i + 1}:\n", Color.DarkBlue, false);
                var macLines = macs[i].Split('\n');
                foreach (var line in macLines)
                {
                    if (line.Contains("MAC:"))
                    {
                        var parts = line.Split(':');
                        AppendText(rtb, parts[0] + ": ", Color.Black, false);
                        // MAC地址格式: XX:XX:XX:XX:XX:XX
                        var macAddr = string.Join(":", parts.Skip(1));
                        AppendText(rtb, macAddr.Trim() + "\n", Color.Blue, true);
                    }
                    else
                    {
                        AppendText(rtb, line + "\n", Color.Black, false);
                    }
                }
                if (i < macs.Count - 1)
                {
                    AppendText(rtb, "\n", Color.Black, false);
                }
            }
        }
        else
        {
            AppendText(rtb, "  未找到网卡信息\n", Color.Gray, false);
        }
        AppendText(rtb, "\n========================================\n", Color.DarkBlue, true);
    }

    private void AppendText(RichTextBox rtb, string text, Color color, bool bold)
    {
        int start = rtb.TextLength;
        rtb.AppendText(text);
        int end = rtb.TextLength;

        rtb.Select(start, end - start);
        rtb.SelectionColor = color;
        if (bold)
        {
            rtb.SelectionFont = new Font(rtb.Font, FontStyle.Bold);
        }
        rtb.Select(end, 0);
    }
}
