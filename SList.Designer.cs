﻿namespace SControls
{
    partial class SList
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором компонентов

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.hScroll = new System.Windows.Forms.HScrollBar();
            this.vScroll = new System.Windows.Forms.VScrollBar();
            this.panel = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // hScroll
            // 
            this.hScroll.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.hScroll.Location = new System.Drawing.Point(1, 229);
            this.hScroll.Name = "hScroll";
            this.hScroll.Size = new System.Drawing.Size(364, 17);
            this.hScroll.TabIndex = 0;
            this.hScroll.ValueChanged += new System.EventHandler(this.hScrolling);
            // 
            // vScroll
            // 
            this.vScroll.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.vScroll.Location = new System.Drawing.Point(365, 1);
            this.vScroll.Name = "vScroll";
            this.vScroll.Size = new System.Drawing.Size(17, 228);
            this.vScroll.TabIndex = 2;
            this.vScroll.ValueChanged += new System.EventHandler(this.vScrolling);
            // 
            // panel
            // 
            this.panel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.panel.Location = new System.Drawing.Point(365, 229);
            this.panel.Name = "panel";
            this.panel.Size = new System.Drawing.Size(18, 18);
            this.panel.TabIndex = 3;
            this.panel.Visible = false;
            // 
            // SList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel);
            this.Controls.Add(this.vScroll);
            this.Controls.Add(this.hScroll);
            this.Name = "SList";
            this.Size = new System.Drawing.Size(383, 247);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.SList_KeyUp);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.SList_MouseDown);
            this.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.SList_PreviewKeyDown);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.HScrollBar hScroll;
        private System.Windows.Forms.VScrollBar vScroll;
        private System.Windows.Forms.Panel panel;
    }
}
