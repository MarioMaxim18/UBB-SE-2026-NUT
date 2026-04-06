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

    time TEXT,             -- Use TEXT for C# TimeSpan compatibility

    reminder_date TEXT,    -- Stored as 'YYYY-MM-DD'

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




INSERT INTO Users (Username, Password, Role) VALUES ('JaneNutrition', 'SecurePass123', 'Nutritionist');
INSERT INTO Users (Username, Password, Role) VALUES ('JohnDoe', 'UserPass456', 'User');


INSERT INTO Ingredients (name, calories_per_100g, protein_per_100g, carbs_per_100g, fat_per_100g) VALUES
('Chicken Breast', 165, 31, 0, 3.6),
('Salmon', 208, 20, 0, 13),
('Eggs', 155, 13, 1.1, 11),
('Greek Yogurt', 59, 10, 3.6, 0.4),
('Turkey', 135, 30, 0, 1.5),
('Tuna', 132, 28, 0, 1.3),
('Tofu', 76, 8, 1.9, 4.8),
('Ground Beef', 250, 26, 0, 15),
('Cottage Cheese', 98, 11, 3.4, 4.3),

('Brown Rice', 112, 2.6, 23.5, 0.9),
('Quinoa', 120, 4.4, 21.3, 1.9),
('Oats', 389, 16.9, 66.3, 6.9),
('Whole Wheat Bread', 247, 13, 41, 3.4),
('Sweet Potato', 86, 1.6, 20.1, 0.1),
('Pasta', 131, 5, 25, 1.1),
('White Rice', 130, 2.7, 28.2, 0.3),

('Broccoli', 34, 2.8, 7, 0.4),
('Spinach', 23, 2.9, 3.6, 0.4),
('Tomato', 18, 0.9, 3.9, 0.2),
('Bell Pepper', 31, 1, 6, 0.3),
('Avocado', 160, 2, 8.5, 14.7),
('Cucumber', 15, 0.7, 3.6, 0.1),
('Lettuce', 15, 1.4, 2.9, 0.2),
('Carrot', 41, 0.9, 10, 0.2),

('Banana', 89, 1.1, 23, 0.3),
('Apple', 52, 0.3, 14, 0.2),
('Berries', 57, 0.7, 14, 0.3),
('Orange', 47, 0.9, 12, 0.1),

('Olive Oil', 884, 0, 0, 100),
('Almonds', 579, 21, 22, 50),
('Peanut Butter', 588, 25, 20, 50),
('Cheese', 402, 25, 1.3, 33),
('Milk', 42, 3.4, 5, 1),
('Butter', 717, 0.9, 0.1, 81),
('Honey', 304, 0.3, 82, 0);

INSERT INTO Meals (name, imageUrl, isKeto, isVegan, isNutFree, isLactoseFree, isGlutenFree, description) VALUES
('Classic Oatmeal with Berries', NULL, 0, 1, 1, 1, 1, 'Hearty oatmeal topped with fresh berries and honey'),
('Scrambled Eggs with Toast', NULL, 0, 0, 1, 0, 0, 'Fluffy scrambled eggs served with whole wheat toast'),
('Greek Yogurt Parfait', NULL, 0, 0, 1, 0, 1, 'Creamy Greek yogurt layered with berries and honey'),
('Avocado Toast', NULL, 0, 1, 1, 1, 0, 'Whole wheat toast topped with mashed avocado and tomatoes'),
('Protein Pancakes', NULL, 0, 0, 1, 0, 0, 'High-protein pancakes made with eggs and oats'),

('Grilled Chicken Salad', NULL, 1, 0, 1, 1, 1, 'Mixed greens with grilled chicken breast and vegetables'),
('Salmon Quinoa Bowl', NULL, 0, 0, 1, 1, 1, 'Baked salmon served over quinoa with roasted vegetables'),
('Turkey Sandwich', NULL, 0, 0, 1, 0, 0, 'Whole wheat sandwich with turkey, lettuce, and tomato'),
('Tuna Salad', NULL, 1, 0, 1, 0, 1, 'Fresh tuna mixed with vegetables and olive oil'),
('Tofu Stir Fry', NULL, 0, 1, 1, 1, 1, 'Crispy tofu with mixed vegetables in a light sauce'),

