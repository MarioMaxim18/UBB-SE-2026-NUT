
DROP TABLE IF EXISTS Messages;
DROP TABLE IF EXISTS Conversations;
DROP TABLE IF EXISTS Reminders;
DROP TABLE IF EXISTS Inventory;
DROP TABLE IF EXISTS UserBehaviour;
DROP TABLE IF EXISTS Favorites;
DROP TABLE IF EXISTS DailyLogs;
DROP TABLE IF EXISTS MealPlanMeal;
DROP TABLE IF EXISTS MealPlan;
DROP TABLE IF EXISTS MealsIngredients;
DROP TABLE IF EXISTS Ingredients;
DROP TABLE IF EXISTS Meals;
DROP TABLE IF EXISTS UserData;
DROP TABLE IF EXISTS Users;

CREATE TABLE Users (
    id INT IDENTITY(1,1) PRIMARY KEY,
    username VARCHAR(100) NOT NULL,
    password VARCHAR(100) NOT NULL,
    role VARCHAR(50) NOT NULL
);

CREATE TABLE UserData (
    id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT,
    weight FLOAT,
    height FLOAT,
    age INT,
    gender VARCHAR(20),
    goal VARCHAR(50),
    bmi FLOAT,
    calorie_needs INT,
    protein_needs INT,
    carb_needs INT,
    fat_needs INT,
    FOREIGN KEY (user_id) REFERENCES Users(id) ON DELETE CASCADE
);

CREATE TABLE Meals (
    meal_id INT IDENTITY(1,1) PRIMARY KEY,
    imageUrl VARCHAR(255),
    name VARCHAR(100) NOT NULL,
    isKeto BIT DEFAULT 0,
    isLactoseFree BIT DEFAULT 0,
    isNutFree BIT DEFAULT 0,
    isVegan BIT DEFAULT 0,
    isGlutenFree BIT DEFAULT 0,
    description VARCHAR(255)
);

CREATE TABLE Ingredients (
    food_id INT IDENTITY(1,1) PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    calories_per_100g FLOAT,
    protein_per_100g FLOAT,
    carbs_per_100g FLOAT,
    fat_per_100g FLOAT
);

CREATE TABLE MealsIngredients (
    id INT IDENTITY(1,1) PRIMARY KEY,
    meal_id INT,
    food_id INT,
    quantity FLOAT,
    FOREIGN KEY (meal_id) REFERENCES Meals(meal_id) ON DELETE CASCADE,
    FOREIGN KEY (food_id) REFERENCES Ingredients(food_id) ON DELETE CASCADE
);

CREATE TABLE MealPlan (
    mealplan_id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT,
    created_at DATETIME DEFAULT GETDATE(),
    goal_type VARCHAR(50),
    FOREIGN KEY (user_id) REFERENCES Users(id) ON DELETE CASCADE
);

CREATE TABLE MealPlanMeal (
    id INT IDENTITY(1,1) PRIMARY KEY,
    mealPlanId INT,
    mealId INT,
    isConsumed BIT DEFAULT 0,
    mealType VARCHAR(50),
    assigned_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (mealPlanId) REFERENCES MealPlan(mealplan_id) ON DELETE CASCADE,
    FOREIGN KEY (mealId) REFERENCES Meals(meal_id) ON DELETE CASCADE
);

CREATE TABLE DailyLogs (
    id INT IDENTITY(1,1) PRIMARY KEY,
    mealPlanId INT,
    totalCalories FLOAT,
    FOREIGN KEY (mealPlanId) REFERENCES MealPlan(mealplan_id) ON DELETE CASCADE
);

CREATE TABLE Favorites (
    id INT IDENTITY(1,1) PRIMARY KEY,
    userId INT,
    mealId INT,
    created_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (userId) REFERENCES Users(id) ON DELETE CASCADE,
    FOREIGN KEY (mealId) REFERENCES Meals(meal_id) ON DELETE CASCADE
);

CREATE TABLE UserBehaviour (
    id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT,
    mealType VARCHAR(50),
    actionType VARCHAR(50),
    mealId INT,
    created_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES Users(id) ON DELETE CASCADE,
    FOREIGN KEY (mealId) REFERENCES Meals(meal_id) ON DELETE CASCADE
);

CREATE TABLE Inventory (
    id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT,
    ingredient_id INT,
    quantity_grams FLOAT,
    FOREIGN KEY (user_id) REFERENCES Users(id) ON DELETE CASCADE,
    FOREIGN KEY (ingredient_id) REFERENCES Ingredients(food_id) ON DELETE CASCADE
);

CREATE TABLE Reminders (
    id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT,
    name VARCHAR(100),
    has_sound BIT DEFAULT 1,
    time TIME,
    frequency VARCHAR(50),
    FOREIGN KEY (user_id) REFERENCES Users(id) ON DELETE CASCADE
);

CREATE TABLE Conversations (
    id INT IDENTITY(1,1) PRIMARY KEY,
    has_unanswered BIT DEFAULT 0,
    user_id INT,
    FOREIGN KEY (user_id) REFERENCES Users(id) ON DELETE CASCADE
);

CREATE TABLE Messages (
    id INT IDENTITY(1,1) PRIMARY KEY,
    sent_at DATETIME DEFAULT GETDATE(),
    conversation_id INT,
    sender_id INT,
    text_content VARCHAR(300),
    FOREIGN KEY (conversation_id) REFERENCES Conversations(id) ON DELETE CASCADE,
    FOREIGN KEY (sender_id) REFERENCES Users(id) ON DELETE NO ACTION
);