namespace kaConv
{
    partial class FormMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            buttonGet = new Button();
            label1 = new Label();
            textBoxDir = new TextBox();
            label2 = new Label();
            textBoxUrl = new TextBox();
            label3 = new Label();
            SuspendLayout();
            // 
            // buttonGet
            // 
            buttonGet.Location = new Point(282, 115);
            buttonGet.Name = "buttonGet";
            buttonGet.Size = new Size(104, 23);
            buttonGet.TabIndex = 5;
            buttonGet.Text = "取得";
            buttonGet.UseVisualStyleBackColor = true;
            buttonGet.Click += buttonGet_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 16);
            label1.Name = "label1";
            label1.Size = new Size(60, 15);
            label1.TabIndex = 0;
            label1.Text = "ログフォルダ";
            // 
            // textBoxDir
            // 
            textBoxDir.Location = new Point(78, 13);
            textBoxDir.Name = "textBoxDir";
            textBoxDir.Size = new Size(573, 23);
            textBoxDir.TabIndex = 2;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 73);
            label2.Name = "label2";
            label2.Size = new Size(46, 15);
            label2.TabIndex = 3;
            label2.Text = "スレURL";
            // 
            // textBoxUrl
            // 
            textBoxUrl.Location = new Point(78, 70);
            textBoxUrl.Name = "textBoxUrl";
            textBoxUrl.Size = new Size(573, 23);
            textBoxUrl.TabIndex = 4;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(78, 39);
            label3.Name = "label3";
            label3.Size = new Size(423, 15);
            label3.TabIndex = 6;
            label3.Text = "（例：C:\\Data　※トラブルを避けるためギコナビのログフォルダは指定しないでください。）";
            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(669, 155);
            Controls.Add(label3);
            Controls.Add(textBoxUrl);
            Controls.Add(label2);
            Controls.Add(textBoxDir);
            Controls.Add(label1);
            Controls.Add(buttonGet);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormMain";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "kaConv";
            FormClosing += FormMain_FormClosing;
            Load += FormMain_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button buttonGet;
        private Label label1;
        private TextBox textBoxDir;
        private Label label2;
        private TextBox textBoxUrl;
        private Label label3;
    }
}
