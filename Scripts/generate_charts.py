import sys
import json
import pandas as pd
import matplotlib.pyplot as plt
import numpy as np
import os

def main():
    try:
        # 1. Read the Patient ID passed from C#
        patient_study_id = (
            sys.argv[1] if len(sys.argv) > 1 else "Unknown"
        )

        # 2. Read the JSON text sent from C#
        raw_input = sys.stdin.read()

        if not raw_input.strip():
            print(
                "PythonErr: No JSON data received from C#",
                file=sys.stderr
            )
            sys.exit(1)

        # 3. Load JSON into Pandas
        data = json.loads(raw_input)
        df = pd.DataFrame(data)

        # Clean and sort dates
        df['DateRecorded'] = pd.to_datetime(
            df['DateRecorded']
        )

        df = df.sort_values(
            by='DateRecorded'
        )

        df['DateStr'] = df['DateRecorded'].dt.strftime(
            '%Y-%m-%d'
        )

        
        # 4. CREATE ONE CHART FOR ALL THREE HEALTH METRICS
       

        fig, ax = plt.subplots(
            figsize=(10, 6)
        )

        x_indexes = np.arange(len(df))
        bar_width = 0.25

        # Systolic Blood Pressure
        ax.bar(
            x_indexes - bar_width,
            df['SystolicBp'],
            width=bar_width,
            edgecolor='black',
            linewidth=0.5,
            color='red',
            alpha=0.7,
            label='Systolic BP'
        )

        # Heart Rate
        ax.bar(
            x_indexes,
            df['HeartRate'],
            width=bar_width,
            edgecolor='black',
            linewidth=0.5,
            color='orangered',
            alpha=0.7,
            label='Heart Rate (BPM)'
        )

        # Blood Glucose
        ax.bar(
            x_indexes + bar_width,
            df['Glucose'],
            width=bar_width,
            edgecolor='black',
            linewidth=0.5,
            color='seagreen',
            alpha=0.85,
            label='Blood Glucose'
        )

        # Hypertension / glucose reference line
        ax.axhline(
            y=140,
            color='darkred',
            linestyle='--',
            alpha=0.5,
            label='140 Alert Threshold'
        )

        # Chart title
        ax.set_title(
            f'Clinical Analytics Timeline - ID: {patient_study_id}'
        )

        # Axis labels
        ax.set_ylabel(
            'Metric Value'
        )

        ax.set_xlabel(
            'Dates Checked'
        )

        # X-axis dates
        ax.set_xticks(
            x_indexes
        )

        ax.set_xticklabels(
            df['DateStr'],
            rotation=15
        )

        # Legend
        ax.legend(
            loc='upper left'
        )

        # Grid
        ax.grid(
            True,
            axis='y',
            alpha=0.2
        )

        plt.tight_layout()

     
        # 5. SAVE THE CHART
       

        script_dir = os.path.dirname(
            os.path.abspath(__file__)
        )

        # Move from Scripts folder to main project folder
        base_app_dir = os.path.dirname(
            script_dir
        )

        # wwwroot/charts
        output_dir = os.path.join(
            base_app_dir,
            "wwwroot",
            "charts"
        )

        os.makedirs(
            output_dir,
            exist_ok=True
        )

        # Chart filename
        output_filename = os.path.join(
            output_dir,
            f"chart_{patient_study_id}.png"
        )

        # Remove previous chart
        if os.path.exists(
            output_filename
        ):
            os.remove(
                output_filename
            )

        # Save chart
        plt.savefig(
            output_filename,
            dpi=150
        )

        plt.close('all')

        print("Chart generated Cleanly")

    except Exception as e:
        print(
            f"PYTHON ERROR: {str(e)}",
            file=sys.stderr
        )
        sys.exit(1)

