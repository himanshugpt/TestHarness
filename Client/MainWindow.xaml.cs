/////////////////////////////////////////////////////////////////////
// MainWindow.xaml.cs - Test Harness                              //
// ver 1.0                                                         //
//                                                                 //
// Application: Test harness Client                                //
// Author:      Himanshu Gupta, Syracuse University,               //
//              hgupta01@syr.edu,                                  //
/////////////////////////////////////////////////////////////////////
/*
 * Module Operations:
 * ==================
 * This module provides the functionalities
 * to be done when a specific event occurs.
 * 
 * Build Process:
 * ==============
 * Files Required:
 *   ClientController.cs
 */
using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Net;
using System.IO;
using System.Threading;
using System.Windows.Threading;
using System.Xml.Linq;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //list used to save the file names
        public List<String> fileList = new List<string>();
        ClientController cc = new ClientController();

        public MainWindow()
        {
            InitializeComponent();
            cc.OpenChannel();
            con_Status.Content = "Connected";
        }

        // this method opens the FileDialog
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".dll"; // Default file extension
            dlg.Filter = "All Files|*.*"; // Filter files by extension
            dlg.Multiselect = true;

            Nullable<bool> result = dlg.ShowDialog();
            if(null != dlg.FileName){
                foreach (string fNmae in dlg.FileNames)
                {
                   if(!fileList.Contains(fNmae))
                       fileList.Add(fNmae);
                }
                RefreshFileList();
            }
        }

        // refresh teh filelist to update the listbox
        private void RefreshFileList()
        {
            fileListBox.ItemsSource = null;
            fileListBox.ItemsSource = fileList;
        }

        //moves the file one level up
        private void up_Click(object sender, RoutedEventArgs e)
        {
            int location = fileListBox.SelectedIndex;
            if (location > 0)
            {   
                string rememberMe = fileListBox.SelectedItem as string;
                fileList.RemoveAt(location);
                fileList.Insert(location - 1, rememberMe);
                RefreshFileList();
                fileListBox.SelectedIndex = location - 1;
            }
        }

        //moves the file one level down
        private void down_Click(object sender, RoutedEventArgs e)
        {
            int location = fileListBox.SelectedIndex;
            if (location >= 0 && location!= (fileList.Count-1))
            {
                string rememberMe = fileListBox.SelectedItem as string;
                fileList.RemoveAt(location);
                fileList.Insert(location + 1, rememberMe);
                RefreshFileList();
                fileListBox.SelectedIndex = location + 1;
            }
        }

        // starts the testign at server end
        private void startTesting_Click(object sender, RoutedEventArgs e)
        {
            if (prjname.Text.Trim() != "" && fileList.Count > 0)
            {
                cc.makeXmlMessgae(fileList, prjname.Text, username.Text);
            }
            else
            {
                MessageBox.Show("Please provide test suite name and files to test.");
                return;
            }
            cc.StartTesting(fileList);
        }

        // deletes the file from the listbox and file set.
        private void remove_button_Click(object sender, RoutedEventArgs e)
        {
            int location = fileListBox.SelectedIndex;
            string selectedItem = fileListBox.SelectedItem as string;
            fileList.Remove(selectedItem);
            RefreshFileList();
        }

        // makes the directory structure at the server
        private void Configure_Server_Click(object sender, RoutedEventArgs e)
        {
            if (prjname.Text.Trim() != "" && fileList.Count>0)
            {
                cc.makeXmlMessgae(fileList, prjname.Text, username.Text);
            }
            else
                MessageBox.Show("Please provide test suite name and files to test.");
        }

        // displays the putput grid
        private void ShowOutputGrid()
        {
                    loginGrid.Visibility = Visibility.Hidden;
                    grid1.Visibility = Visibility.Visible;
                    inpBox.Visibility = Visibility.Visible;
                    fetchResultGrid.Visibility = Visibility.Visible;
                    tt.X = 10; tt.Y = -240;
        }

        // for login
        private void login_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            if (username.Text.Trim() != "" && password.Text.Trim() != "")
            {
                bool status = cc.Login(username.Text, password.Text);
                if (status)
                {
                    ShowOutputGrid();
                }
                else
                    MessageBox.Show("Username and password doesnt match");
            }
            else
                 MessageBox.Show("Please provide username and passwprd");
           Cursor = Cursors.Arrow;
        }

        // registers a new user
        private void register_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            if (reg_password.Text != "" && reg_username.Text != "")
            {
                if ((reg_password.Text.Trim() == reg_conf_password.Text.Trim()))
                {
                    cc.Register(reg_username.Text, reg_password.Text);
                    ShowOutputGrid();  
                }
                else
                    MessageBox.Show("Passwords do not match.");
            }
            else
                MessageBox.Show("Please provide correct details");
            Cursor = Cursors.Arrow;
        }

        // displays the results
        private void show_results_Click(object sender, RoutedEventArgs e)
        {
            if (testSuitname.Text == "")
            {
                MessageBox.Show("Please provide Test Suite Name");
                return;
            }
            logfilespath.Document.Blocks.Clear() ;
            String filename = "";
            filename = cc.GetLogFile(testSuitname.Text, username.Text, "detailedResults.log");
            //MessageBox.Show(filename);
            RichTextBox rctxtBx = new RichTextBox();
            rctxtBx.Height = 250;
            rctxtBx.Width = 400;
            rctxtBx.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            Paragraph p = rctxtBx.Document.Blocks.FirstBlock as Paragraph;
            p.Margin = new Thickness(0);
            p.LineHeight = 10;
            fetchResultGrid.Children.Add(rctxtBx);
               filename = cc.GetLogFile(testSuitname.Text,username.Text, "results.xml");
               //MessageBox.Show(filename);
               if (filename == null || !filename.Contains("xml"))
               {
                   MessageBox.Show("File Not Found. Pleasey try after some time or run the tests suite again.");
                   return;
               }
              // MessageBox.Show(filename);
               TextReader tr = new StreamReader("Test Results\\" +testSuitname.Text + "_" + "results.xml");
               StringBuilder sb = new StringBuilder();
               sb.Append(tr.ReadToEnd());
               string tempStr = sb.ToString();
               sb.Replace("><",">\n<");
               rctxtBx.AppendText(sb.ToString());
               tr.Close();
               DirectoryInfo d = Directory.CreateDirectory("Test Results");
               String logFilesPath = System.IO.Path.GetFullPath("Test Results\\" +testSuitname.Text + "_" + "results.xml");
               logfilespath.AppendText(logFilesPath);
               logfilespath.AppendText("\n");
               logFilesPath = System.IO.Path.GetFullPath("Test Results\\" + testSuitname.Text + "_" + "detailedResults.log");
               logfilespath.AppendText(logFilesPath);
        }

   
    }
}
