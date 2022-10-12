using Newtonsoft.Json;
using Ookii.Dialogs.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;

namespace Image_Crop
{
    public partial class Form1 : Form
    {
        Rectangle rect;
        int pixelsCounter = 0;
        Color SelectedColor = Color.LightGreen;
        List<DrawingRectangle> DrawingRects = new List<DrawingRectangle>();
        Bitmap rectImage;
        int saveRectanglesCounter = 1;
        bool drawBorder = true;
        bool clearRectangles = true;
        bool saveRectangles = true;
        //bool drawnNewRectangle = false; // To think how to work with this flag bool variable
        // where to use it how and how to solce this problem !!!!!
        string rectangleName;
        Dictionary<string, string> FileList = new Dictionary<string, string>();
        string selectedPath;
        int x, y;
        bool addToListBox = false;

        public Form1()
        {
            InitializeComponent();

            //LoadFile(textBox1, )

            textBox2.Text = Properties.Settings.Default.CroppedImagesFolder;
            selectedPath = textBox2.Text;

            if (textBox1.Text != "")
            {
                pictureBox2.Image = System.Drawing.Image.FromFile(textBox1.Text);
            }
            
            checkBoxDrawBorder.Checked = true;
            checkBoxClearRectangles.Checked = true;
            checkBoxSaveRectangles.Checked = true;

            // To make here loadfile and use other places the savefile !!!!!
            // or even before in start of constructor to use loadfile ? !!!!!

            if (selectedPath != "" && selectedPath != null)
            {
                if (System.IO.File.Exists(Path.Combine(selectedPath, "rectangles.txt")))
                {
                    string g = System.IO.File.ReadAllText(Path.Combine(selectedPath, "rectangles.txt"));
                    FileList = JsonConvert.DeserializeObject<Dictionary<string, string>>(g);
                    listBox1.DataSource = FileList.ToList();
                }
            }

            int counter = 1;
            for(int i = 2; i < 79; i++)
            {
                counter++;
            }
        }

        public class DrawingRectangle
        {
            public Rectangle Rect => new Rectangle(Location, Size);
            public Size Size { get; set; }
            public Point Location { get; set; }
            public Control Owner { get; set; }
            public Point StartPosition { get; set; }
            public Color DrawingcColor { get; set; } = Color.LightGreen;
            public float PenSize { get; set; } = 3f;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pictureBox2_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            //addToListBox = true;

            if (pictureBox2.Image != null && selectedPath != null)
            {
                DrawingRects.Add(new DrawingRectangle()
                {
                    Location = e.Location,
                    Size = Size.Empty,
                    StartPosition = e.Location,
                    Owner = (Control)sender,
                    DrawingcColor = SelectedColor // <= Shape's Border Color
                });
            }
        }

        private void pictureBox2_MouseMove(object sender, MouseEventArgs e)
        {
            int X = e.X;
            int Y = e.Y;

            if (e.Button != MouseButtons.Left) return;

            if ((X >= 0 && X <= pictureBox2.Width) && (Y >= 0 && Y <= pictureBox2.Height))
            {
                if (pictureBox2.Image != null && selectedPath != null)
                {
                    x = e.X;
                    y = e.Y;

                    var dr = DrawingRects[DrawingRects.Count - 1];
                    if (e.Y < dr.StartPosition.Y) { dr.Location = new Point(dr.Rect.Location.X, e.Y); }
                    if (e.X < dr.StartPosition.X) { dr.Location = new Point(e.X, dr.Rect.Location.Y); }

                    dr.Size = new Size(Math.Abs(dr.StartPosition.X - e.X), Math.Abs(dr.StartPosition.Y - e.Y));
                    pictureBox2.Invalidate();
                }
            }
        }

        private void pictureBox2_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            //if (addToListBox == false) return;

            if (DrawingRects.Count > 0 && pictureBox2.Image != null && selectedPath != null)
            {
                // In the mouseup evebt here to make it will save and add information to
                // the listBox only if trying to draw inside the image in the
                // picturebox2 if the image is smaller then the pictureBox2.
                // like i did with this line in the pictureBox2 paint event :
                // if ((x >= 0 && x <= pictureBox2.Image.Size.Width) && (y >= 0 && y <= pictureBox2.Image.Size.Height))
                // maybe to use a flag in the pictureBox paint event so this line will affect also here
                // in the mouse move events and maybe if needed also in other mouse events too !!!!!

                // The last drawn shape
                var dr = DrawingRects.Last();
                if (dr.Rect.Width > 0 && dr.Rect.Height > 0)
                {
                    // Not working good yet with the addToListBox flag.
                    // to check why here in this event mouseup and other mouse event.
                    // and in the pictureBox2 paint event. !!!!!

                    rectImage = cropAtRect((Bitmap)pictureBox2.Image, dr.Rect);
                    if (saveRectangles)
                    {


                        rectangleName = GetNextName(Path.Combine(selectedPath, "Rectangle"), ".bmp");

                        //rectangleName = @"d:\Rectangles\rectangle" + saveRectanglesCounter + ".bmp";
                        FileList.Add($"{dr.Location}, {dr.Size}", rectangleName);
                        string json = JsonConvert.SerializeObject(
    FileList,
    Formatting.Indented // this for pretty print
);
                        using (StreamWriter sw = new StreamWriter(Path.Combine(selectedPath, "rectangles.txt"), false))
                        {
                            sw.Write(json);
                            sw.Close();
                        }

                        rectImage.Save(rectangleName);
                        saveRectanglesCounter++;
                    }
                    pixelsCounter = rect.Width * rect.Height;

                    pictureBox1.Invalidate();

                    listBox1.DataSource = FileList.ToList();
                    listBox1.SelectedIndex = listBox1.Items.Count - 1;

                    pictureBox2.Focus();
                    Graphics g = Graphics.FromImage(this.pictureBox1.Image);
                    g.Clear(this.pictureBox1.BackColor);
                }
            }
        }

