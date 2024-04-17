import neurokit2 as nk
import pandas as pd
import numpy as np
import os
import csv
import json
from keras.models import Sequential
from keras.layers import Dense, LSTM
from sklearn.model_selection import train_test_split
from sklearn.metrics import mean_squared_error
import random
import string
import argparse

parser = argparse.ArgumentParser(description='Process CSV files in a folder.')
parser.add_argument('folder_path', type=str, help='Path to the folder containing CSV files')
args = parser.parse_args()

folder_path = args.folder_path + '\\'

#constants
needed_sampling_rate=125

def read_csv_files_from_folder_and_combine(folder_path):
    # Get a list of all CSV files in the folder
    csv_files = [file for file in os.listdir(folder_path) if file.endswith('.csv')]
    # Read each CSV file and append the data to a list
    data_list = []
    for file in csv_files:
        file_path = os.path.join(folder_path, file)
        data = pd.read_csv(file_path)
        data_list.append(data)
    # Combine the data from all the files into a single DataFrame
    combined_data = pd.concat(data_list)
    combined_data = combined_data.rename(columns={' PLETH': 'ppg_data', '0': 'ppg_data', 'Time [s]': 'time' })
    return combined_data

def generate_unique_filename():
    """
    Generates a unique filename by appending a random string to the base filename.
    """
    # Generate a random string of 8 characters
    random_string = ''.join(random.choices(string.ascii_lowercase + string.digits, k=8))

    # Combine the base filename and the random string with an underscore
    unique_filename = f"{random_string}"

    # If the unique filename does not exist, return it
    return unique_filename

file_prefix = generate_unique_filename()

combined_data = read_csv_files_from_folder_and_combine(folder_path)
ppgFromData = combined_data['ppg_data']

input_data = combined_data[['ppg_data']].values
target_data = combined_data['ppg_data'].values

# Split the data into training and testing sets
train_size = int(0.8 * len(input_data))
input_train, input_test = input_data[:train_size], input_data[train_size:]
target_train, target_test = target_data[:train_size], target_data[train_size:]

# Convert the NumPy arrays to Pandas DataFrames
input_train = pd.DataFrame(input_train)
input_test = pd.DataFrame(input_test)
target_train = pd.DataFrame(target_train)
target_test = pd.DataFrame(target_test)

# Drop any rows in the DataFrames that contain NaN values
input_train = input_train.dropna()
input_test = input_test.dropna()
target_train = target_train.dropna()
target_test = target_test.dropna()
target_test

# Create and compile the model
model = Sequential()
model.add(LSTM(128, input_shape=(input_data.shape[1], 1)))
model.add(Dense(1))
model.compile(loss='mean_squared_error', optimizer='adam')

# Train the model
model.fit(input_train, target_train, epochs=10, batch_size=32, validation_data=(input_test, target_test))

predictions = model.predict(input_test)
mse = mean_squared_error(target_test, predictions)
print('Mean Squared Error:', mse)

# Generate predictions for new data with a size equal to 20% of the original dataset
new_data = input_data[-int(0.2 * len(input_data)):]

new_predictions = model.predict(new_data)

# Save the predictions in a new CSV file
predictions_df = pd.DataFrame(new_predictions, columns=['Predicted'])
predictions_df.to_csv(folder_path + 'predictions_' + file_prefix + '.csv', index=False)

ppgFromData = pd.concat([ppgFromData, pd.Series(new_predictions.flatten())], ignore_index=True)

ppg_clean = nk.ppg_clean(ppgFromData, method='elgendi')

signals = pd.DataFrame({'PPG_Raw' : ppgFromData,
                        'PPG_Clean' : ppg_clean})
nk.signal_plot(signals, sampling_rate=needed_sampling_rate)


clean_ppg_peaks, info = nk.ppg_peaks(ppg_clean, sampling_rate=needed_sampling_rate, method='elgendi', correct_artifacts=True, show=False)

info, clean_peaks_corrected = nk.signal_fixpeaks(
    clean_ppg_peaks, sampling_rate=needed_sampling_rate, iterative=True, method="Kubios", show=False)

hrv_times = nk.hrv_time(clean_peaks_corrected, sampling_rate=needed_sampling_rate, show=False)
hrv_times.columns

hrv_freqs = nk.hrv_frequency(clean_peaks_corrected, sampling_rate=needed_sampling_rate, show=False, psd_method='welch')

ppgProcess, info = nk.ppg_process(ppg_clean, needed_sampling_rate)

ppgProcess = ppgProcess['PPG_Rate'].iloc[info['PPG_Peaks']]
# Create a DataFrame from the array with a specific column name
ppgProcess = pd.DataFrame(data=ppgProcess, columns=['PPG_Rate'])

# Select the columns to include in the new dataframe
columns_to_include = ['PPG_Rate', 'HRV_RMSSD', 'HRV_SDNN', 'HRV_pNN50', 'HRV_LF', 'HRV_HF', 'HRV_CVNN', 'HRV_LFn', 'HRV_HFn', 'HRV_SDSD']

# Create a dictionary with the selected columns from each dataframe
data = {}
for i, df in enumerate([ppgProcess, hrv_times, hrv_freqs]):
    common_columns = df.columns.intersection(columns_to_include)
    for column in common_columns:
        data[column] = df[column].tolist()

result_file_name=folder_path + 'output_' + file_prefix + '.json'

# Write the dictionary to a JSON file
with open(result_file_name, 'w') as f:
    json.dump(data, f, indent=4)
