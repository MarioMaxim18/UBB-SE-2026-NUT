using Microsoft.VisualStudio.TestTools.UnitTesting;
using TeamNut.Models;
using System;

namespace TeamNut.Tests;

[TestClass]
public class MacroCalculationTests
{
    [TestMethod]
    public void ProteinBulk()
    {
        var ud = new UserData { Weight = 80, Height = 180, Age = 25, Gender = "male", Goal = "bulk" };
        Assert.AreEqual(160, ud.CalculateProteinNeeds()); // 80kg * 2.0
    }

    [TestMethod]
    public void ProteinCut()
    {
        var ud = new UserData { Weight = 80, Height = 180, Age = 25, Gender = "male", Goal = "cut" };
        Assert.AreEqual(176, ud.CalculateProteinNeeds()); // 80 * 2.2
    }

    [TestMethod]
    public void ProteinWellBeing()
    {
        var ud = new UserData { Weight = 70, Height = 170, Age = 25, Gender = "female", Goal = "well-being" };
        Assert.AreEqual(112, ud.CalculateProteinNeeds()); // 70 * 1.6
    }

    [TestMethod]
    public void FatNeedsBulk()
    {
        var ud = new UserData { Weight = 80, Height = 180, Age = 25, Gender = "male", Goal = "bulk" };
        // cals = 3098, 25% fat -> 774.5 / 9 = 86
        Assert.AreEqual(86, ud.CalculateFatNeeds());
    }

    [TestMethod]
    public void MacrosAllZeroOnBadWeight()
    {
        var ud = new UserData { Weight = 0, Height = 180, Age = 25, Gender = "male", Goal = "bulk" };
        Assert.AreEqual(0, ud.CalculateProteinNeeds());
        Assert.AreEqual(0, ud.CalculateFatNeeds());
        Assert.AreEqual(0, ud.CalculateCarbNeeds());
    }

    [TestMethod]
    public void CarbNeedsBulk()
    {
        var ud = new UserData { Weight = 80, Height = 180, Age = 25, Gender = "male", Goal = "bulk" };
        int cals = ud.CalculateCalorieNeeds();
        int prot = ud.CalculateProteinNeeds();
        int fat = ud.CalculateFatNeeds();
        int expected = (int)Math.Round((cals - prot * 4 - fat * 9) / 4.0);
        Assert.AreEqual(expected, ud.CalculateCarbNeeds());
    }

    [TestMethod]
    public void WellBeingCalories()
    {
        var ud = new UserData { Weight = 70, Height = 170, Age = 25, Gender = "female", Goal = "well-being" };
        // BMR = 700 + 1062.5 - 125 - 161 = 1476.5, TDEE = 2288.575 -> 2289
        Assert.AreEqual(2289, ud.CalculateCalorieNeeds());
    }

    [TestMethod]
    public void AgeCalcNormal()
    {
        var ud = new UserData();
        var bday = new DateTimeOffset(2000, 1, 15, 0, 0, 0, TimeSpan.Zero);
        int age = ud.CalculateAge(bday);
        // born jan 15 2000, today apr 14 2026 -> already had bday -> 26
        Assert.AreEqual(26, age);
    }

    [TestMethod]
    public void AgeCalcBirthdayNotYet()
    {
        var ud = new UserData();
        var bday = new DateTimeOffset(2000, 12, 25, 0, 0, 0, TimeSpan.Zero);
        int age = ud.CalculateAge(bday);
        // dec 25 hasn't happened yet this year -> 25
        Assert.AreEqual(25, age);
    }

    [TestMethod]
    public void AgeCalcNullReturnsZero()
    {
        Assert.AreEqual(0, new UserData().CalculateAge(null));
    }

    [TestMethod]
    public void MaintenanceFatHigherPercentage()
    {
        var ud = new UserData { Weight = 75, Height = 175, Age = 28, Gender = "male", Goal = "maintenance" };
        // maintenance uses 28% fat (vs 25% for bulk/cut)
        int cals = ud.CalculateCalorieNeeds(); // 2649
        int expected = (int)Math.Round(cals * 0.28 / 9.0);
        Assert.AreEqual(expected, ud.CalculateFatNeeds());
    }
}
