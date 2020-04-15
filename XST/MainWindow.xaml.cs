using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using WinForm = System.Windows.Forms;

namespace XST
{
    #region json读取
    public class Json
    {
        static string a = Directory.GetCurrentDirectory() + "\\XST.json";
        public static string Read(string Section, string Name)
        {
            if (System.IO.File.Exists(a))
            {
                string b = System.IO.File.ReadAllText(a);
                try
                {
                    return JObject.Parse(b)[Section][Name].ToString();
                }
                catch { throw; }
            }
            else
            {
                throw new Exception() ;
            }
        }
        public static void Write(string Section, string Name, string Text)
        {
            if (System.IO.File.Exists(a))
            {
                string b = System.IO.File.ReadAllText(a);
                try
                {
                    JObject jObject = JObject.Parse(b);
                    jObject[Section][Name] = Text;
                    string convertString = Convert.ToString(jObject);
                    System.IO.File.WriteAllText(a, convertString);
                }
                catch { throw; }
            }
            else
            {
                throw new Exception();
            }
        }

    }
    #endregion
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        #region MainWindow控件交互
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
        private void Button_Close(object sender, RoutedEventArgs e)
        {
            try { process.Kill(); } catch { }
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
        private void Button_Mini(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
            GC.Collect();
        }
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            Page1.Visibility = Visibility.Visible;
            Page2.Visibility = Page3.Visibility = Visibility.Collapsed;
        }
        private void RadioButton_Checked_1(object sender, RoutedEventArgs e)
        {
            Page1.Visibility = Page3.Visibility = Visibility.Collapsed;
            Page2.Visibility = Visibility.Visible;
        }
        private void RadioButton_Checked_2(object sender, RoutedEventArgs e)
        {
            Page3.Visibility = Visibility.Visible;
            Page1.Visibility = Page2.Visibility = Visibility.Collapsed;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Page1.Visibility = Visibility.Visible;
            Page2.Visibility = Visibility.Collapsed;
            SubPage1.Visibility = Visibility.Visible;
            SubPage2.Visibility = Visibility.Collapsed;
            Thread thread = new Thread(() =>
            {
                if (File.Exists(LocalPath + "\\XST.json"))
                { }
                else
                {
                    File.WriteAllText(LocalPath + "\\XST.json", Properties.Resources.String1);
                }
            });
            thread.Start();
            try
            {
                if (Convert.ToBoolean(Json.Read("Files", "UseDefaultDirectory")))
                { R1.IsChecked = true; TextBox_RunPath.IsEnabled = Button_OpenRunPath.IsEnabled = false; }
                else { R2.IsChecked = true; }
                TextBox_RunPath.Text = Json.Read("Files", "WorkingPath");
                TextBox_JavaPath.Text = Json.Read("Files", "JavaPath");
                TextBox_Memory.Text = Json.Read("JVM", "Memory");
                if (Json.Read("Files", "DownloadFilesPath").Length>0)
                { }
                else Json.Write("Files", "DownloadFilesPath", LocalPath + "\\Download");
                TextBox_FilePath.Text = Json.Read("Files", "DownloadFilesPath");
                C1.Text = Json.Read("Files", "DownloadForm");
            }
            catch
            {
                ShowTip("未能正确的加载配置文件,请检查配置文件", 2);
            }
            try { Directory.CreateDirectory(LocalPath + "\\server"); } catch { }
        }
        private void ComboBox1_DropDownOpened(object sender, EventArgs e)
        {
            ComboBox1.ItemsSource = GetServerVersions();
        }
        private void ComboBox1_DropDownClosed(object sender, EventArgs e)
        {
            Thread thread = new Thread(() =>
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    List.ItemsSource = GetServerList(ComboBox1.Text);
                }));
            });
            thread.Start();
        }
        private void ToSubPage1(object sender, MouseButtonEventArgs e)
        {
            SubPage1.Visibility = Visibility.Visible;
            SubPage2.Visibility = Visibility.Collapsed;
        }
        private void ToSubPage2(object sender, MouseButtonEventArgs e)
        {
            SubPage2.Visibility = Visibility.Visible;
            SubPage1.Visibility = Visibility.Collapsed;
        }
        private void ServerTip(object sender, RoutedEventArgs e)
        {
            string a = Properties.Resources.ServerTips;
            JArray jArray = JArray.Parse(a);
            for (int i = 0; i < jArray.Count; i++)
            {
                if (jArray[i].ToString().Split('#')[0] == ComboBox1.Text)
                {
                    ShowTip(jArray[i].ToString().Split('#')[1], 5);
                }
            }
        }
        public void ShowTip(string text, int seconds)
        {
            this.Activate();
            snackbar.MessageQueue.Enqueue(text, null, null, null, false, false, TimeSpan.FromSeconds(seconds));
        }
        private void DownloadVersion_Click(object sender, RoutedEventArgs e)
        {
            int a = List.SelectedIndex;
            string URL = url[a];
            string v = ComboBox1.Text;
            string name = List.SelectedItem.ToString();
            Thread thread = new Thread(() =>
            {
                try
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        ShowTip("开始下载", 1);
                    }));

                    HttpWebRequest httpWebRequest = WebRequest.Create(URL) as HttpWebRequest;
                    HttpWebResponse httpWebResponse = httpWebRequest.GetResponse() as HttpWebResponse;

                    Stream stream = httpWebResponse.GetResponseStream();

                    byte[] BArr = new byte[1024];
                    int size = stream.Read(BArr, 0, BArr.Length);
                    if (Directory.Exists(Json.Read("Files", "DownloadFilesPath")))
                    { }
                    else Directory.CreateDirectory(Json.Read("Files", "DownloadFilesPath"));
                    FileStream file = File.OpenWrite(Json.Read("Files", "DownloadFilesPath") + "\\" + name);
                    while (size > 0)
                    {
                        file.Write(BArr, 0, size);
                        size = stream.Read(BArr, 0, BArr.Length);
                    }
                    file.Close();
                    stream.Close();
                    httpWebResponse.Close();

                    System.GC.Collect();
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        ShowTip("下载完成" , 1);
                    }));
                }
                catch (Exception ex)
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        ShowTip("下载失败" + ex.Message, 1);
                    }));
                }
            });
            thread.Start();
        }
        private void Button_OpenRunPath_Click(object sender, RoutedEventArgs e)
        {
            WinForm.FolderBrowserDialog dialog = new WinForm.FolderBrowserDialog();
            dialog.ShowDialog();
            TextBox_RunPath.Text = dialog.SelectedPath;
            dialog.Dispose();
        }
        private void OpenJava(object sender, RoutedEventArgs e)
        {
            WinForm.OpenFileDialog dialog = new WinForm.OpenFileDialog();
            dialog.Filter = "java.exe|java.exe";
            dialog.Title = "选择Java";
            dialog.ShowDialog();
            TextBox_JavaPath.Text = dialog.FileName;
            dialog.Dispose();
        }
        private void OpenFilesPath(object sender, RoutedEventArgs e)
        {
            WinForm.FolderBrowserDialog dialog = new WinForm.FolderBrowserDialog();
            dialog.ShowDialog();
            TextBox_FilePath.Text = dialog.SelectedPath;
            dialog.Dispose();
        }
        private void Page3_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SaveJson();
        }
        private void R2_Checked(object sender, RoutedEventArgs e)
        {
            TextBox_RunPath.IsEnabled = Button_OpenRunPath.IsEnabled = true;
        }
        private void R1_Checked(object sender, RoutedEventArgs e)
        {
            TextBox_RunPath.IsEnabled = Button_OpenRunPath.IsEnabled = false;
        }
        #endregion
        #region 服务器操作
        public static Process process;
        private void StartServer(object sender, RoutedEventArgs e)
        {
            process = new Process();
            process.StartInfo.WorkingDirectory = Json.Read("Files","WorkingPath");
            process.StartInfo.FileName = Json.Read("Files", "JavaPath");
            process.StartInfo.Arguments = "-Xmx" + Json.Read("JVM", "Memory") + "M" + " -XX:+AggressiveOpts -XX:+UseCompressedOops" + " -jar " + Json.Read("Files", "WorkingPath") + "\\server.jar nogui";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardInput = true;
            process.EnableRaisingEvents = true;
            process.Start();
            T1.Clear();
            process.BeginErrorReadLine(); process.BeginOutputReadLine();
            process.OutputDataReceived += Process_OutputDataReceived;
            process.Exited += new EventHandler(Process_Exited);
            Start.IsEnabled = false; Stop.IsEnabled = Send.IsEnabled = true;
            ShowTip("服务器已启动...", 1);
        }
        private void Process_Exited(object sender, EventArgs e)
        {
            process.Dispose();
            this.Dispatcher.Invoke(new Action(() =>
            {
                T1.Clear();
                Start.IsEnabled = true; Stop.IsEnabled = Send.IsEnabled = false;
            }));
        }
        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                T1.AppendText(e.Data + "\r\n");
                T1.ScrollToEnd();
            }));
        }
        private void StopServer(object sender, RoutedEventArgs e)
        {
            process.StandardInput.WriteLine("stop");
        }
        private void SendTo(object sender, RoutedEventArgs e)
        {
            if (T2.Text.Length > 0)
                process.StandardInput.WriteLine(T2.Text.Replace("/", ""));
            T2.Clear();
        }
        #endregion
        public static string[] GetServerVersions()
        {
            try
            {
                List<string> vs = new List<string>();
                string versions = "";
                WebClient webClient = new WebClient();
                versions = webClient.DownloadString("http://106.14.64.250/api.html");
                JArray jArray = JArray.Parse(versions);
                for (int i = 0; i < jArray.Count; i++)
                {
                    vs.Add(jArray[i].ToString());
                }
                //vs.Add("CatServer");
                webClient.Dispose();
                return vs.ToArray();
            }
            catch { return null; }
        }
        static List<string> url;
        public static string[] GetServerList(string version)
        {
            if (version.Length > 0)
            {
                List<string> vs = new List<string>();
                string versions = "";
                WebClient webClient = new WebClient();
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                versions = webClient.DownloadString("http://106.14.64.250/" + version + ".html");
                //if (versions != "CatServer")
                JArray jArray = JArray.Parse(versions);
                url = new List<string>();
                for (int i = 0; i < jArray.Count; i++)
                {
                    try
                    {
                        JObject jObject1 = JObject.Parse(jArray[i].ToString());
                        vs.Add(jObject1["name"].ToString());
                        url.Add(jObject1["file"].ToString());
                    }
                    catch { }
                }
                return vs.ToArray();
            }
            else return null;
        }
        public static string LocalPath = System.IO.Directory.GetCurrentDirectory();
        public int CopyFolder(string sourceFolder, string destFolder)
        {
            try
            {
                //如果目标路径不存在,则创建目标路径
                if (!System.IO.Directory.Exists(destFolder))
                {
                    System.IO.Directory.CreateDirectory(destFolder);
                }
                //得到原文件根目录下的所有文件
                string[] files = System.IO.Directory.GetFiles(sourceFolder);
                foreach (string file in files)
                {
                    string name = System.IO.Path.GetFileName(file);
                    string dest = System.IO.Path.Combine(destFolder, name);
                    System.IO.File.Copy(file, dest);//复制文件
                }
                //得到原文件根目录下的所有文件夹
                string[] folders = System.IO.Directory.GetDirectories(sourceFolder);
                foreach (string folder in folders)
                {
                    string name = System.IO.Path.GetFileName(folder);
                    string dest = System.IO.Path.Combine(destFolder, name);
                    CopyFolder(folder, dest);//构建目标路径,递归复制文件
                }
                return 1;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return 0;
            }

        }
        public void SaveJson()
        {
            try
            {
                if (R1.IsChecked == true)
                    Json.Write("Files", "UseDefaultDirectory", "true");
                else Json.Write("Files", "UseDefaultDirectory", "false");
                Json.Write("Files", "WorkingPath", TextBox_RunPath.Text);
                Json.Write("Files", "JavaPath", TextBox_JavaPath.Text);
                Json.Write("Files", "DownloadFilesPath", TextBox_FilePath.Text);
                Json.Write("Files", "DownloadForm", C1.Text);
            }
            catch
            {
                ShowTip("未能正确的加载配置文件,请检查配置文件", 2);
            }
        }
        private void SetUpServer(object sender, RoutedEventArgs e)
        {
            try
            {
                Window1.Window = this;
                string path;
                if (Convert.ToBoolean(Json.Read("Files", "UseDefaultDirectory")))
                    path = Directory.GetCurrentDirectory() + "\\server";
                else path = Json.Read("Files", "WorkingPath");
                Window1.SetUp(path, ComboBox1.Text, List.SelectedItem.ToString(), Json.Read("Files", "JavaPath"));
                ShowTip("安装成功", 1);
            }
            catch
            {
                ShowTip("安装失败", 1);
            }
        }
    }
}
