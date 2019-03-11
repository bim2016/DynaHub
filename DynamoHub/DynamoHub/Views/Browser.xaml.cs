﻿using DynaHub.ViewModels;
using Octokit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DynaHub.Views
{
    /// <summary>
    /// Interaction logic for Browser.xaml
    /// </summary>
    public partial class Browser : Window
    {
        //// Get dict from main Dynamo method
        //Dictionary<string, string> allPaths = new Dictionary<string, string>();
        // Create variable to pass to main Dynamo method
        public static string toOpen = null;

        private bool alreadyLoaded = false;

        public Browser()
        {
            InitializeComponent();

            selectReposCB.IsEditable = true;
            selectReposCB.IsReadOnly = true;
            selectReposCB.Text = "select a repository";
        }

        private List<Repository> reposList = new List<Repository>();

        private async void PopulateComboAsync(object sender, RoutedEventArgs e)
        {
            // Only loads the list of repos the first time. Save calculations
            if (!alreadyLoaded)
            {
                // Start async process straight away
                Task<IReadOnlyList<Repository>> repos = BrowserEngine.GetUserReposAsync();

                // Display in-the-meantime text
                selectReposCB.Text = "...retrieving repos";

                // Wait to get the repos
                IReadOnlyList<Repository> repositories = await repos;

                // Populate combobox
                foreach (Repository r in repositories)
                {
                    selectReposCB.Items.Add(r.FullName);
                    reposList.Add(r);
                }
                alreadyLoaded = true;
            }
        }

        // Initialise selection variable
        internal static Repository selectedRepo;

        private async void OnSelectedAsync(object sender, SelectionChangedEventArgs e)
        {
            string selectionString = null;
            selectionString = selectReposCB.SelectedItem.ToString();

            // Show text in combobox
            selectReposCB.Text = selectionString;

            // Get repository object from user selection
            // It's supposed that the user has only one repo with that path (GH rule)
            selectedRepo = reposList.Where(r => r.FullName == selectionString).First();

            // Initialize process to get repo's content
            Task<SortedDictionary<string, string>> repoContentTask = null;
            repoContentTask = BrowserEngine.GetRepoContentAsync(selectedRepo);

            // Get async result
            SortedDictionary<string, string> repoContent = await repoContentTask;

            BrowserEngine.PopulateTree(repoContent, filesTree);

            // Clear lists not to repeat if user changes selection
            ClearPrevious();
        }

        private void TreeViewItem_OnItemSelected(object sender, RoutedEventArgs e)
        {
            //var tree = sender as TreeView;

            //// ... Determine type of SelectedItem.
            //if (tree.SelectedItem is TreeViewItem)
            //{
            //    // Do nothing
            //}

            //// Else if it's the child element
            //else if (tree.SelectedItem is string)
            //{
            //    List<string> keysList = allPaths.Keys.ToList();

            //    string path = null;

            //    // get path from name of the file selected by the user
            //    foreach (string k in keysList)
            //    {
            //        if (k.Contains(tree.SelectedItem.ToString()))
            //        {
            //            path = k;
            //        }
            //    }

            //    // Instantiate web client to download file
            //    WebClient wc = new WebClient();

            //    // Get file's uri from dictionary using path/key
            //    string uri = allPaths[path];


            //    string tempFold;

            //    // Create temp directory
            //    try
            //    {
            //        tempFold = GlobalSettings.CreateTempFolder();
            //    }
            //    catch
            //    {
            //        MessageBox.Show("Something went wrong creating the temporary folder to store the graph.",
            //            "Error");
            //        return;
            //    }

            //    // Assemble download path
            //    string fName = tempFold + tree.SelectedItem.ToString();

            //    // Download file locally
            //    try
            //    {
            //        wc.DownloadFile(uri, fName);
            //    }
            //    catch (WebException)
            //    {
            //        MessageBox.Show("Sorry, I couldn't find that file.", "Web Exception");
            //        return;
            //    }
            //    catch
            //    {
            //        MessageBox.Show("Ooops, something went wrong.", "Error");
            //        return;
            //    }
            //    // Notify user but don't block process in case user doesn't close window
            //    AutoClosingMessageBox.Show("Downloaded (temporarily)! Opening now...", "Success!", 2000);

            //    // Pass path to downloaded file to main Dynamo method
            //    toOpen = fName;
            //    // And close window
            //    Close();
            //}
        }


        private void ClearPrevious()
        {
            BrowserEngine.repoLevel.Clear();
            BrowserEngine.repoFolders.Clear();
            BrowserEngine.repoFiles.Clear();
        }
    }
}
