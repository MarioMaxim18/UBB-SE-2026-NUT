-- Create DailyMealLogs table for tracking individual meals from meal plans

CREATE TABLE DailyMealLogs (
    id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL,
    meal_id INT NOT NULL,
    meal_name NVARCHAR(255) NOT NULL,
    log_date DATETIME NOT NULL,
    calories INT NOT NULL,
    protein INT NOT NULL,
    carbs INT NOT NULL,
    fat INT NOT NULL,
    meal_type NVARCHAR(50) NOT NULL,
    CONSTRAINT FK_DailyMealLogs_Users FOREIGN KEY (user_id) REFERENCES Users(id) ON DELETE CASCADE,
    CONSTRAINT FK_DailyMealLogs_Meals FOREIGN KEY (meal_id) REFERENCES Meals(id)
);

-- Create index for faster queries by user and date
CREATE INDEX IX_DailyMealLogs_UserDate ON DailyMealLogs(user_id, log_date);

-- Create index for date-based queries
CREATE INDEX IX_DailyMealLogs_Date ON DailyMealLogs(log_date);
