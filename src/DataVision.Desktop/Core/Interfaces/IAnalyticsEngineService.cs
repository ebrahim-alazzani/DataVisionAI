using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DataVision.Desktop.Models;

namespace DataVision.Desktop.Core.Interfaces;

/// <summary>
/// واجهة محرك التحليل الإحصائي وذكاء الأعمال التنفيذي.
/// Contract for statistical analysis, anomaly detection, and executive business insights generation.
/// Follows SOLID: Dependency Inversion (DIP) & Open/Closed Principle (OCP).
/// </summary>
public interface IAnalyticsEngineService
{
    /// <summary>
    /// يولد تقريراً شاملاً بالرؤى الإحصائية والتنفيذية بناءً على خوارزميات رياضية دقيقة.
    /// Generates executive insights and statistical alerts using deterministic mathematical algorithms.
    /// </summary>
    /// <param name="profile">توصيف جدول البيانات / Data table profile</param>
    /// <param name="cancellationToken">رمز إلغاء العملية / Cancellation token</param>
    /// <returns>قائمة الرؤى والتنبيهات الإحصائية / List of executive insights</returns>
    Task<List<ExecutiveInsight>> GenerateExecutiveInsightsAsync(DataTableProfile profile, CancellationToken cancellationToken = default);

    /// <summary>
    /// ينفذ استعلاماً إحصائياً تحليلياً على البيانات دون الحاجة لأي محركات خارجية.
    /// Executes a deterministic analytical query on the dataset offline.
    /// </summary>
    /// <param name="profile">توصيف جدول البيانات / Data table profile</param>
    /// <param name="query">الاستفسار التحليلي / Analytical query</param>
    /// <param name="cancellationToken">رمز إلغاء العملية / Cancellation token</param>
    /// <returns>نتيجة التحليل الرقمية والوصفية / Formatted analytical answer</returns>
    Task<string> ExecuteStatisticalQueryAsync(DataTableProfile profile, string query, CancellationToken cancellationToken = default);
}
