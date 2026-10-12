using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace JRPG.UI.GameMenu.Controls
{
    /// <summary>A yes / no question shown over a page, replacing Windows message boxes.</summary>
    public partial class ConfirmDialog : UserControl
    {
        private TaskCompletionSource<bool> answer;

        public ConfirmDialog()
        {
            InitializeComponent();
        }

        /// <summary>Shows the question and waits. True if the player confirmed.</summary>
        public Task<bool> AskAsync(string message, string confirmText = "OK")
        {
            MessageText.Text = message;
            ConfirmButton.Content = confirmText;
            Visibility = Visibility.Visible;

            answer = new TaskCompletionSource<bool>();
            return answer.Task;
        }

        private void Confirm_Click(object sender, RoutedEventArgs e) => Close(true);

        private void Cancel_Click(object sender, RoutedEventArgs e) => Close(false);

        private void Close(bool confirmed)
        {
            Visibility = Visibility.Collapsed;
            answer?.TrySetResult(confirmed);
            answer = null;
        }
    }
}
