using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using Microsoft.VisualBasic.Devices;
using System.IO;

namespace XST
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
        }
        public static Window Window;
        public static Window1 window1;
        public static void SetUp(string Path, string version, string version1,string JavaPath)
        {
            window1 = new Window1();
            window1.Owner = Window;
            window1.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window1.Show();
            WebClient webClient = new WebClient();
            string a = webClient.DownloadString("http://106.14.64.250/" + version + ".html");
            JArray jArray = JArray.Parse(a);
            List<string> vs = new List<string>();
            if (version == "SpongeForge")
            {
                for (int i = 0; i < jArray.Count; i++)
                {
                    JObject jObject = JObject.Parse(jArray[i].ToString());
                    if (jObject["name"].ToString() == version1)
                    {
                        vs.Add(jObject["file"].ToString());
                        vs.Add(jObject["Minecraft"].ToString());
                        vs.Add(jObject["Forge"].ToString());
                    }
                }
                Task task = new Task(() =>
                {
                    for(int i=0;i<vs.Count;i++)
                    {
                        window1.DownloadFile(vs[i], Path + "\\" + System.IO.Path.GetFileName(vs[i]), window1.PB1, window1.Label1);
                    }
                });
                task.Start();
                Thread thread = new Thread(() =>
                {
                    task.Wait();
                    task.Dispose();
                    window1.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        window1.PB1.IsIndeterminate = true;
                        window1.Label1.Content = "正在安装Forge";
                    }));
                    Process process = new Process();
                    process.StartInfo.FileName = JavaPath;
                    process.StartInfo.Arguments = " -jar " + Path  + "\\" + System.IO.Path.GetFileName(vs[2]) + " nogui --installServer";
                    process.StartInfo.WorkingDirectory = Path;
                    process.Start();
                    process.StartInfo.RedirectStandardOutput = true;
                    process.OutputDataReceived += window1.Process_OutputDataReceived;
                    process.WaitForExit();
                    window1.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        window1.Label1.Content = "安装完成";
                        window1.Button.Visibility = Visibility.Visible;
                    }));
                    if (Directory.Exists(Path + "\\mods"))
                    { } else Directory.CreateDirectory(Path + "\\mods");
                    try { File.Move(Path + "\\" + System.IO.Path.GetFileName(vs[0]), Path + "\\mods\\" + System.IO.Path.GetFileName(vs[0])); } catch { }
                    try
                    {
                        Computer computer = new Computer();
                        computer.FileSystem.RenameFile(Path + "\\" + System.IO.Path.GetFileName(vs[2]).Replace("installer", "universal"), "server.jar");
                    } catch { }
                }); 
                thread.Start();
            }
            else if (version == "Forge")
            {
                for (int i = 0; i < jArray.Count; i++)
                {
                    JObject jObject = JObject.Parse(jArray[i].ToString());
                    if (jObject["name"].ToString() == version1)
                    {
                        vs.Add(jObject["file"].ToString());
                        vs.Add(jObject["Minecraft"].ToString());
                    }
                }
                Task task = new Task(() =>
                {
                    for (int i = 0; i < vs.Count; i++)
                    {
                        window1.DownloadFile(vs[i], Path + "\\" + System.IO.Path.GetFileName(vs[i]), window1.PB1, window1.Label1);
                    }
                });
                task.Start();
                Thread thread = new Thread(() =>
                {
                    task.Wait();
                    task.Dispose();
                    window1.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        window1.PB1.IsIndeterminate = true;
                        window1.Label1.Content = "正在安装Forge";
                    }));
                    Process process = new Process();
                    process.StartInfo.FileName = JavaPath;
                    process.StartInfo.Arguments = " -jar " + Path + "\\" + System.IO.Path.GetFileName(vs[0]) + " nogui --installServer";
                    process.StartInfo.WorkingDirectory = Path;
                    process.Start();
                    process.StartInfo.RedirectStandardOutput = true;
                    process.OutputDataReceived += window1.Process_OutputDataReceived;
                    process.WaitForExit();
                    window1.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        window1.Label1.Content = "安装完成";
                        window1.Button.Visibility = Visibility.Visible;
                    }));
                    try
                    {
                        Computer computer = new Computer();
                        computer.FileSystem.RenameFile(Path + "\\" + System.IO.Path.GetFileName(vs[0]).Replace("installer", "universal"), "server.jar");
                    } catch { }
                });
                thread.Start();
            }
            else
            {
                for (int i = 0; i < jArray.Count; i++)
                {
                    JObject jObject = JObject.Parse(jArray[i].ToString());
                    if (jObject["name"].ToString() == version1)
                    {
                        vs.Add(jObject["file"].ToString());
                    }
                }
                Task task = new Task(() =>
                {
                    for (int i = 0; i < vs.Count; i++)
                    {
                        window1.DownloadFile(vs[i], Path + "\\" + System.IO.Path.GetFileName(vs[i]), window1.PB1, window1.Label1);
                    }
                    if (File.Exists(Path + "\\server.jar"))
                        File.Delete(Path + "\\server.jar");
                    Computer computer = new Computer();
                    computer.FileSystem.RenameFile(Path + "\\" + System.IO.Path.GetFileName(vs[0]), "server.jar");
                });
                task.Start();
                Thread thread = new Thread(() =>
                {
                    task.Wait();
                    task.Dispose();
                    window1.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        window1.Label1.Content = "安装完成";
                        window1.Button.Visibility = Visibility.Visible;
                    }));
                });
                thread.Start();
            }
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                Label2.Content = e.Data;
            }));
        }

        public void DownloadFile(string URL, string filename, ProgressBar prog, Label label)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            float percent = 0;
            try
            {
                System.Net.HttpWebRequest Myrq = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(URL);
                System.Net.HttpWebResponse myrp = (System.Net.HttpWebResponse)Myrq.GetResponse();
                long totalBytes = myrp.ContentLength;
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (prog != null)
                    {
                        prog.Maximum = (int)totalBytes;
                    }
                }));
                string a = System.IO.Path.GetFileName(URL);
                System.IO.Stream st = myrp.GetResponseStream();
                System.IO.Stream so = new System.IO.FileStream(filename, System.IO.FileMode.Create);
                long totalDownloadedByte = 0;
                byte[] by = new byte[1024];
                int osize = st.Read(by, 0, (int)by.Length);
                while (osize > 0)
                {
                    totalDownloadedByte = osize + totalDownloadedByte;
                    System.Windows.Forms.Application.DoEvents();
                    so.Write(by, 0, osize);
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (prog != null)
                        {
                            prog.Value = (int)totalDownloadedByte;
                        }
                    }));
                    osize = st.Read(by, 0, (int)by.Length);
                    percent = (float)totalDownloadedByte / (float)totalBytes * 100;
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        label.Content = "正在下载..." + percent.ToString() + "%    " + a;
                    }));
                }
                myrp.Dispose();
                so.Close();
                st.Close();
            }
            catch (System.Exception)
            { }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

