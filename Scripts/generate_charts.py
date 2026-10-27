import sys
import json
import pandas as pd
import matplotlib.pyplot as plt
import numpy as np
import os


def main():
    try:
        # 1. Read the Patient ID passed from C#
        patient_study_id = sys.argv[1] if len(sys.argv) > 1 else "Unknown"

        # 2. Read the JSON text that C# sends straight into memory
        raw_input = sys.stdin.read()

        if not raw_input.strip():
            print("PythonErr: No JSON data received from C#", file=sys.stderr)
            sys.exit(1)

        # 3. Use Pandas to load that JSON data into a table
        data = json.loads(raw_input)
        df = pd.DataFrame(data)

        if df.empty:
            print("PythonErr: Dataframe is empty", file=sys.stderr)
            sys.exit(1)

        # FIX: Ensure all columns exist to prevent KeyError exceptions if C# sends partial records
        if 'SystolicBp' not in df.columns:
            df['SystolicBp'] = 0
        if 'HeartRate' not in df.columns:
            df['HeartRate'] = 0
        if 'Glucose' not in df.columns:
            df['Glucose'] = 0

        # Fill any individual null/NaN values with 0 so Matplotlib can safely plot them
        df['SystolicBp'] = df['SystolicBp'].fillna(0)
        df['HeartRate'] = df['HeartRate'].fillna(0)
        df['Glucose'] = df['Glucose'].fillna(0)

        # Clean the date column for plotting
        if 'DateRecorded' in df.columns:
            df['DateRecorded'] = pd.to_datetime(df['DateRecorded'])
            df = df.sort_values(by='DateRecorded')
            df['DateStr'] = df['DateRecorded'].dt.strftime('%Y-%m-%d')
        else:
            df['DateStr'] = "Unknown Date"

        # 4. Use Matplotlib to draw ONE graph with three side-by-side bars
        fig, ax = plt.subplots(figsize=(10, 6))

        x_indexes = np.arange(len(df))
        
        # Reduced width slightly to 0.22 so all 3 bars sit cleanly next to each other
        if len(df) == 1:
            bar_width = 0.08
        else:
            bar_width = 0.22    

        # All three items use x_indexes numerical alignment positions safely now
        ax.bar(x_indexes - bar_width, df['SystolicBp'], width=bar_width, edgecolor='black', linewidth=0.5, color='red', alpha=0.7, label='Systolic BP')
        ax.bar(x_indexes, df['HeartRate'], width=bar_width, edgecolor='black', linewidth=0.5, color='orangered', alpha=0.7, label='Heart Rate (BPM)')
        ax.bar(x_indexes + bar_width, df['Glucose'], width=bar_width, edgecolor='black', linewidth=0.5, color='seagreen', alpha=0.85, label='Blood Glucose')

        # Threshold alert line at 140
        ax.axhline(y=140, color='darkred', linestyle='--', alpha=0.5, label='Alert Threshold (140)')

        #fixed y axis intervals: steps of 40 up to 200 max
        ax.set_yticks(np.arange(0, 201, 40))
        ax.set_ylim(0, 205)
        
        # Labels and Styling
        ax.set_ylabel('Metrics Scale (mmHg / BPM / mg/dL)')
        ax.set_xlabel('Dates Checked')
        ax.set_title(f'Clinical Analytics Timeline - ID: {patient_study_id}')
        
        # Center the date tags directly under your three-bar blocks
        ax.set_xticks(x_indexes)
        ax.set_xticklabels(df['DateStr'], rotation=15)


        if len(df) == 1:
            ax.set_xlim(-0.5, 0.5)  # Center the single bar in the middle of the plot
        
        ax.legend(loc='upper left')
        ax.grid(True, axis='y', alpha=0.2)

        plt.tight_layout()

        # 5. Path management (Matches Dockerfile structure perfectly)
        script_dir = os.path.dirname(os.path.abspath(__file__)) 
        base_app_dir = os.path.dirname(script_dir)  # Points straight to /app inside Docker
        
        output_dir = os.path.join(base_app_dir, "wwwroot", "charts")

        os.makedirs(output_dir, exist_ok=True)
        
        output_filename = os.path.join(output_dir, f"chart_{patient_study_id}.png")
       
        if os.path.exists(output_filename):
            os.remove(output_filename)  
        
        plt.savefig(output_filename, dpi=150)
        plt.close('all')

        print("Chart generated Cleanly")

    except Exception as e:
        print(f"PYTHON ERROR: {str(e)}", file=sys.stderr)
        sys.exit(1)

if __name__ == "__main__":
    main()


 