import os
import numpy as np
import pandas as pd
from sklearn.model_selection import train_test_split
from sklearn.ensemble import RandomForestClassifier
import joblib

def generate_synthetic_clinical_data(samples=2000):
    """Generates a synthetic medical dataset to train our health AI model."""
    np.random.seed(42)
    
    # Simulate realistic vital variations for patients
    heart_rate = np.random.normal(75, 15, samples)
    systolic_bp = np.random.normal(120, 18, samples)
   
    glucose = np.random.normal(95, 25, samples)
    
    df = pd.DataFrame({
        'HeartRate': heart_rate,
        'SystolicBp': systolic_bp,
        
        'Glucose': glucose
    })
    
    # Medical Rule Logic: Label as an Anomaly (1) if vitals cross dangerous thresholds
    conditions = (
        (df['SystolicBp'] > 145) | (df['SystolicBp'] < 90) |
        (df['HeartRate'] > 105) | (df['HeartRate'] < 50) |
        (df['Glucose'] > 150) | (df['Glucose'] < 60)
    )
    df['Anomaly'] = np.where(conditions, 1, 0)
    
    # Introduce small real-world noise (errors/outliers) so the AI learns to adapt
    noise = np.random.choice([0, 1], size=samples, p=[0.96, 0.04])
    df['Anomaly'] = np.bitwise_xor(df['Anomaly'], noise)
    
    return df

def main():
    print("Initializing Clinical Dataset Synthesis Pipeline...")
    df = generate_synthetic_clinical_data()
    
    # Features match the exact matrix columns sent from your ASP.NET backend
    X = df[['SystolicBp', 'HeartRate', 'Glucose']]
    y = df['Anomaly']
    
    X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2, random_state=42)
    
    print(f"Training Random Forest Classifier on {len(X_train)} clinical snapshots...")
    model = RandomForestClassifier(n_estimators=100, max_depth=8, random_state=42)
    model.fit(X_train, y_train)
    
    accuracy = model.score(X_test, y_test)
    print(f"Model Training Complete. Validation Accuracy: {accuracy * 100:.2f}%")
    
    # FIX: Get current directory (Scripts/models/) and save directly inside it
    current_dir = os.path.dirname(os.path.abspath(__file__))
    model_path = os.path.join(current_dir, "biometric_rf_model.pkl")
    
    joblib.dump(model, model_path)
    print(f"Success! Model binary serialized safely to disk at: {model_path}")

if __name__ == "__main__":
    main()
