﻿
namespace Hoardor
{
    partial class MainGUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainGUI));
            uploadTargetFile = new Button();
            progressUpload = new ProgressBar();
            HoardorLog = new ListBox();
            commandBox = new TextBox();
            SuspendLayout();
            // 
            // uploadTargetFile
            // 
            uploadTargetFile.Location = new Point(784, 460);
            uploadTargetFile.Name = "uploadTargetFile";
            uploadTargetFile.Size = new Size(201, 52);
            uploadTargetFile.TabIndex = 0;
            uploadTargetFile.Text = "Upload";
            uploadTargetFile.UseVisualStyleBackColor = true;
            uploadTargetFile.Click += uploadTargetFile_Click;
            // 
            // progressUpload
            // 
            progressUpload.Location = new Point(12, 431);
            progressUpload.Name = "progressUpload";
            progressUpload.Size = new Size(973, 23);
            progressUpload.TabIndex = 1;
            // 
            // HoardorLog
            // 
            HoardorLog.FormattingEnabled = true;
            HoardorLog.ItemHeight = 15;
            HoardorLog.Location = new Point(12, 460);
            HoardorLog.Name = "HoardorLog";
            HoardorLog.Size = new Size(767, 49);
            HoardorLog.TabIndex = 2;
            HoardorLog.SelectedIndexChanged += HoardorLog_SelectedIndexChanged;
            // 
            // commandBox
            // 
            commandBox.Location = new Point(693, 402);
            commandBox.Name = "commandBox";
            commandBox.Size = new Size(292, 23);
            commandBox.TabIndex = 3;
            commandBox.KeyDown += commandBox_KeyDown;
            // 
            // MainGUI
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveBorder;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Center;
            ClientSize = new Size(997, 524);
            Controls.Add(commandBox);
            Controls.Add(HoardorLog);
            Controls.Add(progressUpload);
            Controls.Add(uploadTargetFile);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "MainGUI";
            Text = "Hoardor";
            ResumeLayout(false);
            PerformLayout();
        }

        private void HoardorLog_SelectedIndexChanged(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        #endregion

        private Button uploadTargetFile;
        private ProgressBar progressUpload;
        private ListBox HoardorLog;
        private TextBox commandBox;
    }
}
