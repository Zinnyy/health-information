import sys
import json
import os
import pandas as pd
import joblib


MODEL_FILENAME = "biometric_rf_model.pkl"


def load_ai_model():
    """Load the trained machine-learning model artifact."""

    script_dir = os.path.dirname(os.path.abspath(__file__))
    model_path = os.path.join(
        script_dir,
        "models",
        MODEL_FILENAME
    )

    try:
        return joblib.load(model_path)

    except FileNotFoundError:
        # Model hasn't been trained/saved yet.
        # The demo will use the rule-based fallback.
        return None


def validate_data(df):
    """Validate and convert biometric measurements."""

    required_features = [
        "SystolicBp",
        "HeartRate",
        "Glucose"
    ]

    missing_features = [
        feature
        for feature in required_features
        if feature not in df.columns
    ]

    if missing_features:
        raise ValueError(
            f"Missing required features: "
            f"{', '.join(missing_features)}"
        )

    for feature in required_features:
        df[feature] = pd.to_numeric(
            df[feature],
            errors="coerce"
        )

    if df[required_features].isnull().any().any():
        raise ValueError(
            "One or more biometric values are missing or malformed."
        )

    return df


def run_rule_based_fallback(latest):
    """
    Simple heuristic fallback used only when the trained
    ML model is unavailable.

    This is a demonstration rule engine and is not
    a clinically validated risk model.
    """

    score = 0.0

    systolic_bp = latest["SystolicBp"]
    heart_rate = latest["HeartRate"]
    glucose = latest["Glucose"]

    # Systolic blood pressure
    if systolic_bp >= 140 or systolic_bp <= 90:
        score += 0.40

    elif 120 <= systolic_bp < 140:
        score += 0.20

    # Heart rate
    if heart_rate >= 100 or heart_rate <= 50:
        score += 0.40

    elif 85 <= heart_rate < 100:
        score += 0.15

    # Glucose
    if glucose >= 140 or glucose <= 70:
        score += 0.40

    elif 100 <= glucose < 140:
        score += 0.15

    score = min(score, 1.0)

    anomaly_detected = score >= 0.45

    return anomaly_detected, score


def calculate_stability_index(df):
    """
    Calculate a simple demonstration stability index
    using heart-rate variation.
    """

    if len(df) <= 1:
        return 100.0

    heart_rate_std = df["HeartRate"].std()

    if pd.isna(heart_rate_std):
        return 100.0

    stability_index = 100.0 - (
        float(heart_rate_std) * 5.0
    )

    return float(
        round(
            max(
                0.0,
                min(stability_index, 100.0)
            ),
            2
        )
    )


def get_severity(risk_score):
    """Convert the risk score into a dashboard category."""

    if risk_score >= 0.65:
        return "Critical"

    if risk_score >= 0.35:
        return "Moderate Warning"

    return "Normal"


def main():

    try:

         # 1. Read JSON from ASP.NET
       
        raw_input = sys.stdin.read()

        if not raw_input.strip():
            raise ValueError(
                "No input payload received."
            )

        data = json.loads(raw_input)

        # Support both a single record and a list of records
        if isinstance(data, dict):
            data = [data]

        if not isinstance(data, list) or len(data) == 0:
            raise ValueError(
                "Payload must be a non-empty JSON array."
            )

         # 2. Convert input to Pandas DataFrame
      
        df = pd.DataFrame(data)

        df = validate_data(df)

        latest = df.iloc[-1]

         # 3. Load trained model
    
        model = load_ai_model()

         # 4. Run trained ML model
      
        if model is not None:

            features = df[
                [
                    "SystolicBp",
                    "HeartRate",
                    "Glucose"
                ]
            ]

            predictions = model.predict(features)

            anomaly_detected = bool(
                predictions[-1]
            )

            if hasattr(model, "predict_proba"):

                probabilities = model.predict_proba(
                    features
                )

                latest_probabilities = probabilities[-1]

                # Binary classifier:
                # class 1 = anomaly
                if len(latest_probabilities) > 1:
                    risk_score = float(
                        latest_probabilities[1]
                    )
                else:
                    risk_score = float(
                        latest_probabilities[0]
                    )

            else:
                risk_score = (
                    1.0
                    if anomaly_detected
                    else 0.0
                )

            prediction_source = "trained_ml_model"

        # 5. Fallback if model isn't available
      
        else:

            (
                anomaly_detected,
                risk_score
            ) = run_rule_based_fallback(latest)

            prediction_source = (
                "rule_based_demo_fallback"
            )

         # 6. Calculate dashboard metrics
     
        stability_index = calculate_stability_index(df)

        risk_percentage = int(
            round(risk_score * 100)
        )

        severity = get_severity(
            risk_score
        )

        status_text = (
            "Anomaly Alert"
            if anomaly_detected
            else "Standard Status"
        )

        # 7. Create JSON response for ASP.NET
        
        analysis_result = {

            "isSuccess": True,

            "status": status_text,

            "anomalyDetected": anomaly_detected,

            "riskProbability": round(
                risk_score,
                4
            ),

            "riskIndex": risk_percentage,

            "actionTier": severity,

            "stabilityIndex": stability_index,

            "metricsProcessed": int(
                len(df)
            ),

            "predictionSource": prediction_source
        }

         # 8. Send JSON back to ASP.NET
     
        print(
            json.dumps(
                analysis_result
            )
        )

    except json.JSONDecodeError:

        error_payload = {
            "isSuccess": False,
            "status": "Pipeline Error",
            "riskIndex": 0,
            "actionTier": "Error",
            "error": "Invalid JSON received."
        }

        print(
            json.dumps(error_payload),
            file=sys.stderr
        )

        sys.exit(1)

    except Exception as e:

        error_payload = {
            "isSuccess": False,
            "status": "Pipeline Error",
            "riskIndex": 0,
            "actionTier": "Error",
            "error": str(e)
        }

        print(
            json.dumps(error_payload),
            file=sys.stderr
        )

        sys.exit(1)