('Grilled Chicken with Rice', NULL, 0, 0, 1, 1, 1, 'Seasoned grilled chicken breast with brown rice and broccoli'),
('Beef and Sweet Potato', NULL, 0, 0, 1, 1, 1, 'Lean ground beef with roasted sweet potato and spinach'),
('Baked Salmon with Vegetables', NULL, 1, 0, 1, 1, 1, 'Oven-baked salmon fillet with mixed roasted vegetables'),
('Pasta with Turkey Meatballs', NULL, 0, 0, 1, 0, 0, 'Whole wheat pasta with homemade turkey meatballs'),
('Vegetarian Buddha Bowl', NULL, 0, 1, 1, 1, 1, 'Quinoa bowl with roasted vegetables and avocado');


INSERT INTO MealsIngredients (meal_id, food_id, quantity) VALUES
(1, (SELECT food_id FROM Ingredients WHERE name = 'Oats'), 80),
(1, (SELECT food_id FROM Ingredients WHERE name = 'Berries'), 100),
(1, (SELECT food_id FROM Ingredients WHERE name = 'Honey'), 20),
(1, (SELECT food_id FROM Ingredients WHERE name = 'Almonds'), 15);

INSERT INTO MealsIngredients (meal_id, food_id, quantity) VALUES
(2, (SELECT food_id FROM Ingredients WHERE name = 'Eggs'), 150),
(2, (SELECT food_id FROM Ingredients WHERE name = 'Whole Wheat Bread'), 70),
(2, (SELECT food_id FROM Ingredients WHERE name = 'Butter'), 10);

INSERT INTO MealsIngredients (meal_id, food_id, quantity) VALUES
(3, (SELECT food_id FROM Ingredients WHERE name = 'Greek Yogurt'), 250),
(3, (SELECT food_id FROM Ingredients WHERE name = 'Berries'), 100),
(3, (SELECT food_id FROM Ingredients WHERE name = 'Honey'), 30),
(3, (SELECT food_id FROM Ingredients WHERE name = 'Oats'), 30);

INSERT INTO MealsIngredients (meal_id, food_id, quantity) VALUES
(4, (SELECT food_id FROM Ingredients WHERE name = 'Whole Wheat Bread'), 80),
(4, (SELECT food_id FROM Ingredients WHERE name = 'Avocado'), 100),
(4, (SELECT food_id FROM Ingredients WHERE name = 'Tomato'), 50),
(4, (SELECT food_id FROM Ingredients WHERE name = 'Eggs'), 50);

INSERT INTO MealsIngredients (meal_id, food_id, quantity) VALUES
(5, (SELECT food_id FROM Ingredients WHERE name = 'Oats'), 80),
(5, (SELECT food_id FROM Ingredients WHERE name = 'Eggs'), 150),
(5, (SELECT food_id FROM Ingredients WHERE name = 'Banana'), 100),
(5, (SELECT food_id FROM Ingredients WHERE name = 'Honey'), 20);

INSERT INTO MealsIngredients (meal_id, food_id, quantity) VALUES
(6, (SELECT food_id FROM Ingredients WHERE name = 'Chicken Breast'), 200),
(6, (SELECT food_id FROM Ingredients WHERE name = 'Lettuce'), 100),
(6, (SELECT food_id FROM Ingredients WHERE name = 'Tomato'), 80),
(6, (SELECT food_id FROM Ingredients WHERE name = 'Cucumber'), 80),
(6, (SELECT food_id FROM Ingredients WHERE name = 'Olive Oil'), 15);

INSERT INTO MealsIngredients (meal_id, food_id, quantity) VALUES
(7, (SELECT food_id FROM Ingredients WHERE name = 'Salmon'), 180),
(7, (SELECT food_id FROM Ingredients WHERE name = 'Quinoa'), 150),
(7, (SELECT food_id FROM Ingredients WHERE name = 'Broccoli'), 100),
(7, (SELECT food_id FROM Ingredients WHERE name = 'Bell Pepper'), 80);

