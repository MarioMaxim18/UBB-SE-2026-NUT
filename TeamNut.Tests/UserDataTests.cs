using Microsoft.VisualStudio.TestTools.UnitTesting;
using TeamNut.Models;
using System;

namespace TeamNut.Tests;

[TestClass]
public class UserDataTests
{
    private UserData MakeUser(int weight, int height, int age, string gender, string goal)
    {
        return new UserData
        {
            Weight = weight,
            Height = height,
            Age = age,
            Gender = gender,
            Goal = goal
        };
    }

    [TestMethod]
    public void BmiNormalGuy()
    {
        var ud = MakeUser(80, 180, 25, "male", "maintenance");
        Assert.AreEqual(25, ud.CalculateBmi());
    }

    [TestMethod]
    public void BmiShortHeavy()
    {
        var ud = MakeUser(100, 160, 30, "male", "bulk");
        Assert.AreEqual(39, ud.CalculateBmi()); // 100 / 1.6^2
    }

    [TestMethod]
    public void BmiZeroOnBadInput()
    {
        Assert.AreEqual(0, MakeUser(70, 0, 25, "female", "cut").CalculateBmi());
        Assert.AreEqual(0, MakeUser(0, 175, 25, "male", "cut").CalculateBmi());
    }

    [TestMethod]
    public void CaloriesMaleBulk()
    {
        var ud = MakeUser(80, 180, 25, "male", "bulk");
        // BMR = 10*80 + 6.25*180 - 5*25 + 5 = 1805
        // TDEE = 1805 * 1.55 = 2797.75, bulk +300 -> 3098
        Assert.AreEqual(3098, ud.CalculateCalorieNeeds());
    }

    [TestMethod]
    public void CaloriesFemaleCut()
    {
        var ud = MakeUser(60, 165, 30, "female", "cut");
        // BMR = 600 + 1031.25 - 150 - 161 = 1320.25
        // TDEE = 2046.39, cut -300 -> 1746
        Assert.AreEqual(1746, ud.CalculateCalorieNeeds());
    }

    [TestMethod]
    public void CaloriesMaintenance()
    {
        var ud = MakeUser(75, 175, 28, "male", "maintenance");
        // BMR = 750 + 1093.75 - 140 + 5 = 1708.75, TDEE = 2648.56 -> 2649
        Assert.AreEqual(2649, ud.CalculateCalorieNeeds());
    }

    [TestMethod]
    public void CaloriesBadInputs()
    {
        Assert.AreEqual(0, MakeUser(0, 170, 25, "male", "bulk").CalculateCalorieNeeds());
        Assert.AreEqual(0, MakeUser(70, 0, 25, "male", "bulk").CalculateCalorieNeeds());
        Assert.AreEqual(0, MakeUser(70, 170, 0, "male", "bulk").CalculateCalorieNeeds());
        Assert.AreEqual(0, MakeUser(70, 170, 25, "other", "bulk").CalculateCalorieNeeds());
    }
}
