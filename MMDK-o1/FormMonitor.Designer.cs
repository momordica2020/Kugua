namespace MMDK
{
    partial class FormMonitor
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            tbMmdk = new System.Windows.Forms.TextBox();
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            清空日志ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            button1 = new System.Windows.Forms.Button();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            textInputTest = new System.Windows.Forms.TextBox();
            button2 = new System.Windows.Forms.Button();
            textLocalTest = new System.Windows.Forms.TextBox();
            button3 = new System.Windows.Forms.Button();
            menuStrip1 = new System.Windows.Forms.MenuStrip();
            文件ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            打开ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            bot配置ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            启动ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            lbUpdateTime = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            lbUseNum = new System.Windows.Forms.Label();
            lbGroupNum = new System.Windows.Forms.Label();
            label15 = new System.Windows.Forms.Label();
            lbFriendNum = new System.Windows.Forms.Label();
            label13 = new System.Windows.Forms.Label();
            label12 = new System.Windows.Forms.Label();
            lbQQ = new System.Windows.Forms.Label();
            label10 = new System.Windows.Forms.Label();
            lbTimeSpan = new System.Windows.Forms.Label();
            label8 = new System.Windows.Forms.Label();
            lbBeginTime = new System.Windows.Forms.Label();
            label6 = new System.Windows.Forms.Label();
            lbPort = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            lbVersion = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            lbState = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            pbCPU = new System.Windows.Forms.ProgressBar();
            lbCPU = new System.Windows.Forms.Label();
            lbMem = new System.Windows.Forms.Label();
            pbMem = new System.Windows.Forms.ProgressBar();
            contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            menuStrip1.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // tbMmdk
            // 
            tbMmdk.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            tbMmdk.BorderStyle = System.Windows.Forms.BorderStyle.None;
            tbMmdk.ContextMenuStrip = contextMenuStrip1;
            tbMmdk.Dock = System.Windows.Forms.DockStyle.Right;
            tbMmdk.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
            tbMmdk.ForeColor = System.Drawing.SystemColors.Window;
            tbMmdk.Location = new System.Drawing.Point(781, 32);
            tbMmdk.Margin = new System.Windows.Forms.Padding(4);
            tbMmdk.MaxLength = 32767000;
            tbMmdk.Multiline = true;
            tbMmdk.Name = "tbMmdk";
            tbMmdk.ReadOnly = true;
            tbMmdk.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            tbMmdk.Size = new System.Drawing.Size(619, 734);
            tbMmdk.TabIndex = 0;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { 清空日志ToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(153, 34);
            // 
            // 清空日志ToolStripMenuItem
            // 
            清空日志ToolStripMenuItem.Name = "清空日志ToolStripMenuItem";
            清空日志ToolStripMenuItem.Size = new System.Drawing.Size(152, 30);
            清空日志ToolStripMenuItem.Text = "清空日志";
            清空日志ToolStripMenuItem.Click += 清空日志ToolStripMenuItem_Click;
            // 
            // button1
            // 
            button1.BackColor = System.Drawing.Color.Red;
            button1.Font = new System.Drawing.Font("宋体", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
            button1.ForeColor = System.Drawing.Color.Yellow;
            button1.Location = new System.Drawing.Point(13, 36);
            button1.Margin = new System.Windows.Forms.Padding(4);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(169, 55);
            button1.TabIndex = 1;
            button1.Text = "波特启动";
            button1.UseVisualStyleBackColor = false;
            button1.Click += button1_Click;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            splitContainer1.IsSplitterFixed = true;
            splitContainer1.Location = new System.Drawing.Point(0, 0);
            splitContainer1.Margin = new System.Windows.Forms.Padding(4);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(flowLayoutPanel1);
            splitContainer1.Panel1.Controls.Add(button3);
            splitContainer1.Panel1.Controls.Add(tbMmdk);
            splitContainer1.Panel1.Controls.Add(button1);
            splitContainer1.Panel1.Controls.Add(menuStrip1);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(tableLayoutPanel1);
            splitContainer1.Size = new System.Drawing.Size(1820, 766);
            splitContainer1.SplitterDistance = 1400;
            splitContainer1.TabIndex = 2;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(textInputTest);
            flowLayoutPanel1.Controls.Add(button2);
            flowLayoutPanel1.Controls.Add(textLocalTest);
            flowLayoutPanel1.Enabled = false;
            flowLayoutPanel1.Location = new System.Drawing.Point(202, 36);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new System.Drawing.Size(567, 710);
            flowLayoutPanel1.TabIndex = 7;
            flowLayoutPanel1.Visible = false;
            // 
            // textInputTest
            // 
            textInputTest.BorderStyle = System.Windows.Forms.BorderStyle.None;
            textInputTest.ContextMenuStrip = contextMenuStrip1;
            textInputTest.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
            textInputTest.Location = new System.Drawing.Point(4, 4);
            textInputTest.Margin = new System.Windows.Forms.Padding(4);
            textInputTest.MaxLength = 32767000;
            textInputTest.Multiline = true;
            textInputTest.Name = "textInputTest";
            textInputTest.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            textInputTest.Size = new System.Drawing.Size(567, 148);
            textInputTest.TabIndex = 4;
            textInputTest.TextChanged += textInputTest_TextChanged;
            // 
            // button2
            // 
            button2.BackColor = System.Drawing.Color.CadetBlue;
            button2.Font = new System.Drawing.Font("微软雅黑", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
            button2.ForeColor = System.Drawing.Color.Yellow;
            button2.Location = new System.Drawing.Point(4, 160);
            button2.Margin = new System.Windows.Forms.Padding(4);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(169, 49);
            button2.TabIndex = 6;
            button2.Text = "发送";
            button2.UseVisualStyleBackColor = false;
            button2.Click += button2_Click;
            // 
            // textLocalTest
            // 
            textLocalTest.BackColor = System.Drawing.SystemColors.ActiveCaption;
            textLocalTest.BorderStyle = System.Windows.Forms.BorderStyle.None;
            textLocalTest.ContextMenuStrip = contextMenuStrip1;
            textLocalTest.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
            textLocalTest.Location = new System.Drawing.Point(4, 217);
            textLocalTest.Margin = new System.Windows.Forms.Padding(4);
            textLocalTest.MaxLength = 32767000;
            textLocalTest.Multiline = true;
            textLocalTest.Name = "textLocalTest";
            textLocalTest.ReadOnly = true;
            textLocalTest.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            textLocalTest.Size = new System.Drawing.Size(567, 400);
            textLocalTest.TabIndex = 3;
            // 
            // button3
            // 
            button3.BackColor = System.Drawing.Color.CadetBlue;
            button3.Font = new System.Drawing.Font("微软雅黑", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
            button3.ForeColor = System.Drawing.Color.Yellow;
            button3.Location = new System.Drawing.Point(13, 253);
            button3.Margin = new System.Windows.Forms.Padding(4);
            button3.Name = "button3";
            button3.Size = new System.Drawing.Size(169, 49);
            button3.TabIndex = 7;
            button3.Text = "储存数据";
            button3.UseVisualStyleBackColor = false;
            button3.Click += button3_Click;
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { 文件ToolStripMenuItem, bot配置ToolStripMenuItem });
            menuStrip1.Location = new System.Drawing.Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new System.Drawing.Size(1400, 32);
            menuStrip1.TabIndex = 5;
            menuStrip1.Text = "menuStrip1";
            // 
            // 文件ToolStripMenuItem
            // 
            文件ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { 打开ToolStripMenuItem });
            文件ToolStripMenuItem.Name = "文件ToolStripMenuItem";
            文件ToolStripMenuItem.Size = new System.Drawing.Size(62, 28);
            文件ToolStripMenuItem.Text = "文件";
            // 
            // 打开ToolStripMenuItem
            // 
            打开ToolStripMenuItem.Name = "打开ToolStripMenuItem";
            打开ToolStripMenuItem.Size = new System.Drawing.Size(146, 34);
            打开ToolStripMenuItem.Text = "打开";
            // 
            // bot配置ToolStripMenuItem
            // 
            bot配置ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { 启动ToolStripMenuItem });
            bot配置ToolStripMenuItem.Name = "bot配置ToolStripMenuItem";
            bot配置ToolStripMenuItem.Size = new System.Drawing.Size(92, 28);
            bot配置ToolStripMenuItem.Text = "bot配置";
            // 
            // 启动ToolStripMenuItem
            // 
            启动ToolStripMenuItem.Name = "启动ToolStripMenuItem";
            启动ToolStripMenuItem.Size = new System.Drawing.Size(146, 34);
            启动ToolStripMenuItem.Text = "启动";
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Inset;
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 39.58333F));
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60.41667F));
            tableLayoutPanel1.Controls.Add(lbUpdateTime, 1, 2);
            tableLayoutPanel1.Controls.Add(label3, 0, 2);
            tableLayoutPanel1.Controls.Add(lbUseNum, 0, 11);
            tableLayoutPanel1.Controls.Add(lbGroupNum, 0, 10);
            tableLayoutPanel1.Controls.Add(label15, 0, 10);
            tableLayoutPanel1.Controls.Add(lbFriendNum, 0, 9);
            tableLayoutPanel1.Controls.Add(label13, 0, 9);
            tableLayoutPanel1.Controls.Add(label12, 0, 8);
            tableLayoutPanel1.Controls.Add(lbQQ, 0, 8);
            tableLayoutPanel1.Controls.Add(label10, 0, 8);
            tableLayoutPanel1.Controls.Add(lbTimeSpan, 1, 5);
            tableLayoutPanel1.Controls.Add(label8, 0, 5);
            tableLayoutPanel1.Controls.Add(lbBeginTime, 1, 4);
            tableLayoutPanel1.Controls.Add(label6, 0, 4);
            tableLayoutPanel1.Controls.Add(lbPort, 1, 3);
            tableLayoutPanel1.Controls.Add(label4, 0, 3);
            tableLayoutPanel1.Controls.Add(lbVersion, 1, 1);
            tableLayoutPanel1.Controls.Add(label2, 0, 1);
            tableLayoutPanel1.Controls.Add(lbState, 1, 0);
            tableLayoutPanel1.Controls.Add(label1, 0, 0);
            tableLayoutPanel1.Controls.Add(pbCPU, 1, 6);
            tableLayoutPanel1.Controls.Add(lbCPU, 0, 6);
            tableLayoutPanel1.Controls.Add(lbMem, 0, 7);
            tableLayoutPanel1.Controls.Add(pbMem, 1, 7);
            tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 12;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 75F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 63F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 63F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 12F));
            tableLayoutPanel1.Size = new System.Drawing.Size(416, 766);
            tableLayoutPanel1.TabIndex = 2;
            // 
            // lbUpdateTime
            // 
            lbUpdateTime.AutoSize = true;
            lbUpdateTime.Dock = System.Windows.Forms.DockStyle.Fill;
            lbUpdateTime.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
            lbUpdateTime.Location = new System.Drawing.Point(170, 78);
            lbUpdateTime.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbUpdateTime.Name = "lbUpdateTime";
            lbUpdateTime.Size = new System.Drawing.Size(240, 40);
            lbUpdateTime.TabIndex = 25;
            lbUpdateTime.Text = "-";
            lbUpdateTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Dock = System.Windows.Forms.DockStyle.Fill;
            label3.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
            label3.Location = new System.Drawing.Point(6, 78);
            label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(154, 40);
            label3.TabIndex = 24;
            label3.Text = "更新日期";
            label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbUseNum
            // 
            lbUseNum.AutoSize = true;
            lbUseNum.Dock = System.Windows.Forms.DockStyle.Fill;
            lbUseNum.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
            lbUseNum.Location = new System.Drawing.Point(170, 576);
            lbUseNum.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbUseNum.Name = "lbUseNum";
            lbUseNum.Size = new System.Drawing.Size(240, 188);
            lbUseNum.TabIndex = 23;
            lbUseNum.Text = "0";
            lbUseNum.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbGroupNum
            // 
            lbGroupNum.AutoSize = true;
            lbGroupNum.Dock = System.Windows.Forms.DockStyle.Fill;
            lbGroupNum.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
            lbGroupNum.Location = new System.Drawing.Point(170, 524);
            lbGroupNum.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbGroupNum.Name = "lbGroupNum";
            lbGroupNum.Size = new System.Drawing.Size(240, 50);
            lbGroupNum.TabIndex = 22;
            lbGroupNum.Text = "0";
            lbGroupNum.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Dock = System.Windows.Forms.DockStyle.Fill;
            label15.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
            label15.Location = new System.Drawing.Point(6, 576);
            label15.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label15.Name = "label15";
            label15.Size = new System.Drawing.Size(154, 188);
            label15.TabIndex = 21;
            label15.Text = "调用次数";
            label15.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbFriendNum
            // 
            lbFriendNum.AutoSize = true;
            lbFriendNum.Dock = System.Windows.Forms.DockStyle.Fill;
            lbFriendNum.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
            lbFriendNum.Location = new System.Drawing.Point(170, 477);
            lbFriendNum.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbFriendNum.Name = "lbFriendNum";
            lbFriendNum.Size = new System.Drawing.Size(240, 45);
            lbFriendNum.TabIndex = 20;
            lbFriendNum.Text = "0";
            lbFriendNum.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Dock = System.Windows.Forms.DockStyle.Fill;
            label13.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
            label13.Location = new System.Drawing.Point(6, 524);
            label13.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label13.Name = "label13";
            label13.Size = new System.Drawing.Size(154, 50);
            label13.TabIndex = 19;
            label13.Text = "群数";
            label13.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Dock = System.Windows.Forms.DockStyle.Fill;
            label12.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
            label12.Location = new System.Drawing.Point(6, 431);
            label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label12.Name = "label12";
            label12.Size = new System.Drawing.Size(154, 44);
            label12.TabIndex = 18;
            label12.Text = "Bot QQ";
            label12.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbQQ
            // 
            lbQQ.AutoSize = true;
            lbQQ.Dock = System.Windows.Forms.DockStyle.Fill;
            lbQQ.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
            lbQQ.Location = new System.Drawing.Point(170, 431);
            lbQQ.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbQQ.Name = "lbQQ";
            lbQQ.Size = new System.Drawing.Size(240, 44);
            lbQQ.TabIndex = 17;
            lbQQ.Text = "0";
            lbQQ.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Dock = System.Windows.Forms.DockStyle.Fill;
            label10.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
            label10.Location = new System.Drawing.Point(6, 477);
            label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label10.Name = "label10";
            label10.Size = new System.Drawing.Size(154, 45);
            label10.TabIndex = 16;
            label10.Text = "好友数";
            label10.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbTimeSpan
            // 
            lbTimeSpan.AutoSize = true;
            lbTimeSpan.Dock = System.Windows.Forms.DockStyle.Fill;
            lbTimeSpan.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
            lbTimeSpan.Location = new System.Drawing.Point(170, 239);
            lbTimeSpan.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbTimeSpan.Name = "lbTimeSpan";
            lbTimeSpan.Size = new System.Drawing.Size(240, 63);
            lbTimeSpan.TabIndex = 15;
            lbTimeSpan.Text = "222天\r\n10小时33分22秒";
            lbTimeSpan.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Dock = System.Windows.Forms.DockStyle.Fill;
            label8.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
            label8.Location = new System.Drawing.Point(6, 239);
            label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(154, 63);
            label8.TabIndex = 14;
            label8.Text = "运行时长";
            label8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbBeginTime
            // 
            lbBeginTime.AutoSize = true;
            lbBeginTime.Dock = System.Windows.Forms.DockStyle.Fill;
            lbBeginTime.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
            lbBeginTime.Location = new System.Drawing.Point(170, 162);
            lbBeginTime.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbBeginTime.Name = "lbBeginTime";
            lbBeginTime.Size = new System.Drawing.Size(240, 75);
            lbBeginTime.TabIndex = 13;
            lbBeginTime.Text = "2020-11-22 11:22:33";
            lbBeginTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Dock = System.Windows.Forms.DockStyle.Fill;
            label6.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
            label6.Location = new System.Drawing.Point(6, 162);
            label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(154, 75);
            label6.TabIndex = 12;
            label6.Text = "启动时间";
            label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbPort
            // 
            lbPort.AutoSize = true;
            lbPort.Dock = System.Windows.Forms.DockStyle.Fill;
            lbPort.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
            lbPort.Location = new System.Drawing.Point(170, 120);
            lbPort.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbPort.Name = "lbPort";
            lbPort.Size = new System.Drawing.Size(240, 40);
            lbPort.TabIndex = 11;
            lbPort.Text = "9999";
            lbPort.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Dock = System.Windows.Forms.DockStyle.Fill;
            label4.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
            label4.Location = new System.Drawing.Point(6, 120);
            label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(154, 40);
            label4.TabIndex = 4;
            label4.Text = "Mirai";
            label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbVersion
            // 
            lbVersion.AutoSize = true;
            lbVersion.Dock = System.Windows.Forms.DockStyle.Fill;
            lbVersion.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
            lbVersion.Location = new System.Drawing.Point(170, 44);
            lbVersion.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbVersion.Name = "lbVersion";
            lbVersion.Size = new System.Drawing.Size(240, 32);
            lbVersion.TabIndex = 10;
            lbVersion.Text = "v 0.1.0";
            lbVersion.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Dock = System.Windows.Forms.DockStyle.Fill;
            label2.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
            label2.Location = new System.Drawing.Point(6, 44);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(154, 32);
            label2.TabIndex = 3;
            label2.Text = "版本";
            label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbState
            // 
            lbState.AutoSize = true;
            lbState.Dock = System.Windows.Forms.DockStyle.Fill;
            lbState.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
            lbState.Location = new System.Drawing.Point(170, 2);
            lbState.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbState.Name = "lbState";
            lbState.Size = new System.Drawing.Size(240, 40);
            lbState.TabIndex = 3;
            lbState.Text = "初始";
            lbState.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Dock = System.Windows.Forms.DockStyle.Fill;
            label1.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
            label1.Location = new System.Drawing.Point(6, 2);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(154, 40);
            label1.TabIndex = 0;
            label1.Text = "运行状态";
            label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pbCPU
            // 
            pbCPU.Dock = System.Windows.Forms.DockStyle.Fill;
            pbCPU.Location = new System.Drawing.Point(170, 308);
            pbCPU.Margin = new System.Windows.Forms.Padding(4);
            pbCPU.Name = "pbCPU";
            pbCPU.Size = new System.Drawing.Size(240, 55);
            pbCPU.TabIndex = 4;
            // 
            // lbCPU
            // 
            lbCPU.AutoSize = true;
            lbCPU.Dock = System.Windows.Forms.DockStyle.Fill;
            lbCPU.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
            lbCPU.Location = new System.Drawing.Point(6, 304);
            lbCPU.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbCPU.Name = "lbCPU";
            lbCPU.Size = new System.Drawing.Size(154, 63);
            lbCPU.TabIndex = 5;
            lbCPU.Text = "CPU\r\n(100%)";
            lbCPU.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbMem
            // 
            lbMem.AutoSize = true;
            lbMem.Dock = System.Windows.Forms.DockStyle.Fill;
            lbMem.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
            lbMem.Location = new System.Drawing.Point(6, 369);
            lbMem.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbMem.Name = "lbMem";
            lbMem.Size = new System.Drawing.Size(154, 60);
            lbMem.TabIndex = 8;
            lbMem.Text = "内存\r\n(0%)";
            lbMem.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pbMem
            // 
            pbMem.Dock = System.Windows.Forms.DockStyle.Fill;
            pbMem.Location = new System.Drawing.Point(170, 373);
            pbMem.Margin = new System.Windows.Forms.Padding(4);
            pbMem.Name = "pbMem";
            pbMem.Size = new System.Drawing.Size(240, 52);
            pbMem.TabIndex = 9;
            // 
            // FormMonitor
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1820, 766);
            Controls.Add(splitContainer1);
            Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
            MainMenuStrip = menuStrip1;
            Margin = new System.Windows.Forms.Padding(4);
            MinimumSize = new System.Drawing.Size(890, 722);
            Name = "FormMonitor";
            Text = "MIraiKUgua";
            FormClosing += Form1_FormClosing;
            Load += Form1_Load;
            contextMenuStrip1.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel1.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TextBox tbMmdk;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label lbState;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ProgressBar pbCPU;
        private System.Windows.Forms.Label lbCPU;
        private System.Windows.Forms.Label lbMem;
        private System.Windows.Forms.ProgressBar pbMem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 清空日志ToolStripMenuItem;
        private System.Windows.Forms.Label lbUseNum;
        private System.Windows.Forms.Label lbGroupNum;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label lbFriendNum;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label lbQQ;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label lbTimeSpan;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label lbBeginTime;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lbPort;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lbVersion;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textInputTest;
        private System.Windows.Forms.TextBox textLocalTest;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 文件ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bot配置ToolStripMenuItem;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ToolStripMenuItem 启动ToolStripMenuItem;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.ToolStripMenuItem 打开ToolStripMenuItem;
        private System.Windows.Forms.Label lbUpdateTime;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}

