using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RGB_CanalsBlender;

public class CanalsBlender : Form
{
    public enum Canal
    {
        Red,
        Green,
        Blue
    }

    private PictureBox R_Picture;

    private Panel panel1;

    private Panel panel2;

    private PictureBox G_Picture;

    private Panel panel3;

    private PictureBox B_Picture;

    private Panel panel5;

    private PictureBox T_Package;

    private Panel panel4;

    private Panel panel6;
    private Button Btn_Delete;
    private Button Btn_Download;

    public CanalsBlender()
    {
        InitializeComponent();
        Control.CheckForIllegalCrossThreadCalls = false;
    }

    private void CanalsBlender_Load(object sender, EventArgs e)
    {
        R_Picture.AllowDrop = true;
        G_Picture.AllowDrop = true;
        B_Picture.AllowDrop = true;
    }

    private void CompileCanals()
    {
        Bitmap bitmap = ((R_Picture.Image != null) ? new Bitmap(R_Picture.Image) : null);
        Bitmap bitmap2 = ((G_Picture.Image != null) ? new Bitmap(G_Picture.Image) : null);
        Bitmap bitmap3 = ((B_Picture.Image != null) ? new Bitmap(B_Picture.Image) : null);
        Bitmap bitmap4 = new(bitmap?.Width ?? bitmap2?.Width ?? bitmap3.Width, bitmap?.Height ?? bitmap2?.Height ?? bitmap3.Height);
        for (int i = 0; i < bitmap4.Width; i++)
        {
            for (int j = 0; j < bitmap4.Height; j++)
            {
                Color color = bitmap?.GetPixel(i, j) ?? Color.Black;
                Color color2 = bitmap2?.GetPixel(i, j) ?? Color.Black;
                Color color3 = bitmap3?.GetPixel(i, j) ?? Color.Black;
                Color color4 = Color.FromArgb(color.R, color2.G, color3.B);
                bitmap4.SetPixel(i, j, color4);
            }
        }
        T_Package.Image = bitmap4;
    }

    private void DecompileCanals()
    {
        Bitmap originalImage = new(T_Package.Image);
        R_Picture.Image = ConvertToBlackAndWhite(GetChannel(originalImage, Canal.Red));
        G_Picture.Image = ConvertToBlackAndWhite(GetChannel(originalImage, Canal.Green));
        B_Picture.Image = ConvertToBlackAndWhite(GetChannel(originalImage, Canal.Blue));
    }

    private Bitmap GetChannel(Bitmap originalImage, Canal channel)
    {
        Bitmap bitmap = new(originalImage.Width, originalImage.Height);
        for (int i = 0; i < originalImage.Height; i++)
        {
            for (int j = 0; j < originalImage.Width; j++)
            {
                Color pixel = originalImage.GetPixel(j, i);
                Color color = Color.Black;
                switch (channel)
                {
                    case Canal.Red:
                        color = Color.FromArgb(pixel.R, pixel.R, pixel.R);
                        break;
                    case Canal.Green:
                        color = Color.FromArgb(pixel.G, pixel.G, pixel.G);
                        break;
                    case Canal.Blue:
                        color = Color.FromArgb(pixel.B, pixel.B, pixel.B);
                        break;
                }
                bitmap.SetPixel(j, i, color);
            }
        }
        return bitmap;
    }

    private Bitmap ConvertToBlackAndWhite(Bitmap image)
    {
        Bitmap bitmap = new(image.Width, image.Height);
        for (int i = 0; i < image.Height; i++)
        {
            for (int j = 0; j < image.Width; j++)
            {
                Color pixel = image.GetPixel(j, i);
                int num = (pixel.R + pixel.G + pixel.B) / 3;
                Color color = Color.FromArgb(num, num, num);
                bitmap.SetPixel(j, i, color);
            }
        }
        return bitmap;
    }

