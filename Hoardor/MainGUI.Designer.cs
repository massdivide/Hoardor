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
            uploadTargetFile = new Button();
            progressUpload = new ProgressBar();
            HoardorLog = new ListBox();
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
            progressUpload.Location = new Point(12, 489);
            progressUpload.Name = "progressUpload";
            progressUpload.Size = new Size(766, 23);
            progressUpload.TabIndex = 1;
            // 
            // HoardorLog
            // 
            HoardorLog.FormattingEnabled = true;
            HoardorLog.ItemHeight = 15;
            HoardorLog.Location = new Point(1, 402);
            HoardorLog.Name = "HoardorLog";
            HoardorLog.Size = new Size(767, 79);
            HoardorLog.TabIndex = 2;
            HoardorLog.SelectedIndexChanged += HoardorLog_SelectedIndexChanged;
            // 
            // MainGUI
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(997, 524);
            Controls.Add(HoardorLog);
            Controls.Add(progressUpload);
            Controls.Add(uploadTargetFile);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "MainGUI";
            Text = "Hoardor";
            ResumeLayout(false);
        }

        #endregion

        private Button uploadTargetFile;
        private ProgressBar progressUpload;
        private ListBox HoardorLog;
    }
}
