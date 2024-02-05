﻿using Microsoft.Win32;
using NAudio.CoreAudioApi;
using StreamTools.Audio;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StreamToolsUI.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            
            if((bool)ofd.ShowDialog())
            {
                MMDeviceEnumerator enumerator = new();
                MMDeviceCollection devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

                AudioPlayer player = new(devices[8]);
                player.WaveFormUpdate += Player_WaveFormUpdate;

                player.Load(ofd.FileName);
                player.Play();
            }
        }

        private void Player_WaveFormUpdate(object? sender, StreamTools.Events.WaveFormUpdateEventArgs e)
        {
            Console.WriteLine(e.CurrentTime.ToString());
        }
    }
}