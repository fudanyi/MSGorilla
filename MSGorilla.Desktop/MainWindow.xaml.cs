﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Diagnostics;

using MSGorilla.WebAPI.Client;
using MSGorilla.WebAPI.Models.ViewModels;

namespace MSGorilla.Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static string GetCurrentUserID()
        {
            string[] array = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split('\\');
            if (array.Length > 1)
            {
                return array[1];
            }
            return array[0];
        }

        GorillaStatusHelper _helper;

        private void OnStartup()
        {
            // Get Reference to the current Process
            Process thisProc = Process.GetCurrentProcess();
            // Check how many total processes have the same name as the current one
            if (Process.GetProcessesByName(thisProc.ProcessName).Length > 1)
            {
                // If ther is more than one, than it is already running.
                MessageBox.Show("MSGorilla Desktop Tool is already running.");
                Application.Current.Shutdown();
                return;
            }

            this.Left = SystemParameters.PrimaryScreenWidth - this.Width - 30;

            SetLabelCount(LabelHome, 0);
            SetLabelCount(LabelAt, 0);
            SetLabelCount(LabelOwn, 0);
            SetLabelCount(LabelReply, 0);
            SetLabelCount(LabelImportant, 0);


            _helper = new GorillaStatusHelper(GetCurrentUserID());
            TBStatus.Text = "Welcome to use MSGorilla, " + _helper.Userid;

            Thread workThread = new Thread(new ThreadStart(Update));
            workThread.IsBackground = true;
            workThread.Start();

            Thread magnetThread = new Thread(new ThreadStart(Magnet));
            magnetThread.IsBackground = true;
            magnetThread.Start();


            Thread shinkMainWindowMonitor = new Thread(new ThreadStart(ShinkMainWindowMonitor));
            shinkMainWindowMonitor.IsBackground = true;
            shinkMainWindowMonitor.Start();
        }

        public MainWindow()
        {
            InitializeComponent();
            OnStartup();
        }

        private void Drag_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    this.DragMove();
                }
                if (e.LeftButton == MouseButtonState.Released)
                {
                    if (this.Top < 0)
                    {
                        ShowMainWindow();
                    }
                }
            }
            catch { }
        }

        //////////////////////////////////////////UI Animation///////////////////////////////////////////////////////
        private void Magnet()
        {
            while (true)
            {
                try
                {
                    Thread.Sleep(1);
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        if (this.Top < 20 && this.Top > 0)
                        {
                            this.Top = 0;
                        }
                    }));
                }
                catch { }
            }
        }

        private void ShinkMainWindowMonitor()
        {
            while (true)
            {
                Thread.Sleep(1);
                ShinkMainWindowChecker();
            }
        }

        private void ShinkMainWindowChecker()
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                if (this.Top != 0)
                {
                    return;
                }
            }));

            Thread.Sleep(3000);

            this.Dispatcher.Invoke((Action)(() =>
            {
                if (this.Top == 0)
                {
                    ShinkMainWindow();
                }
            }));
        }
        private void ShinkMainWindow()
        {
            int height = (int)(0 - (this.Height - 5));            
            for (int i = 0; i >= height; i--)
            {
                this.Top = i;
                Thread.Sleep(1);
            }
        }

        private void ShowMainWindow()
        {
            while (this.Top < 0)
            {
                this.Top++;
                Thread.Sleep(1);
            }
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void BtnHome_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://msgorilla.cloudapp.net");
            SetLabelCount(LabelHome, 0);
            //LabelHome.Content = "0";
            //LabelImportant.Content = "0";
        }

        private void BtnOwn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://msgorilla.cloudapp.net/Notification/index?category=ownerline");
            SetLabelCount(LabelOwn, 0);
            //LabelOwn.Content = "0";
        }

        private void BtnAt_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://msgorilla.cloudapp.net/Notification/index?category=atline");
            SetLabelCount(LabelAt, 0);
            //LabelAt.Content = "0";
        }

        private void BtnReply_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://msgorilla.cloudapp.net/Notification/index?category=replyline");
            SetLabelCount(LabelReply, 0);
            //LabelReply.Content = "0";
        }

        private void Update()
        {
            UpdateCount();
            while (true)
            {
                Thread.Sleep(120000);
                UpdateCount();
            }
        }

        private void SetLabelCount(Label label, int count)
        {
            Grid parent = VisualTreeHelper.GetParent(label) as Grid;

            var pos = label.Margin;
            Image image = null;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is Image)
                {
                    image = child as Image;
                }
            }

            if (count <= 0)
            {
                label.Content = "";
                pos = image.Margin;
                pos.Left = 0;
                image.Margin = pos;

                label.Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                label.Content = count;
                pos = image.Margin;
                pos.Left = -15;
                image.Margin = pos;
                label.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void UpdateCount()
        {
            try
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    TBStatus.Text = "Updating ...";
                }));
                NotificationCount notif = _helper.UpdateStatus();

                this.Dispatcher.Invoke((Action)(() =>
                {
                    SetLabelCount(LabelHome, notif.UnreadHomelineMsgCount);
                    SetLabelCount(LabelAt, notif.UnreadAtlineMsgCount);
                    SetLabelCount(LabelOwn, notif.UnreadOwnerlineMsgCount);
                    SetLabelCount(LabelReply, notif.UnreadReplyCount);
                    SetLabelCount(LabelImportant, notif.UnreadImportantMsgCount);

                    TBStatus.Text = "Welcome to use MSGorilla, " + notif.Userid;
                    TBStatus.Foreground = Brushes.Black;
                }));
            }
            catch (Exception e)
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    TBStatus.Text = e.ToString();
                    TBStatus.Foreground = Brushes.Red;

                    //MessageBoxResult result = MessageBox.Show(e.ToString(), "Confirmation", MessageBoxButton.OK, MessageBoxImage.Error);
                }));
            }
        }
    }

    class GorillaStatusHelper
    {
        private GorillaWebAPI _client;
        public string Userid { get; set; }

        public GorillaStatusHelper(string userid)
        {
            _client = new GorillaWebAPI("ShareAccount", "********");
            this.Userid = userid;
        }

        public NotificationCount UpdateStatus()
        {
            return _client.GetNotificationCount(Userid);
        }
    }
}
