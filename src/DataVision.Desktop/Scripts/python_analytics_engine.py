#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
DataVision Studio - Python Data Science & Machine Learning Engine
محرك بايثون العلمي ونماذج التعلم الآلي لتحليل الجداول والتنبؤ بالاتجاهات
Developed by: Ebrahim Al-Azzani
"""

import sys
import os
import json
import argparse
from typing import Dict, Any, List

def run_analysis(file_path: str, mode: str = "full") -> Dict[str, Any]:
    try:
        import pandas as pd
        import numpy as np
    except ImportError:
        return {
            "success": False,
            "error": "مكتبات pandas و numpy غير مثبتة في بيئة بايثون الحالية.",
            "mode": mode
        }

    if not os.path.exists(file_path):
        return {"success": False, "error": f"الملف غير موجود: {file_path}"}

    ext = os.path.splitext(file_path)[1].lower()
    try:
        if ext == ".csv":
            df = pd.read_csv(file_path)
        elif ext in [".xlsx", ".xls"]:
            df = pd.read_excel(file_path)
        else:
            return {"success": False, "error": f"صيغة غير مدعومة: {ext}"}
    except Exception as e:
        return {"success": False, "error": f"فشل قراءة الملف: {str(e)}"}

    result: Dict[str, Any] = {
        "success": True,
        "engine": "Python Data Science (Pandas + Scikit-Learn)",
        "rows": len(df),
        "columns": len(df.columns),
        "column_names": list(df.columns),
        "correlations": [],
        "ml_forecast": None,
        "clusters": [],
        "insights": []
    }

    # 1. Select Numeric and Categorical columns
    numeric_cols = df.select_dtypes(include=[np.number]).columns.tolist()
    cat_cols = df.select_dtypes(exclude=[np.number]).columns.tolist()

    # 2. Correlation Matrix Analysis (Pandas)
    if len(numeric_cols) >= 2:
        corr_matrix = df[numeric_cols].corr()
        corr_pairs = []
        for i in range(len(numeric_cols)):
            for j in range(i + 1, len(numeric_cols)):
                c1, c2 = numeric_cols[i], numeric_cols[j]
                val = corr_matrix.loc[c1, c2]
                if not np.isnan(val):
                    corr_pairs.append({
                        "col1": c1,
                        "col2": c2,
                        "correlation": round(float(val), 3),
                        "strength": "قوي جداً" if abs(val) > 0.7 else ("متوسط" if abs(val) > 0.4 else "ضعيف")
                    })
        corr_pairs.sort(key=lambda x: abs(x["correlation"]), reverse=True)
        result["correlations"] = corr_pairs[:8]

    # 3. Machine Learning Predictive Model (Scikit-Learn Regression)
    if len(numeric_cols) >= 1:
        try:
            from sklearn.linear_model import LinearRegression
            from sklearn.metrics import r2_score

            target_col = numeric_cols[0]
            # Use index or other numeric cols as features
            X = np.arange(len(df)).reshape(-1, 1)
            y = df[target_col].fillna(df[target_col].median()).values

            if len(y) >= 5:
                model = LinearRegression()
                model.fit(X, y)
                y_pred = model.predict(X)
                r2 = r2_score(y, y_pred)
                slope = float(model.coef_[0])

                # Forecast next 5 steps
                future_X = np.arange(len(df), len(df) + 5).reshape(-1, 1)
                future_preds = model.predict(future_X).tolist()

                trend_direction = "صاعد (Upward Trend)" if slope > 0 else "هابط (Downward Trend)"

                result["ml_forecast"] = {
                    "target_column": target_col,
                    "model_type": "Linear Regression (Scikit-Learn)",
                    "trend_direction": trend_direction,
                    "slope_per_step": round(slope, 3),
                    "r2_accuracy": round(float(r2), 3),
                    "forecast_next_steps": [round(float(p), 2) for p in future_preds]
                }

                result["insights"].append({
                    "title": f"نموذج التعلم الآلي التنبؤي لـ ({target_col})",
                    "description": f"أظهر نموذج الانحدار الخطي اتجاهاً {trend_direction} بمعامل تحديد R² = {round(float(r2), 3)}. التنبؤات المتوقعة للفترات القادمة هي: {[round(float(p), 1) for p in future_preds]}",
                    "impact": trend_direction
                })
        except Exception as ml_err:
            result["ml_forecast_error"] = str(ml_err)

    # 4. K-Means Clustering on top numeric pillars
    if len(numeric_cols) >= 2 and len(df) >= 10:
        try:
            from sklearn.cluster import KMeans
            from sklearn.preprocessing import StandardScaler

            features = df[numeric_cols[:3]].dropna()
            if len(features) >= 10:
                scaler = StandardScaler()
                scaled = scaler.fit_transform(features)
                k = min(3, len(features))
                kmeans = KMeans(n_clusters=k, random_state=42, n_init=10)
                labels = kmeans.fit_predict(scaled)

                cluster_counts = pd.Series(labels).value_counts().to_dict()
                result["clusters"] = [
                    {"cluster_id": int(cid), "records_count": int(count), "ratio": round(count / len(features) * 100, 1)}
                    for cid, count in cluster_counts.items()
                ]

                result["insights"].append({
                    "title": f"تجميع الأنماط (K-Means Clustering: {k} مجموعات)",
                    "description": f"تم تصنيف السجلات آلياً إلى {k} شرائح متماثلة الخصائص. الشريحة الأكبر تمثل {result['clusters'][0]['ratio']}% من إجمالي العينة.",
                    "impact": f"{k} شرائح"
                })
        except Exception as cl_err:
            result["clustering_error"] = str(cl_err)

    return result

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="DataVision Python Engine")
    parser.add_argument("--input", required=True, help="Path to CSV or Excel file")
    parser.add_argument("--mode", default="full", help="Analysis mode")
    args = parser.parse_args()

    output = run_analysis(args.input, args.mode)
    print(json.dumps(output, ensure_ascii=False, indent=2))
