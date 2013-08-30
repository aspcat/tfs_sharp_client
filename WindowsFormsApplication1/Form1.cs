using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.IO;

using RestSharp;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        private bool isFullScreen = false;
        private int formHeight;
        private int formWidth;
        private int picBoxHeight;
        private int picBoxWidth;
        private int picBoxLeft;
        private int picBoxTop;

        Image bufferpic;//加快GDI读取用缓存图片
        Point M_pot_p = new Point();//原始位置 
        int M_int_mx = 0, M_int_my = 0;//下次能继续 
        int M_int_maxX, M_int_maxY;//加快读取用 

        public Form1()
        {
            InitializeComponent();
            //双缓存
            base.SetStyle(ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint, true);
            base.SetStyle(ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);

            foreach (Control c in this.Controls)
            {
                if (c is PictureBox)
                {
                    c.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseMove);
                    c.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseDown);
                    c.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox1_Paint);
                }
            }
        }

        public string strURL = string.Empty;
        public string strLocalFile = string.Empty;
        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog  fld = new OpenFileDialog();

            //fld.Description = "选择一个文件";

            if (fld.ShowDialog() == DialogResult.OK)
            {
                this.txtLocalFile.Text = fld.FileName;
                strLocalFile = txtLocalFile.Text;

                if (txtRemoteFile.Text.Trim() == string.Empty)
                {
                    FileInfo fln = new FileInfo(strLocalFile);
                    txtRemoteFile.Text = "/"+ fln.Name;
                }
            }
            else
            {
                fld.Dispose();
            }
        }


        private void btnWriteFile_Click(object sender, EventArgs e)
        {

            if (txtAppUrl.Text.Trim() == string.Empty || txtRemoteFile.Text.Trim() == string.Empty)
            {
                MessageBox.Show("请填写应用URL 和 远程文件名 ");
                return;
            }
            strURL = txtAppUrl.Text + "/file" + txtRemoteFile.Text;
            if (!System.IO.File.Exists(txtLocalFile.Text))
            {
                MessageBox.Show("选择一个文件");
                return;
            }
            //var client1 = new RestClient(strURL);
            //var request1 = new RestRequest("", Method.HEAD);
            //IRestResponse response1 = client1.Execute(request1);
            //switch ((int)response1.StatusCode)
            //{
            //    case 200:
            //        //if (response1.ContentLength > 0)
            //        //{
            //            if (MessageBox.Show("远程文件已经存在是否要追加写入", "远程文件存在", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
            //            {
            //                return;
            //            }
            //        //}
            //        break;
            //}

            var client = new RestClient(strURL);
            var request = new RestRequest("", Method.PUT);
            request.AddFile("", strLocalFile);
            IRestResponse response = client.Execute(request);
            switch ((int)response.StatusCode)
            {
                case 200: MessageBox.Show("写入成功，可以访问"); txtURI.Text = strURL; break;
                case 404: MessageBox.Show("文件不存在，请先创建"); break;
                default: MessageBox.Show(response.StatusDescription); break;
            }

            //switch ((int)response.StatusCode)
            //{
            //    case 200: Console.WriteLine("创建成功"); break;
            //}
            var content = response.Content; //
        
        }

        private void btnCreateFile_Click(object sender, EventArgs e)
        {
            if (txtAppUrl.Text.Trim() == string.Empty || txtRemoteFile.Text.Trim() == string.Empty)
            {
                MessageBox.Show("请填写应用URL 和 远程文件名 ");
                return;
            }
            strURL = txtAppUrl.Text + "/file" + txtRemoteFile.Text;
            var request = new RestRequest("", Method.POST);
            if (chkRecursive.Checked)
            {
                strURL += "?recursive=1";
            }
            var client = new RestClient(strURL);
            IRestResponse response = client.Execute(request);
            switch ((int)response.StatusCode)
            {
                case 201: MessageBox.Show("创建成功"); break;
                case 409: MessageBox.Show("文件已经存在"); break;
                default: MessageBox.Show(response.StatusDescription); break;
            }
        }

        private void btnDeleteFile_Click(object sender, EventArgs e)
        {
            if (txtAppUrl.Text.Trim() == string.Empty || txtRemoteFile.Text.Trim() == string.Empty)
            {
                MessageBox.Show("请填写应用URL 和 远程文件名 ");
                return;
            }
            strURL = txtAppUrl.Text + "/file" + txtRemoteFile.Text;
            var client = new RestClient(strURL);
            var request = new RestRequest("", Method.DELETE);
            IRestResponse response = client.Execute(request);
            switch ((int)response.StatusCode)
            {
                case 200: MessageBox.Show("删除成功"); break;
                case 404: MessageBox.Show("文件不存在"); break;
                default: MessageBox.Show(response.StatusDescription); break;
            }

        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtLocalFile.Text = "";
            txtRemoteFile.Text = "";
            txtURI.Text = "";
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnMoveFile_Click(object sender, EventArgs e)
        {
            if (txtAppUrl.Text.Trim() == string.Empty || txtRemoteFile.Text.Trim() == string.Empty || txtRemoteFileDest.Text.Trim() == string.Empty)
            {
                MessageBox.Show("请填写应用URL 和 远程文件 和 移动/重命名文件");
                return;
            }
            strURL = txtAppUrl.Text + "/file" + txtRemoteFileDest.Text;
            var request = new RestRequest("", Method.POST);
            if (chkRecursive.Checked)
            {
                strURL += "?recursive=1";
            }
            var client = new RestClient(strURL);
            request.AddHeader("x-ali-move-source", txtRemoteFile.Text);
            IRestResponse response = client.Execute(request);
            switch ((int)response.StatusCode)
            {
                case 200: MessageBox.Show("操作成功"); break;
                case 404: MessageBox.Show("源文件不存在，或目的文件已存在，或目的文件父目录不存在"); break;
                default: MessageBox.Show(response.StatusDescription); break;
            }

        }

        private void btnDeleteFolder_Click(object sender, EventArgs e)
        {
            if (txtAppUrl.Text.Trim() == string.Empty || txtRemoteFile.Text.Trim() == string.Empty)
            {
                MessageBox.Show("请填写应用URL 和 远程文件夹 ");
                return;
            }
            strURL = txtAppUrl.Text + "/dir" + txtRemoteFile.Text;
            var client = new RestClient(strURL);
            var request = new RestRequest("", Method.DELETE);
            IRestResponse response = client.Execute(request);
            switch ((int)response.StatusCode)
            {
                case 200: MessageBox.Show("删除成功"); break;
                case 404: MessageBox.Show("目录或父目录不存在"); break;
                default: MessageBox.Show(response.StatusDescription); break;
            }
        }

        private void btnCreateFolder_Click(object sender, EventArgs e)
        {
            if (txtAppUrl.Text.Trim() == string.Empty || txtRemoteFile.Text.Trim() == string.Empty)
            {
                MessageBox.Show("请填写应用URL 和 远程文件夹 ");
                return;
            }
            strURL = txtAppUrl.Text + "/dir" + txtRemoteFile.Text;
            var request = new RestRequest("", Method.POST);
            if (chkRecursive.Checked)
            {
                strURL += "?recursive=1";
            }
            var client = new RestClient(strURL);
            IRestResponse response = client.Execute(request);
            switch ((int)response.StatusCode)
            {
                case 201: MessageBox.Show("创建成功"); break;
                case 409: MessageBox.Show("目录已存在"); break;
                default: MessageBox.Show(response.StatusDescription); break;
            }
        }

        private void btnFileInfo_Click(object sender, EventArgs e)
        {
            if (txtAppUrl.Text.Trim() == string.Empty || txtRemoteFile.Text.Trim() == string.Empty)
            {
                MessageBox.Show("请填写应用URL 和 远程文件名 ");
                return;
            }
            strURL = "http://192.168.1.101:83/v2/tfs1/metadata/3/10001/file" + txtRemoteFile.Text;
            var client = new RestClient(strURL);
            var request = new RestRequest("", Method.GET);
            IRestResponse response = client.Execute(request);
            //switch ((int)response.StatusCode)
            //{
            //    case 200: MessageBox.Show("删除成功"); break;
            //    case 404: MessageBox.Show("文件不存在"); break;
            //    default: MessageBox.Show(response.StatusDescription); break;
            //}

            var content = response.Content; //
            MessageBox.Show(content);
        }

        private void button5_Click(object sender, EventArgs e)
        {

            if (txtAppUrl.Text.Trim() == string.Empty || txtRemoteFile.Text.Trim() == string.Empty)
            {
                MessageBox.Show("请填写应用URL 和 远程文件名 ");
                return;
            }
            strURL = txtAppUrl.Text +"/file" + txtRemoteFile.Text;
            var client = new RestClient(strURL);
            var request = new RestRequest("", Method.HEAD);
            IRestResponse response = client.Execute(request);
            switch ((int)response.StatusCode)
            {
                case 200: MessageBox.Show("文件存在"); break;
                case 404: MessageBox.Show("文件不存在"); break;
                default: MessageBox.Show(response.StatusDescription); break;
            }

            var content = response.Content; //
            MessageBox.Show(content);
        }

        private void btnFolderInfo_Click(object sender, EventArgs e)
        {
            if (txtAppUrl.Text.Trim() == string.Empty || txtRemoteFile.Text.Trim() == string.Empty)
            {
                MessageBox.Show("请填写应用URL 和 远程文件名 ");
                return;
            }
            strURL = "http://192.168.1.101:83/v2/tfs1/metadata/3/10001/dir" + txtRemoteFile.Text;
            var client = new RestClient(strURL);
            var request = new RestRequest("", Method.GET);
            IRestResponse response = client.Execute(request);
            //switch ((int)response.StatusCode)
            //{
            //    case 200: MessageBox.Show("删除成功"); break;
            //    case 404: MessageBox.Show("文件不存在"); break;
            //    default: MessageBox.Show(response.StatusDescription); break;
            //}

            var content = response.Content; //
            MessageBox.Show(content);
        }

        private void btnReadFile_Click(object sender, EventArgs e)
        {
            if (txtAppUrl.Text.Trim() == string.Empty || txtRemoteFile.Text.Trim() == string.Empty)
            {
                MessageBox.Show("请填写应用URL 和 远程文件名 ");
                return;
            }
            strURL = "http://192.168.1.101:83/v2/tfs1/3/10001/file" + txtRemoteFile.Text;
            var client = new RestClient(strURL);
            var request = new RestRequest("", Method.GET);
            IRestResponse response = client.Execute(request);
            switch ((int)response.StatusCode)
            {
                case 200:
                    //MessageBox.Show(response.ContentType);
                           byte[] bt = response.RawBytes;
            MemoryStream ms = new MemoryStream(bt);
            bufferpic = Image.FromStream(ms);
            pictureBox1.Image = bufferpic; 
                    
                    break;
                case 404: MessageBox.Show("文件不存在"); break;
                default: MessageBox.Show(response.StatusDescription); break;
            }

            //var content = response.Content; //
            //MessageBox.Show(content);
   
        }

        private void pictureBox1_DoubleClick(object sender, EventArgs e)
        {
            if (!isFullScreen)
            {
                formHeight = this.Height;
                formWidth = this.Width;
                this.SetVisibleCore(false);
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
                this.SetVisibleCore(true);


                picBoxHeight = pictureBox1.Height;
                picBoxWidth = pictureBox1.Width;
                picBoxLeft = pictureBox1.Left;
                picBoxTop = pictureBox1.Top;

                pictureBox1.Height = this.Height;
                pictureBox1.Width = this.Width;
                pictureBox1.SetBounds(0, 0, this.Width, this.Height);
                isFullScreen = true;

            }
            else
            {
                this.Width = formWidth;
                this.Height = formHeight;
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.WindowState = FormWindowState.Normal;

                pictureBox1.Height = picBoxHeight;
                pictureBox1.Width = picBoxWidth;
                pictureBox1.Left = picBoxLeft;
                pictureBox1.Top = picBoxTop;
                isFullScreen = false;
                Cursor = Cursors.Default;
            }
        }


        /// <summary>
        /// 传入内存缓存中
        /// </summary>
        /// <param name="P_str_path">图片地址</param>
        private void inPutBuffer(string P_str_path)
        {
            FileStream fileS = new FileStream(P_str_path, FileMode.Open);
            bufferpic = Image.FromStream(fileS);
            fileS.Dispose();
        }


        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)//当按左键的时候 
            {
                //算差值 
                M_int_mx = M_int_mx - M_pot_p.X + e.X;
                M_int_my = M_int_my - M_pot_p.Y + e.Y;
                //锁定范围 
                M_int_mx = Math.Min(0, Math.Max(M_int_maxX, M_int_mx));
                M_int_my = Math.Min(0, Math.Max(M_int_maxY, M_int_my));
                pictureBox1.Invalidate();
                M_pot_p = e.Location;
            }
        }
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (bufferpic != null)
            {
                e.Graphics.DrawImage(bufferpic, e.ClipRectangle,
                    new Rectangle(-M_int_mx, -M_int_my, pictureBox1.Width, pictureBox1.Height),
                    GraphicsUnit.Pixel
                );
            }
        }
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            M_pot_p = e.Location;
            M_int_maxX = pictureBox1.Width - bufferpic.Width;
            M_int_maxY = pictureBox1.Height - bufferpic.Height;
            Cursor = Cursors.SizeAll;
        }
    }
}


