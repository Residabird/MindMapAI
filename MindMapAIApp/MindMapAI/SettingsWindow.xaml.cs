using System.Windows;
using System.Windows.Media;
using MindMapAI.Properties;

namespace MindMapAI
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            HighlightCurrentFontSize();
        }

        private void HighlightCurrentFontSize()
        {
            int currentSize = Settings.Default.EditorFontSize;

            var defaultBg = (Brush)FindResource("BgCard");
            var selectedBg = (Brush)FindResource("AccentColor");
            var selectedFg = (Brush)FindResource("TextPrimary");
            var defaultFg = (Brush)FindResource("TextPrimary");

            SmallFontBtn.Background = defaultBg;
            MediumFontBtn.Background = defaultBg;
            LargeFontBtn.Background = defaultBg;
            SmallFontBtn.Foreground = defaultFg;
            MediumFontBtn.Foreground = defaultFg;
            LargeFontBtn.Foreground = defaultFg;

            if (currentSize == 14)
            {
                SmallFontBtn.Background = selectedBg;
                SmallFontBtn.Foreground = selectedFg;
            }
            else if (currentSize == 16)
            {
                MediumFontBtn.Background = selectedBg;
                MediumFontBtn.Foreground = selectedFg;
            }
            else if (currentSize == 18)
            {
                LargeFontBtn.Background = selectedBg;
                LargeFontBtn.Foreground = selectedFg;
            }
        }

        private void SmallFont_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.EditorFontSize = 14;
            HighlightCurrentFontSize();
        }

        private void MediumFont_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.EditorFontSize = 16;
            HighlightCurrentFontSize();
        }

        private void LargeFont_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.EditorFontSize = 18;
            HighlightCurrentFontSize();
        }

        private void SaveAndClose_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Save();
            DialogResult = true;
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}