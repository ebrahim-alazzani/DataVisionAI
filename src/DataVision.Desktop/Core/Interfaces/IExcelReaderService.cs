using System.Threading;
using System.Threading.Tasks;
using DataVision.Desktop.Models;

namespace DataVision.Desktop.Core.Interfaces;

/// <summary>
/// واجهة خدمة قراءة وتحليل ملفات الجداول الممتدة (Excel و CSV).
/// Contract for loading and profiling spreadsheet datasets (XLSX, CSV).
/// Follows SOLID: Single Responsibility Principle (SRP) & Interface Segregation (ISP).
/// </summary>
public interface IExcelReaderService
{
    /// <summary>
    /// يقرأ ملف إكسل أو CSV ويستخرج بيانات الهيكل الإحصائي الكامل.
    /// Loads the spreadsheet file and extracts its complete statistical profile.
    /// </summary>
    /// <param name="filePath">المسار الكامل للملف / Absolute file path</param>
    /// <param name="targetSheetName">اسم ورقة العمل المستهدفة (اختياري) / Target sheet name</param>
    /// <param name="cancellationToken">رمز إلغاء العملية / Cancellation token</param>
    /// <returns>كائن توصيف الجدول الشامل / Comprehensive DataTable profile</returns>
    Task<DataTableProfile> LoadAndProfileAsync(string filePath, string? targetSheetName = null, CancellationToken cancellationToken = default);
}
