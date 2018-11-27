﻿using System;
using System.Collections.Generic;

namespace CSP_LeavePlanner
{
    class Program
    {
        static void Main(string[] args)
        {
            List<AvailableDate> data = new List<AvailableDate>
            {
                new AvailableDate(0, "JAN0A", new DateTime(2019, 1, 1), new DateTime(2019, 1, 5), 6),
                new AvailableDate(1, "JAN0B", new DateTime(2019, 1, 6), new DateTime(2019, 1, 10), 6),
                new AvailableDate(2, "JAN1A", new DateTime(2019, 1, 11), new DateTime(2019, 1, 15), 6),
                new AvailableDate(3, "JAN1B", new DateTime(2019, 1, 16), new DateTime(2019, 1, 20), 6),
                new AvailableDate(4, "JAN2A", new DateTime(2019, 1, 21), new DateTime(2019, 1, 25), 6),
                new AvailableDate(5, "JAN2B", new DateTime(2019, 1, 26), new DateTime(2019, 1, 30), 6),
				//FEB
                new AvailableDate(6, "FEB0A", new DateTime(2019, 2, 1), new DateTime(2019, 2, 5), 5),
                new AvailableDate(7, "FEB0B", new DateTime(2019, 2, 6), new DateTime(2019, 2, 10), 5),
                new AvailableDate(8, "FEB1A", new DateTime(2019, 2, 11), new DateTime(2019, 2, 15), 5),
                new AvailableDate(9, "FEB1B", new DateTime(2019, 2, 16), new DateTime(2019, 2, 20), 5),
                new AvailableDate(10, "FEB2A", new DateTime(2019, 2, 21), new DateTime(2019, 2, 25), 5),
                //MAR
                new AvailableDate(11, "MAR0A", new DateTime(2019, 3, 1), new DateTime(2019, 3, 5), 5),
                new AvailableDate(12, "MAR0B", new DateTime(2019, 3, 6), new DateTime(2019, 3, 10), 5),
                new AvailableDate(13, "MAR1A", new DateTime(2019, 3, 11), new DateTime(2019, 3, 15), 5),
                new AvailableDate(14, "MAR1B", new DateTime(2019, 3, 16), new DateTime(2019, 3, 20), 5),
                new AvailableDate(15, "MAR2A", new DateTime(2019, 3, 21), new DateTime(2019, 3, 25), 5),
                new AvailableDate(16, "MAR2B", new DateTime(2019, 3, 26), new DateTime(2019, 3, 30), 5),
                //APR
                new AvailableDate(17, "APR0A", new DateTime(2019, 4, 1), new DateTime(2019, 4, 5), 4),
                new AvailableDate(18, "APR0B", new DateTime(2019, 4, 6), new DateTime(2019, 4, 10), 4),
                new AvailableDate(19, "APR1A", new DateTime(2019, 4, 11), new DateTime(2019, 4, 15), 4),
                new AvailableDate(20, "APR1B", new DateTime(2019, 4, 16), new DateTime(2019, 4, 20), 4),
                new AvailableDate(21, "APR2A", new DateTime(2019, 4, 21), new DateTime(2019, 4, 25), 4),
                new AvailableDate(22, "APR2B", new DateTime(2019, 4, 26), new DateTime(2019, 4, 30), 4),
                //MAY
                new AvailableDate(23, "MAY0A", new DateTime(2019, 5, 1), new DateTime(2019, 5, 5), 4),
                new AvailableDate(24, "MAY0B", new DateTime(2019, 5, 6), new DateTime(2019, 5, 10), 4),
                new AvailableDate(25, "MAY1A", new DateTime(2019, 5, 11), new DateTime(2019, 5, 15), 4),
                new AvailableDate(26, "MAY1B", new DateTime(2019, 5, 16), new DateTime(2019, 5, 20), 4),
                new AvailableDate(27, "MAY2A", new DateTime(2019, 5, 21), new DateTime(2019, 5, 25), 4),
                new AvailableDate(28, "MAY2B", new DateTime(2019, 5, 26), new DateTime(2019, 5, 30), 4),

                //JAN10DAY
                new AvailableDate(29, "JAN0", new DateTime(2019, 1, 1), new DateTime(2019, 1, 10), 3),
                new AvailableDate(30, "JAN1", new DateTime(2019, 1, 11), new DateTime(2019, 1, 20), 3),
                new AvailableDate(31, "JAN2", new DateTime(2019, 1, 21), new DateTime(2019, 1, 30), 3),
                //FEB10DAY
                new AvailableDate(32, "FEB0", new DateTime(2019, 2, 1), new DateTime(2019, 2, 10), 3),
                new AvailableDate(33, "FEB1", new DateTime(2019, 2, 11), new DateTime(2019, 2, 20), 3),
                //MAR10DAY
                new AvailableDate(34, "MAR0", new DateTime(2019, 3, 1), new DateTime(2019, 3, 10), 3),
                new AvailableDate(35, "MAR1", new DateTime(2019, 3, 11), new DateTime(2019, 3, 20), 3),
                new AvailableDate(36, "MAR2", new DateTime(2019, 3, 21), new DateTime(2019, 3, 30), 3),
                //APR10DAY
                new AvailableDate(37, "APR0", new DateTime(2019, 4, 1), new DateTime(2019, 4, 10), 3),
                new AvailableDate(38, "APR1", new DateTime(2019, 4, 11), new DateTime(2019, 4, 20), 3),
                new AvailableDate(39, "APR2", new DateTime(2019, 4, 21), new DateTime(2019, 4, 30), 3),
                //MAY10DAY
                new AvailableDate(40, "MAY0", new DateTime(2019, 5, 1), new DateTime(2019, 5, 10), 3),
                new AvailableDate(41, "MAY1", new DateTime(2019, 5, 11), new DateTime(2019, 5, 20), 3),
                new AvailableDate(42, "MAY2", new DateTime(2019, 5, 21), new DateTime(2019, 5, 30), 3)

                //new AvailableDate("FAKE", new DateTime(DateTime.Now.Year, 1, 26), new DateTime(DateTime.Now.Year, 1, 30)),
                //new AvailableDate("FAKE", new DateTime(DateTime.Now.Year, 1, 26), new DateTime(DateTime.Now.Year, 1, 30)),
                //new AvailableDate("FAKE", new DateTime(DateTime.Now.Year, 1, 26), new DateTime(DateTime.Now.Year, 1, 30)),
			};

            List<Employee> employees = new List<Employee>
            {
                new Employee(0, new List<AvailableDate>(), 3, Employee.LeaveMode.Days5, "JAN0A"),
                new Employee(1, new List<AvailableDate>(), 3, Employee.LeaveMode.Days5, "APR1B"),
                new Employee(2, new List<AvailableDate>(), 3, Employee.LeaveMode.Days5, "APR0A"),
                new Employee(3, new List<AvailableDate>(), 3, Employee.LeaveMode.Days5,  "MAY0A"),
                new Employee(4, new List<AvailableDate>(), 3, Employee.LeaveMode.Days5,  "JAN0A"),
                new Employee(5, new List<AvailableDate>(), 3, Employee.LeaveMode.Days5,  "FEB0A"),
                new Employee(6, new List<AvailableDate>(), 3, Employee.LeaveMode.Days5,  "MAY1B"),
                new Employee(7, new List<AvailableDate>(), 3, Employee.LeaveMode.Days5,  "MAY2B"),
                new Employee(8, new List<AvailableDate>(), 3, Employee.LeaveMode.Days5,  "FEB1B"),
                new Employee(9, new List<AvailableDate>(), 3, Employee.LeaveMode.Days5,  "JAN1A"),
                new Employee(10, new List<AvailableDate>(), 3, Employee.LeaveMode.Days5,  "MAY0B"),
                new Employee(11, new List<AvailableDate>(), 3, Employee.LeaveMode.Days5,  "MAY1B"),
                new Employee(12, new List<AvailableDate>(), 3, Employee.LeaveMode.Days5,  "MAR2A"),
                new Employee(13, new List<AvailableDate>(), 3, Employee.LeaveMode.Days5,  "MAR2B"),
                new Employee(14, new List<AvailableDate>(), 3, Employee.LeaveMode.Days5,  "APR0A"),
                new Employee(15, new List<AvailableDate>(), 3, Employee.LeaveMode.Days5,  "MAY2B"),
                new Employee(16, new List<AvailableDate>(), 3, Employee.LeaveMode.Days5,  "FEB0B"),
                new Employee(17, new List<AvailableDate>(), 3, Employee.LeaveMode.Days5,  "JAN2B"),
                new Employee(18, new List<AvailableDate>(), 3, Employee.LeaveMode.Days5,  "FEB0A"),
                new Employee(19, new List<AvailableDate>(), 3, Employee.LeaveMode.Days5,  "MAY0B"),
                new Employee(20, new List<AvailableDate>(), 3, Employee.LeaveMode.Days5,  "MAR2B"),
                new Employee(21, new List<AvailableDate>(), 3, Employee.LeaveMode.Days5,  "MAR2A"),
                new Employee(22, new List<AvailableDate>(), 3, Employee.LeaveMode.Days5,  "JAN1A"),
                new Employee(23, new List<AvailableDate>(), 3, Employee.LeaveMode.Days5,  "MAR2A"),
                new Employee(24, new List<AvailableDate>(), 3, Employee.LeaveMode.Days5,  "JAN2A"),
                new Employee(25, new List<AvailableDate>(), 3, Employee.LeaveMode.Days5,  "APR1A"),
                new Employee(26, new List<AvailableDate>(), 3, Employee.LeaveMode.Days5,  "FEB2A"),
                new Employee(27, new List<AvailableDate>(), 3, Employee.LeaveMode.Days5,  "FEB2A"),
                new Employee(28, new List<AvailableDate>(), 3, Employee.LeaveMode.Days5,  "APR1B"),
                new Employee(29, new List<AvailableDate>(), 3, Employee.LeaveMode.Days5,  "APR1B"),
                new Employee(30, new List<AvailableDate>(), 3, Employee.LeaveMode.Days10),
                new Employee(31, new List<AvailableDate>(), 3, Employee.LeaveMode.Days10),
                new Employee(32, new List<AvailableDate>(), 3, Employee.LeaveMode.Days10),
                new Employee(33, new List<AvailableDate>(), 3, Employee.LeaveMode.Days10),
                new Employee(34, new List<AvailableDate>(), 3, Employee.LeaveMode.Days10),
                new Employee(35, new List<AvailableDate>(), 3, Employee.LeaveMode.Days10),
                new Employee(36, new List<AvailableDate>(), 3, Employee.LeaveMode.Days10),
                new Employee(37, new List<AvailableDate>(), 3, Employee.LeaveMode.Days10),
                new Employee(38, new List<AvailableDate>(), 3, Employee.LeaveMode.Days10),
                new Employee(39, new List<AvailableDate>(), 3, Employee.LeaveMode.Days10),
                new Employee(40, new List<AvailableDate>(), 2, Employee.LeaveMode.Days10_5),
                new Employee(41, new List<AvailableDate>(), 2, Employee.LeaveMode.Days10_5),
                new Employee(42, new List<AvailableDate>(), 2, Employee.LeaveMode.Days10_5),
                new Employee(43, new List<AvailableDate>(), 2, Employee.LeaveMode.Days10_5),
                new Employee(44, new List<AvailableDate>(), 2, Employee.LeaveMode.Days10_5),
                new Employee(45, new List<AvailableDate>(), 2, Employee.LeaveMode.Days10_5),
                new Employee(46, new List<AvailableDate>(), 2, Employee.LeaveMode.Days10_5),
                new Employee(47, new List<AvailableDate>(), 2, Employee.LeaveMode.Days10_5),
                new Employee(48, new List<AvailableDate>(), 2, Employee.LeaveMode.Days10_5),
                new Employee(49, new List<AvailableDate>(), 2, Employee.LeaveMode.Days10_5)
            };

            Season season = new Season();
            season.HandleInput(employees, data);

            //season.SeasonInfo();
            season.Init();
            season.Solve();

            Console.ReadLine();
        }
    }
}