namespace WinFormsApp1
{
    partial class Form1
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
            btnCaptureDesktopWithoutAudio = new Button();
            button1 = new Button();
            button2 = new Button();
            button3 = new Button();
            button4 = new Button();
            SuspendLayout();
            // 
            // btnCaptureDesktopWithoutAudio
            // 
            btnCaptureDesktopWithoutAudio.Location = new Point(12, 12);
            btnCaptureDesktopWithoutAudio.Name = "btnCaptureDesktopWithoutAudio";
            btnCaptureDesktopWithoutAudio.Size = new Size(250, 23);
            btnCaptureDesktopWithoutAudio.TabIndex = 0;
            btnCaptureDesktopWithoutAudio.Text = "Capturar Desktop sem áudio";
            btnCaptureDesktopWithoutAudio.UseVisualStyleBackColor = true;
            btnCaptureDesktopWithoutAudio.Click += btnCaptureDesktopWithoutAudio_Click;
            // 
            // button1
            // 
            button1.Location = new Point(12, 51);
            button1.Name = "button1";
            button1.Size = new Size(250, 23);
            button1.TabIndex = 1;
            button1.Text = "Capturar Desktop com áudio";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.Location = new Point(13, 88);
            button2.Name = "button2";
            button2.Size = new Size(249, 23);
            button2.TabIndex = 2;
            button2.Text = "Capturar Camera com Áudio";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // button3
            // 
            button3.Location = new Point(17, 126);
            button3.Name = "button3";
            button3.Size = new Size(245, 23);
            button3.TabIndex = 3;
            button3.Text = "Stream de Midia";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // button4
            // 
            button4.Location = new Point(138, 219);
            button4.Name = "button4";
            button4.Size = new Size(124, 23);
            button4.TabIndex = 4;
            button4.Text = "Parar Stream";
            button4.UseVisualStyleBackColor = true;
            button4.Click += button4_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(277, 267);
            Controls.Add(button4);
            Controls.Add(button3);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(btnCaptureDesktopWithoutAudio);
            Name = "Form1";
            Text = "Form1";
            FormClosing += Form1_FormClosing;
            Load += Form1_Load;
            ResumeLayout(false);
        }

        #endregion

        private Button btnCaptureDesktopWithoutAudio;
        private Button button1;
        private Button button2;
        private Button button3;
        private Button button4;
    }
}
