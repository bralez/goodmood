﻿/*
 * Copyright 2015 Andrea Del Signore sejerpz@gmail.com
 */

using MetroFramework.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Deployment.Application;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GoodMood
{
    public partial class FormAbout : MetroForm
    {
        public FormAbout()
        {
            InitializeComponent();
            Defaults();
        }

        private void Defaults()
        {
            this.Text = Application.ProductName;
            metroLabelVersion.Text = string.Format("version {0}", Application.ProductVersion);
            metroLabelCopyRight.Text = string.Format("copyright © 2015 by {0}", Application.CompanyName);
            metroLinkWebSite.Text = "GoodMood Web Site";

            metroLinkCheckUpdates.Visible = ApplicationDeployment.IsNetworkDeployed;
        }

        private void FormAbout_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void metroLinkWebSite_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://sejerpz.github.io/goodmood/");
        }

        private void metroLinkCheckUpdates_Click(object sender, EventArgs e)
        {
            CheckAndInstallUpdate();
        }

        private void CheckAndInstallUpdate()
        {
            UpdateCheckInfo info = null;

            if (ApplicationDeployment.IsNetworkDeployed)
            {
                ApplicationDeployment ad = ApplicationDeployment.CurrentDeployment;
               
                try
                {
                    using (var wait = new WaitCursor())
                    {
                        info = ad.CheckForDetailedUpdate();
                    }
                }
                catch (DeploymentDownloadException dde)
                {
                    Interaction.Warning("Download failed.", detailMessage: "Please check your network connection, or try again later. Error: " + dde.Message);
                    return;
                }
                catch (InvalidDeploymentException ide)
                {
                    Interaction.Warning("Updated check failed.", detailMessage: "The ClickOnce deployment is corrupt. Please redeploy the application and try again. Error: " + ide.Message);
                    return;
                }
                catch (InvalidOperationException ioe)
                {
                    Interaction.Info("This application cannot be updated.", detailMessage: "It is likely not a ClickOnce application. Error: " + ioe.Message);
                    return;
                }
                

                if (info.UpdateAvailable)
                {
                    Boolean doUpdate = true;

                    if (!info.IsUpdateRequired)
                    {
                        var  result = Interaction.Query("An update is available.", "Would you like to update the application now?", "Updated", "No");
                        if (result != FormDialog.MessageResult.Ok)
                        {
                            doUpdate = false;
                        }
                    }
                    else
                    {
                        // Display a message that the app MUST reboot. Display the minimum required version.
                        Interaction.Info("Mandatory update", "An update is required from your current " +
                            "version to version " + info.MinimumRequiredVersion.ToString() +
                            ". The application will now install the update and restart.",
                            "Update now");
                    }

                    if (doUpdate)
                    {
                        try
                        {
                            using (var wait = new WaitCursor())
                            {
                                ad.Update();
                            }
                            Interaction.Info("The application has been upgraded", "A restart is required.", "Restart");
                            Application.Restart();
                        }
                        catch (DeploymentDownloadException ex)
                        {
                            Interaction.Error(ex);
                        }
                    }
                }
                else
                {
                    Interaction.Info("No update available.", detailMessage: "You are running the latest version :)");
                }
            }
        }
    }
}