"""
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

        # Clean the date column for plotting
        df['DateRecorded'] = pd.to_datetime(df['DateRecorded'])
        df = df.sort_values(by='DateRecorded')

        df['DateStr'] = df['DateRecorded'].dt.strftime('%Y-%m-%d')  #'%b %d' For better x-axis labels

        # 4. Use Matplotlib to draw the graph (Two charts on top of each other)
        
        fig, (ax1, ax2) = plt.subplots(2, 1, figsize=(8, 6), sharex=True)

        x_indexes = np.arange(len(df))
        bar_width = 0.25

        #ax1.bar(x_indexes - bar_width/2, df['SystolicBp'], width=bar_width, edgecolor='black', linewidth=0.5,color='red', alpha=0.7, label='Systolic BP')
        #ax1.bar(x_indexes + bar_width/2, df['HeartRate'], width=bar_width,edgecolor='black', linewidth=0.5, color='orangered', alpha=0.7, label='Heart Rate (BPM)')

        ax1.bar(x_indexes - bar_width/2, df['SystolicBp'], width=bar_width, edgecolor='black', linewidth=0.5,color='red', alpha=0.7, label='Systolic BP')
        ax1.bar(x_indexes + bar_width/2, df['HeartRate'], width=bar_width,edgecolor='black', linewidth=0.5, color='orangered', alpha=0.7, label='Heart Rate (BPM)')


        # Top chart: Blood pressure over time
        #ax1.plot(df['DateRecorded'], df['SystolicBp'], marker='o', color='red', linewidth=2, label='Systolic BP')
        #ax1.plot(df['DateRecorded'], df['HeartRate'], marker='x', color='orangered', linestyle=':', label='Heart Rate (BPM)')
       
        ax1.axhline(y=140, color='darkred', linestyle='--', alpha=0.5, label='Hypertension Alert')
        ax1.set_ylabel('Cardiovascular Metrics')
        ax1.set_xlabel('Dates Checked')
        ax1.set_title(f'Clinical Analytics Timeline - ID: {patient_study_id}')
        ax1.set_xticks(x_indexes)
        ax1.set_xticklabels(df['DateStr'], rotation=15)
        ax1.legend(loc='upper left')
        ax1.grid(True, axis='y' ,alpha=0.2)

        # Bottom chart: Metabolic Trends (Glucose)
        # FIX: Changed column keys to match C# uppercase names
        ax2.bar(df['DateStr'], df['Glucose'],width=bar_width,edgecolor='black', linewidth=0.5 ,color='seagreen', alpha=0.85, label='Blood Glucose')
        ax2.axhline(y=140, color='darkgreen', linestyle='--', alpha=0.5, label='Pre-Diabetes Alert')
        ax2.set_ylabel('Glucose Level (mg/dL)')
        ax2.set_xlabel('Dates Checked')
        ax2.legend(loc='upper left')
        ax2.grid(True, alpha=0.2, axis='y')

        plt.xticks(rotation=15)
        plt.tight_layout()

        script_dir = os.path.dirname(os.path.abspath(__file__)) 
        
        # Find the word 'Scripts' and split the path there to find the main source folder
        # This completely bypasses the hidden 'bin/Debug' folders!
        # base_source_dir = script_dir.split(os.sep + "Scripts")[0]

        base_app_dir = os.path.dirname(script_dir)  # Move up one level from Scripts to the main project folder
        
        # Point directly to the source directory's wwwroot/charts folder
        #output_dir = r"C:\Users\PC\source\repos\health information\wwwroot\charts"
        
        output_dir = os.path.join(base_app_dir, "wwwroot", "charts")

        # Force Windows to create the folders in your source directory
        os.makedirs(output_dir, exist_ok=True)
        
        # Generate the absolute file saving path
        output_filename = os.path.join(output_dir, f"chart_{patient_study_id}.png")
       
        if os.path.exists(output_filename):
            os.remove(output_filename)  
        
        plt.savefig(output_filename, dpi=150)
        plt.close('all')
        # =============================================================

        print("Chart generated Cleanly")

    except Exception as e:
        print(f"PYTHON ERROR: {str(e)}", file=sys.stderr)
        sys.exit(1)

if __name__ == "__main__":
    main()

        # 5. SAVE the chart to the public folder where C# can display it
        #output_filename = f"wwwroot/charts/chart_{patient_study_id}.png"
        #plt.savefig(output_filename, dpi=150)
        #plt.close()

        #print("SUCCESS") # Tells C# it worked perfectly

    #except Exception as e:
     #   print(f"PYTHON ERROR: {str(e)}")
      #  sys.exit(1)

#if __name__ == "__main__":
 #   main()
"""
 