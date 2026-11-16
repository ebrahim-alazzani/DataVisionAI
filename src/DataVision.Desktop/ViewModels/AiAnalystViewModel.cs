using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataVision.Desktop.Models;
using DataVision.Desktop.Services;

namespace DataVision.Desktop.ViewModels;

public class ChatEntry
{
    public string Sender { get; set; } = "User"; // "User" or "AI"
    public string Message { get; set; } = string.Empty;
    public DateTime Time { get; set; } = DateTime.Now;
    public bool IsAi => Sender == "AI";
    public string TimeFormatted => Time.ToString("HH:mm");
}

public partial class AiAnalystViewModel : ObservableObject
{
    private readonly AiAnalysisService _aiService;
    private readonly MainViewModel _mainViewModel;

    [ObservableProperty]
    private string _userInput = string.Empty;

    [ObservableProperty]
    private bool _isThinking;

    public ObservableCollection<ChatEntry> Messages { get; } = new();

    public AiAnalystViewModel(AiAnalysisService aiService, MainViewModel mainViewModel)
    {
        _aiService = aiService;
        _mainViewModel = mainViewModel;

        // Welcome message
        Messages.Add(new ChatEntry
        {
            Sender = "AI",
            Message = "مرحباً بك! أنا مساعدك الذكي لتحليل البيانات. اسألني أي سؤال عن ملف الإكسل (مثلاً: ما هي أكثر الفئات مبيعاً؟ أين توجد القيم الشاذة؟ أو اقترح خطة لتحسين الأداء)."
        });
    }

    [RelayCommand]
    public async Task SendQuestionAsync()
    {
        if (string.IsNullOrWhiteSpace(UserInput) || IsThinking) return;

        var question = UserInput.Trim();
        UserInput = string.Empty;

        Messages.Add(new ChatEntry { Sender = "User", Message = question });

        if (_mainViewModel.CurrentProfile == null)
        {
            Messages.Add(new ChatEntry
            {
                Sender = "AI",
                Message = "يرجى أولاً رفع أو فتح ملف إكسل من لوحة التحكم حتى أتمكن من فحصه والإجابة عن استفساراتك بدقة."
            });
            return;
        }

        try
        {
            IsThinking = true;
            var answer = await _aiService.AskQuestionAsync(_mainViewModel.CurrentProfile, question);
            Messages.Add(new ChatEntry { Sender = "AI", Message = answer });
        }
        catch (Exception ex)
        {
            Messages.Add(new ChatEntry
            {
                Sender = "AI",
                Message = $"عذراً، حدث خطأ أثناء معالجة السؤال: {ex.Message}"
            });
        }
        finally
        {
            IsThinking = false;
        }
    }
}
