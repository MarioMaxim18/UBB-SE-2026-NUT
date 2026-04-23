// <copyright file="PaginationServiceTests.cs" company="TeamNut">
// Copyright (c) TeamNut. All rights reserved.
// </copyright>

﻿using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using TeamNut.Services;
using Xunit;

namespace TeamNut.Tests.Services
{
    public class PaginationServiceTests
    {
        private readonly PaginationService service;

        public PaginationServiceTests()
        {
            service = new PaginationService();
        }

        [Fact]
        public void GetPage_WithNullItems_ReturnsEmptyList()
        {
            var result = service.GetPage<int>(null!, 1, 10);

            result.Should().BeEmpty();
        }

        [Fact]
        public void GetPage_WithZeroPageSize_ReturnsEmptyList()
        {
            var items = new List<int> { 1, 2, 3, 4, 5 };

            var result = service.GetPage(items, 1, 0);

            result.Should().BeEmpty();
        }

        [Fact]
        public void GetPage_WithNegativePageSize_ReturnsEmptyList()
        {
            var items = new List<int> { 1, 2, 3, 4, 5 };

            var result = service.GetPage(items, 1, -10);

            result.Should().BeEmpty();
        }

        [Fact]
        public void GetPage_WithPageLessThanOne_ReturnsEmptyList()
        {
            var items = new List<int> { 1, 2, 3, 4, 5 };

            var result = service.GetPage(items, 0, 10);

            result.Should().BeEmpty();
        }

        [Fact]
        public void GetPage_WithFirstPage_ReturnsFirstPageItems()
        {
            var items = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            var result = service.GetPage(items, 1, 3);

            result.Should().Equal(1, 2, 3);
        }

        [Fact]
        public void GetPage_WithSecondPage_ReturnsSecondPageItems()
        {
            var items = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            var result = service.GetPage(items, 2, 3);

            result.Should().Equal(4, 5, 6);
        }

        [Fact]
        public void GetPage_WithLastPartialPage_ReturnsRemainingItems()
        {
            var items = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 };

            var result = service.GetPage(items, 3, 3);

            result.Should().Equal(7, 8);
        }

        [Fact]
        public void GetPage_WithPageBeyondData_ReturnsEmptyList()
        {
            var items = new List<int> { 1, 2, 3 };

            var result = service.GetPage(items, 5, 3);

            result.Should().BeEmpty();
        }

        [Theory]
        [InlineData(0, 10, 1)]
        [InlineData(10, 10, 1)]
        [InlineData(11, 10, 2)]
        [InlineData(20, 10, 2)]
        [InlineData(21, 10, 3)]
        [InlineData(100, 10, 10)]
        public void GetTotalPages_WithVariousItemCounts_ReturnsCorrectPageCount(
            int totalItems,
            int pageSize,
            int expected)
        {
            var result = service.GetTotalPages(totalItems, pageSize);

            result.Should().Be(expected);
        }

        [Fact]
        public void GetTotalPages_WithZeroPageSize_ReturnsOne()
        {
            var result = service.GetTotalPages(100, 0);

            result.Should().Be(1);
        }

        [Fact]
        public void GetTotalPages_WithNegativePageSize_ReturnsOne()
        {
            var result = service.GetTotalPages(100, -10);

            result.Should().Be(1);
        }

        [Theory]
        [InlineData(1, 5, true)]
        [InlineData(3, 5, true)]
        [InlineData(5, 5, true)]
        [InlineData(0, 5, false)]
        [InlineData(6, 5, false)]
        [InlineData(-1, 5, false)]
        public void IsValidPage_WithVariousPages_ReturnsExpected(
            int currentPage,
            int totalPages,
            bool expected)
        {
            var result = service.IsValidPage(currentPage, totalPages);

            result.Should().Be(expected);
        }

        [Fact]
        public void GetPage_WithStringList_WorksCorrectly()
        {
            var items = new List<string> { "a", "b", "c", "d", "e", "f" };

            var result = service.GetPage(items, 2, 2);

            result.Should().Equal("c", "d");
        }
    }
}