    private void SaveImage(PictureBox pictureBox, string title)
    {
        // Vérification image présente
        if (pictureBox?.Image == null)
        {
            MessageBox.Show(
                "Please drag and drop any image to download..",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            return;
        }

        using SaveFileDialog dialog = new()
        {
            Title = $"Save {title}",
            Filter = "PNG (*.png)|*.png|TGA (*.TGA)|*.TGA",
            DefaultExt = "png",
            AddExtension = false
        };

        if (dialog.ShowDialog() != DialogResult.OK)
            return;

        try
        {
            ImageFormat format = dialog.FilterIndex switch
            {
                1 => ImageFormat.Png,
                2 => ImageFormat.Jpeg,
                _ => throw new NotImplementedException(),
            };

            pictureBox.Image.Save(dialog.FileName, format);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Échec de l’enregistrement :\n{ex.Message}",
                "Erreur",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }

    private async void ShowDroppedImage(DragEventArgs e, PictureBox SelectedPictureBox, bool CompileCanal)
    {
        string[] array = (string[])e.Data.GetData(DataFormats.FileDrop);
        if (array.Length == 0)
        {
            return;
        }
        switch (Path.GetExtension(array[0]).ToLower())
        {
            case ".jpg":
            case ".jpeg":
            case ".png":
            case ".TGA":
                {
                    if (SelectedPictureBox.Image == null)
                    {
                    }
                    else
                    {
                        SelectedPictureBox.Image.Dispose();
                    }
                    Image image = Image.FromFile(array[0]);
                    SelectedPictureBox.Image = image;
                    if (CompileCanal)
                    {
                        await Task.Run(delegate
                        {
                            CompileCanals();
                        });
                    }
                    else
                    {
                        await Task.Run(delegate
                        {
                            DecompileCanals();
                        });
                    }
                    break;
                }
            default:
                MessageBox.Show("The file must be a JPEG, JPG or PNG.", "Invalid File", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                break;
        }
    }

    private void pictureBoxs_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effect = DragDropEffects.Copy;
        }
        else
        {
            e.Effect = DragDropEffects.None;
        }
    }

    private void R_Picture_DragDrop(object sender, DragEventArgs e)
    {
        ShowDroppedImage(e, R_Picture, CompileCanal: true);
    }

    private void G_Picture_DragDrop(object sender, DragEventArgs e)
    {
        ShowDroppedImage(e, G_Picture, CompileCanal: true);
    }

    private void B_Picture_DragDrop(object sender, DragEventArgs e)
    {
        ShowDroppedImage(e, B_Picture, CompileCanal: true);
    }

    private void InitializeComponent()
    {
#pragma warning disable IDE0090 // Utiliser 'new(...)'
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CanalsBlender));
#pragma warning restore IDE0090 // Utiliser 'new(...)'
        this.R_Picture = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.G_Picture = new System.Windows.Forms.PictureBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.B_Picture = new System.Windows.Forms.PictureBox();
            this.panel5 = new System.Windows.Forms.Panel();
            this.T_Package = new System.Windows.Forms.PictureBox();
            this.panel4 = new System.Windows.Forms.Panel();
            this.panel6 = new System.Windows.Forms.Panel();
            this.Btn_Download = new System.Windows.Forms.Button();
            this.Btn_Delete = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.R_Picture)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.G_Picture)).BeginInit();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.B_Picture)).BeginInit();
            this.panel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.T_Package)).BeginInit();
            this.SuspendLayout();
            // 
            // R_Picture
            // 
            this.R_Picture.BackColor = System.Drawing.Color.Black;
            this.R_Picture.Cursor = System.Windows.Forms.Cursors.Default;
            this.R_Picture.Location = new System.Drawing.Point(1, 1);
            this.R_Picture.Name = "R_Picture";
            this.R_Picture.Size = new System.Drawing.Size(173, 173);
            this.R_Picture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.R_Picture.TabIndex = 0;
            this.R_Picture.TabStop = false;
            this.R_Picture.DragDrop += new System.Windows.Forms.DragEventHandler(this.R_Picture_DragDrop);
            this.R_Picture.DragEnter += new System.Windows.Forms.DragEventHandler(this.pictureBoxs_DragEnter);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Red;
            this.panel1.Controls.Add(this.R_Picture);
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(175, 175);
            this.panel1.TabIndex = 3;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.Green;
            this.panel2.Controls.Add(this.G_Picture);
            this.panel2.Location = new System.Drawing.Point(193, 13);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(175, 175);
            this.panel2.TabIndex = 4;
            // 
            // G_Picture
            // 
            this.G_Picture.BackColor = System.Drawing.Color.Black;
            this.G_Picture.Cursor = System.Windows.Forms.Cursors.Default;
            this.G_Picture.Location = new System.Drawing.Point(1, 1);
            this.G_Picture.Name = "G_Picture";
            this.G_Picture.Size = new System.Drawing.Size(173, 173);
            this.G_Picture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.G_Picture.TabIndex = 0;
            this.G_Picture.TabStop = false;
            this.G_Picture.DragDrop += new System.Windows.Forms.DragEventHandler(this.G_Picture_DragDrop);
            this.G_Picture.DragEnter += new System.Windows.Forms.DragEventHandler(this.pictureBoxs_DragEnter);
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.Blue;
            this.panel3.Controls.Add(this.B_Picture);
            this.panel3.Location = new System.Drawing.Point(374, 12);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(175, 175);
            this.panel3.TabIndex = 5;
            // 
            // B_Picture
            // 
            this.B_Picture.BackColor = System.Drawing.Color.Black;
            this.B_Picture.Cursor = System.Windows.Forms.Cursors.Default;
            this.B_Picture.Location = new System.Drawing.Point(1, 1);
            this.B_Picture.Name = "B_Picture";
            this.B_Picture.Size = new System.Drawing.Size(173, 173);
            this.B_Picture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.B_Picture.TabIndex = 0;
            this.B_Picture.TabStop = false;
            this.B_Picture.DragDrop += new System.Windows.Forms.DragEventHandler(this.B_Picture_DragDrop);
            this.B_Picture.DragEnter += new System.Windows.Forms.DragEventHandler(this.pictureBoxs_DragEnter);
            // 
            // panel5
            // 
            this.panel5.BackColor = System.Drawing.Color.Green;
            this.panel5.Controls.Add(this.T_Package);
            this.panel5.Controls.Add(this.panel4);
            this.panel5.Controls.Add(this.panel6);
            this.panel5.Location = new System.Drawing.Point(193, 194);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(175, 175);
            this.panel5.TabIndex = 6;
            // 
            // T_Package
            // 
            this.T_Package.BackColor = System.Drawing.Color.Black;
            this.T_Package.Cursor = System.Windows.Forms.Cursors.Default;
            this.T_Package.Location = new System.Drawing.Point(1, 1);
            this.T_Package.Name = "T_Package";
            this.T_Package.Size = new System.Drawing.Size(173, 173);
            this.T_Package.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.T_Package.TabIndex = 0;
            this.T_Package.TabStop = false;
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.Red;
            this.panel4.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel4.Location = new System.Drawing.Point(0, 0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(60, 175);
            this.panel4.TabIndex = 8;
            // 
            // panel6
            // 
            this.panel6.BackColor = System.Drawing.Color.Blue;
            this.panel6.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel6.Location = new System.Drawing.Point(115, 0);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(60, 175);
            this.panel6.TabIndex = 7;
            // 
            // Btn_Download
            // 
            this.Btn_Download.AutoSize = true;
            this.Btn_Download.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Btn_Download.ForeColor = System.Drawing.Color.White;
            this.Btn_Download.Location = new System.Drawing.Point(193, 405);
            this.Btn_Download.Name = "Btn_Download";
            this.Btn_Download.Size = new System.Drawing.Size(175, 25);
            this.Btn_Download.TabIndex = 10;
            this.Btn_Download.Text = "Download";
            this.Btn_Download.UseVisualStyleBackColor = true;
            this.Btn_Download.Click += new System.EventHandler(this.Btn_Download_Click);
            // 
            // Btn_Delete
            // 
            this.Btn_Delete.AutoSize = true;
            this.Btn_Delete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Btn_Delete.ForeColor = System.Drawing.Color.White;
            this.Btn_Delete.Location = new System.Drawing.Point(193, 374);
            this.Btn_Delete.Name = "Btn_Delete";
            this.Btn_Delete.Size = new System.Drawing.Size(175, 25);
            this.Btn_Delete.TabIndex = 11;
            this.Btn_Delete.Text = "Delete images";
            this.Btn_Delete.UseVisualStyleBackColor = true;
            this.Btn_Delete.Click += new System.EventHandler(this.Btn_Delete_Click);
            // 
            // CanalsBlender
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(10)))), ((int)(((byte)(10)))));
            this.ClientSize = new System.Drawing.Size(559, 436);
            this.Controls.Add(this.Btn_Delete);
            this.Controls.Add(this.Btn_Download);
            this.Controls.Add(this.panel5);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(575, 475);
            this.MinimumSize = new System.Drawing.Size(575, 475);
            this.Name = "CanalsBlender";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CanalsBlender";
            this.Load += new System.EventHandler(this.CanalsBlender_Load);
            ((System.ComponentModel.ISupportInitialize)(this.R_Picture)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.G_Picture)).EndInit();
            this.panel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.B_Picture)).EndInit();
            this.panel5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.T_Package)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    private void Btn_Download_Click(object sender, EventArgs e)
    {
        SaveImage(T_Package, "Blended Image");
    }

    private void Btn_Delete_Click(object sender, EventArgs e)
    {
        if (T_Package.Image == null)
        {
        }
        else
        {
            T_Package.Image.Dispose();
            T_Package.Image = null;
        }

        if (R_Picture.Image == null)
        {
        }
        else
        {
            R_Picture.Image.Dispose();
            R_Picture.Image = null;
        }

        if (G_Picture.Image == null)
        {
        }
        else
        {
            G_Picture.Image.Dispose();
            G_Picture.Image = null;
        }

        if (B_Picture.Image == null)
        {
            return;
        }
        B_Picture.Image.Dispose();
        B_Picture.Image = null;
    }
}
