namespace cui
{
    partial class Form2
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.name = new System.Windows.Forms.TextBox();
            this.ip = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.port = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.errorName = new System.Windows.Forms.Label();
            this.errorIP = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "设备名称";
            // 
            // name
            // 
            this.name.Location = new System.Drawing.Point(75, 13);
            this.name.Name = "name";
            this.name.Size = new System.Drawing.Size(205, 20);
            this.name.TabIndex = 1;
            // 
            // ip
            // 
            this.ip.Location = new System.Drawing.Point(75, 39);
            this.ip.Name = "ip";
            this.ip.Size = new System.Drawing.Size(205, 20);
            this.ip.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "IP地址";
            // 
            // port
            // 
            this.port.Location = new System.Drawing.Point(75, 65);
            this.port.Name = "port";
            this.port.Size = new System.Drawing.Size(59, 20);
            this.port.TabIndex = 5;
            this.port.Text = "12345";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 68);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(31, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "端口";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(75, 104);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 6;
            this.button1.Text = "保存";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(157, 103);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 7;
            this.button2.Text = "取消";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // errorName
            // 
            this.errorName.AutoSize = true;
            this.errorName.ForeColor = System.Drawing.Color.Red;
            this.errorName.Location = new System.Drawing.Point(285, 17);
            this.errorName.Name = "errorName";
            this.errorName.Size = new System.Drawing.Size(91, 13);
            this.errorName.TabIndex = 8;
            this.errorName.Text = "设备名称已存在";
            this.errorName.Visible = false;
            // 
            // errorIP
            // 
            this.errorIP.AutoSize = true;
            this.errorIP.ForeColor = System.Drawing.Color.Red;
            this.errorIP.Location = new System.Drawing.Point(285, 42);
            this.errorIP.Name = "errorIP";
            this.errorIP.Size = new System.Drawing.Size(77, 13);
            this.errorIP.TabIndex = 9;
            this.errorIP.Text = "IP地址已存在";
            this.errorIP.Visible = false;
            // 
            // Form2
            // 
            this.AcceptButton = this.button1;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(410, 147);
            this.Controls.Add(this.errorIP);
            this.Controls.Add(this.errorName);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.port);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.ip);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.name);
            this.Controls.Add(this.label1);
            this.Name = "Form2";
            this.Text = "设备信息";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox name;
        private System.Windows.Forms.TextBox ip;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox port;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label errorName;
        private System.Windows.Forms.Label errorIP;
    }
}