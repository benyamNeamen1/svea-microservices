# hotel-booking-prediction

This is Group 12's main repository containing the code used for the hotel booking prediction project for the machine learning course. This project uses the ["Hotel Booking Demand" dataset from Kaggle](https://www.kaggle.com/datasets/jessemostipak/hotel-booking-demand) to understand the factors influencing hotel booking cancellations and predict the likelihood of cancellation. We will explore the dataset's characteristics, gain insights through analysis, and evaluate the performance of various machine learning classifiers.

## Demo web application

View the final web application running on [http://group-12-demo.norwayeast.cloudapp.azure.com](http://group-12-demo.norwayeast.cloudapp.azure.com)

## Running the ML model training process

If you want to run the dataset pre-processing and data model training processes, follow these steps:

1. Clone the repo and go to the root directory
2. Have python 3 installed (we built it on v3.12)
3. Install required packages using `pip install -r requirements.txt`
4. Run the step-by-step Jupyter notebook stored under [src/GROUP_12_ML_BTH.ipynb](src/GROUP_12_ML_BTH.ipynb)
5. View the each cell's output in the notebook

## Running the web app locally

To run the project locally on your machine follow these steps:

1. Clone the repo and go to the root directory
2. Build the docker image using `docker build . -t hotel-booking-prediction`
3. Run the docker image using `docker run hotel-booking-prediction -p 3000`
4. View the web app in your browser running on [localhost:3000](http://localhost:3000)

## Dataset Overview

The dataset contains information about hotel bookings, including booking details, guest demographics, and reservation status. It comprises 119391 rows and 32 columns. The key features include:

- `hotel`: Type of hotel (Resort Hotel or City Hotel)
- `is_canceled`: Whether the booking was canceled (1) or not (0)
- `lead_time`: Number of days between booking and arrival
- `arrival_date_year`/`month`/`week_number`/`day_of_month`: Arrival date details
- `stays_in_weekend_nights`/`stays_in_week_nights`: Number of weekend/week nights stayed
- `adults`/`children`/`babies`: Number of adults, children, and babies in the booking
- `meal`: Type of meal booked
- `country`: Country of origin of the guest
- `market_segment`: Market segment designation
- `distribution_channel`: Booking distribution channel
- `is_repeated_guest`: Whether the guest is a repeated guest (1) or not (0)
- `previous_cancellations`: Number of previous bookings canceled by the guest
- `previous_bookings_not_canceled`: Number of previous bookings not canceled by the guest
- `reserved_room_type`: Code of room type reserved
- `assigned_room_type`: Code for the type of room assigned to the booking
- `booking_changes`: Number of changes/amendments made to the booking
- `deposit_type`: Indication of deposit type (No Deposit, Non Refund, Refundable)
- `agent`: ID of the travel agent that made the booking
- `company`: ID of the company/entity that made the booking
- `days_in_waiting_list`: Number of days the booking was in the waiting list
- `customer_type`: Type of booking, assuming one of four categories
- `adr`: Average Daily Rate, as defined by dividing the sum of all lodging transactions by the total number of staying nights
- `required_car_parking_spaces`: Number of car parking spaces required by the customer
- `total_of_special_requests`: Number of special requests made by the customer
- `reservation_status`: Reservation last status, assuming one of three categories
- `reservation_status_date`: Date at which the last status was set. This variable can be used in conjunction with the ReservationStatus to understand when was the booking canceled or when did the customer checked-out of the hotel

## Exploratory Data Analysis (EDA) and Insights

EDA revealed key insights about hotel booking patterns and guest behavior.

### Guest Origin

Most guests originate from Portugal and other European countries. This information can be visualized using a choropleth map:

![Guest Origin Map](images/map.png)

### Room Prices

Room prices vary significantly based on room type, meal arrangements, and seasonal factors. Box plots illustrate the average price per room type for each hotel:

![reserved_room_type](images/reserved_room_type.png)

### Price Variation Over the Year

Prices at the Resort Hotel are generally higher during the summer months, while prices at the City Hotel peak during spring and autumn. This trend can be observed in a line chart:

![price-per-night](images/price-per-night.png)

### Busy Months

Both hotels experience peak occupancy during the summer months and lower occupancy during the winter months. A line chart can visualize the total number of guests per month:

![no-of-guests](images/no-of-guests.png)

### Length of Stay

Guests typically stay for a few nights, with stays of 1-4 nights being the most common. A bar chart can depict the distribution of stay durations:

![total-nights](images/total-nights.png)

## Data Preprocessing

Prior to model building, the dataset underwent preprocessing steps:

- Handling missing values: Missing values were filled with zero.
- Removing irrelevant columns: Columns deemed irrelevant for prediction, such as `days_in_waiting_list` and `arrival_date_year`, were removed.
- Encoding categorical variables: Categorical features were encoded using numerical representations.
- Normalizing numerical variables: Numerical features were normalized using logarithmic transformations to address skewed distributions.
- Balancing the dataset: The dataset was balanced using SMOTE (Synthetic Minority Over-sampling Technique) to address class imbalance.

## Machine Learning Models and Performance

Several machine learning classifiers were trained and evaluated to predict booking cancellations. The table below shows the final accuracy of each model, sorted from worst to best:

| Model                           | Accuracy |
| ------------------------------- | -------- |
| Logistic Regression             | 0.811845 |
| K-Nearest Neighbors (KNN)       | 0.889439 |
| Gradient Boosting Classifier    | 0.907670 |
| Decision Tree Classifier        | 0.946341 |
| Ada Boost Classifier            | 0.947012 |
| Extra Trees Classifier          | 0.951123 |
| Random Forest Classifier        | 0.954618 |
| Voting Classifier               | 0.964041 |
| LGBM Classifier                 | 0.968123 |
| XgBoost Classifier              | 0.983391 |
| Artificial Neural Network (ANN) | 0.990018 |

![models-comparison](images/models-comparison.png)

All of these results and the way we calculated them can be viewed in detail in the [src/GROUP_12_ML_BTH.ipynb](src/GROUP_12_ML_BTH.ipynb) notebook.

## Web Application

Based on the superior performance of the Artificial Neural Network (ANN) model, a user-friendly interface was developed using Streamlit to facilitate real-time prediction of hotel booking cancellations. This interface allows users, such as hotel managers or booking agents, to input specific booking details and receive an immediate assessment of the likelihood of that booking being canceled.

### Interface Description

The interface, titled "🏨 Hotel Booking Cancellation Predictor", presents a clean and intuitive layout. Key components include:

- Sidebar for Input: All required booking details are entered via widgets located in a sidebar. This keeps the main area clutter-free for displaying results.
- Categorical Feature Inputs: Dropdown menus are used for selecting predefined options for features like 'Hotel Type', 'Meal Type', 'Market Segment', 'Distribution Channel', 'Reserved Room Type', 'Deposit Type', 'Customer Type', and 'Status Update Year (Encoded)'.
- Numerical Feature Inputs: Sliders and number input fields allow the user to specify values for 'Status Update Month', 'Status Update Day', 'Lead Time (days)', 'Arrival Week Number', 'Arrival Day of Month', 'Weekend Nights', 'Week Nights', 'Number of Adults', 'Number of Children', 'Number of Babies', 'Previous Cancellations', 'Previous Bookings Not Canceled', 'Agent ID', 'Company ID', 'Average Daily Rate (ADR)', 'Required Car Parking Spaces', and 'Total Special Requests'. A separate select box is used for 'Is Repeated Guest?'.
- Prediction Button: A button labeled "Predict Cancellation" initiates the prediction process after the user has entered all necessary details.
- The interface displays the processed input features for user verification. This step ensures transparency by showing how the raw inputs are transformed (e.g., numerical encoding, log transformations) before being fed into the model, mirroring the preprocessing steps applied during training.
- The final prediction is presented clearly, indicating whether the booking is likely to be canceled or not, along with the calculated probability. Warnings are used for likely cancellations, and success messages for non-cancellations.

![web-app-interface](images/web-app-interface.png)

### Prediction Process

When the user clicks "Predict Cancellation":

1. Data Collection & Preprocessing: The interface gathers all the inputs provided by the user. It then applies the exact same preprocessing steps used during the model training phase. This includes mapping the selected categorical options to their numerical codes (e.g., 'Resort Hotel' to 0, 'City Hotel' to 1) and applying necessary transformations like logarithmic scaling to specific numerical features (e.g., 'lead_time', 'adr', 'agent', 'company') to match the format the ANN model expects.
2. Model Inference: The preprocessed features are structured into the correct format (a feature vector or DataFrame with columns in the precise order the model was trained on). This data is then fed into the pre-loaded ANN model (hotel_booking_ann_model.keras).
3. Output Generation: The ANN model outputs a probability score (specifically, the probability of the booking being canceled, assuming a sigmoid activation in the final layer for binary classification). The interface interprets this probability (using a 0.5 threshold) to classify the booking as 'Likely Canceled' or 'Likely Not Canceled' and displays the result along with the probability score.

![web-app-result](images/web-app-result.png)

### Use Case

This predictive tool serves several practical purposes for hotel management:

- Proactive Risk Assessment: By inputting details of a new or existing booking, staff can quickly assess its cancellation risk.
- Targeted Retention Strategies: For bookings identified as high-risk, hotels can implement proactive measures. This might include targeted communication, offering incentives (e.g., a small discount, complimentary service) closer to the arrival date, or requiring a more stringent deposit type upon booking.
- Resource Management: Understanding the likelihood of cancellations can aid in more accurate forecasting of occupancy rates, allowing for better management of staffing, inventory, and pricing strategies.
- Overbooking Strategy Refinement: Hotels often overbook to compensate for anticipated cancellations. This tool can provide data-driven insights to refine overbooking levels more accurately, minimizing both revenue loss from empty rooms and guest dissatisfaction from relocation.

By integrating the predictive power of the ANN model into a simple web interface, this tool provides actionable insights to help mitigate revenue loss associated with booking cancellations.

The source code of the web application can be found in the [src/main.py](src/main.py) file.
