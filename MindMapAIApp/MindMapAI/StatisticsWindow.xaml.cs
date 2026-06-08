using MindMapAICore.Models;
using MindMapAICore.Services;
using System.Windows;
using System.Windows.Automation;

namespace MindMapAI
{
    public partial class StatisticsWindow : Window
    {
        public StatisticsWindow(IDatabaseService databaseService)
        {
            InitializeComponent();

            LoadStatistics(databaseService);
        }

        private void LoadStatistics(IDatabaseService db)
        {
            TotalNotesCount.Text = db.GetTotalNoteCount().ToString();

            var periodStats = db.GetNotesPeriodStats();
            TodayCount.Text = periodStats.Today.ToString();
            WeekCount.Text = periodStats.ThisWeek.ToString();
            MonthCount.Text = periodStats.ThisMonth.ToString();

            var topTags = db.GetTopTags(5);
            TopTagsList.ItemsSource = topTags;

            AutomationProperties.SetName(TotalNotesCount, $"Всего заметок: {TotalNotesCount.Text}");
            AutomationProperties.SetName(TodayCount, $"За сегодня: {TodayCount.Text}");
            AutomationProperties.SetName(WeekCount, $"За неделю: {WeekCount.Text}");
            AutomationProperties.SetName(MonthCount, $"За месяц: {MonthCount.Text}");
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
