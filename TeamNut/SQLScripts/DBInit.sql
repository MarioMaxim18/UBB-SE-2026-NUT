
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
DROP TABLE IF EXISTS ShoppingItems;
CREATE TABLE Users (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    username VARCHAR(100) NOT NULL,
    password VARCHAR(100) NOT NULL,
    role VARCHAR(50) NOT NULL
);

CREATE TABLE UserData (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
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
    meal_id INTEGER PRIMARY KEY AUTOINCREMENT,
    imageUrl VARCHAR(255),
    name VARCHAR(100) NOT NULL,
    isKeto INTEGER DEFAULT 0,
    isLactoseFree INTEGER DEFAULT 0,
    isNutFree INTEGER DEFAULT 0,
    isVegan INTEGER DEFAULT 0,
    isGlutenFree INTEGER DEFAULT 0,
    description VARCHAR(255)
);

CREATE TABLE Ingredients (
    food_id INTEGER PRIMARY KEY AUTOINCREMENT,
    name VARCHAR(100) NOT NULL,
    calories_per_100g FLOAT,
    protein_per_100g FLOAT,
    carbs_per_100g FLOAT,
    fat_per_100g FLOAT
);

CREATE TABLE MealsIngredients (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    meal_id INT,
    food_id INT,
    quantity FLOAT,
    FOREIGN KEY (meal_id) REFERENCES Meals(meal_id) ON DELETE CASCADE,
    FOREIGN KEY (food_id) REFERENCES Ingredients(food_id) ON DELETE CASCADE
);

CREATE TABLE MealPlan (
    mealplan_id INTEGER PRIMARY KEY AUTOINCREMENT,
    user_id INT,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    goal_type VARCHAR(50),
    FOREIGN KEY (user_id) REFERENCES Users(id) ON DELETE CASCADE
);

CREATE TABLE MealPlanMeal (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    mealPlanId INT,
    mealId INT,
    isConsumed INTEGER DEFAULT 0,
    mealType VARCHAR(50),
    assigned_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (mealPlanId) REFERENCES MealPlan(mealplan_id) ON DELETE CASCADE,
    FOREIGN KEY (mealId) REFERENCES Meals(meal_id) ON DELETE CASCADE
);

CREATE TABLE DailyLogs (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    user_id INT,
    mealId INT,
    calories FLOAT,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES Users(id) ON DELETE CASCADE,
    FOREIGN KEY (mealId) REFERENCES Meals(meal_id) ON DELETE CASCADE
);

CREATE TABLE Favorites (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    userId INT,
    mealId INT,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (userId) REFERENCES Users(id) ON DELETE CASCADE,
    FOREIGN KEY (mealId) REFERENCES Meals(meal_id) ON DELETE CASCADE
);

CREATE TABLE UserBehaviour (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    user_id INT,
    mealType VARCHAR(50),
    actionType VARCHAR(50),
    mealId INT,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES Users(id) ON DELETE CASCADE,
    FOREIGN KEY (mealId) REFERENCES Meals(meal_id) ON DELETE CASCADE
);

CREATE TABLE Inventory (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    user_id INT,
    ingredient_id INT,
    quantity_grams FLOAT,
    FOREIGN KEY (user_id) REFERENCES Users(id) ON DELETE CASCADE,
    FOREIGN KEY (ingredient_id) REFERENCES Ingredients(food_id) ON DELETE CASCADE
);

CREATE TABLE Reminders (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    user_id INT,
    name VARCHAR(100),
    has_sound INTEGER DEFAULT 1,
    time TIME,
    frequency VARCHAR(50),
    FOREIGN KEY (user_id) REFERENCES Users(id) ON DELETE CASCADE
);

CREATE TABLE Conversations (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    has_unanswered INTEGER DEFAULT 0,
    user_id INT,
    FOREIGN KEY (user_id) REFERENCES Users(id) ON DELETE CASCADE
);

CREATE TABLE Messages (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    sent_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    conversation_id INT,
    sender_id INT,
    text_content VARCHAR(300),
    FOREIGN KEY (conversation_id) REFERENCES Conversations(id) ON DELETE CASCADE,
    FOREIGN KEY (sender_id) REFERENCES Users(id) ON DELETE NO ACTION
);

CREATE TABLE ShoppingItems (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    user_id INT,
    ingredient_id INT,
    quantity_grams FLOAT DEFAULT 0,
    is_checked INTEGER DEFAULT 0,
    FOREIGN KEY (user_id) REFERENCES Users(id) ON DELETE CASCADE,
    FOREIGN KEY (ingredient_id) REFERENCES Ingredients(food_id) ON DELETE CASCADE
);
