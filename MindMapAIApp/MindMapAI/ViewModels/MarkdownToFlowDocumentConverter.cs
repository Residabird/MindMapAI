using System.IO;
using System.Windows.Documents;
using Markdig;
using Markdig.Wpf;

namespace MindMapAI.ViewModels
{
    public static class MarkdownToFlowDocumentConverter
    {
        private static readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder().UseSupportedExtensions().Build();

        public static FlowDocument Convert(string markdownText)
        {
            if (string.IsNullOrWhiteSpace(markdownText))
                return new FlowDocument(new Paragraph(new Run("Нет содержимого")));

            // Указываем явно, какой Markdown используем (из Markdig.Wpf)
            return Markdig.Wpf.Markdown.ToFlowDocument(markdownText, _pipeline);
        }
    }
}