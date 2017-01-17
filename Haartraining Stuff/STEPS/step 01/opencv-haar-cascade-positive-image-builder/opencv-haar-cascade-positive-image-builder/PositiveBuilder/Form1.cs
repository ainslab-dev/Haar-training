using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Xml;
using System.Configuration;
using System.Diagnostics;
using System.IO;

using OpenCvSharp;
using OpenCvSharp.UserInterface;

#region The MIT License
/*
The MIT License

Copyright (c) 2010 David J Barnes

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE. 
*/
#endregion

namespace PositiveBuilder
{
    public partial class Form1 : Form
    {
        private CvCapture FrameCapture = Cv.CreateFileCapture(ConfigurationManager.AppSettings["VideoFile"].ToString());
        private IplImage Frame,FrameBak,FrameSave;
        private bool FirstClick = true;
        private int x1, x2, y1, y2, width, height, ImgCount = Convert.ToInt32(ConfigurationManager.AppSettings["ImgIndexBegin"]);
        
        public Form1()
        {
            InitializeComponent();
            this.KeyPreview = true;

            //this.KeyPress += new KeyPressEventHandler(Form_KeyPress);
            this.ImgFrame.MouseClick += new MouseEventHandler(Frame_DrawCrop);
            this.ImgFrame.MouseMove += new MouseEventHandler(Frame_MouseMove);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Frame = FrameCapture.QueryFrame();
            FrameBak = Cv.CreateImage(Cv.GetSize(Frame), Frame.Depth, Frame.NChannels);
            FrameSave = Cv.CreateImage(Cv.GetSize(Frame), Frame.Depth, Frame.NChannels);
            Cv.Copy(Frame, FrameBak, null);
            Cv.Copy(Frame, FrameSave, null);

            this.ImgFrame.Image = Frame.ToBitmap();
        }

        private void WritePositive(string WriteData)
        {
            StreamWriter sw = File.AppendText(ConfigurationManager.AppSettings["PositivesFile"].ToString());
            sw.WriteLine(WriteData);
            sw.Close();
        }
        
        void Form_KeyPress(object sender, KeyPressEventArgs e)
        {
            string KeyPressed = e.KeyChar.ToString().ToLower();

            //'S' key pressed
            //Save region
            if ((e.KeyChar == 115) && (width > 0))
            {

                //Thread.Sleep(5000);
                string SaveImageAs = ConfigurationManager.AppSettings["ImgDirectory"].ToString() + ConfigurationManager.AppSettings["ImgNamePrefix"].ToString() + "_" + ImgCount + ".jpg";

                FrameSave.SaveImage(SaveImageAs);

                WritePositive(SaveImageAs + " 1 " + x1 + " " + y1 + " " + width + " " + height);

                ImgCount++;

                this.CropCount.Text = "Images saved: " + ImgCount.ToString();

                Frame = FrameCapture.QueryFrame();
                this.ImgFrame.Image = Frame.ToBitmap();


            }

            //'Q' key pressed
            //Next frame
            if (e.KeyChar == 113)
            {
                Frame = FrameCapture.QueryFrame();
                FrameBak = Cv.CreateImage(Cv.GetSize(Frame), Frame.Depth, Frame.NChannels);
                Cv.Copy(Frame, FrameBak, null);
                Cv.Copy(Frame, FrameSave, null);

                this.ImgFrame.Image = Frame.ToBitmap();
            }

            //'N' key pressed
            //Negative
            if (e.KeyChar == 110)
            {
                ImgCount++;

                this.ImgFrame.Image.Save(ConfigurationManager.AppSettings["ImgNegatives"].ToString() + "Negative_" + ImgCount + ".jpg");
            }

            //'Escape' key pressed
            if (e.KeyChar == 27)
            {
                this.Close();
            }
        }

        void Frame_DrawCrop(object sender, MouseEventArgs e)
        {
            if (FirstClick)
            {
                x1 = e.X;
                y1 = e.Y;
                
                Cv.Copy(Frame, FrameBak, null);

                FirstClick = false;
            }
            else
            {
                x2 = e.X;
                y2 = e.Y;
                width = x2 - x1;
                height = y2 - y1;

                if ((x2 > x1) && (y2 > y1))
                {
                    Cv.Copy(Frame, FrameBak, null);

                    this.ImgFrame.Image = Frame.ToBitmap();

                    FirstClick = true;
                }
                else
                {
                    FirstClick = true;

                    MessageBox.Show("Please start in the upper left corner. Proceed to the lower left corner.");

                    this.ImgFrame.Image = FrameBak.ToBitmap();
                }
            }
        }

        void Frame_MouseMove(object sender, MouseEventArgs e)
        {
            Cv.Copy(FrameBak, Frame, null);
            Cv.DrawLine(Frame, new CvPoint(0, e.Y), new CvPoint(Frame.Width, e.Y), new CvColor(0, 255, 0));
            Cv.DrawLine(Frame, new CvPoint(e.X, 0), new CvPoint(e.X, Frame.Height), new CvColor(0, 255, 0));

            this.ImgFrame.Image = Frame.ToBitmap(); 
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("IExplore", "http://creativecommons.org/licenses/MIT/");
        }

    }
}