INSERT INTO MealsIngredients (meal_id, food_id, quantity) VALUES
(8, (SELECT food_id FROM Ingredients WHERE name = 'Turkey'), 120),
(8, (SELECT food_id FROM Ingredients WHERE name = 'Whole Wheat Bread'), 100),
(8, (SELECT food_id FROM Ingredients WHERE name = 'Lettuce'), 30),
(8, (SELECT food_id FROM Ingredients WHERE name = 'Tomato'), 50),
(8, (SELECT food_id FROM Ingredients WHERE name = 'Cheese'), 20);

INSERT INTO MealsIngredients (meal_id, food_id, quantity) VALUES
(9, (SELECT food_id FROM Ingredients WHERE name = 'Tuna'), 180),
(9, (SELECT food_id FROM Ingredients WHERE name = 'Lettuce'), 100),
(9, (SELECT food_id FROM Ingredients WHERE name = 'Cucumber'), 80),
(9, (SELECT food_id FROM Ingredients WHERE name = 'Tomato'), 80),
(9, (SELECT food_id FROM Ingredients WHERE name = 'Olive Oil'), 15);

INSERT INTO MealsIngredients (meal_id, food_id, quantity) VALUES
(10, (SELECT food_id FROM Ingredients WHERE name = 'Tofu'), 200),
(10, (SELECT food_id FROM Ingredients WHERE name = 'Brown Rice'), 150),
(10, (SELECT food_id FROM Ingredients WHERE name = 'Broccoli'), 100),
(10, (SELECT food_id FROM Ingredients WHERE name = 'Bell Pepper'), 80),
(10, (SELECT food_id FROM Ingredients WHERE name = 'Olive Oil'), 10);

INSERT INTO MealsIngredients (meal_id, food_id, quantity) VALUES
(11, (SELECT food_id FROM Ingredients WHERE name = 'Chicken Breast'), 220),
(11, (SELECT food_id FROM Ingredients WHERE name = 'Brown Rice'), 180),
(11, (SELECT food_id FROM Ingredients WHERE name = 'Broccoli'), 120);

INSERT INTO MealsIngredients (meal_id, food_id, quantity) VALUES
(12, (SELECT food_id FROM Ingredients WHERE name = 'Ground Beef'), 180),
(12, (SELECT food_id FROM Ingredients WHERE name = 'Sweet Potato'), 250),
(12, (SELECT food_id FROM Ingredients WHERE name = 'Spinach'), 100);

INSERT INTO MealsIngredients (meal_id, food_id, quantity) VALUES
(13, (SELECT food_id FROM Ingredients WHERE name = 'Salmon'), 200),
(13, (SELECT food_id FROM Ingredients WHERE name = 'Broccoli'), 120),
(13, (SELECT food_id FROM Ingredients WHERE name = 'Bell Pepper'), 100),
(13, (SELECT food_id FROM Ingredients WHERE name = 'Olive Oil'), 15);

INSERT INTO MealsIngredients (meal_id, food_id, quantity) VALUES
(14, (SELECT food_id FROM Ingredients WHERE name = 'Pasta'), 200),
(14, (SELECT food_id FROM Ingredients WHERE name = 'Turkey'), 180),
(14, (SELECT food_id FROM Ingredients WHERE name = 'Tomato'), 100),
(14, (SELECT food_id FROM Ingredients WHERE name = 'Olive Oil'), 10);

INSERT INTO MealsIngredients (meal_id, food_id, quantity) VALUES
(15, (SELECT food_id FROM Ingredients WHERE name = 'Quinoa'), 150),
(15, (SELECT food_id FROM Ingredients WHERE name = 'Avocado'), 80),
(15, (SELECT food_id FROM Ingredients WHERE name = 'Sweet Potato'), 150),
(15, (SELECT food_id FROM Ingredients WHERE name = 'Spinach'), 80),
(15, (SELECT food_id FROM Ingredients WHERE name = 'Broccoli'), 80);


INSERT INTO Reminders (user_id, name, has_sound, time, reminder_date, frequency) VALUES 
(2, 'Morning Vitamins', 1, '08:00:00', '2026-04-10', 'Daily'),
(2, 'Evening Protein Shake', 0, '20:30:00', '2026-04-10', 'Daily');