        string GetNextName(string baseName, string extension)
        {
            int counter = 1;
            string nextName = baseName + counter + extension;
            while (System.IO.File.Exists(nextName))
            {
                counter++;
                nextName = baseName + counter + extension;
            }
            return nextName;
        }

        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, pictureBox2.ClientRectangle, Color.Red, ButtonBorderStyle.Solid);

            if (pictureBox2.Image != null && selectedPath != null)
            {
                if ((x >= 0 && x <= pictureBox2.Image.Size.Width) && (y >= 0 && y <= pictureBox2.Image.Size.Height))
                {
                    //addToListBox = true;

                    DrawShapes(e.Graphics);
                }
                else
                {
                    //addToListBox = false;
                }
            }
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (drawBorder)
            {
                ControlPaint.DrawBorder(e.Graphics, pictureBox1.ClientRectangle, Color.Red, ButtonBorderStyle.Solid);
            }

            //if (addToListBox == false) return;

            if (rectImage != null && DrawingRects.Count > 0)
            {
                var dr = DrawingRects.Last();
                e.Graphics.DrawImage(rectImage, dr.Rect);

                if (clearRectangles)
                {
                    DrawingRects.Clear();
                    pictureBox2.Invalidate();
                }
            }
        }

        private void DrawShapes(Graphics g)
        {
            if (DrawingRects.Count == 0) return;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            foreach (var dr in DrawingRects)
            {
                using (Pen pen = new Pen(dr.DrawingcColor, dr.PenSize))
                {
                    g.DrawRectangle(pen, dr.Rect);
                };
            }
        }

        public Bitmap cropAtRect(Bitmap b, Rectangle r)
        {
            Bitmap nb = new Bitmap(r.Width, r.Height);
            using (Graphics g = Graphics.FromImage(nb))
            {
                g.DrawImage(b, -r.X, -r.Y);
                return nb;
            }
        }

        private void checkBoxDrawBorder_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxDrawBorder.Checked)
            {
                drawBorder = true;
            }
            else
            {
                drawBorder = false;
            }

            pictureBox1.Invalidate();
        }

        private void checkBoxClearRectangles_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxClearRectangles.Checked)
            {
                clearRectangles = true;
            }
            else
            {
                clearRectangles = false;
            }

            pictureBox2.Invalidate();
        }

        private void checkBoxSaveRectangles_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var item = ((ListBox)sender).SelectedItem;
            var itemCast = (KeyValuePair<string, string>)item;
            pictureBox1.Image = System.Drawing.Image.FromFile(itemCast.Value);

            // Here to make that it will show the cropped image in pictureBox1
            // in the position where it was taken in pictureBox2
            // and not like now that its showing it in position 0,0 in pictureBox1.
            // !!!!!
        }

        private void lblImageToCrop_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.ShowDialog();
        }

        private void SaveFile(string contentToSave, string fileName)
        {
            string applicationPath = Path.GetFullPath(System.AppDomain.CurrentDomain.BaseDirectory); // the directory that your program is installed in
            string saveFilePath = Path.Combine(applicationPath, fileName);
            File.WriteAllText(saveFilePath, contentToSave);
        }

        private void LoadFile(TextBox loadTo, string fileName)
        {
            string applicationPath = Path.GetFullPath(System.AppDomain.CurrentDomain.BaseDirectory); // the directory that your program is installed in
            string saveFilePath = Path.Combine(applicationPath, fileName); // add a file name to this path.  This is your full file path.

            if (File.Exists(saveFilePath))
            {
                loadTo.Text = File.ReadAllText(saveFilePath);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            VistaOpenFileDialog dialog = new VistaOpenFileDialog();
            {
                dialog.Filter = "JPG,BMP|*.jpg;*.bmp";
            };
            
            if(dialog.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = dialog.FileName;

                Bitmap bitmap = new Bitmap(dialog.FileName);
                if(bitmap.Size != new Size(512,512))
                {
                    //var size = pictureBox2.Image.Size;
                    //pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
                }
                else
                {
                    //var size = pictureBox2.Image.Size;
                    //pictureBox2.SizeMode = PictureBoxSizeMode.Normal;
                }

                pictureBox2.Image = Image.FromFile(dialog.FileName);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = dialog.SelectedPath;
                selectedPath = dialog.SelectedPath;

                Properties.Settings.Default.CroppedImagesFolder = selectedPath;
                Properties.Settings.Default.Save();
            }
        }
    }
}

// Still i can draw a rectangle with height 0 !!!!! to check why and to check if i can draw
// also a rectangle when width is 0. i should not be able to draw rectangle when width or 
// height are 0 if both or one of them !!!!! 

// To draw border in red around each pictureBox1 and 2 to see the pictureBoxes edges !!!!!
// also to limit the mouse movement to be only in the pictureBox are from the inside.
// now i can draw inside the pictureBox2 out(inside) !!!!!