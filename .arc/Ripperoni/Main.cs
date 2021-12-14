﻿using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.WindowsAPICodePack.Dialogs;

using YoutubeDLSharp;
using YoutubeDLSharp.Options;
using YoutubeDLSharp.Metadata;

namespace Ripperoni
{
    public partial class Main : Form
    {
        private Point mouselocation;

        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            Format.SelectedItem = ".MP4";

            Format_SelectedIndexChanged(sender, e);
        
            //Input.Text = "https://youtu.be/dQw4w9WgXcQ";
            Input.Text = "https://www.youtube.com/watch?v=tPEE9ZwTmy0";
            Output.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);


            Record rec = new Record();
            rec.ControlTitle = "Never Gonna Give You Up";
            rec.ControlImage = @"https://i.ytimg.com/vi_webp/tPEE9ZwTmy0/sddefault.webp";
            rec.LoadImage();            
            tableLayoutPanel1.Controls.Add(rec);
            //rec.Refresh();

            Record rec2 = new Record();
            tableLayoutPanel1.Controls.Add(rec2);

            Record rec3 = new Record();
            tableLayoutPanel1.Controls.Add(rec3);

        }

        protected override CreateParams CreateParams
        {
            get
            {
                const int CS_DROPSHADOW = 0x20000;
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }

        #region Handle Bar
        private void Exit_Click(object sender, EventArgs e)
        {
            ExitProcess();
        }

        private void Minimize_Click(object sender, EventArgs e)
        {
            MinimizeProcess();
        }

        private void HandleLayout_MouseDown(object sender, MouseEventArgs e)
        {
            mouselocation = e.Location;
        }

        private void HandleLayout_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int dx = e.Location.X - mouselocation.X;
                int dy = e.Location.Y - mouselocation.Y;
                this.Location = new Point(this.Location.X + dx, this.Location.Y + dy);
            }
        }

        private void Icon_MouseDown(object sender, MouseEventArgs e)
        {
            mouselocation = e.Location;
        }

        private void Icon_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int dx = e.Location.X - mouselocation.X;
                int dy = e.Location.Y - mouselocation.Y;
                this.Location = new Point(this.Location.X + dx, this.Location.Y + dy);
            }
        }

        private void Title_MouseDown(object sender, MouseEventArgs e)
        {
            mouselocation = e.Location;
        }

        private void Title_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int dx = e.Location.X - mouselocation.X;
                int dy = e.Location.Y - mouselocation.Y;
                this.Location = new Point(this.Location.X + dx, this.Location.Y + dy);
            }
        }
        #endregion

        #region Main UI
        private void Format_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (Format.SelectedItem)
            {
                case ".MP4":
                case ".WebM":
                case ".FLV":
                case ".3GP":
                case ".MOV":
                case ".AVI":
                    Elements.Items.Clear();
                    Elements.Items.Add("Audio/Video");
                    Elements.Items.Add("Video Only");
                    Elements.SelectedItem = "Audio/Video";

                    Resolution.Items.Clear();
                    Resolution.Items.Add("4320p");
                    Resolution.Items.Add("2160p");
                    Resolution.Items.Add("1440p");
                    Resolution.Items.Add("1080p");
                    Resolution.Items.Add("720p");
                    Resolution.Items.Add("480p");
                    Resolution.Items.Add("360p");
                    Resolution.Items.Add("240p");
                    Resolution.Items.Add("144p");
                    Resolution.SelectedItem = "1080p";
                    break;
                default:
                    Elements.Items.Clear();
                    Elements.Items.Add("Audio Only");
                    Elements.SelectedItem = "Audio Only";

                    Resolution.Items.Clear();
                    Resolution.Items.Add("None");
                    Resolution.SelectedItem = "None";
                    break;
            }

            switch (Format.SelectedItem)
            {
                case "-- Video --":
                    Format.SelectedItem = ".MP4";
                    break;
                case "-- Audio --":
                    Format.SelectedItem = ".MP3";
                    break;
            }
        }

        private void OpenFolder_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog
            {
                InitialDirectory = Output.Text,
                IsFolderPicker = true
            };
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Output.Text = dialog.FileName;
            }
        }

        private void Convert_Click(object sender, EventArgs e)
        {
            if (Path.IsPathRooted(Output.Text))
            {
                try
                {
                    Path.GetFullPath(Output.Text);

                    try
                    {
                        Directory.CreateDirectory(Output.Text);

                        Rip();
                    }
                    catch
                    {
                        ErrorProcess("Could not create directory...", "Error");
                    }
                }
                catch
                {
                    ErrorProcess("Please enter a valid path...", "Error");
                }
            }
            else
            {
                ErrorProcess("Please enter a global path...", "Error");
            }
        }

        private void Support_Click(object sender, EventArgs e)
        {
            Process.Start("https://ytdl-org.github.io/youtube-dl/supportedsites.html");
        }

        private void Metadata_Click(object sender, EventArgs e)
        {
            Metadata metadata = new Metadata(Input.Text);
            metadata.ShowDialog();
        }
        #endregion

        #region Ripper
        private void Rip()
        {
            FetchData();

            FetchMedia();
        }

        private async void FetchData()
        {
            var youtube = new YoutubeDL
            {
                YoutubeDLPath = "ytdlp.exe",
                FFmpegPath = "ffmpeg.exe"
            };

            var res = await youtube.RunVideoDataFetch(Input.Text);
            VideoData video = res.Data;
            string title = video.Title;
            string uploader = video.Uploader;
            DateTime date = video.UploadDate ?? default;
            float length = video.Duration ?? default;
            string thumbnailData = video.Thumbnail;

            FormatData[] asdf = video.Formats;

            //VideoTitle.Text = title;
            //VideoUploader.Text = uploader;
            //VideoLength.Text = TimeSpan.FromSeconds(length).ToString(@"hh\:mm\:ss");
            //VideoDate.Text = date.ToString("MM/dd/yyyy");
        }

        private async void FetchMedia()
        {
            var youtube = new YoutubeDL
            {
                YoutubeDLPath = "ytdlp.exe",
                FFmpegPath = "ffmpeg.exe",

                OutputFolder = Output.Text
            };

            if ((string)Elements.SelectedItem == "Video Only")
            {
                await youtube.RunVideoDownload(Input.Text, recodeFormat: VideoRecodeFormat.Mp4);
            }
            else if ((string)Elements.SelectedItem == "Audio Only")
            {
                await youtube.RunAudioDownload(Input.Text, AudioConversionFormat.Mp3);
            }
            else
            {
                await youtube.RunVideoDownload(Input.Text, "bestvideo+bestaudio/best", DownloadMergeFormat.Unspecified, VideoRecodeFormat.Mp4);
            }

            // // a progress handler with a callback that updates a progress bar
            // var progress = new Progress<DownloadProgress>(p => progressBar.Value = p.Progress);
            // // a cancellation token source used for cancelling the download
            // // use `cts.Cancel();` to perform cancellation
            // var cts = new CancellationTokenSource();
            // // ...
            // await ytdl.RunVideoDownload(Input.Text, progress: progress, ct: cts.Token);
        }
        #endregion

        #region Footer
        private void Settings_Click(object sender, EventArgs e)
        {
            Settings settings = new Settings();
            settings.ShowDialog();
        }

        private void Repository_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.github.com/aproxus/ripperoni");
        }

        private void Website_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.aprox.us/service/ripperoni");
        }

        private void FooterIcon_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.aprox.us/");
        }

        private void Settings_MouseHover(object sender, EventArgs e)
        {
            Font font = new Font("Segoe UI", 8.25f, FontStyle.Underline);
            Settings.Font = font;
        }

        private void Repository_MouseHover(object sender, EventArgs e)
        {
            Font font = new Font("Segoe UI", 8.25f, FontStyle.Underline);
            Repository.Font = font;
        }

        private void Website_MouseHover(object sender, EventArgs e)
        {
            Font font = new Font("Segoe UI", 8.25f, FontStyle.Underline);
            Website.Font = font;
        }

        private void Website_MouseLeave(object sender, EventArgs e)
        {
            Font font = new Font("Segoe UI", 8.25f);
            Website.Font = font;
        }

        private void Repository_MouseLeave(object sender, EventArgs e)
        {
            Font font = new Font("Segoe UI", 8.25f);
            Repository.Font = font;
        }

        private void Settings_MouseLeave(object sender, EventArgs e)
        {
            Font font = new Font("Segoe UI", 8.25f);
            Settings.Font = font;
        }
        #endregion

        #region Auxiliary
        private void ErrorProcess(string m, string t)
        {
            MessageBox.Show(m, t, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        
        private void MinimizeProcess()
        {
            WindowState = FormWindowState.Minimized;
        }

        private void ExitProcess()
        {
            Close();
        }
        #endregion
    }
}