if __name__ == "__main__":
    main()

"""import sys
import json
import os
import pandas as pd
import numpy as np
import joblib # Used to load your trained .pkl or .joblib model files

def load_ai_model():
    """
   # Trained models are typically saved as pickle/joblib files.
    #If you don't have one trained yet, we fall back to a deterministic rule-engine
   # so your pipeline won't break during testing.
    """
    try:
        # Replace this with the actual path to your saved Random Forest or XGBoost model
    
        script_dir = os.path.dirname(os.path.abspath(__file__))
        model_path = os.path.join(script_dir, "models", "biometric_rf_model.pkl")
        return joblib.load(model_path)
    except:
        return None

def main():
    try:
        # Read the raw JSON list streamed from C#
        raw_input = sys.stdin.read()
        if not raw_input.strip():
            print(json.dumps({"error": "No JSON data received from C#"}), file=sys.stderr)
            sys.exit(1)

        data = json.loads(raw_input)
        
        # Enforce list formatting if single record is sent
        if isinstance(data, dict):
            data = [data]
            
        df = pd.DataFrame(data)

        # 1. Try running your real trained ML model
        model = load_ai_model()
        
        if model is not None:
            # Map features strictly to match your training set matrix 
            # (e.g., [['SystolicBp', 'DiastolicBp', 'HeartRate', 'Glucose']])
            features = df[['SystolicBp', 'HeartRate', 'Glucose']]
            predictions = model.predict(features)          # Returns 0 or 1 for anomaly
            probabilities = model.predict_proba(features)  # Returns float risk scales
            
            anomaly_detected = bool(predictions[-1]) 
            risk_score = float(probabilities[-1][1])
        else:
            # 2. FALLBACK/MOCK ML: Simulating Random Forest probabilities for testing
            # If you haven't saved a .pkl model yet, this ensures your pipeline works!
            latest = df.iloc[-1]
            score = 0.1
            if latest['SystolicBp'] > 140 or latest['HeartRate'] > 100: score += 0.4
            if latest['Glucose'] > 140: score += 0.3

            spo2 = latest.get('SpO2', 98)  
            if spo2 < 95: score += 0.15
            
            anomaly_detected = score >= 0.5
            risk_score = min(score, 1.0)

        # Calculate a rolling stability trend index
        stability_index = 100 - (df['HeartRate'].std() if len(df) > 1 else 0)
        
        risk_percentage = int(risk_score * 100)
        status_text = "Anomaly Alert" if anomaly_detected else "Standard Status"
        severity_tier = "Critical" if risk_score >= 0.70 else "Moderate Warning" if risk_score >= 0.35 else "Normal"

        
        # 3. Stream the execution evaluation result back to C# as JSON
        analysis_result = {
            """
          #  "isSuccess": True,
          #  "anomalyDetected": anomaly_detected,
           # "riskProbability": round(risk_score, 4),
            #"stabilityIndex": round(max(0, min(stability_index, 100)), 2),
            #"clinicalSeverity": "Critical" if risk_score >= 0.75 else "Moderate Warning" if risk_score >= 0.45 else "Normal"
            """

            "status": status_text,
            "riskIndex": risk_percentage,
            "actionTier": severity_tier,
            "stabilityIndex": round(max(0, min(stability_index, 100)), 2),
            "metricsProcessed": len(df)
        }
        
        print(json.dumps(analysis_result))

    except Exception as e:
        error_payload = {"isSuccess": False, "error": str(e)}
        print(json.dumps(error_payload), file=sys.stderr)
        sys.exit(1)

if __name__ == "__main__":
    main()
"""
