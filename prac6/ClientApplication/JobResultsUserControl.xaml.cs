using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using APIClasses;

namespace ClientApplication
{
    /// <summary>
    /// Interaction logic for JobResultsUserControl.xaml
    /// </summary>
    public partial class JobResultsUserControl : UserControl
    {
        public JobResultsUserControl(ICollection<JobData> completedJobs)
        {
            InitializeComponent();

            List<TextBlock> completedJobBlocks = new List<TextBlock>();
            foreach (JobData job in completedJobs)
            {
                TextBlock block = new TextBlock();
                block.Text = $"Job ID: {job.Id}\n" +
                             $"Python Code:\n" +
                             $"{job.Python}\n" + //TODO maybe make this whole thing prettier
                             $"Result:\n" +
                             $"{job.Result}\n" +
                             $"======================================================";

                completedJobBlocks.Add(block);
            }

            CompletedJobsContainer.ItemsSource = completedJobBlocks;
        }
    }
}
