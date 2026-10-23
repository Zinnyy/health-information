import sys
import json
import pandas as pd
import numpy as np
import joblib # Used to load your trained .pkl or .joblib model files

def load_ai_model():
    """
    Trained models are typically saved as pickle/joblib files.
    If you don't have one trained yet, we fall back to a deterministic rule-engine
    so your pipeline won't break during testing.
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
            if latest['SpO2'] < 95: score += 0.15
            
            anomaly_detected = score >= 0.5
            risk_score = min(score, 1.0)

        # Calculate a rolling stability trend index
        stability_index = 100 - (df['HeartRate'].std() if len(df) > 1 else 0)

        # 3. Stream the execution evaluation result back to C# as JSON
        analysis_result = {
            "isSuccess": True,
            "anomalyDetected": anomaly_detected,
            "riskProbability": round(risk_score, 4),
            "stabilityIndex": round(max(0, min(stability_index, 100)), 2),
            "clinicalSeverity": "Critical" if risk_score >= 0.75 else "Moderate Warning" if risk_score >= 0.45 else "Normal"
        }
        
        print(json.dumps(analysis_result))

    except Exception as e:
        error_payload = {"isSuccess": False, "error": str(e)}
        print(json.dumps(error_payload), file=sys.stderr)
        sys.exit(1)

if __name__ == "__main__":
    main()
