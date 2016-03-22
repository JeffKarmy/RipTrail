using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;

namespace RipTrail
{
    public partial class About : PhoneApplicationPage
    {
        public About()
        {
            InitializeComponent();
            txtVersion.Text = "Version: 1.0.0.0";
            txtCopyRight.Text = string.Format("Copyright \u00A9 {0} RipTrail.com",System.DateTime.Now.Year);
        }

        private void btnFeedBack_Click(object sender, RoutedEventArgs e)
        {
            EmailComposeTask email = new EmailComposeTask();
            email.To = "feedback@riptrail.com";
            email.Subject = "Rip Trail Feedback";
            email.Show();
        }


    }
}