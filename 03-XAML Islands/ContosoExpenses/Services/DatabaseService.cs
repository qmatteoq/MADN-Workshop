﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bogus;
using LiteDB;
using ContosoExpenses.Models;

namespace ContosoExpenses.Services
{
    public class DatabaseService
    {
        readonly int numberOfEmployees = 10;
        readonly int numberOfExpenses = 5;

        string filePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\ContosoExpenses\\data.db";
        string directoryPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\ContosoExpenses\\";

        public Employee GetEmployee(int employeeId)
        {
            using (var connection = new LiteDatabase(filePath))
            {
                var employees = connection.GetCollection<Employee>();
                return employees.FindById(employeeId);
            }
        }

        public List<Employee> GetEmployees()
        {
            using (var connection = new LiteDatabase(filePath))
            {
                var employees = connection.GetCollection<Employee>();
                return employees.FindAll().ToList();
            }
        }

        public List<Expense> GetExpenses(int employeedId)
        {
            using (var connection = new LiteDatabase(filePath))
            {
                var expenses = connection.GetCollection<Expense>();
                return expenses.Find(x => x.EmployeeId == employeedId).ToList();
            }
        }

        public void SaveExpense(Expense expense)
        {
            using (var connection = new LiteDatabase(filePath))
            {
                var expenses = connection.GetCollection<Expense>();
                expenses.Insert(expense);
            }
        }

        public void InitializeDatabase()
        {
            if (!File.Exists(filePath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            using (var connection = new LiteDatabase(filePath))
            {
                var employees = connection.GetCollection<Employee>();
                var expenses = connection.GetCollection<Expense>();

                int result = employees.Count();
                if (result == 0)
                {
                    GenerateFakeData(numberOfEmployees, numberOfExpenses);
                }
            }
        }

        private void GenerateFakeData(int numberOfEmployees, int numberOfExpenses)
        {
            using (var connection = new LiteDatabase(filePath))
            {
                var employees = connection.GetCollection<Employee>();
                var expenses = connection.GetCollection<Expense>();

                for (int cont = 0; cont < numberOfEmployees; cont++)
                {
                    var employee = new Faker<Employee>()
                        .RuleFor(x => x.FirstName, (f, u) => f.Name.FirstName())
                        .RuleFor(x => x.LastName, (f, u) => f.Name.LastName())
                        .RuleFor(x => x.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName, "contoso.com"))
                        .Generate();

                    int employeeId = employees.Insert(employee).AsInt32;

                    for (int contExpenses = 0; contExpenses < numberOfExpenses; contExpenses++)
                    {
                        var expense = new Faker<Expense>()
                       .RuleFor(x => x.Description, (f, u) => f.Commerce.ProductName())
                       .RuleFor(x => x.Type, (f, u) => f.Finance.TransactionType())
                       .RuleFor(x => x.Cost, (f, u) => (double)f.Finance.Amount())
                       .RuleFor(x => x.Address, (f, u) => f.Address.FullAddress())
                       .RuleFor(x => x.City, (f, u) => f.Address.City())
                       .RuleFor(x => x.Date, (f, u) => f.Date.Past())
                       .Generate();

                        expense.EmployeeId = employeeId;

                        expenses.Insert(expense);
                    }
                }
            }
           
        }
    }
}
